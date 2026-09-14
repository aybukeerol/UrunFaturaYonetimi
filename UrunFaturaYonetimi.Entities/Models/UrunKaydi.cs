namespace UrunFaturaYonetimi
{
    public class UrunKaydi
    {
        public string StokKodu { get; set; }
        public string UrunAdi { get; set; }
        public string Kategori { get; set; }
        public string Birim { get; set; }

        public decimal BirimFiyat { get; set; }
        public decimal KdvOrani { get; set; }

        public int Stok { get; set; }

        public bool Favori { get; set; }
    }
}