using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailService(ILogger<EmailService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using (var client = CreateSmtpClient())
                {
                    var message = new MailMessage
                    {
                        From = new MailAddress(GetSenderEmail(), GetSenderName()),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    message.To.Add(to);

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SendEmailAsync error to {to}");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string[] to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using (var client = CreateSmtpClient())
                {
                    var message = new MailMessage
                    {
                        From = new MailAddress(GetSenderEmail(), GetSenderName()),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    foreach (var recipient in to)
                    {
                        message.To.Add(recipient);
                    }

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SendEmailAsync error to multiple recipients");
                return false;
            }
        }

        public async Task<bool> SendEmailToAdminsAsync(string subject, string body, bool isHtml = true)
        {
            try
            {
                var adminEmails = GetAdminEmails();
                if (adminEmails.Count == 0)
                {
                    _logger.LogWarning("No admin emails configured");
                    return false;
                }

                return await SendEmailAsync(adminEmails.ToArray(), subject, body, isHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendEmailToAdminsAsync error");
                return false;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtpServer = GetSmtpServer();
            var smtpPort = GetSmtpPort();
            var smtpUsername = GetSmtpUsername();
            var smtpPassword = GetSmtpPassword();
            var useSsl = GetSmtpUseSsl();

            var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = useSsl
            };

            return client;
        }

        #region Helper Methods

        private string GetSmtpServer()
        {
            return _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpServer")?.Deger ?? "smtp.gmail.com";
        }

        private int GetSmtpPort()
        {
            var portStr = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpPort")?.Deger;
            return int.TryParse(portStr, out int port) ? port : 587;
        }

        private string GetSmtpUsername()
        {
            return _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpUsername")?.Deger ?? "";
        }

        private string GetSmtpPassword()
        {
            return _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpPassword")?.Deger ?? "";
        }

        private bool GetSmtpUseSsl()
        {
            var sslStr = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpUseSsl")?.Deger;
            return !string.IsNullOrEmpty(sslStr) && sslStr.ToLower() == "true";
        }

        private string GetSenderEmail()
        {
            return _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SenderEmail")?.Deger ?? "noreply@muhasebe-stok.com";
        }

        private string GetSenderName()
        {
            return _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SenderName")?.Deger ?? "Muhasebe ve Stok Takip Sistemi";
        }

        private List<string> GetAdminEmails()
        {
            var adminEmailsStr = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "AdminEmails")?.Deger;
            if (string.IsNullOrEmpty(adminEmailsStr))
            {
                return new List<string>();
            }

            return adminEmailsStr.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        #endregion
    }
} 