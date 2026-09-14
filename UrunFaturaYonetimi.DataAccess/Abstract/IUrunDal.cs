using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.DataAccess.Abstract
{
    public interface IUrunDal
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