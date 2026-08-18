using Blog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Infrastructure;

/// <summary>
/// Application katmanından fırlayan özel hataları HTTP + ProblemDetails'e çevirir.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is AppValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                new ValidationProblemDetails(validationException.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Doğrulama hatası"
                },
                cancellationToken);
            return true;
        }

        if (exception is NotFoundException notFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Kayıt bulunamadı",
                    Detail = notFoundException.Message
                },
                cancellationToken);
            return true;
        }

        if (exception is UnauthorizedException unauthorizedException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Yetkisiz",
                    Detail = unauthorizedException.Message
                },
                cancellationToken);
            return true;
        }

        if (exception is ForbiddenException forbiddenException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Bu işlem için yetkiniz yok",
                    Detail = forbiddenException.Message
                },
                cancellationToken);
            return true;
        }

        if (exception is ConflictException conflictException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "İşlem bu durumda yapılamaz",
                    Detail = conflictException.Message
                },
                cancellationToken);
            return true;
        }

        return false;
    }
}
