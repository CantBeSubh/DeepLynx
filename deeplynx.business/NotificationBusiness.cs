using System.Net;
using System.Net.Mail;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers;
using Npgsql;
using System.Text.Json;

namespace deeplynx.business;


/// Initializes a new instance of the <see cref="NotificationBusiness"/> class.
public class NotificationBusiness : INotificationBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    public NotificationBusiness(
        DeeplynxContext context
    )
    {
        _context = context;
    }

  /// <summary>
    /// Sends an email notification
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    public async Task<bool> SendEmail(string toEmail, string subject, string body)
    {
       try 
       {
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") 
            ?? throw new InvalidOperationException("SMTP_SERVER environment variable is not set");
        
        var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
        if (!int.TryParse(smtpPortStr, out int smtpPort))
        {
            smtpPort = 587; //default 
        }

        var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") 
            ?? throw new InvalidOperationException("FROM_EMAIL environment variable is not set");
        
        var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") 
            ?? throw new InvalidOperationException("EMAIL_PASSWORD environment variable is not set");
        
        var fromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "DeepLynx Nexus Notification";
        
        var enableSslStr = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true";
        bool.TryParse(enableSslStr, out bool enableSsl);

        // Create message
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(fromEmail, fromName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = body;

        // Configure SMTP client
        using var smtpClient = new SmtpClient(smtpServer, smtpPort);
        smtpClient.EnableSsl = enableSsl;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(fromEmail, emailPassword);

        // Send the email
        await smtpClient.SendMailAsync(mailMessage);
        
        // _logger.LogInformation("Email sent successfully to {ToEmail} with subject: {Subject}", toEmail, subject);
        return true;
       }
       catch (SmtpException smtpEx)
       {
            // _logger.LogError(smtpEx, "SMTP error occurred while sending email to {ToEmail}: {ErrorMessage}", toEmail, smtpEx.Message);
            return false;
       }
       catch (Exception ex)
       {
            // _logger.LogError(ex, "Unexpected error occurred while sending email to {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
            return false;
       }
    }
    
}