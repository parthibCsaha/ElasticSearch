using Elastic.Clients.Elasticsearch;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ =>
{
    var settings = new ElasticsearchClientSettings(
        new Uri("https://localhost:9200"))
        .DefaultIndex("products");

    return new ElasticsearchClient(settings);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
