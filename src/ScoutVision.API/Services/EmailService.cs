using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ScoutVision.API.Services
{
    public interface IEmailService
    {
        Task SendPaymentConfirmationAsync(string customerEmail, string customerName, decimal amount, string invoiceId, DateTime paymentDate);
        Task SendPaymentReminderAsync(string customerEmail, string customerName, decimal amount, string invoiceId, DateTime dueDate);
        Task SendSubscriptionRenewalReminderAsync(string customerEmail, string customerName, string planName, decimal amount, DateTime renewalDate);
        Task SendWelcomeEmailAsync(string customerEmail, string customerName, string planName);
        Task SendSubscriptionCancelledAsync(string customerEmail, string customerName, string planName, DateTime endDate);
        Task SendInvoiceAsync(string customerEmail, string customerName, string invoiceId, decimal amount, DateTime dueDate, byte[] pdfAttachment);
        Task SendTrialWelcomeAsync(string customerEmail, string customerName, object trialData);
        Task SendTrialExtendedAsync(string customerEmail, string customerName, DateTime newEndDate);
        Task SendTrialConvertedAsync(string customerEmail, string customerName, string planName);
    }

    public class EmailService : IEmailService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Configure SMTP client
            _smtpClient = new SmtpClient
            {
                Host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
                Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                EnableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true"),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"] ?? "",
                    _configuration["Email:Password"] ?? ""
                )
            };
        }

        public async Task SendPaymentConfirmationAsync(string customerEmail, string customerName, decimal amount, string invoiceId, DateTime paymentDate)
        {
            var subject = "✅ Payment Confirmation - ScoutVision";
            var htmlBody = GeneratePaymentConfirmationHtml(customerName, amount, invoiceId, paymentDate);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Payment confirmation sent to {Email} for invoice {InvoiceId}", customerEmail, invoiceId);
        }

        public async Task SendPaymentReminderAsync(string customerEmail, string customerName, decimal amount, string invoiceId, DateTime dueDate)
        {
            var subject = "💳 Payment Reminder - ScoutVision";
            var htmlBody = GeneratePaymentReminderHtml(customerName, amount, invoiceId, dueDate);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Payment reminder sent to {Email} for invoice {InvoiceId}", customerEmail, invoiceId);
        }

        public async Task SendSubscriptionRenewalReminderAsync(string customerEmail, string customerName, string planName, decimal amount, DateTime renewalDate)
        {
            var subject = "🔄 Subscription Renewal Reminder - ScoutVision";
            var htmlBody = GenerateRenewalReminderHtml(customerName, planName, amount, renewalDate);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Renewal reminder sent to {Email} for plan {PlanName}", customerEmail, planName);
        }

        public async Task SendWelcomeEmailAsync(string customerEmail, string customerName, string planName)
        {
            var subject = "🎉 Welcome to ScoutVision - Your Account is Ready!";
            var htmlBody = GenerateWelcomeHtml(customerName, planName);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Welcome email sent to {Email}", customerEmail);
        }

        public async Task SendSubscriptionCancelledAsync(string customerEmail, string customerName, string planName, DateTime endDate)
        {
            var subject = "Subscription Cancelled - ScoutVision";
            var htmlBody = GenerateCancellationHtml(customerName, planName, endDate);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Cancellation email sent to {Email}", customerEmail);
        }

        public async Task SendTrialWelcomeAsync(string customerEmail, string customerName, object trialData)
        {
            var subject = "Welcome to Your ScoutVision Trial!";
            var htmlBody = GenerateTrialWelcomeHtml(customerName, trialData);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Trial welcome email sent to {Email}", customerEmail);
        }

        public async Task SendTrialExtendedAsync(string customerEmail, string customerName, DateTime newEndDate)
        {
            var subject = "Trial Extended - ScoutVision";
            var htmlBody = GenerateTrialExtendedHtml(customerName, newEndDate);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Trial extended email sent to {Email}", customerEmail);
        }

        public async Task SendTrialConvertedAsync(string customerEmail, string customerName, string planName)
        {
            var subject = "Welcome to ScoutVision - Subscription Activated!";
            var htmlBody = GenerateTrialConvertedHtml(customerName, planName);

            await SendEmailAsync(customerEmail, customerName, subject, htmlBody);
            _logger.LogInformation("Trial converted email sent to {Email}", customerEmail);
        }

        public async Task SendInvoiceAsync(string customerEmail, string customerName, string invoiceId, decimal amount, DateTime dueDate, byte[]? pdfAttachment = null)
        {
            var subject = $"📄 Invoice {invoiceId} - ScoutVision";
            var htmlBody = GenerateInvoiceHtml(customerName, invoiceId, amount, dueDate);

            var message = CreateMailMessage(customerEmail, customerName, subject, htmlBody);
            
            // Add PDF attachment if provided
            if (pdfAttachment != null)
            {
                var attachment = new Attachment(new MemoryStream(pdfAttachment), $"invoice_{invoiceId}.pdf", "application/pdf");
                message.Attachments.Add(attachment);
            }

            await _smtpClient.SendMailAsync(message);
            _logger.LogInformation("Invoice email sent to {Email} with{Attachment}", customerEmail, pdfAttachment != null ? " attachment" : "out attachment");
        }

        // Legacy methods for backward compatibility
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromEmail"] ?? "noreply@scoutvision.com", 
                                     _configuration["Email:FromName"] ?? "ScoutVision"),
                To = { new MailAddress(to) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            await _smtpClient.SendMailAsync(message);
        }

        private async Task SendEmailAsync(string email, string name, string subject, string htmlBody)
        {
            try
            {
                var message = CreateMailMessage(email, name, subject, htmlBody);
                await _smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }

        private MailMessage CreateMailMessage(string email, string name, string subject, string htmlBody)
        {
            var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@scoutvision.com";
            var fromName = _configuration["Email:FromName"] ?? "ScoutVision";

            return new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                To = { new MailAddress(email, name) },
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };
        }

        private string GeneratePaymentConfirmationHtml(string customerName, decimal amount, string invoiceId, DateTime paymentDate)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #0066cc, #004499); color: white; padding: 30px 20px; text-align: center; }}
                    .logo {{ font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
                    .content {{ padding: 30px 20px; }}
                    .success-badge {{ background-color: #d4edda; border: 2px solid #28a745; color: #155724; padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center; }}
                    .details {{ background-color: #f8f9fa; border-left: 4px solid #0066cc; padding: 20px; margin: 20px 0; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .button {{ display: inline-block; padding: 12px 24px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                    .amount {{ font-size: 24px; font-weight: bold; color: #28a745; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>⚽ ScoutVision</div>
                        <h2 style='margin: 0;'>Payment Confirmation</h2>
                    </div>
                    <div class='content'>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='success-badge'>
                            <h3 style='margin: 0 0 10px 0;'>✅ Payment Successful!</h3>
                            <p style='margin: 0;'>Thank you for your payment. Your account remains active and all services are available.</p>
                        </div>
                        
                        <div class='details'>
                            <h3 style='margin-top: 0;'>💰 Payment Details</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0;'><strong>Amount Paid:</strong></td><td style='text-align: right;'><span class='amount'>{amount:C}</span></td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Invoice Number:</strong></td><td style='text-align: right;'>{invoiceId}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Payment Date:</strong></td><td style='text-align: right;'>{paymentDate:MMMM dd, yyyy}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Transaction ID:</strong></td><td style='text-align: right;'>{Guid.NewGuid().ToString("N")[..8].ToUpper()}</td></tr>
                            </table>
                        </div>
                        
                        <p>You can view your complete billing history and download invoices anytime in your customer portal:</p>
                        
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='https://app.scoutvision.com/portal' class='button'>View Customer Portal</a>
                        </p>
                        
                        <p>If you have any questions about this payment or need assistance, our support team is here to help.</p>
                        
                        <p>Thank you for choosing ScoutVision!</p>
                        
                        <p>Best regards,<br><strong>The ScoutVision Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p>Need help? Contact us at <a href='mailto:support@scoutvision.com'>support@scoutvision.com</a></p>
                        <p>© 2025 ScoutVision. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GeneratePaymentReminderHtml(string customerName, decimal amount, string invoiceId, DateTime dueDate)
        {
            var daysUntilDue = (dueDate - DateTime.UtcNow).Days;
            var urgencyLevel = daysUntilDue <= 3 ? "urgent" : daysUntilDue <= 7 ? "warning" : "info";
            var urgencyColor = urgencyLevel == "urgent" ? "#dc3545" : urgencyLevel == "warning" ? "#ffc107" : "#17a2b8";

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, {urgencyColor}, {urgencyColor}dd); color: white; padding: 30px 20px; text-align: center; }}
                    .content {{ padding: 30px 20px; }}
                    .reminder-badge {{ background-color: {(urgencyLevel == "urgent" ? "#f8d7da" : urgencyLevel == "warning" ? "#fff3cd" : "#d1ecf1")}; border: 2px solid {urgencyColor}; padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center; }}
                    .details {{ background-color: #f8f9fa; border-left: 4px solid {urgencyColor}; padding: 20px; margin: 20px 0; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .pay-button {{ display: inline-block; padding: 15px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div style='font-size: 28px; font-weight: bold; margin-bottom: 10px;'>⚽ ScoutVision</div>
                        <h2 style='margin: 0;'>Payment Reminder</h2>
                    </div>
                    <div class='content'>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='reminder-badge'>
                            <h3 style='margin: 0 0 10px 0;'>{(urgencyLevel == "urgent" ? "⚠️ URGENT:" : "💳")} Payment Due {(daysUntilDue <= 0 ? "Today" : daysUntilDue == 1 ? "Tomorrow" : $"in {daysUntilDue} days")}</h3>
                            <p style='margin: 0;'>Your ScoutVision subscription payment is {(daysUntilDue <= 0 ? "due today" : $"due {(daysUntilDue == 1 ? "tomorrow" : $"in {daysUntilDue} days")}")}. Please ensure payment is made by the due date to avoid service interruption.</p>
                        </div>
                        
                        <div class='details'>
                            <h3 style='margin-top: 0;'>📋 Invoice Details</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0;'><strong>Amount Due:</strong></td><td style='text-align: right; font-size: 18px; font-weight: bold; color: {urgencyColor};'>{amount:C}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Invoice Number:</strong></td><td style='text-align: right;'>{invoiceId}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Due Date:</strong></td><td style='text-align: right;'>{dueDate:MMMM dd, yyyy}</td></tr>
                            </table>
                        </div>
                        
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='https://app.scoutvision.com/pay/{invoiceId}' class='pay-button'>💳 Pay Now</a>
                        </p>
                        
                        <p><strong>What happens next?</strong></p>
                        <ul>
                            <li>✅ Pay by {dueDate:MMMM dd} to keep your service active</li>
                            <li>📱 Access your account remains uninterrupted</li>
                            <li>📧 Receive instant payment confirmation</li>
                        </ul>
                        
                        <p>If you've already made this payment, please disregard this reminder. If you need assistance or have questions about your invoice, we're here to help!</p>
                        
                        <p>Best regards,<br><strong>The ScoutVision Billing Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p>Questions? Contact us at <a href='mailto:billing@scoutvision.com'>billing@scoutvision.com</a></p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateRenewalReminderHtml(string customerName, string planName, decimal amount, DateTime renewalDate)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #6c5ce7, #5a4fcf); color: white; padding: 30px 20px; text-align: center; }}
                    .content {{ padding: 30px 20px; }}
                    .renewal-info {{ background-color: #f0f3ff; border: 2px solid #6c5ce7; padding: 20px; border-radius: 10px; margin: 20px 0; }}
                    .plan-details {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .button {{ display: inline-block; padding: 12px 24px; background-color: #6c5ce7; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div style='font-size: 28px; font-weight: bold; margin-bottom: 10px;'>⚽ ScoutVision</div>
                        <h2 style='margin: 0;'>Subscription Renewal</h2>
                    </div>
                    <div class='content'>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='renewal-info'>
                            <h3 style='margin: 0 0 15px 0;'>🔄 Your subscription renews soon!</h3>
                            <p style='margin: 0;'>Your <strong>{planName}</strong> subscription will automatically renew on <strong>{renewalDate:MMMM dd, yyyy}</strong>. No action is required from you.</p>
                        </div>
                        
                        <div class='plan-details'>
                            <h3 style='margin-top: 0;'>📋 Renewal Summary</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0;'><strong>Plan:</strong></td><td style='text-align: right;'>{planName}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Renewal Amount:</strong></td><td style='text-align: right; font-weight: bold; color: #28a745;'>{amount:C}/month</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Renewal Date:</strong></td><td style='text-align: right;'>{renewalDate:MMMM dd, yyyy}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Payment Method:</strong></td><td style='text-align: right;'>Card ending in ••••</td></tr>
                            </table>
                        </div>
                        
                        <p><strong>What's included in your {planName} plan:</strong></p>
                        <ul>
                            <li>🎯 Advanced player analytics and insights</li>
                            <li>🤖 AI-powered injury prevention recommendations</li>
                            <li>📊 Real-time performance tracking</li>
                            <li>📱 Mobile app access for on-the-go scouting</li>
                            <li>💬 Priority customer support</li>
                        </ul>
                        
                        <p>Want to make changes to your subscription or update your payment method?</p>
                        
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='https://app.scoutvision.com/portal' class='button'>Manage Subscription</a>
                        </p>
                        
                        <p>Thank you for being a valued ScoutVision customer. We're excited to continue supporting your sports intelligence needs!</p>
                        
                        <p>Best regards,<br><strong>The ScoutVision Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p>Need help? Contact us at <a href='mailto:support@scoutvision.com'>support@scoutvision.com</a></p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateWelcomeHtml(string customerName, string planName)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #00b894, #00a085); color: white; padding: 40px 20px; text-align: center; }}
                    .content {{ padding: 30px 20px; }}
                    .welcome-badge {{ background: linear-gradient(135deg, #d1f2eb, #a3e4d7); border: 2px solid #00b894; padding: 25px; border-radius: 15px; margin: 25px 0; text-align: center; }}
                    .feature-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 25px 0; }}
                    .feature-item {{ background-color: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #00b894; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .get-started {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #00b894, #00a085); color: white; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div style='font-size: 32px; font-weight: bold; margin-bottom: 15px;'>⚽ ScoutVision</div>
                        <h2 style='margin: 0; font-size: 24px;'>Welcome to the Team!</h2>
                    </div>
                    <div class='content'>
                        <p style='font-size: 16px;'>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='welcome-badge'>
                            <h2 style='margin: 0 0 15px 0; color: #00b894;'>🎉 Welcome to ScoutVision!</h2>
                            <p style='margin: 0; font-size: 16px;'>Thank you for choosing <strong>ScoutVision {planName}</strong>. You're now part of the most advanced sports intelligence platform used by top teams worldwide.</p>
                        </div>
                        
                        <h3 style='color: #00b894; margin-bottom: 20px;'>🚀 Ready to get started? Here's what you can do:</h3>
                        
                        <div class='feature-grid'>
                            <div class='feature-item'>
                                <strong>👥 Add Your Team</strong><br>
                                <small>Create player profiles and import existing data</small>
                            </div>
                            <div class='feature-item'>
                                <strong>📊 Explore Analytics</strong><br>
                                <small>Discover powerful performance insights</small>
                            </div>
                            <div class='feature-item'>
                                <strong>🤖 Try AI Features</strong><br>
                                <small>Get injury prevention and transfer recommendations</small>
                            </div>
                            <div class='feature-item'>
                                <strong>📱 Download Mobile App</strong><br>
                                <small>Scout players on-the-go</small>
                            </div>
                        </div>
                        
                        <div style='background-color: #e8f5e8; padding: 20px; border-radius: 10px; margin: 25px 0;'>
                            <h4 style='margin: 0 0 10px 0; color: #00b894;'>💡 Pro Tip:</h4>
                            <p style='margin: 0;'>Start with our interactive onboarding tutorial to get the most out of ScoutVision from day one!</p>
                        </div>
                        
                        <p style='text-align: center; margin: 35px 0;'>
                            <a href='https://app.scoutvision.com/onboarding' class='get-started'>🎯 Start Your Journey</a>
                        </p>
                        
                        <h3 style='color: #00b894;'>📞 Need Help?</h3>
                        <ul style='margin-bottom: 25px;'>
                            <li>📚 Check out our <a href='https://docs.scoutvision.com' style='color: #00b894;'>comprehensive guides</a></li>
                            <li>🎥 Watch <a href='https://learn.scoutvision.com' style='color: #00b894;'>video tutorials</a></li>
                            <li>💬 Chat with our support team (available 24/7)</li>
                            <li>📧 Email us at <a href='mailto:support@scoutvision.com' style='color: #00b894;'>support@scoutvision.com</a></li>
                        </ul>
                        
                        <p>We're thrilled to have you on board and can't wait to see how ScoutVision transforms your team's performance!</p>
                        
                        <p style='margin-bottom: 0;'>Welcome to the future of sports intelligence!</p>
                        
                        <p><strong>The ScoutVision Team</strong><br>
                        <em>Empowering teams to achieve greatness</em></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p style='margin: 10px 0;'>
                            <a href='https://app.scoutvision.com' style='color: #00b894;'>Dashboard</a> | 
                            <a href='https://docs.scoutvision.com' style='color: #00b894;'>Documentation</a> | 
                            <a href='mailto:support@scoutvision.com' style='color: #00b894;'>Support</a>
                        </p>
                        <p>© 2025 ScoutVision. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateCancellationHtml(string customerName, string planName, DateTime endDate)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #e17055, #d63031); color: white; padding: 30px 20px; text-align: center; }}
                    .content {{ padding: 30px 20px; }}
                    .cancellation-info {{ background-color: #fff5f5; border: 2px solid #e17055; padding: 20px; border-radius: 10px; margin: 20px 0; }}
                    .timeline {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .feedback-button {{ display: inline-block; padding: 12px 24px; background-color: #e17055; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                    .reactivate-button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div style='font-size: 28px; font-weight: bold; margin-bottom: 10px;'>⚽ ScoutVision</div>
                        <h2 style='margin: 0;'>Subscription Cancelled</h2>
                    </div>
                    <div class='content'>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='cancellation-info'>
                            <h3 style='margin: 0 0 15px 0;'>📋 We've processed your cancellation</h3>
                            <p style='margin: 0;'>We're sorry to see you go! Your <strong>{planName}</strong> subscription has been successfully cancelled as requested.</p>
                        </div>
                        
                        <div class='timeline'>
                            <h3 style='margin-top: 0;'>⏰ What happens next?</h3>
                            <ul style='margin: 0; padding-left: 20px;'>
                                <li><strong>Today:</strong> Cancellation confirmed, no future charges</li>
                                <li><strong>Until {endDate:MMMM dd}:</strong> Full access to all ScoutVision features</li>
                                <li><strong>After {endDate:MMMM dd}:</strong> Account deactivated, data safely stored for 90 days</li>
                                <li><strong>Within 90 days:</strong> Easy reactivation available if you change your mind</li>
                            </ul>
                        </div>
                        
                        <p><strong>Important details:</strong></p>
                        <ul>
                            <li>✅ <strong>No additional charges</strong> will be made to your account</li>
                            <li>📅 <strong>Service continues until {endDate:MMMM dd, yyyy}</strong></li>
                            <li>💾 <strong>Your data is safe</strong> - we'll keep it for 90 days in case you return</li>
                            <li>📧 <strong>Account access</strong> remains active until your subscription ends</li>
                        </ul>
                        
                        <h3 style='color: #e17055;'>💭 Help us improve</h3>
                        <p>Your feedback is invaluable. Could you spare 2 minutes to tell us why you're leaving and how we can do better?</p>
                        
                        <p style='text-align: center; margin: 25px 0;'>
                            <a href='https://feedback.scoutvision.com/cancellation' class='feedback-button'>📝 Share Feedback</a>
                        </p>
                        
                        <h3 style='color: #28a745;'>🔄 Changed your mind?</h3>
                        <p>You can easily reactivate your subscription anytime before {endDate:MMMM dd, yyyy}. All your data and settings will be exactly as you left them.</p>
                        
                        <p style='text-align: center; margin: 25px 0;'>
                            <a href='https://app.scoutvision.com/reactivate' class='reactivate-button'>🚀 Reactivate Account</a>
                        </p>
                        
                        <p>Thank you for being part of the ScoutVision community. We hope to welcome you back in the future!</p>
                        
                        <p>Best wishes for your continued success,<br><strong>The ScoutVision Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p>Questions about your cancellation? Contact us at <a href='mailto:support@scoutvision.com'>support@scoutvision.com</a></p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateInvoiceHtml(string customerName, string invoiceId, decimal amount, DateTime dueDate)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #2d3436, #636e72); color: white; padding: 30px 20px; text-align: center; }}
                    .content {{ padding: 30px 20px; }}
                    .invoice-badge {{ background-color: #e8f5e8; border: 2px solid #28a745; padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center; }}
                    .invoice-details {{ background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .footer {{ background-color: #f1f3f4; text-align: center; padding: 20px; font-size: 14px; color: #666; }}
                    .view-button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div style='font-size: 28px; font-weight: bold; margin-bottom: 10px;'>⚽ ScoutVision</div>
                        <h2 style='margin: 0;'>New Invoice Available</h2>
                    </div>
                    <div class='content'>
                        <p>Hi <strong>{customerName}</strong>,</p>
                        
                        <div class='invoice-badge'>
                            <h3 style='margin: 0 0 10px 0;'>📄 Invoice Ready</h3>
                            <p style='margin: 0;'>Your latest ScoutVision invoice is available for review. Payment will be automatically processed on the due date.</p>
                        </div>
                        
                        <div class='invoice-details'>
                            <h3 style='margin-top: 0;'>📋 Invoice Summary</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0;'><strong>Invoice Number:</strong></td><td style='text-align: right;'>{invoiceId}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Amount Due:</strong></td><td style='text-align: right; font-size: 18px; font-weight: bold; color: #28a745;'>{amount:C}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Due Date:</strong></td><td style='text-align: right;'>{dueDate:MMMM dd, yyyy}</td></tr>
                                <tr><td style='padding: 8px 0;'><strong>Auto-Pay:</strong></td><td style='text-align: right;'>✅ Enabled</td></tr>
                            </table>
                        </div>
                        
                        <p><strong>Payment Information:</strong></p>
                        <ul>
                            <li>💳 <strong>Automatic payment</strong> will be processed on {dueDate:MMMM dd, yyyy}</li>
                            <li>📧 <strong>Payment confirmation</strong> will be sent after processing</li>
                            <li>🔒 <strong>Secure processing</strong> via Stripe with industry-standard encryption</li>
                        </ul>
                        
                        <p>You can view the complete invoice details, download a PDF copy, or update your payment method in your customer portal:</p>
                        
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='https://app.scoutvision.com/portal/invoices/{invoiceId}' class='view-button'>📄 View Invoice</a>
                        </p>
                        
                        <p>If you have any questions about this invoice or need to make changes to your subscription, our billing team is here to help.</p>
                        
                        <p>Thank you for your continued business!</p>
                        
                        <p>Best regards,<br><strong>The ScoutVision Billing Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p><strong>ScoutVision</strong> - The Complete Sports Intelligence Platform</p>
                        <p>Billing questions? Contact us at <a href='mailto:billing@scoutvision.com'>billing@scoutvision.com</a></p>
                        <p>© 2025 ScoutVision. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateTrialWelcomeHtml(string customerName, object trialData)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #00b894; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .trial {{ background-color: #d1f2eb; border: 1px solid #a3e4d7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                    .button {{ display: inline-block; padding: 15px 30px; background-color: #00b894; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⚽ ScoutVision</h1>
                        <h2>Your Trial Has Started!</h2>
                    </div>
                    <div class='content'>
                        <p>Hi {customerName},</p>
                        <div class='trial'>
                            <h3>🚀 14-Day Free Trial Activated!</h3>
                            <p>You now have full access to all ScoutVision features for 14 days.</p>
                        </div>
                        <p style='text-align: center;'>
                            <a href='https://app.scoutvision.com/dashboard' class='button'>Start Exploring</a>
                        </p>
                        <p>Best regards,<br>The ScoutVision Team</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateTrialExtendedHtml(string customerName, DateTime newEndDate)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #6c5ce7; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .extension {{ background-color: #e3f2fd; border: 1px solid #bbdefb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⚽ ScoutVision</h1>
                        <h2>Trial Extended!</h2>
                    </div>
                    <div class='content'>
                        <p>Hi {customerName},</p>
                        <div class='extension'>
                            <h3>⏰ More Time to Explore!</h3>
                            <p>Your trial has been extended until {newEndDate:MMMM dd, yyyy}.</p>
                        </div>
                        <p>Best regards,<br>The ScoutVision Team</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateTrialConvertedHtml(string customerName, string planName)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #00b894; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .success {{ background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⚽ ScoutVision</h1>
                        <h2>Welcome to {planName}!</h2>
                    </div>
                    <div class='content'>
                        <p>Hi {customerName},</p>
                        <div class='success'>
                            <h3>🎉 Subscription Activated!</h3>
                            <p>Your {planName} subscription is now active. Enjoy unlimited access to all features!</p>
                        </div>
                        <p>Best regards,<br>The ScoutVision Team</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
