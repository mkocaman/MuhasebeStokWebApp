namespace MuhasebeStokWebApp.Models
{
    public class Urun
    {
        public Guid Id { get; set; }
        public string Ad { get; set; }
        public string Kategori { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal StokMiktari { get; set; }
        public string Birim { get; set; }
        public string Aciklama { get; set; }
    }
} 