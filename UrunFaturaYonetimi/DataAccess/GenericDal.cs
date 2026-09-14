using System;
using System.Collections.Generic;

namespace UrunFaturaYonetimi.DataAccess
{
    public class GenericDal<T> where T : class, new()
    {
        // Bellek içi geçici sanal veritabanı listesi
        private static readonly List<T> _items = new List<T>();

        public List<T> GetAll()
        {
            return _items;
        }

        public void Add(T entity)
        {
            _items.Add(entity);
        }
    }
}
