namespace FitTrackPro.API.Middlewares;

using System.Net;
using System.Text.Json;
using FitTrackPro.API.Errors;
using FluentValidation;

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
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        ApiErrorResponse errorResponse = exception switch
        {
            ValidationException validationException => new ValidationApiError
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validation failed",
                Errors = validationException.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            },
            _ => new GenericApiError
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An error occurred while processing your request",
                Details = context.RequestServices.GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment() ? exception.Message : null
            }
        };

        response.StatusCode = errorResponse.StatusCode;

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}