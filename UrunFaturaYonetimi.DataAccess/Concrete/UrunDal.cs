using System.Collections.Generic;
using System.Linq;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.DataAccess.Abstract;

namespace UrunFaturaYonetimi.DataAccess.Concrete
{
    public class UrunDal : IUrunDal
    {
        private static readonly List<UrunKaydi> _urunler =
            new List<UrunKaydi>();

        private static bool _ornekVerilerOlusturuldu = false;

        public UrunDal()
        {
            OrnekVerileriOlustur();
        }

        private void OrnekVerileriOlustur()
        {
            if (_ornekVerilerOlusturuldu)
                return;

            _ornekVerilerOlusturuldu = true;

            _urunler.Add(
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

            _urunler.Add(
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

            _urunler.Add(
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

            _urunler.Add(
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
        }

        public List<UrunKaydi> GetAll()
        {
            return _urunler;
        }

        public UrunKaydi GetByCode(
            string stokKodu)
        {
            return _urunler.FirstOrDefault(
                x => x.StokKodu == stokKodu);
        }

        public void Add(
            UrunKaydi urun)
        {
            _urunler.Add(urun);
        }

        public void Update(
            UrunKaydi urun)
        {
            UrunKaydi mevcut =
                GetByCode(
                    urun.StokKodu);

            if (mevcut == null)
                return;

            mevcut.UrunAdi =
                urun.UrunAdi;

            mevcut.Kategori =
                urun.Kategori;

            mevcut.Birim =
                urun.Birim;

            mevcut.BirimFiyat =
                urun.BirimFiyat;

            mevcut.KdvOrani =
                urun.KdvOrani;

            mevcut.Stok =
                urun.Stok;

            mevcut.Favori =
                urun.Favori;
        }

        public void Delete(
            string stokKodu)
        {
            UrunKaydi mevcut =
                GetByCode(
                    stokKodu);

            if (mevcut == null)
                return;

            _urunler.Remove(
                mevcut);
        }
    }
}