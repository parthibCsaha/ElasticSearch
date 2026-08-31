using ElasticSearchDemo.Dto;
using ElasticSearchDemo.Models;
using ElasticSearchDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearchDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductIndexService _productIndexService;

        public ProductsController(ProductIndexService productIndexService)
        {
            _productIndexService = productIndexService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productIndexService.GetAllProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productIndexService.GetProductById(id);
            if (product is null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var created = await _productIndexService.CreateProduct(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            var updated = await _productIndexService.UpdateProduct(id, product);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productIndexService.DeleteProduct(id);
            if (!deleted) return NotFound();
            return NoContent();
        }


        [HttpPost("sync")]
        public async Task<IActionResult> SyncToElasticsearch()
        {
            var count = await _productIndexService.SyncToElasticsearch();
            return Ok($"Synced {count} products from DB → Elasticsearch.");
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(string term)
        {
            var results = await _productIndexService.SearchProducts(term);
            return Ok(results);
        }
    }
}
