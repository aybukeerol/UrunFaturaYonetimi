using System.Collections.Generic;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.DataAccess.Abstract;
using UrunFaturaYonetimi.DataAccess.Concrete;

namespace UrunFaturaYonetimi.Business.Managers
{
    public class FaturaManager : IFaturaService
    {
        private readonly IFaturaDal _faturaDal;

        public FaturaManager()
        {
            _faturaDal = new FaturaDal();
        }

        public FaturaManager(
            IFaturaDal faturaDal)
        {
            _faturaDal = faturaDal;
        }

        public List<FaturaKaydi> GetAll()
        {
            return _faturaDal.GetAll();
        }

        public FaturaKaydi GetByNo(
            string faturaNo)
        {
            return _faturaDal.GetByNo(
                faturaNo);
        }

        public void Add(
            FaturaKaydi fatura)
        {
            _faturaDal.Add(fatura);
        }

        public void Update(
            FaturaKaydi fatura)
        {
            _faturaDal.Update(fatura);
        }

        public void Delete(
            string faturaNo)
        {
            _faturaDal.Delete(faturaNo);
        }
    }
}