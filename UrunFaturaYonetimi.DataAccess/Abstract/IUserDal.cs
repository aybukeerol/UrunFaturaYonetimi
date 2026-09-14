using System.Collections.Generic;
using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.DataAccess.Abstract
{
    public interface IUserDal
    {
        List<UserAccount> GetAll();

        UserAccount FindForLogin(string value);

        UserAccount FindByEmail(string email);

        bool UsernameExists(string username);

        bool EmailExists(string email);

        bool PhoneExists(string phone);

        void Add(UserAccount user);

        void Update(UserAccount user);
    }
}