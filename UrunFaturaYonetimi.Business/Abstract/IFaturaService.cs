using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.Business.Abstract
{
    public interface IFaturaService
    {
        List<FaturaKaydi> GetAll();

        FaturaKaydi GetByNo(
            string faturaNo);

        void Add(
            FaturaKaydi fatura);

        void Update(
            FaturaKaydi fatura);

        void Delete(
            string faturaNo);
    }
}