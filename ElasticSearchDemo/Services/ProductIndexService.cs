using Elastic.Clients.Elasticsearch;
using ElasticSearchDemo.Data;
using ElasticSearchDemo.Dto;
using ElasticSearchDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearchDemo.Services
{
    public class ProductIndexService
    {
        private readonly ElasticsearchClient _esClient;
        private readonly AppDbContext _context;
        public ProductIndexService(ElasticsearchClient esClient, AppDbContext context)
        {
            _esClient = esClient;
            _context = context;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            var response = await _esClient.SearchAsync<Product>(s => s
                    .Indices("products")
                    .Query(q => q.MatchAll())
                );
            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to retrieve products from Elasticsearch: {response.DebugInformation}");
            }
            return response.Documents.ToList();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> CreateProduct(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _esClient.IndexAsync(product, i => i.Index("products").Id(product.Id));
            return product;
        }

        public async Task<Product?> UpdateProduct(int id, Product product)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing is null) return null;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Category = product.Category;

            await _context.SaveChangesAsync();

            await _esClient.IndexAsync(existing,  i => i
                            .Index("products")
                            .Id(existing.Id));
            return existing;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _esClient.DeleteAsync("products", id);
            return true;
        }

        public async Task<int> SyncToElasticsearch()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();

            foreach (var product in products)
            {
                await _esClient.IndexAsync(product, i => i.Index("products").Id(product.Id));
            }

            return products.Count;
        }

        public async Task<List<Product>> SearchProducts(string term)
        {
            var response = await _esClient.SearchAsync<Product>(s => s
                .Indices("products")
                .Query(q => q.MultiMatch(mm => mm
                    .Query(term)
                    .Fields(new[] { "name", "description", "category" })
                    .Fuzziness(new Elastic.Clients.Elasticsearch.Fuzziness("AUTO"))
                ))
            );

            if (!response.IsValidResponse)
                throw new Exception($"Product Search failed: {response.DebugInformation}");

            return response.Documents.ToList();
        }
    }
}
