using System.Collections.Generic;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.DataAccess.Abstract;
using UrunFaturaYonetimi.DataAccess.Concrete;

namespace UrunFaturaYonetimi.Business.Managers
{
    public class CariManager : ICariService
    {
        private readonly ICariDal _cariDal;

        public CariManager()
        {
            _cariDal =
                new CariDal();
        }

        public CariManager(
            ICariDal cariDal)
        {
            _cariDal =
                cariDal;
        }

        public List<CariKaydi> GetAll()
        {
            return _cariDal.GetAll();
        }

        public CariKaydi GetByCode(
            string cariKodu)
        {
            return _cariDal.GetByCode(
                cariKodu);
        }

        public void Add(
            CariKaydi cari)
        {
            _cariDal.Add(
                cari);
        }

        public void Update(
            CariKaydi cari)
        {
            _cariDal.Update(
                cari);
        }

        public void Delete(
            string cariKodu)
        {
            _cariDal.Delete(
                cariKodu);
        }
    }
}