using API.Common;
using Application.Common.Exceptions;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, message, errors) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, null),
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Message, null),
            ValidationException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message, ex.Errors),
            UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.Message, null),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            data = (object?)null,
            message,
            errors
        });
    }
}
