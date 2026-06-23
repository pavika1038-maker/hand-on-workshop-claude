namespace LeaveRequest.Infrastructure.Messaging;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;  // TLS 1.2+ enforced via OS/runtime
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@abc.com";
    public string FromDisplayName { get; set; } = "ABC Leave System";
    public int TimeoutMs { get; set; } = 10_000;
}
