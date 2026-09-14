using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.Business.Abstract
{
    public interface ICariService
    {
        List<CariKaydi> GetAll();

        CariKaydi GetByCode(
            string cariKodu);

        void Add(
            CariKaydi cari);

        void Update(
            CariKaydi cari);

        void Delete(
            string cariKodu);
    }
}