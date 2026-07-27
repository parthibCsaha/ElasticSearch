using Elastic.Clients.Elasticsearch;
using ElasticSearchDemo.Data;
using ElasticSearchDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearchDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ElasticsearchClient _esClient;
        private readonly AppDbContext _db;

        public ProductsController(ElasticsearchClient esClient, AppDbContext db)
        {
            _esClient = esClient;
            _db = db;
        }

        // ──────────────────────────────────────
        //  CRUD — Database Operations
        // ──────────────────────────────────────

        /// <summary>Get all products from the database.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _db.Products.AsNoTracking().ToListAsync();
            return Ok(products);
        }

        /// <summary>Get a single product by ID from the database.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();
            return Ok(product);
        }

        /// <summary>Create a new product → save to DB + index into Elasticsearch.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            // Sync to Elasticsearch
            await _esClient.IndexAsync(product, i => i
                .Index("products")
                .Id(product.Id));

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>Update an existing product → update DB + re-index in Elasticsearch.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            var existing = await _db.Products.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Category = product.Category;

            await _db.SaveChangesAsync();

            // Sync to Elasticsearch
            await _esClient.IndexAsync(existing, i => i
                .Index("products")
                .Id(existing.Id));

            return Ok(existing);
        }

        /// <summary>Delete a product → remove from DB + delete from Elasticsearch index.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            // Remove from Elasticsearch
            await _esClient.DeleteAsync("products", id);

            return NoContent();
        }

        // ──────────────────────────────────────
        //  Seed — Insert sample data into DB + Elasticsearch
        // ──────────────────────────────────────

        /// <summary>Seed sample products into the database and index them in Elasticsearch.</summary>
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            // Only seed if table is empty
            if (await _db.Products.AnyAsync())
                return BadRequest("Products already seeded. Delete them first or use POST to add more.");

            var products = new List<Product>
            {
                new() { Name = "iPhone 15", Description = "Apple smartphone", Price = 1200, Category = "Electronics" },
                new() { Name = "Samsung Galaxy S24", Description = "Android flagship phone", Price = 1100, Category = "Electronics" },
                new() { Name = "MacBook Pro", Description = "Apple laptop", Price = 2500, Category = "Computers" }
            };

            // Save to database
            _db.Products.AddRange(products);
            await _db.SaveChangesAsync();

            // Index into Elasticsearch
            foreach (var product in products)
            {
                var response = await _esClient.IndexAsync(product, i => i
                    .Index("products")
                    .Id(product.Id));

                if (!response.IsValidResponse)
                {
                    return BadRequest($"DB saved OK, but Elasticsearch failed for '{product.Name}': {response.DebugInformation}");
                }
            }

            return Ok($"Seeded {products.Count} products into DB + Elasticsearch.");
        }

        // ──────────────────────────────────────
        //  Sync — Re-index all DB products into Elasticsearch
        // ──────────────────────────────────────

        /// <summary>Re-index all products from the database into Elasticsearch.</summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncToElasticsearch()
        {
            var products = await _db.Products.AsNoTracking().ToListAsync();

            foreach (var product in products)
            {
                await _esClient.IndexAsync(product, i => i
                    .Index("products")
                    .Id(product.Id));
            }

            return Ok($"Synced {products.Count} products from DB → Elasticsearch.");
        }

        // ──────────────────────────────────────
        //  Search — via Elasticsearch (unchanged logic)
        // ──────────────────────────────────────

        /// <summary>Full-text search via Elasticsearch.</summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string term)
        {
            var response = await _esClient.SearchAsync<Product>(s => s
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
