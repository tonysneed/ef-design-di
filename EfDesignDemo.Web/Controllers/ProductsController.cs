using System.Threading.Tasks;
using EfDesignDemo.Abstractions;
using EfDesignDemo.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EfDesignDemo.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public IProductRepository ProductsRepo { get; }

        public ProductsController(IProductRepository productsRepo)
        {
            ProductsRepo = productsRepo;
        }

        // GET api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await ProductsRepo.GetProduct(id);
            return Ok(product);
        }
    }
}
