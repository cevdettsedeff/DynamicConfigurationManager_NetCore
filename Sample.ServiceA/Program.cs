using ConfigurationReader.Library;
using Sample.ServiceA.Services;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES ==========

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sample Service A",
        Version = "v1",
        Description = "Demo service using ConfigurationReader Library"
    });
});

// ✅ ConfigurationReader Library
builder.Services.AddConfigurationReader(options =>
{
    var config = builder.Configuration.GetSection("ConfigurationReader");
    options.ApplicationName = config["ApplicationName"] ?? "SERVICE-A";
    options.ConnectionString = config["ConnectionString"]
        ?? throw new InvalidOperationException("ConnectionString is required");
    options.RedisConnectionString = config["RedisConnectionString"];
    options.RefreshIntervalSeconds = int.Parse(config["RefreshIntervalSeconds"] ?? "30");
    options.CacheExpirationMinutes = int.Parse(config["CacheExpirationMinutes"] ?? "5");
    options.EnableLogging = bool.Parse(config["EnableLogging"] ?? "true");
});

// Business Services
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// ========== MIDDLEWARE ==========

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample Service A v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ========== DEMO ENDPOINTS ==========

app.MapGet("/health", () => Results.Ok(new
{
    service = "Sample.ServiceA",
    status = "Healthy",
    timestamp = DateTime.UtcNow
})).WithTags("Health");

app.Run();