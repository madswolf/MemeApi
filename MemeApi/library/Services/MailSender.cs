using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;

namespace MemeApi.library.Services;

public class MailSender : IMailSender
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;
    public MailSender(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpClient = new SmtpClient
        {
            Host = _configuration["Email.Bot.Host"],
            Port = int.Parse(_configuration["Email.Bot.Host.Port"]),
            EnableSsl = true,
            Credentials = new NetworkCredential(_configuration["Email.Bot.Mail"], _configuration["Email.Bot.Password"])
        };
    }

    public bool sendMail(MailAddress recipient, string subject, string body)
    {
        var botMail = new MailAddress(_configuration["Email.Bot.Mail"]);
        using (var message = new MailMessage(botMail, recipient)
        {
            Subject = subject,
            Body = body
        })
        {
            try
            {
                _smtpClient.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
