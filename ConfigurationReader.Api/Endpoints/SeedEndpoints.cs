using ConfigurationReader.Persistence.Seeders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ConfigurationReader.Api.Endpoints;

public static class SeedEndpoints
{
    public static IEndpointRouteBuilder MapSeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/seed")
            .WithTags("Seed")
            .WithOpenApi();

        group.MapPost("/", async (
            [FromServices] DatabaseSeeder seeder,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Seed endpoint called");
                await seeder.SeedAsync(force: false);
                return Results.Ok(new
                {
                    message = "Database seeded successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Seed failed: {Message}", ex.Message);
                logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                logger.LogError("Inner exception: {InnerException}", ex.InnerException?.Message);

                return Results.Problem(
                    title: "Seed failed",
                    detail: ex.InnerException?.Message ?? ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SeedDatabase")
        .WithSummary("Seeds the database if empty");

        group.MapPost("/force", async (
            [FromServices] DatabaseSeeder seeder,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Force seed endpoint called");
                await seeder.SeedAsync(force: true);
                return Results.Ok(new
                {
                    message = "Database force seeded successfully",
                    warning = "All existing data was cleared",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Force seed failed: {Message}", ex.Message);
                logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                logger.LogError("Inner exception: {InnerException}", ex.InnerException?.Message);

                return Results.Problem(
                    title: "Force seed failed",
                    detail: ex.InnerException?.Message ?? ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ForceSeedDatabase")
        .WithSummary("Clears database and reseeds with fresh data");

        return app;
    }
}