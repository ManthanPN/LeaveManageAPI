using LeaveManageAPI.Model;
using LeaveManageAPI.Service;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using System.Net;
//using System.Net.Mail;
using MailKit.Net.Smtp;


namespace LeaveManageAPI.Container
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> options)
        {
            this.emailSettings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Admin", emailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using var client = new SmtpClient();
            try
            {
                Console.WriteLine($"Connecting to {emailSettings.Host}:{emailSettings.Port}...");
                await client.ConnectAsync(emailSettings.Host, emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                Console.WriteLine("Connected to SMTP server.");

                Console.WriteLine("Authenticating...");
                await client.AuthenticateAsync(emailSettings.UserName, emailSettings.Password);
                Console.WriteLine("Authenticated successfully.");
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"SMTP connection failed: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
            /*{
                "id": 1018,
                "email": "manthan7086@gmail.com"
            }*/
        }
    }
}
