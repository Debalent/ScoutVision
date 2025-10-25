using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ScoutVision.API.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = "smtp.example.com"; // Replace with actual SMTP host
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "user@example.com";
        private readonly string _smtpPass = "password";
        private readonly string _fromEmail = "billing@scoutvision.com";

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };
            var mail = new MailMessage(_fromEmail, to, subject, body);
            mail.IsBodyHtml = true;
            await client.SendMailAsync(mail);
        }

        public async Task SendInvoiceEmailAsync(string to, string invoiceDetails)
        {
            var subject = "Your ScoutVision Invoice";
            var body = $"<h2>Invoice Details</h2><p>{invoiceDetails}</p>";
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPaymentConfirmationAsync(string to, string paymentDetails)
        {
            var subject = "Payment Confirmation";
            var body = $"<h2>Payment Received</h2><p>{paymentDetails}</p>";
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRenewalReminderAsync(string to, string renewalDetails)
        {
            var subject = "Subscription Renewal Reminder";
            var body = $"<h2>Renewal Reminder</h2><p>{renewalDetails}</p>";
            await SendEmailAsync(to, subject, body);
        }
    }
}
