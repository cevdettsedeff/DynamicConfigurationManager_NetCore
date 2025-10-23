// ConfigurationReader.Api/Middleware/ExceptionHandlingMiddleware.cs
using ConfigurationReader.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ConfigurationReader.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        (errorResponse.StatusCode, response.StatusCode, errorResponse.Message) = exception switch
        {
            ConfigurationNotFoundException =>
                ((int)HttpStatusCode.NotFound, (int)HttpStatusCode.NotFound, exception.Message),

            InvalidConfigurationTypeException =>
                ((int)HttpStatusCode.BadRequest, (int)HttpStatusCode.BadRequest, exception.Message),

            ArgumentException or ArgumentNullException =>
                ((int)HttpStatusCode.BadRequest, (int)HttpStatusCode.BadRequest, exception.Message),

            _ =>
                ((int)HttpStatusCode.InternalServerError, (int)HttpStatusCode.InternalServerError,
                 "An internal error occurred. Please try again later.")
        };


        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}