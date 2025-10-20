using Microsoft.Extensions.Configuration;
using SEP490_BE.BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            var smtpSection = _config.GetSection("Smtp");
            string host = smtpSection["Host"];
            int port = int.Parse(smtpSection["Port"]);
            bool enableSsl = bool.Parse(smtpSection["EnableSsl"]);
            string senderName = smtpSection["SenderName"];
            string user = smtpSection["User"];
            string pass = smtpSection["Pass"];

            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.Credentials = new NetworkCredential(user, pass);
                smtpClient.EnableSsl = enableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(user, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
        }
    }
}
