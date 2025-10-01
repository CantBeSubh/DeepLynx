using System.Net;
using System.Net.Mail;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using Microsoft.Extensions.Logging;


namespace deeplynx.business;


/// Initializes a new instance of the <see cref="NotificationBusiness"/> class.
public class NotificationBusiness : INotificationBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<NotificationBusiness> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for class operations</param>
    /// /// <param name="logger">Logging</param>
    public NotificationBusiness(
        DeeplynxContext context, ILogger<NotificationBusiness> logger
    )
    {
        _logger = logger;
        _context = context;
    }

  /// <summary>
    /// Sends an email notification
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="name">Recipient name (optional, defaults to "User")</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    public async Task<bool> SendEmail(string toEmail, string? name)
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

        var emailPassword = "";

        var fromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "DeepLynx Nexus Notification";

        var url = Environment.GetEnvironmentVariable("INVITE_URL")
            ?? throw new InvalidOperationException("Invite URL environment variable is not set");;

        var enableSslStr = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true";
        bool.TryParse(enableSslStr, out bool enableSsl);

        string templateContent = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
        <html xmlns=""http://www.w3.org/1999/xhtml"">
        <head>
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            <meta name=""color-scheme"" content=""light dark"" />
            <meta name=""supported-color-schemes"" content=""light dark"" />
            <title>{{subject}}</title>
            <style type=""text/css"">
                @media (prefers-color-scheme: dark) {
                    .dark-bg { background-color: #1a1a1a !important; }
                    .dark-card { background-color: #2a2a2a !important; }
                    .dark-text { color: #ffffff !important; }
                    .dark-text-secondary { color: #cccccc !important; }
                    .dark-border { border-color: #444444 !important; }
                }
            </style>
        </head>
        <body style=""font-family: 'Roboto', Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;"" class=""dark-bg"">
        <table style=""width: 100%; max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);"" cellpadding=""0"" cellspacing=""0"" class=""dark-card"">
            <tr>
                <td style=""background-color: #07519e; padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                    <h1 style=""color: #ffffff; font-size: 24px; font-weight: bold; margin: 0;"">
                        <img src=""https://deeplynx.dev.inl.gov/images/lynx-white.png""
                             alt=""DeepLynx Nexus""
                             style=""width: 50%; height: auto;""/>
                    </h1>
                </td>
            </tr>
            <tr>
                <td style=""padding: 40px 30px;"">
                    <h2 style=""color: #333333; font-size: 20px; margin: 0 0 20px 0;"" class=""dark-text"">Hello {{name}}!</h2>
                    <p style=""color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;"" class=""dark-text-secondary"">
                        You've been invited to join DeepLynx Nexus. Click the button below to get started.
                    </p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{{url}}"" style=""display: inline-block; background-color: #07519e; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold;"">Accept Invitation</a>
                    </div>
                    <div style=""margin-top: 30px; padding-top: 20px; border-top: 1px solid #eeeeee;"" class=""dark-border"">
                        <p style=""color: #666666; font-size: 14px; margin: 0;"" class=""dark-text-secondary"">
                            Best regards,<br>
                            The DeepLynx Nexus Team
                        </p>
                    </div>
                    <div style=""margin-top: 30px; padding-top: 15px; border-top: 1px solid #eeeeee;"" class=""dark-border"">
                        <p style=""color: #999999; font-size: 12px; margin: 0; line-height: 1.4;"" class=""dark-text-secondary"">
                            This invitation was sent to {{email}}.<br>
                            If you believe you received this email in error, please contact our support team @
                            {{support}}.
                        </p>
                    </div>
                </td>
            </tr>
        </table>
        </body>
        </html>";

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

        return true;
       }
       catch (SmtpException smtpEx)
       {
           _logger.LogError(smtpEx, "SMTP error occurred while sending email to {ToEmail}: {ErrorMessage}", toEmail, smtpEx.Message);
           return false;
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Unexpected error occurred while sending email to {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
           return false;
       }
    }

}