using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blog.Infrastructure.Email;

/// <summary>
/// Gerçek SMTP yerine log'a yazar — öğrenme için.
/// İleride SmtpEmailService ile değiştirilebilir; Application farkı görmez.
/// </summary>
public sealed class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;
    private readonly EmailOptions _settings;

    public LoggingEmailService(
        ILogger<LoggingEmailService> logger,
        IOptions<EmailOptions> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """

            ========== EMAIL (dev) ==========
            From:    {From}
            To:      {To}
            Subject: {Subject}
            Body:
            {Body}
            =================================

            """,
            _settings.From,
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}
