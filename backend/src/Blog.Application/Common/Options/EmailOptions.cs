namespace Blog.Application.Common.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string NotifyTo { get; set; } = "admin@blog.local";
    public string From { get; set; } = "noreply@blog.local";
}
