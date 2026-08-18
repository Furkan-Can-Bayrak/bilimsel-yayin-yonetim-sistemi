using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using Blog.Domain.Entities;

namespace Blog.Application.Manuscripts;

internal static class ManuscriptPublication
{
    public static async Task NotifyPublishedAsync(
        INotificationService notifications,
        IEmailService email,
        EmailOptions emailOptions,
        Manuscript manuscript,
        CancellationToken cancellationToken)
    {
        await notifications.NotifyAsync(
            "Makale yayınlandı",
            $"\"{manuscript.Title}\" yayınlandı.",
            manuscript.Id,
            cancellationToken);

        await email.SendAsync(
            emailOptions.NotifyTo,
            $"Yeni makale: {manuscript.Title}",
            $"Makale yayınlandı.\nBaşlık: {manuscript.Title}\nSlug: {manuscript.Slug}",
            cancellationToken);
    }
}
