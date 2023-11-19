using System.Net.Mail;

namespace MemeApi.library.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IMailSender
{
    public bool sendMail(MailAddress recipient, string subject, string body);
}
