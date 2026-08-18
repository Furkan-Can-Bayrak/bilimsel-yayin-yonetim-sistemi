namespace Blog.Application.Common.Exceptions;

/// <summary>Kimlik doğrulandı ama bu kayda / işleme yetki yok (HTTP 403).</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
