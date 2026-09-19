using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Services.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        object response = new { message = exception.Message };

        // Si la excepción proviene de FluentValidation, cambiamos a 400 Bad Request
        if (exception is ValidationException validationException)
        {
            statusCode = StatusCodes.Status400BadRequest;

            var errors = validationException.Errors
                .Select(e => new { campo = e.PropertyName, error = e.ErrorMessage })
                .ToList();

            response = new
            {
                message = "Se produjeron errores de validación.",
                errors
            };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(jsonResponse);
    }
}