namespace Blog.Application.Common.Exceptions;

/// <summary>Login başarısız veya yetkisiz işlem.</summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
