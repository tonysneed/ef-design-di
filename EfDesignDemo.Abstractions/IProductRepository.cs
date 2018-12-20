using System.Threading.Tasks;
using EfDesignDemo.Entities;

namespace EfDesignDemo.Abstractions
{
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
}