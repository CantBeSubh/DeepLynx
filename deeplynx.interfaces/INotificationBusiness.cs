using deeplynx.models;

namespace deeplynx.interfaces;

public interface INotificationBusiness
{
    Task<bool> SendEmail(string toEmail, string subject, string body);
}