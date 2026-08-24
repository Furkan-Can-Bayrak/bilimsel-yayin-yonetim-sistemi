using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Options;
using Blog.Domain.Entities;
using Blog.Infrastructure.Auth;
using Blog.Infrastructure.Email;
using Blog.Infrastructure.Notifications;
using Blog.Infrastructure.Persistence;
using Blog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<BlogDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<BlogDbContext>());

        // Generic repository + entity-specific overrides (Manuscript, ResearchArea)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IManuscriptRepository, ManuscriptRepository>();
        services.AddScoped<IRepository<Manuscript>>(sp =>
            sp.GetRequiredService<IManuscriptRepository>());
        services.AddScoped<IResearchAreaRepository, ResearchAreaRepository>();
        services.AddScoped<IRepository<ResearchArea>>(sp =>
            sp.GetRequiredService<IResearchAreaRepository>());
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRepository<Notification>>(sp =>
            sp.GetRequiredService<INotificationRepository>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailService, LoggingEmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
