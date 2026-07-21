using Elastic.Clients.Elasticsearch;
using ElasticSearchDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearchDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ElasticsearchClient _client;

        public ProductsController(ElasticsearchClient client)
        {
            _client = client;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var products = new List<Product>
            {
                new() { Id = 1, Name = "iPhone 15", Description = "Apple smartphone", Price = 1200, Category = "Electronics" },
                new() { Id = 2, Name = "Samsung Galaxy S24", Description = "Android flagship phone", Price = 1100, Category = "Electronics" },
                new() { Id = 3, Name = "MacBook Pro", Description = "Apple laptop", Price = 2500, Category = "Computers" }
            };

            foreach (var product in products)
            {
                var response = await _client.IndexAsync(product, i => i
                    .Index("products")
                    .Id(product.Id));

                if (!response.IsValidResponse)
                {
                    return BadRequest(response.DebugInformation);
                }
            }

            return Ok("Products indexed successfully");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string term)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index("products")
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(term)
                        .Fields(new[] { "name", "description", "category" })
                        .Fuzziness(new Elastic.Clients.Elasticsearch.Fuzziness("AUTO"))
                    )
                )
            );

            return Ok(response.Documents);
        }
    }
}
