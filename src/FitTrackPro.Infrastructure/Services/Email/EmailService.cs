namespace FitTrackPro.Infrastructure.Services.Email;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = $"Welcome to FitTrack Pro, {firstName}! 🎉";
        var body = GenerateWelcomeEmailHtml(firstName);
        
        await SendEmailInternalAsync(email, subject, body);
    }

    public async Task SendGoalReminderAsync(string email, string firstName, string goalDetails)
    {
        var subject = "Don't forget to log your progress! 💪";
        var body = GenerateGoalReminderHtml(firstName, goalDetails);
        
        await SendEmailInternalAsync(email, subject, body);
    }

    public async Task SendWeeklySummaryAsync(string email, string firstName, object summaryData)
    {
        var subject = $"Your Weekly Progress Report - {DateTime.UtcNow:dd MMM yyyy} 📊";
        var body = GenerateWeeklySummaryHtml(firstName, summaryData);
        
        await SendEmailInternalAsync(email, subject, body);
    }

    public async Task SendWeeklyReportWithAttachmentAsync(string email, string firstName, byte[] pdfReport)
    {
        var subject = $"Your Weekly Progress Report (PDF) - {DateTime.UtcNow:dd MMM yyyy} 📊";
        var body = GenerateWeeklySummaryHtml(firstName, null); // Dùng template tóm tắt làm body
        var fileName = $"FitTrack_Weekly_Report_{DateTime.UtcNow:yyyyMMdd}.pdf";

        await SendEmailWithAttachmentInternalAsync(email, subject, body, pdfReport, fileName);
    }

    public async Task SendPasswordResetEmailAsync(string email, string encodedToken)
    {
        string resetUrl = $"fittrackpro://reset-password?token={encodedToken}&email={email}";

        var subject = "Reset Your Password - FitTrack Pro";
        var body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h3>Password Reset Request</h3>
                <p>You requested to reset your password. Tap the button below to set a new one in the app:</p>
                <a href='{resetUrl}' style='display: inline-block; padding: 12px 24px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                <p style='margin-top: 20px; color: #777; font-size: 12px;'>If you did not request this, please ignore this email.</p>
            </div>";

        await SendEmailInternalAsync(email, subject, body);
    }

    public async Task SendEmailConfirmationAsync(string email, string encodedToken)
    {
        string mobileDeepLink = $"fittrackpro://verify-email?token={encodedToken}&email={email}";

        var subject = "Verify Your Email - FitTrack Pro";
        var body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h3>Welcome to FitTrack Pro!</h3>
                <p>Please verify your email address to secure your account.</p>
                <a href='{mobileDeepLink}' style='display: inline-block; padding: 12px 24px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Verify Email</a>
                <p style='margin-top: 20px; color: #777; font-size: 12px;'>If the button doesn't work, ensure you have the app installed.</p>
            </div>";

        await SendEmailInternalAsync(email, subject, body);
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        await ExecuteSendAsync(toEmail, subject, htmlBody, null);
    }

    private async Task SendEmailWithAttachmentInternalAsync(string toEmail, string subject, string htmlBody, byte[] attachmentData, string fileName)
    {
        await ExecuteSendAsync(toEmail, subject, htmlBody, (builder) => 
        {
            builder.Attachments.Add(fileName, attachmentData, ContentType.Parse("application/pdf"));
        });
    }

    private async Task ExecuteSendAsync(string toEmail, string subject, string htmlBody, Action<BodyBuilder>? attachmentConfig = null)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };

            if (attachmentConfig != null)
            {
                attachmentConfig(bodyBuilder);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

            // Authenticate
            if (!string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            // Send
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email '{Subject}' sent successfully to {Email}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {Email}", subject, toEmail);
            throw; // Rethrow to let caller handle if needed
        }
    }

    private static string GenerateWelcomeEmailHtml(string firstName)
    {
        return $@"<!DOCTYPE html><html><head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
            </style></head><body>
            <div class='container'>
                <div class='header'><h1>🎉 Welcome to FitTrack Pro!</h1></div>
                <div class='content'>
                    <h2>Hi {firstName},</h2>
                    <p>We're thrilled to have you join our fitness community! 💪</p>
                    <p>FitTrack Pro helps you:</p>
                    <ul>
                        <li>📊 Track your nutrition with 1000+ Vietnamese foods</li>
                        <li>⚖️ Monitor your weight and body measurements</li>
                    </ul>
                    <a href='https://fittrackpro.com/dashboard' class='button'>Go to Dashboard</a>
                    <p>Best regards,<br>The FitTrack Pro Team</p>
                </div>
                <div class='footer'><p>© 2025 FitTrack Pro. All rights reserved.</p></div>
            </div></body></html>";
    }

    private static string GenerateGoalReminderHtml(string firstName, string goalDetails)
    {
        return $@"<!DOCTYPE html><html><head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: #667eea; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 30px; background: #f9f9f9; }}
                .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; }}
            </style></head><body>
            <div class='container'>
                <div class='header'><h2>💪 Keep Going, {firstName}!</h2></div>
                <div class='content'>
                    <p>Hi {firstName},</p>
                    <p>Just a friendly reminder to log your progress today!</p>
                    <p><strong>Your Goal:</strong> {goalDetails}</p>
                    <a href='https://fittrackpro.com/progress' class='button'>Log Progress Now</a>
                </div>
            </div></body></html>";
    }

    private static string GenerateWeeklySummaryHtml(string firstName, object? summaryData)
    {
        return $@"<!DOCTYPE html><html><head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                .content {{ padding: 30px; background: #f9f9f9; }}
                .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
            </style></head><body>
            <div class='container'>
                <div class='header'>
                    <h1>📊 Your Weekly Progress Report</h1>
                    <p>Week of {DateTime.UtcNow.AddDays(-7):dd MMM} - {DateTime.UtcNow:dd MMM yyyy}</p>
                </div>
                <div class='content'>
                    <h2>Hi {firstName},</h2>
                    <p>Here's how your week went! 💪</p>
                    <p>See your detailed report attached as PDF, or view it online:</p>
                    <a href='https://fittrackpro.com/reports/weekly' class='button'>View Full Report</a>
                    <p>Keep pushing forward! 🎯</p>
                </div>
            </div></body></html>";
    }
}