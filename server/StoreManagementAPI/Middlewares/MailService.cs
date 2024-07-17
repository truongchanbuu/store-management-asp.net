using Microsoft.Extensions.Options;
using StoreManagementAPI.Configs;
using System.Net.Mail;

namespace StoreManagementAPI.Middlewares
{
    public class MailService
    {
        public readonly string HOST;
        private readonly string USERNAME;

        private readonly SmtpClient _smtpClient;
        private readonly string _clientBaseURL;

        public MailService(IOptions<MailSettings> mailSettings, IOptions<AppSettings> appSettings)
        {
            HOST = mailSettings.Value.Host;
            USERNAME = mailSettings.Value.Username;

            _smtpClient = new SmtpClient(HOST) 
            {
                Port = mailSettings.Value.Port,
                Credentials = new System.Net.NetworkCredential(USERNAME, mailSettings.Value.Password),
                EnableSsl = true,
            };
            _clientBaseURL = appSettings.Value.ClientBaseURL;
        }

        public async Task<bool> SendResetPasswordMail(string email, string token)
        {
            string subject = "Reset password";
            string url = $"{_clientBaseURL}/auth/reset-password?token={token}";
            string content = $"<p>Click this link to reset your password: <a href=\"{url}\">Reset Password</a></p>";

            return await SendMail(email, subject, content);
        }

        public async Task<bool> SendMail(string email, string subject, string content)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.To.Add(email);
                    message.Subject = subject;
                    message.Body = content;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(USERNAME);

                    await _smtpClient.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
