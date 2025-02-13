using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using HrBackOffice.Helper.EmailSetting;


public class SmtpSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
    public string SenderEmail { get; set; }
    public string Signature { get; set; }
}
public class EmailSender : IEmailSend
{
    private readonly SmtpClient _smtpClient;
    private readonly string _emailAddress;
    private readonly string _signature;

    public EmailSender(SmtpSettings smtpSettings)
    {
        _smtpClient = new SmtpClient(smtpSettings.Server, smtpSettings.Port)
        {
            EnableSsl = smtpSettings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
        };

        _emailAddress = smtpSettings.SenderEmail;
        _signature = smtpSettings.Signature;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailAddress),
            Subject = subject,
            Body = message + _signature,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await _smtpClient.SendMailAsync(mailMessage);
    }
}


