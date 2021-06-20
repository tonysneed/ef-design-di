using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EfDesignDemo.EF.Design
{
    public class ProductsDbContextFactory4: IDesignTimeDbContextFactory<ProductsDbContext>
    {
        public ProductsDbContext CreateDbContext(string[] args)
        {
            // debug
            // Console.WriteLine("input args: " + string.Join(", ", args));

            // Get environment from command line (.NET 5+)
            var argsList = new List<string>(args);
            var environment = args[argsList.IndexOf("--environment") + 1];
            Console.WriteLine($"Environment: {environment}");

            // Build config
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EfDesignDemo.Web"))
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
}