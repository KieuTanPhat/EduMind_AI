namespace StudyAI.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "no-reply@edumind.local";
    public string FromName { get; set; } = "EduMind AI";
    public string FrontendBaseUrl { get; set; } = "http://127.0.0.1:5173";
    public bool UseSsl { get; set; } = true;
}
