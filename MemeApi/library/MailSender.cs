using System.Net.Mail;
using System.Threading.Tasks;

namespace MemeApi.library
{
    public class MailSender : IMailSender
    {
        public Task<bool> sendMail(MailAddress recipient, string subject, string body)
        {
            throw new System.NotImplementedException();
        }
    }
}
