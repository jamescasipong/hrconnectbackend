using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace hrconnectbackend.Helper
{
    public class EmailService
    {
        public EmailService()
        {

        }

        public async Task SendEmailAsync()
        {
            var message = new MimeMessage();
        }
    }
}
