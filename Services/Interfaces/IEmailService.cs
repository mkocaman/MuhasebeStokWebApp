using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailAsync(string[] to, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailToAdminsAsync(string subject, string body, bool isHtml = true);
    }
} 