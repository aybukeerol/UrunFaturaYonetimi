using System;

namespace UrunFaturaYonetimi
{
    [Serializable]
    public class UserAccount
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}