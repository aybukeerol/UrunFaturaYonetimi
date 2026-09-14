using System;

namespace UrunFaturaYonetimi
{
    public class FaturaKaydi
    {
        public string FaturaNo { get; set; }
        public DateTime Tarih { get; set; }
        public string CariAdi { get; set; }
        public string CariTipi { get; set; }
        public string BelgeTipi { get; set; }

        public decimal GenelToplam { get; set; }

        public string Durum { get; set; }
    }
}