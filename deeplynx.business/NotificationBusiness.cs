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
    public async Task<bool> SendEmail(string toEmail, string name)
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
        
        var support = Environment.GetEnvironmentVariable("SUPPORT_EMAIL") 
                        ?? throw new InvalidOperationException("SUPPORT_EMAIL environment variable is not set");
        
        var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") 
            ?? throw new InvalidOperationException("EMAIL_PASSWORD environment variable is not set");
        
        var fromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "DeepLynx Nexus Notification";
        
        var url = Environment.GetEnvironmentVariable("INVITE_URL") 
            ?? throw new InvalidOperationException("Invite URL environment variable is not set");;
        
        var enableSslStr = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true";
        bool.TryParse(enableSslStr, out bool enableSsl);
        
        
        string templateFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "deeplynx.business", "EmailTemplates", "InviteTemplate.html");

        if (!File.Exists(templateFile))
        {
            throw new InvalidOperationException("Email template not found");
        }

        string templateContent = await File.ReadAllTextAsync(templateFile);
      
       
        // Support both {{key}} and {key} placeholder formats
        templateContent = templateContent.Replace("{{name}}", name);
        templateContent = templateContent.Replace("{{email}}", toEmail);
        templateContent = templateContent.Replace("{{url}}", url);
        templateContent = templateContent.Replace("{{support}}", support);
        
       

        // Create message
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(fromEmail, fromName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = "DeepLynx Nexus Notification";
        mailMessage.Body = templateContent;
        mailMessage.IsBodyHtml = true;

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
  
    private async Task<string> ReadAndProcessTemplate(string templatePath, Dictionary<string, string> replacements)
    {
        try
        {
            if (!File.Exists(templatePath))
            {
                // _logger.LogWarning("Email template file not found: {TemplatePath}", templatePath);
                return string.Empty;
            }

            string templateContent = await File.ReadAllTextAsync(templatePath);
        
            if (replacements != null && replacements.Any())
            {
                foreach (var replacement in replacements)
                {
                    // Support both {{key}} and {key} placeholder formats
                    templateContent = templateContent.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
                    templateContent = templateContent.Replace($"{{{replacement.Key}}}", replacement.Value);
                }
            }

            return templateContent;
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error reading or processing email template: {TemplatePath}", templatePath);
            return string.Empty;
        }
    }
    
}