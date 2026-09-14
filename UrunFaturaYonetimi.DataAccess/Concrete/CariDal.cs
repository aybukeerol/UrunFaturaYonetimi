using System.Collections.Generic;
using System.Linq;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.DataAccess.Abstract;

namespace UrunFaturaYonetimi.DataAccess.Concrete
{
    public class CariDal : ICariDal
    {
        private static readonly List<CariKaydi> _cariler =
            new List<CariKaydi>();

        private static bool _ornekVerilerOlusturuldu = false;

        public CariDal()
        {
            OrnekVerileriOlustur();
        }

        private void OrnekVerileriOlustur()
        {
            if (_ornekVerilerOlusturuldu)
                return;

            _ornekVerilerOlusturuldu = true;

            _cariler.Add(
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

            _cariler.Add(
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

            _cariler.Add(
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
        }

        public List<CariKaydi> GetAll()
        {
            return _cariler;
        }

        public CariKaydi GetByCode(
            string cariKodu)
        {
            return _cariler.FirstOrDefault(
                x => x.CariKodu == cariKodu);
        }

        public void Add(
            CariKaydi cari)
        {
            _cariler.Add(cari);
        }

        public void Update(
            CariKaydi cari)
        {
            CariKaydi mevcut =
                GetByCode(cari.CariKodu);

            if (mevcut == null)
                return;

            mevcut.Tip =
                cari.Tip;

            mevcut.CariAdi =
                cari.CariAdi;

            mevcut.YetkiliKisi =
                cari.YetkiliKisi;

            mevcut.FirmaNo =
                cari.FirmaNo;

            mevcut.FirmaAdi =
                cari.FirmaAdi;

            mevcut.KimlikNo =
                cari.KimlikNo;

            mevcut.Telefon =
                cari.Telefon;

            mevcut.Email =
                cari.Email;

            mevcut.Mahalle =
                cari.Mahalle;

            mevcut.Sehir =
                cari.Sehir;

            mevcut.Ulke =
                cari.Ulke;

            mevcut.Favori =
                cari.Favori;
        }

        public void Delete(
            string cariKodu)
        {
            CariKaydi mevcut =
                GetByCode(cariKodu);

            if (mevcut == null)
                return;

            _cariler.Remove(mevcut);
        }
    }
}