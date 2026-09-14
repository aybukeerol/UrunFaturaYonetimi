using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.DataAccess.Abstract
{
    public interface IFaturaDal
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