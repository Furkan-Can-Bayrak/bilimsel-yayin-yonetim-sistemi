namespace Blog.Application.Common.Exceptions;

/// <summary>Kaynak mevcut durumda bu işlemi kabul etmiyor (HTTP 409).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
