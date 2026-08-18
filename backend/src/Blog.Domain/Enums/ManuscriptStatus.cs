namespace Blog.Domain.Enums;

/// <summary>
/// Makalenin yayın sürecindeki yeri. UnderReview hakem ataması gelene kadar kullanılmaz;
/// numara şimdi sabit ki sonraki migration mevcut satırları kaydırmasın.
/// </summary>
public enum ManuscriptStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Published = 5
}
