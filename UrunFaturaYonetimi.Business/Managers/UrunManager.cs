using System.Collections.Generic;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.DataAccess.Abstract;
using UrunFaturaYonetimi.DataAccess.Concrete;

namespace UrunFaturaYonetimi.Business.Managers
{
    public class UrunManager : IUrunService
    {
        private readonly IUrunDal _urunDal;

        public UrunManager()
        {
            _urunDal =
                new UrunDal();
        }

        public UrunManager(
            IUrunDal urunDal)
        {
            _urunDal =
                urunDal;
        }

        public List<UrunKaydi> GetAll()
        {
            return _urunDal.GetAll();
        }

        public UrunKaydi GetByCode(
            string stokKodu)
        {
            return _urunDal.GetByCode(
                stokKodu);
        }

        public void Add(
            UrunKaydi urun)
        {
            _urunDal.Add(
                urun);
        }

        public void Update(
            UrunKaydi urun)
        {
            _urunDal.Update(
                urun);
        }

        public void Delete(
            string stokKodu)
        {
            _urunDal.Delete(
                stokKodu);
        }
    }
}