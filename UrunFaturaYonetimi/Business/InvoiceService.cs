using System;
using System.Collections.Generic;
using UrunFaturaYonetimi.DataAccess;
using UrunFaturaYonetimi.Models;

namespace UrunFaturaYonetimi.Business
{
    public class InvoiceService
    {
        private readonly GenericDal<Invoice> _invoiceDal = new GenericDal<Invoice>();
        private readonly GenericDal<CurrentAccount> _currentAccountDal = new GenericDal<CurrentAccount>();

        public string GenerateInvoiceNumber()
        {
            return "FTR-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
        }

        public List<CurrentAccount> GetAllCurrentAccounts()
        {
            return _currentAccountDal.GetAll() ?? new List<CurrentAccount>();
        }

        public void CariEkle(CurrentAccount yeniCari)
        {
            if (string.IsNullOrWhiteSpace(yeniCari.AccountName))
                throw new Exception("Cari hesap adı boş bırakılamaz!");

            if (string.IsNullOrWhiteSpace(yeniCari.IdentifierNumber))
                throw new Exception("Kimlik/Vergi numarası boş bırakılamaz!");

            _currentAccountDal.Add(yeniCari);
        }
    }
}
