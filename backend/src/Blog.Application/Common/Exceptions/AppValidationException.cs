namespace Blog.Application.Common.Exceptions;

/// <summary>
/// FluentValidation hatalarını API'ye ProblemDetails olarak taşımak için.
/// </summary>
public sealed class AppValidationException : Exception
{
    public AppValidationException(IDictionary<string, string[]> errors)
        : base("Doğrulama hatası.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
