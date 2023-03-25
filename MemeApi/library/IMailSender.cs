using System.Net.Mail;
using System.Threading.Tasks;

namespace MemeApi.library
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface IMailSender
    {
        public bool sendMail(MailAddress recipient, string subject, string body);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
