using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using eCHU.Models;

namespace eCHU.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, List<string>? attachmentPaths = null);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                _logger.LogInformation($"Starting email sending process to: {to}");

                if (string.IsNullOrEmpty(_smtpSettings.Host) || string.IsNullOrEmpty(_smtpSettings.Username) ||
                    string.IsNullOrEmpty(_smtpSettings.Password) || string.IsNullOrEmpty(_smtpSettings.FromEmail))
                {
                    _logger.LogError("One or more SMTP settings are missing in configuration");
                    return false;
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, "活動報名系統"),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = body
                };

                mailMessage.To.Add(to);

                // Add attachments if provided
                if (attachmentPaths != null)
                {
                    foreach (var path in attachmentPaths)
                    {
                        if (File.Exists(path))
                        {
                            mailMessage.Attachments.Add(new Attachment(path));
                        }
                    }
                }

                using var smtpClient = new SmtpClient
                {
                    Host = _smtpSettings.Host,
                    Port = _smtpSettings.Port,
                    EnableSsl = _smtpSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    Timeout = 60000
                };

                _logger.LogInformation("Attempting to send email...");
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to: {to}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"SMTP Error: {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error Sending Email: {ex.Message}");
                return false;
            }
        }
    }
}