using System.Net.Mail;
using System.Threading.Tasks;

namespace MemeApi.library
{
    public interface IMailSender
    {
        public Task<bool> sendMail(MailAddress recipient, string subject, string body);
    }
}
