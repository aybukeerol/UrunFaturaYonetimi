using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.Business.Abstract
{
    public interface IUrunService
    {
        List<UrunKaydi> GetAll();

        UrunKaydi GetByCode(
            string stokKodu);

        void Add(
            UrunKaydi urun);

        void Update(
            UrunKaydi urun);

        void Delete(
            string stokKodu);
    }
}