namespace FitTrackPro.Application.Common.Models;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp-relay.brevo.com";
    public int Port { get; set; } = 587;
    public required string SenderName { get; set; }
    public required string SenderEmail { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}