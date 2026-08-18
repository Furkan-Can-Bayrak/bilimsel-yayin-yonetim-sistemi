namespace Blog.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = string.Empty;
}
