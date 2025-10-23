// ConfigurationReader.Api/Program.cs
using ConfigurationReader.Api.Endpoints;
using ConfigurationReader.Api.Middleware;
using ConfigurationReader.Application;
using ConfigurationReader.Infrastructure;
using ConfigurationReader.Persistence;
using ConfigurationReader.Persistence.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========== DEBUG: Configuration Kontrol ==========
Console.WriteLine("========================================");
Console.WriteLine("    CONFIGURATION READER API");
Console.WriteLine("========================================");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Content Root: {builder.Environment.ContentRootPath}");
Console.WriteLine();

var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL");
var redisConnection = builder.Configuration.GetConnectionString("Redis");
var refreshInterval = builder.Configuration.GetValue<int>("ConfigurationRefreshIntervalSeconds", 30);

Console.WriteLine("Connection Strings:");
Console.WriteLine($"  PostgreSQL: {(postgresConnection != null ? "✓ Configured" : "✗ NOT FOUND")}");
Console.WriteLine($"  Redis:      {(redisConnection != null ? "✓ Configured" : "✗ NOT FOUND")}");
Console.WriteLine();
Console.WriteLine($"Refresh Interval: {refreshInterval} seconds");
Console.WriteLine("========================================");
Console.WriteLine();

if (string.IsNullOrWhiteSpace(postgresConnection))
{
    throw new InvalidOperationException(
        "PostgreSQL connection string is required! Check appsettings.json -> ConnectionStrings:PostgreSQL");
}

// ========== SERILOG CONFIGURATION ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ConfigurationReader.Api")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Configuration Reader API");
    Log.Information("PostgreSQL: {Connection}", postgresConnection.Split(';')[0]); // Only log server
    Log.Information("Redis: {HasRedis}", redisConnection != null ? "Enabled" : "Disabled");

    // ========== ADD SERVICES ==========

    // Clean Architecture Layers
    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // API Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger/OpenAPI
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Configuration Reader API",
            Version = "v1.0.0",
            Description = "Dynamic configuration management system with real-time updates, Redis caching, and multi-service support",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Configuration Manager",
                Email = "admin@example.com",
                Url = new Uri("https://github.com/yourusername/ConfigurationReader")
            },
            License = new Microsoft.OpenApi.Models.OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Add XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazor", policy =>
        {
            policy.WithOrigins(
                "https://localhost:7001",  // Admin Panel HTTPS
                "http://localhost:7001",   // Admin Panel HTTP
                "http://localhost:5173",   // Vite dev server
                "https://localhost:5001",  // API HTTPS (self)
                "http://localhost:5000")   // API HTTP (self) 
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ConfigurationDbContext>("database")
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    var app = builder.Build();

    // ========== MIDDLEWARE PIPELINE ==========

    // Exception Handling Middleware (en üstte olmalı)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Development-specific middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Configuration Reader API v1");
            options.RoutePrefix = string.Empty; // Swagger at root
            options.DocumentTitle = "Configuration Reader API";
            options.DisplayRequestDuration();
        });
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // Request logging with Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value!);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowBlazor");
    app.UseAuthorization();
    app.MapControllers();

    // ========== ENDPOINTS ==========

    // Seed Endpoints
    app.MapSeedEndpoints();

    // Health check endpoints
    app.MapHealthChecks("/health").WithTags("Health");
    app.MapHealthChecks("/health/ready").WithTags("Health");
    app.MapHealthChecks("/health/live").WithTags("Health");

    // Welcome/Info endpoint
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    app.MapGet("/api", () => Results.Ok(new
    {
        service = "Configuration Reader API",
        version = "1.0.0",
        status = "Running",
        environment = app.Environment.EnvironmentName,
        timestamp = DateTime.UtcNow,
        configuration = new
        {
            refreshIntervalSeconds = refreshInterval,
            hasRedis = !string.IsNullOrWhiteSpace(redisConnection),
            hasPostgreSQL = !string.IsNullOrWhiteSpace(postgresConnection)
        },
        endpoints = new
        {
            swagger = "/swagger",
            health = "/health",
            configurations = "/api/configurations",
            applications = "/api/configurations/applications",
            seed = "/api/seed",
            seedForce = "/api/seed/force"
        }
    }))
    .WithName("GetApiInfo")
    .WithTags("Info")
    .Produces(200);

    // Error handling endpoint (production)
    app.MapGet("/error", () => Results.Problem("An error occurred processing your request"))
        .ExcludeFromDescription();

    // ========== STARTUP VERIFICATION ==========

    // Verify database connection
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (canConnect)
            {
                Log.Information("✓ Database connection verified");

                // Check if database needs migration
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Log.Warning("⚠ Database has pending migrations: {Migrations}",
                        string.Join(", ", pendingMigrations));
                }
            }
            else
            {
                Log.Warning("⚠ Database connection failed - migrations may be needed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "✗ Database connection error");
            Log.Warning("Run: dotnet ef database update --startup-project ConfigurationReader.Api");
        }
    }

    // ========== START APPLICATION ==========

    Log.Information("========================================");
    Log.Information("Configuration Reader API is starting...");
    Log.Information("Swagger UI: {Url}", app.Environment.IsDevelopment() ? "https://localhost:5001" : "Disabled");
    Log.Information("========================================");

    app.Run();

    Log.Information("Configuration Reader API stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Configuration Reader API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}