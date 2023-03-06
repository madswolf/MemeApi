using System.Net.Mail;
using System.Threading.Tasks;

namespace MemeApi.library
{
    public interface IMailSender
    {
        public bool sendMail(MailAddress recipient, string subject, string body);
    }
}
