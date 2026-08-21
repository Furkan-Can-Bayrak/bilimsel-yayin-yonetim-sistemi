namespace Blog.Application.Common.Interfaces;

/// <summary>
/// Değişiklikleri tek seferde kaydeder. Birden fazla repository aynı transaction'da çalışır.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
