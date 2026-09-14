using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace UrunFaturaYonetimi
{
    public static class UserStore
    {
        private static readonly string FilePath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "users.xml");

        public static List<UserAccount> Users
        {
            get;
            private set;
        }

        public static void Initialize()
        {
            Load();

            CreateDefaultAdmin();
        }

        private static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Users =
                        new List<UserAccount>();

                    return;
                }

                XmlSerializer serializer =
                    new XmlSerializer(
                        typeof(List<UserAccount>));

                using (FileStream stream =
                    new FileStream(
                        FilePath,
                        FileMode.Open))
                {
                    Users =
                        serializer.Deserialize(stream)
                        as List<UserAccount>;
                }

                if (Users == null)
                {
                    Users =
                        new List<UserAccount>();
                }
            }
            catch
            {
                Users =
                    new List<UserAccount>();
            }
        }

        public static void Save()
        {
            XmlSerializer serializer =
                new XmlSerializer(
                    typeof(List<UserAccount>));

            using (FileStream stream =
                new FileStream(
                    FilePath,
                    FileMode.Create))
            {
                serializer.Serialize(
                    stream,
                    Users);
            }
        }

        private static void CreateDefaultAdmin()
        {
            if (Users.Count > 0)
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

            admin.Id = 1;
            admin.FullName = "Sistem Yöneticisi";
            admin.Username = "admin";
            admin.Email = "admin@local.com";
            admin.Phone = "05000000000";

            admin.PasswordHash = hash;
            admin.PasswordSalt = salt;

            admin.Role = "Yönetici";
            admin.IsActive = true;
            admin.MustChangePassword = false;
            admin.CreatedAt = DateTime.Now;

            Users.Add(admin);

            Save();
        }

        public static UserAccount FindForLogin(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            return Users.FirstOrDefault(
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

        public static UserAccount FindByEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            email = email.Trim();

            return Users.FirstOrDefault(
                x =>
                    string.Equals(
                        x.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase));
        }

        public static bool UsernameExists(
            string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return Users.Any(
                x =>
                    string.Equals(
                        x.Username,
                        username.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        public static bool EmailExists(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Users.Any(
                x =>
                    string.Equals(
                        x.Email,
                        email.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        public static bool PhoneExists(
            string phone)
        {
            string normalized =
                NormalizePhone(phone);

            if (normalized == "")
            {
                return false;
            }

            return Users.Any(
                x =>
                    NormalizePhone(x.Phone) ==
                    normalized);
        }

        public static void Add(
            UserAccount user)
        {
            int nextId =
                Users.Count == 0
                    ? 1
                    : Users.Max(
                        x => x.Id) + 1;

            user.Id = nextId;

            Users.Add(user);

            Save();
        }

        public static void UpdatePassword(
            UserAccount user,
            string newPassword)
        {
            string hash;
            string salt;

            PasswordService.CreatePasswordHash(
                newPassword,
                out hash,
                out salt);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            user.MustChangePassword = false;

            Save();
        }

        public static void SetMustChangePassword(
            UserAccount user,
            bool value)
        {
            user.MustChangePassword = value;

            Save();
        }

        private static string NormalizePhone(
            string phone)
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