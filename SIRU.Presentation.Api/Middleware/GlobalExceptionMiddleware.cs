using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace SIRU.Presentation.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns a standardized error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex.Message);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string exceptionMessage)
    {
        string exceptionTitle = "An unexpected error occurred";
        string details = exceptionMessage;

        switch (exceptionMessage)
        {
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Title = exceptionTitle,
            Detail = details,
            Status = context.Response.StatusCode,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}