using UrunFaturaYonetimi;

namespace UrunFaturaYonetimi.Business.Abstract
{
    public interface IUserService
    {
        void Initialize();

        UserAccount Login(
            string usernameOrEmail,
            string password);

        UserAccount FindByEmail(string email);

        bool UsernameExists(string username);

        bool EmailExists(string email);

        bool PhoneExists(string phone);

        void Register(
            UserAccount user,
            string password);

        void UpdatePassword(
            UserAccount user,
            string newPassword);

        void SetMustChangePassword(
            UserAccount user,
            bool value);
    }
}