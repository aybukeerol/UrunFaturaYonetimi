using System;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Services;
using UrunFaturaYonetimi.DataAccess.Abstract;
using UrunFaturaYonetimi.DataAccess.Concrete;

namespace UrunFaturaYonetimi.Business.Managers
{
    public class UserManager : IUserService
    {
        private readonly IUserDal _userDal;

        public UserManager()
        {
            _userDal = new UserDal();
        }

        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public void Initialize()
        {
            if (_userDal.GetAll().Count > 0)
            {
                return;
            }

            string hash;
            string salt;

            PasswordService.CreatePasswordHash(
                "Admin123",
                out hash,
                out salt);

            UserAccount admin =
                new UserAccount();

            admin.FullName =
                "Sistem Yöneticisi";

            admin.Username =
                "admin";

            admin.Email =
                "admin@local.com";

            admin.Phone =
                "05000000000";

            admin.PasswordHash =
                hash;

            admin.PasswordSalt =
                salt;

            admin.Role =
                "Yönetici";

            admin.IsActive =
                true;

            admin.MustChangePassword =
                false;

            admin.CreatedAt =
                DateTime.Now;

            _userDal.Add(admin);
        }

        public UserAccount Login(
            string usernameOrEmail,
            string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            UserAccount user =
                _userDal.FindForLogin(
                    usernameOrEmail);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            bool dogru =
                PasswordService.VerifyPassword(
                    password,
                    user.PasswordHash,
                    user.PasswordSalt);

            if (!dogru)
            {
                return null;
            }

            return user;
        }

        public UserAccount FindByEmail(
            string email)
        {
            return _userDal.FindByEmail(email);
        }

        public bool UsernameExists(
            string username)
        {
            return _userDal.UsernameExists(username);
        }

        public bool EmailExists(
            string email)
        {
            return _userDal.EmailExists(email);
        }

        public bool PhoneExists(
            string phone)
        {
            return _userDal.PhoneExists(phone);
        }

        public void Register(
            UserAccount user,
            string password)
        {
            string hash;
            string salt;

            PasswordService.CreatePasswordHash(
                password,
                out hash,
                out salt);

            user.PasswordHash =
                hash;

            user.PasswordSalt =
                salt;

            user.Role =
                "Standart Kullanıcı";

            user.IsActive =
                true;

            user.MustChangePassword =
                false;

            user.CreatedAt =
                DateTime.Now;

            _userDal.Add(user);
        }

        public void UpdatePassword(
            UserAccount user,
            string newPassword)
        {
            string hash;
            string salt;

            PasswordService.CreatePasswordHash(
                newPassword,
                out hash,
                out salt);

            user.PasswordHash =
                hash;

            user.PasswordSalt =
                salt;

            user.MustChangePassword =
                false;

            _userDal.Update(user);
        }

        public void SetMustChangePassword(
            UserAccount user,
            bool value)
        {
            user.MustChangePassword =
                value;

            _userDal.Update(user);
        }
    }
}