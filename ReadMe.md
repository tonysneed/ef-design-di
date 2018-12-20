# EF Code First with Dependency Injection

Demonstrates how to add DI to IDesignTimeDbContextFactory implementation so that the EF Core CLI will pick up the connection string via DI and Config.

## Prerequisites

- .NET Core SDK
    - https://dotnet.microsoft.com/download/
    - This demo uses .NET Core 2.2.

## ASPNET Core Web API Setup

1. Create a new Visual Studio solution and add a **global.json** file with the SDK version.
    - Open a command prompt at the solution root.
    - Enter and run: `dotnet new globaljson`
    - Add an existing item to the solution and select the global.json file.
    - Verify that the SDK version matches that which you previously installed.

1. Add an **ASP.NET Core Web API** project.
    - Use **.Web** suffix.

1. Add a **.NET Standard Class Library** project for entities.
    - Use **.Entities** suffix.
    - Add a `Product` class.

    ```csharp
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
    }
    ```

1. Add a **.NET Standard Class Library** project for repository interfaces.
    - Use **.Abstractions** suffix.
    - Reference the Entities project.
    - Add `IProductRepository` interface.

    ```csharp
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
    ```

1. Add a **.NET Standard Class Library** for EF context and repositories.
    - Use **.EF** suffix.
    - Reference Entities and Abstractions projects.
    - Add Microsoft.EntityFrameworkCore.Sql NuGet package.
    - Add `ProductsDbContext` class.
        - Include ctor that accepts `DbContextOptions`.
        - Override `OnModelCreating` to seed data.

    ```csharp
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var product = new Product
            {
                Id = 1,
                ProductName = "Chai",
                UnitPrice = 10
            };
            modelBuilder.Entity<Product>().HasData(product);
        }
    }
    ```

    - Add `ProductRepository` class.
        - Add ctor accepting `ProductsDbContext`.

    ```csharp
    public class ProductRepository : IProductRepository
    {
        public ProductsDbContext Context { get; }

        public ProductRepository(ProductsDbContext context)
        {
            Context = context;
        }

        public async Task<Product> GetProduct(int id)
        {
            return await Context.Products.SingleOrDefaultAsync(e => e.Id == id);
        }
    }
    ```

1. Add a connection string to the appsettings files in the Web project.
    - Use different database names to simulate Dev vs Prod deployments.
    - Add the following to **appsettings.json**:

    ```json
    "ConnectionStrings": {
        "ProductsDbContext": "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=ProductsDbProd;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

    - Add the following to **appsettings.Development.json**:

    ```json
    "ConnectionStrings": {
        "ProductsDbContext": "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=ProductsDbPDev;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

1. Configure DI in the **Web** project.
    - Add Microsoft.EntityFrameworkCore.Sql NuGet package.
    - Add references to the Entities, Abstractions and EF projects.
    -  Update `ConfigureServices` in `Startup` to register `ProductsDbContext` with the DI system.

    ```csharp
    services.AddDbContext<ProductsDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString(nameof(ProductsDbContext))));
    services.AddScoped<IProductRepository, ProductRepository>();
    ```

1. Refactor `ValuesController` in the Web project to use `IProductRepository` to retrieve a product by id.
    - Rename to `ProductsController`
    - Add ctor accepting `IProductRepository`.

    ```csharp
    public IProductRepository ProductsRepo { get; }

    public ProductsController(IProductRepository productsRepo)
    {
        ProductsRepo = productsRepo;
    }
    ```

    - Remove all methods except `Get(int id)`.
    - Refactor `Get` method.

    ```csharp
    // GET api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await ProductsRepo.GetProduct(id);
        return Ok(product);
    }
    ```

## Initial EF Core Code First Setup

1. Add a **.NET Core Class Library** project with a **.EF.Design** suffix.
    - This needs to be a .NET Core, versus .NET Standard, project in order 
      to support the EF Core CLI tooling.
    - Add the following NuGet packages:  
      Microsoft.EntityFrameworkCore.Design  
      Microsoft.EntityFrameworkCore.SqlServer

    - Add a `ProductsDbContextFactory` class that implements `IDesignTimeDbContextFactory<ProductsDbContext>`.

    ```csharp
    public class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
    {
        public ProductsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
            var connectionString = "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=ProductsDbDev;Integrated Security=True; MultipleActiveResultSets=True";
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("EfDesignDemo.EF.Design"));
            return new ProductsDbContext(optionsBuilder.Options);
        }
    }
    ```

1. This should be sufficient to generage a code migration and create a database.
    - Run the following from the **EF.Design** project directory.

    ```
    dotnet ef migrations add initial
    ```

    - Then run the following to create the database.

    ```
    dotnet ef database update
    ```

2. Try running the application.
    - First `cd` into the **Web** application directory.

    ```
    dotnet run
    ```

    - Then browse to: http://localhost:5000/api/products/1
        - A product should be retrieved from the database.

    ```json
    {
        id: 1,
        productName: "Chai",
        unitPrice: 10
    }
    ```

    - Press **Ctrl+C** to terminate the running app.

## Refactored EF Core Code First Setup

1. Refactor `ProductsDbContextFactory` to retrieve the connection string from the **appsettings.*.json** file.
    - This depends on the value of the `ASPNETCORE_ENVIRONMENT` environment variable.

    ```csharp
    public class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
    {
        public ProductsDbContext CreateDbContext(string[] args)
        {
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Build config
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EfDesignDemo"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
            var connectionString = config.GetConnectionString(nameof(ProductsDbContext));
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("EfDesignDemo.EF.Design"));
            return new ProductsDbContext(optionsBuilder.Options);
        }
    }
    ```

1. From the **EF.Design** project directory run `dotnet ef database update`.
    - This time the **ProductsDbProd** database should be created from the connection string in **appsettings.json**.
    - This is because the `ASPNETCORE_ENVIRONMENT` environment variable is not set.

2. Try setting `ASPNETCORE_ENVIRONMENT` on the command line before running `dotnet ef database update`.

    ```
    set ASPNETCORE_ENVIRONMENT=Development
    dotnet ef database update
    ```

    - First delete the **ProductsDbDev** database.
    - This time the **ProductsDbDev** database should be created.

## Add Some DI Love

1. Add some interfaces to the to the **.Abstractions** project.
    - Add a `IConfigurationService` interface.
        - Add the Microsoft.Extensions.Configuration.Abstractions NuGet package.

    ```csharp
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
    ```

    - Add a `IEnvironmentService` interface.

    ```csharp
    public interface IEnvironmentService
    {
        string EnvironmentName { get; set; }
    }
    ```

1. Add a **.Configuration** .NET Standard Class Library project to the solution.
    - Add the following NuGet packages:

    ```
    Microsoft.Extensions.Configuration
    Microsoft.Extensions.Configuration.EnvironmentVariables
    Microsoft.Extensions.Configuration.Json
    ```

    - Reference the **.Abstractions** project.
    - Add an `EnvironmentService` class.

    ```csharp
    public class EnvironmentService : IEnvironmentService
    {
        public EnvironmentService()
        {
            EnvironmentName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AspnetCoreEnvironment)
                ?? Constants.Environments.Production;
        }

        public string EnvironmentName { get; set; }
    }
    ```

    - Add an `ConfigurationService` class.

    ```csharp
    public class ConfigurationService : IConfigurationService
    {
        public IEnvironmentService EnvService { get; }
        public string CurrentDirectory { get; set; }

        public ConfigurationService(IEnvironmentService envService)
        {
            EnvService = envService;
        }

        public IConfiguration GetConfiguration()
        {
            CurrentDirectory = CurrentDirectory ?? Directory.GetCurrentDirectory();
            return new ConfigurationBuilder()
                .SetBasePath(CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvService.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
    ```

1. Add a **.DI** .NET Standard Class Library project.
    - Add the following NuGet packages.

    ```
    Microsoft.EntityFrameworkCore.Design
    Microsoft.EntityFrameworkCore.SqlServer
    ```

    - Add references to the **.EF**,** .Abstractions**, **.Configuration** projects.
    - Add a `DependencyResolver` class.

    ```csharp
    public class DependencyResolver
    {
        public IServiceProvider ServiceProvider { get; }
        public string CurrentDirectory { get; set; }

        public DependencyResolver()
        {
            // Set up Dependency Injection
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register env and config services
            services.AddTransient<IEnvironmentService, EnvironmentService>();
            services.AddTransient<IConfigurationService, ConfigurationService>
                (provider => new ConfigurationService(provider.GetService<IEnvironmentService>())
                {
                    CurrentDirectory = CurrentDirectory
                });

            // Register DbContext class
            services.AddTransient(provider =>
            {
                var configService = provider.GetService<IConfigurationService>();
                var connectionString = configService.GetConfiguration().GetConnectionString(nameof(ProductsDbContext));
                var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
                optionsBuilder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly("EfDesignDemo.EF.Design"));
                return new ProductsDbContext(optionsBuilder.Options);
            });
        }
    }
    ```

1. Add a reference from the **.EF.Design** project to the **.DI project**.
    - Refactor the `ProductsDbContextFactory` to use the `DependencyResolver` to create the `DbContext`.

    ```csharp
    public class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
    {
        public ProductsDbContext CreateDbContext(string[] args)
        {
            // Get DbContext from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../EfDesignDemo.Web")
            };
            return resolver.ServiceProvider.GetService(typeof(ProductsDbContext)) as ProductsDbContext;
        }
    }
    ```
