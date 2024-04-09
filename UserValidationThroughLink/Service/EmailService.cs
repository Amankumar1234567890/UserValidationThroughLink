
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using System.IO;


namespace UserValidationThroughLink
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> options)
        {
            this.emailSettings = options.Value;

        }
        public async Task SendEmailAsync(Mailrequest mailrequest)
        {
                // create email message
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(emailSettings.Email);
                email.To.Add(MailboxAddress.Parse(mailrequest.ToEmail));
                email.Subject = mailrequest.Subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = mailrequest.Body;
                email.Body = builder.ToMessageBody();




            //Send Mail
            using var smtp = new SmtpClient();
                smtp.Connect(emailSettings.Host, Convert.ToInt32(emailSettings.Port), SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSettings.Email, emailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            

        }
    }
}
