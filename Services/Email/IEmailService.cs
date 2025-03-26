using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = false);
        Task SendEmailToAdminsAsync(string subject, string body, bool isHtml = false);
        Task SendCriticalNotificationAsync(string subject, string body, bool isHtml = false);
    }
} 