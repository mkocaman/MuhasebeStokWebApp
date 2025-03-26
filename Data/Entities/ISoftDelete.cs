namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Soft delete işlevselliği sunan arayüz
    /// </summary>
    public interface ISoftDelete
    {
        bool Silindi { get; set; }
    }
} 