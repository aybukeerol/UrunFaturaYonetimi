using System;
using System.Collections.Generic;
using System.Linq;
using UrunFaturaYonetimi.DataAccess;
using UrunFaturaYonetimi.Models;

namespace UrunFaturaYonetimi.Business
{
    public class ProductService
    {
        private readonly GenericDal<Product> _productDal = new GenericDal<Product>();

        public List<Product> GetSortedProducts(int sortIndex)
        {
            var liste = _productDal.GetAll() ?? new List<Product>();

            switch (sortIndex)
            {
                case 1: return liste.OrderBy(x => x.Name).ToList();
                case 2: return liste.OrderByDescending(x => x.Name).ToList();
                case 3: return liste.OrderBy(x => x.UnitPrice).ToList();
                case 4: return liste.OrderByDescending(x => x.UnitPrice).ToList();
                case 5: return liste.OrderBy(x => x.StockQuantity).ToList();
                case 6: return liste.OrderByDescending(x => x.StockQuantity).ToList();
                default: return liste.OrderBy(x => x.Id).ToList();
            }
        }

        public List<Product> GetAllProducts()
        {
            return _productDal.GetAll() ?? new List<Product>();
        }
    }
}
