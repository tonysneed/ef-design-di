using EfDesignDemo.Abstractions;
using EfDesignDemo.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EfDesignDemo.EF
{
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
}