// Sample.ServiceA/Program.cs
using ConfigurationReader.Library;
using Sample.ServiceA.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Extension method ile (advanced)
builder.Services.AddConfigurationReader(options =>
{
    options.ApplicationName = "SERVICE-A";
    options.ConnectionString = "Host=localhost;Port=5432;Database=ConfigurationDb;Username=postgres;Password=postgres";
    options.RefreshTimerIntervalInMs = 30000;
    options.RedisConnectionString = "localhost:6379,abortConnect=false";
    options.CacheExpirationMinutes = 5;
    options.EnableLogging = true;
});

builder.Services.AddScoped<ProductService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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