using System.Collections.Generic;
using System.Linq;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.DataAccess.Abstract;

namespace UrunFaturaYonetimi.DataAccess.Concrete
{
    public class FaturaDal : IFaturaDal
    {
        private static readonly List<FaturaKaydi> _faturalar =
            new List<FaturaKaydi>();

        private static bool _ornekVerilerOlusturuldu = false;

        public FaturaDal()
        {
            OrnekVerileriOlustur();
        }

        private void OrnekVerileriOlustur()
        {
            if (_ornekVerilerOlusturuldu)
                return;

            _ornekVerilerOlusturuldu = true;

            _faturalar.Add(
                new FaturaKaydi
                {
                    FaturaNo = "SF202600124",
                    Tarih = System.DateTime.Today,
                    CariAdi = "ABC Mobilya A.Ş.",
                    CariTipi = "Kurumsal",
                    BelgeTipi = "E-Fatura",
                    GenelToplam = 18400m,
                    Durum = "Kaydedildi"
                });

            _faturalar.Add(
                new FaturaKaydi
                {
                    FaturaNo = "SF202600123",
                    Tarih = System.DateTime.Today,
                    CariAdi = "Hasan Yılmaz",
                    CariTipi = "Bireysel",
                    BelgeTipi = "E-Arşiv",
                    GenelToplam = 7200m,
                    Durum = "Taslak"
                });
        }

        public List<FaturaKaydi> GetAll()
        {
            return _faturalar;
        }

        public FaturaKaydi GetByNo(
            string faturaNo)
        {
            return _faturalar.FirstOrDefault(
                x => x.FaturaNo == faturaNo);
        }

        public void Add(
            FaturaKaydi fatura)
        {
            _faturalar.Add(
                fatura);
        }

        public void Update(
            FaturaKaydi fatura)
        {
            FaturaKaydi mevcut =
                GetByNo(
                    fatura.FaturaNo);

            if (mevcut == null)
                return;

            mevcut.Tarih =
                fatura.Tarih;

            mevcut.CariAdi =
                fatura.CariAdi;

            mevcut.CariTipi =
                fatura.CariTipi;

            mevcut.BelgeTipi =
                fatura.BelgeTipi;

            mevcut.GenelToplam =
                fatura.GenelToplam;

            mevcut.Durum =
                fatura.Durum;
        }

        public void Delete(
            string faturaNo)
        {
            FaturaKaydi mevcut =
                GetByNo(
                    faturaNo);

            if (mevcut == null)
                return;

            _faturalar.Remove(
                mevcut);
        }
    }
}