using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using EfDesignDemo.DI;

namespace EfDesignDemo.EF.Design
{
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
}