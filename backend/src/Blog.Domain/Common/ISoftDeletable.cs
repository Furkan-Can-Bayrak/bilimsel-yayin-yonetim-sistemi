namespace Blog.Domain.Common;

/// <summary>
/// Kaydı fiziksel olarak silmek yerine silinmiş olarak işaretleyen varlıklar.
/// Silme anı damgalanır; sorgular varsayılan olarak bu kayıtları hariç tutar.
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAtUtc { get; set; }
}
