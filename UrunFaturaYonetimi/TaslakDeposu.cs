using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace UrunFaturaYonetimi
{
    public class TaslakKalemi
    {
        public string StokKodu { get; set; }
        public string Urun { get; set; }
        public string Miktar { get; set; }
        public string BirimFiyat { get; set; }
        public string Kdv { get; set; }
    }

    public class TaslakBelgesi
    {
        public string FaturaNo { get; set; }
        public DateTime Tarih { get; set; }
        public string AliciTipi { get; set; }
        public string Alici { get; set; }
        public string KimlikNo { get; set; }
        public string Senaryo { get; set; }
        public string FaturaTipi { get; set; }
        public string ParaBirimi { get; set; }
        public decimal GenelToplam { get; set; }
        public List<TaslakKalemi> Kalemler { get; set; } = new List<TaslakKalemi>();
    }

    // Bu depo, mevcut SQL tablolarını değiştirmeden taslak formunun TÜM alanlarını saklar.
    public static class TaslakDeposu
    {
        private static readonly string Dosya = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NEXORA", "taslaklar.xml");

        public static List<TaslakBelgesi> Listele()
        {
            if (!File.Exists(Dosya)) return new List<TaslakBelgesi>();
            using (FileStream stream = File.OpenRead(Dosya))
                return (List<TaslakBelgesi>)new XmlSerializer(typeof(List<TaslakBelgesi>)).Deserialize(stream);
        }

        public static TaslakBelgesi Bul(string faturaNo)
        {
            return Listele().FirstOrDefault(x =>
                string.Equals(x.FaturaNo, faturaNo, StringComparison.OrdinalIgnoreCase));
        }

        public static void Kaydet(TaslakBelgesi belge)
        {
            if (belge == null || string.IsNullOrWhiteSpace(belge.FaturaNo))
                throw new InvalidOperationException("Taslak fatura numarası boş olamaz.");
            List<TaslakBelgesi> liste = Listele();
            liste.RemoveAll(x => string.Equals(x.FaturaNo, belge.FaturaNo, StringComparison.OrdinalIgnoreCase));
            liste.Add(belge);
            Directory.CreateDirectory(Path.GetDirectoryName(Dosya));
            string gecici = Dosya + ".tmp";
            using (FileStream stream = File.Create(gecici))
                new XmlSerializer(typeof(List<TaslakBelgesi>)).Serialize(stream, liste);
            if (File.Exists(Dosya)) File.Delete(Dosya);
            File.Move(gecici, Dosya);
        }

        public static void Sil(string faturaNo)
        {
            List<TaslakBelgesi> liste = Listele();
            liste.RemoveAll(x => string.Equals(x.FaturaNo, faturaNo, StringComparison.OrdinalIgnoreCase));
            Directory.CreateDirectory(Path.GetDirectoryName(Dosya));
            using (FileStream stream = File.Create(Dosya))
                new XmlSerializer(typeof(List<TaslakBelgesi>)).Serialize(stream, liste);
        }
    }
}
