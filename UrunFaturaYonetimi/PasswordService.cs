using System;
using System.Security.Cryptography;

namespace UrunFaturaYonetimi
{
    public static class PasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static void CreatePasswordHash(
            string password,
            out string hash,
            out string salt)
        {
            byte[] saltBytes =
                new byte[SaltSize];

            using (var rng =
                new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            byte[] hashBytes;

            using (var pbkdf2 =
                new Rfc2898DeriveBytes(
                    password,
                    saltBytes,
                    Iterations))
            {
                hashBytes =
                    pbkdf2.GetBytes(HashSize);
            }

            salt =
                Convert.ToBase64String(
                    saltBytes);

            hash =
                Convert.ToBase64String(
                    hashBytes);
        }

        public static bool VerifyPassword(
            string password,
            string storedHash,
            string storedSalt)
        {
            if (string.IsNullOrWhiteSpace(storedHash) ||
                string.IsNullOrWhiteSpace(storedSalt))
            {
                return false;
            }

            try
            {
                byte[] saltBytes =
                    Convert.FromBase64String(
                        storedSalt);

                byte[] expectedHash =
                    Convert.FromBase64String(
                        storedHash);

                byte[] actualHash;

                using (var pbkdf2 =
                    new Rfc2898DeriveBytes(
                        password,
                        saltBytes,
                        Iterations))
                {
                    actualHash =
                        pbkdf2.GetBytes(
                            HashSize);
                }

                if (actualHash.Length !=
                    expectedHash.Length)
                {
                    return false;
                }

                int difference = 0;

                for (int i = 0;
                     i < actualHash.Length;
                     i++)
                {
                    difference |=
                        actualHash[i] ^
                        expectedHash[i];
                }

                return difference == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPasswordValid(
            string password)
        {
            if (string.IsNullOrWhiteSpace(
                password))
            {
                return false;
            }

            if (password.Length < 8)
            {
                return false;
            }

            bool harfVar = false;
            bool rakamVar = false;

            foreach (char c in password)
            {
                if (char.IsLetter(c))
                {
                    harfVar = true;
                }

                if (char.IsDigit(c))
                {
                    rakamVar = true;
                }
            }

            return harfVar && rakamVar;
        }
    }
}