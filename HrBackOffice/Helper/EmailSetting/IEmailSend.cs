﻿namespace HrBackOffice.Helper.EmailSetting
{
    public interface IEmailSend
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }

}
