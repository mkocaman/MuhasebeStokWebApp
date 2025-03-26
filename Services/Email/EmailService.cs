using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace MuhasebeStokWebApp.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"E-posta gönderildi - Alıcı: {to}, Konu: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta gönderilirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = false)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder();
                if (isHtml)
                    builder.HtmlBody = body;
                else
                    builder.TextBody = body;

                if (File.Exists(attachmentPath))
                {
                    builder.Attachments.Add(attachmentPath);
                }
                else
                {
                    _logger.LogWarning($"Ek dosyası bulunamadı: {attachmentPath}");
                }

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Ekli e-posta gönderildi - Alıcı: {to}, Konu: {subject}, Ek: {attachmentPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ekli e-posta gönderilirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailToAdminsAsync(string subject, string body, bool isHtml = false)
        {
            try
            {
                if (_emailSettings.AdminEmails == null || _emailSettings.AdminEmails.Count == 0)
                {
                    _logger.LogWarning("Admin e-postaları yapılandırılmamış, e-posta gönderilemiyor");
                    return;
                }

                foreach (var adminEmail in _emailSettings.AdminEmails)
                {
                    await SendEmailAsync(adminEmail, subject, body, isHtml);
                }

                _logger.LogInformation($"Admin e-postası gönderildi - Konu: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Admin e-postası gönderilirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task SendCriticalNotificationAsync(string subject, string body, bool isHtml = false)
        {
            await SendEmailToAdminsAsync($"[KRİTİK] {subject}", body, isHtml);
        }
    }

    public class EmailSettings
    {
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderName { get; set; }
        public bool UseSsl { get; set; }
        public List<string>? AdminEmails { get; set; }
    }
} 