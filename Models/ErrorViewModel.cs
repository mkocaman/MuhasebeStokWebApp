namespace MuhasebeStokWebApp.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    public string Title { get; set; } = "Hata";
    
    public string Message { get; set; } = "Bir hata oluştu.";
}
