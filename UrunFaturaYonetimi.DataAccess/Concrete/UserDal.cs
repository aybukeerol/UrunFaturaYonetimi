using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UrunFaturaYonetimi;
using UrunFaturaYonetimi.DataAccess.Abstract;

namespace UrunFaturaYonetimi.DataAccess.Concrete
{
    public class UserDal : IUserDal
    {
        private static readonly string FilePath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "users.xml");

        private List<UserAccount> _users;

        public UserDal()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    _users = new List<UserAccount>();
                    return;
                }

                XmlSerializer serializer =
                    new XmlSerializer(typeof(List<UserAccount>));

                using (FileStream stream =
                    new FileStream(FilePath, FileMode.Open))
                {
                    _users =
                        serializer.Deserialize(stream)
                        as List<UserAccount>;
                }

                if (_users == null)
                {
                    _users = new List<UserAccount>();
                }
            }
            catch
            {
                _users = new List<UserAccount>();
            }
        }

        private void Save()
        {
            XmlSerializer serializer =
                new XmlSerializer(typeof(List<UserAccount>));

            using (FileStream stream =
                new FileStream(FilePath, FileMode.Create))
            {
                serializer.Serialize(stream, _users);
            }
        }

        public List<UserAccount> GetAll()
        {
            return _users;
        }

        public UserAccount FindForLogin(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            return _users.FirstOrDefault(
                x =>
                    string.Equals(
                        x.Username,
                        value,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        x.Email,
                        value,
                        StringComparison.OrdinalIgnoreCase));
        }

        public UserAccount FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            email = email.Trim();

            return _users.FirstOrDefault(
                x =>
                    string.Equals(
                        x.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase));
        }

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return _users.Any(
                x =>
                    string.Equals(
                        x.Username,
                        username.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        public bool EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return _users.Any(
                x =>
                    string.Equals(
                        x.Email,
                        email.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        public bool PhoneExists(string phone)
        {
            string normalized = NormalizePhone(phone);

            if (normalized == "")
            {
                return false;
            }

            return _users.Any(
                x =>
                    NormalizePhone(x.Phone) == normalized);
        }

        public void Add(UserAccount user)
        {
            int nextId =
                _users.Count == 0
                    ? 1
                    : _users.Max(x => x.Id) + 1;

            user.Id = nextId;

            _users.Add(user);

            Save();
        }

        public void Update(UserAccount user)
        {
            Save();
        }

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return "";
            }

            return phone
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();
        }
    }
}