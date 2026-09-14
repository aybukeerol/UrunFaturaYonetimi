using System;
using System.Collections.Generic;

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

    public static class AppData
    {
        public static List<CariKaydi> Cariler =
            new List<CariKaydi>();

        public static List<UrunKaydi> Urunler =
            new List<UrunKaydi>();

        public static List<FaturaKaydi> Faturalar =
            new List<FaturaKaydi>();

        static AppData()
        {
            OrnekVerileriOlustur();
        }

        private static void OrnekVerileriOlustur()
        {
            // =====================================================
            // CARİLER
            // =====================================================

            Cariler.Add(
                new CariKaydi
                {
                    CariKodu = "120.01.001",
                    Tip = "Müşteri",
                    CariAdi = "Hasan Yılmaz",
                    YetkiliKisi = "Hasan Yılmaz",
                    FirmaNo = "",
                    FirmaAdi = "",
                    KimlikNo = "11111111111",
                    Telefon = "0532 000 00 00",
                    Email = "hasan@example.com",
                    Mahalle = "Yeşilırmak Mah.",
                    Sehir = "Tokat",
                    Ulke = "Türkiye",
                    Favori = true
                });

            Cariler.Add(
                new CariKaydi
                {
                    CariKodu = "320.01.001",
                    Tip = "Tedarikçi",
                    CariAdi = "ABC Mobilya A.Ş.",
                    YetkiliKisi = "Ahmet Demir",
                    FirmaNo = "FRM-0001",
                    FirmaAdi = "ABC Mobilya A.Ş.",
                    KimlikNo = "1234567890",
                    Telefon = "0356 000 00 00",
                    Email = "info@abcmobilya.com",
                    Mahalle = "Karşıyaka Mah.",
                    Sehir = "Tokat",
                    Ulke = "Türkiye",
                    Favori = true
                });

            Cariler.Add(
                new CariKaydi
                {
                    CariKodu = "120.01.002",
                    Tip = "Müşteri",
                    CariAdi = "XYZ Teknoloji Ltd.",
                    YetkiliKisi = "Mehmet Kaya",
                    FirmaNo = "FRM-0002",
                    FirmaAdi = "XYZ Teknoloji Ltd.",
                    KimlikNo = "9876543210",
                    Telefon = "0312 000 00 00",
                    Email = "info@xyz.com",
                    Mahalle = "Çukurambar Mah.",
                    Sehir = "Ankara",
                    Ulke = "Türkiye",
                    Favori = false
                });

            // =====================================================
            // ÜRÜNLER
            // =====================================================

            Urunler.Add(
                new UrunKaydi
                {
                    StokKodu = "STK-0001",
                    UrunAdi = "Masa",
                    Kategori = "Mobilya",
                    Birim = "Adet",
                    BirimFiyat = 5000m,
                    KdvOrani = 20m,
                    Stok = 15,
                    Favori = true
                });

            Urunler.Add(
                new UrunKaydi
                {
                    StokKodu = "STK-0002",
                    UrunAdi = "Sandalye",
                    Kategori = "Mobilya",
                    Birim = "Adet",
                    BirimFiyat = 1000m,
                    KdvOrani = 20m,
                    Stok = 72,
                    Favori = true
                });

            Urunler.Add(
                new UrunKaydi
                {
                    StokKodu = "STK-0003",
                    UrunAdi = "Çalışma Masası",
                    Kategori = "Ofis",
                    Birim = "Adet",
                    BirimFiyat = 7500m,
                    KdvOrani = 20m,
                    Stok = 9,
                    Favori = false
                });

            Urunler.Add(
                new UrunKaydi
                {
                    StokKodu = "STK-0004",
                    UrunAdi = "Monitör",
                    Kategori = "Elektronik",
                    Birim = "Adet",
                    BirimFiyat = 12500m,
                    KdvOrani = 20m,
                    Stok = 21,
                    Favori = false
                });

            // =====================================================
            // FATURALAR
            // =====================================================

            Faturalar.Add(
                new FaturaKaydi
                {
                    FaturaNo = "SF202600124",
                    Tarih = DateTime.Today,
                    CariAdi = "ABC Mobilya A.Ş.",
                    CariTipi = "Kurumsal",
                    BelgeTipi = "E-Fatura",
                    GenelToplam = 18400m,
                    Durum = "Kaydedildi"
                });

            Faturalar.Add(
                new FaturaKaydi
                {
                    FaturaNo = "SF202600123",
                    Tarih = DateTime.Today,
                    CariAdi = "Hasan Yılmaz",
                    CariTipi = "Bireysel",
                    BelgeTipi = "E-Arşiv",
                    GenelToplam = 7200m,
                    Durum = "Taslak"
                });
        }
    }
}