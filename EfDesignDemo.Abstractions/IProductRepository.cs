using EfDesignDemo.Entities;
using System.Threading.Tasks;

namespace EfDesignDemo.Abstractions
{
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
}