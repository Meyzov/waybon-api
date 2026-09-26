namespace Waybon.Infrastructure.Emails;

public sealed class BrevoOptions
{
    // ===================================
    // Constants
    // ===================================

    public const string SectionName = "Brevo";


    // ===================================
    // Properties
    // ===================================

    public string ApiKey { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
}