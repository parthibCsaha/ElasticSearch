using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticSearchDemo.Data;
using ElasticSearchDemo.Services;
using Microsoft.EntityFrameworkCore;


namespace ElasticSearchDemo.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
             IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton(_ =>
            {
                var settings = new ElasticsearchClientSettings(
                    new Uri("https://localhost:9200"))
                .Authentication(
                    new BasicAuthentication(
                        "elastic",
                        "tm14aXThh7PadRnh1qyC"))
                .ServerCertificateValidationCallback(
                    (sender, certificate, chain, sslPolicyErrors) => true)
                .DefaultIndex("products")
                .DisableDirectStreaming();

                return new ElasticsearchClient(settings);
            });

            services.AddScoped<ProductIndexService>();

            return services;
        }
    }
}
