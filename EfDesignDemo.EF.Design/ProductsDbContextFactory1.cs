using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfDesignDemo.EF.Design
{
    public class ProductsDbContextFactory1 //: IDesignTimeDbContextFactory<ProductsDbContext>
    {
        public ProductsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
            var connectionString = "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=ProductsDbDev;Integrated Security=True; MultipleActiveResultSets=True";
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("EfDesignDemo.EF.Design"));
            return new ProductsDbContext(optionsBuilder.Options);
        }
    }
}