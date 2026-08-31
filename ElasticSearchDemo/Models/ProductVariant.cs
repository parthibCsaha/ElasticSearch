using System.Text.Json.Serialization;

namespace ElasticSearchDemo.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public string SellerProductSku { get; set; }

        public string Color { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}