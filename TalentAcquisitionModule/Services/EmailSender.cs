using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace TalentAcquisitionModule.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _emailAddress;
        private readonly string _signature;

        public EmailSender(SmtpClient smtpClient, string emailAddress, string signature)
        {
            _smtpClient = smtpClient;
            _emailAddress = emailAddress;
            _signature = signature;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailAddress, "SoftTrend"),
                Subject = subject,
                Body = htmlMessage + _signature,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            return _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
