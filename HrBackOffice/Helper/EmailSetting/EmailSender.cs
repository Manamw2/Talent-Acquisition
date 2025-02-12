using System.Net.Mail;
using System.Net;

namespace HrBackOffice.Helper.EmailSetting
{
    public class EmailSender : IEmailSend
    {
        private readonly string _smtpServer = "smtp.gmail.com";  
        private readonly int _smtpPort = 587;
        private readonly string _smtpUsername = "mahmoud.amr.nabil23@gmail.com";
        private readonly string _smtpPassword = "aybe vgmx zzqz ypgt";

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUsername),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }

}
