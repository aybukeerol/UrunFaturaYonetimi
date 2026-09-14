using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public class VerificationSession
    {
        public string Username { get; set; }

        public string Code { get; set; }

        public DateTime ExpiresAt { get; set; }

        public int FailedAttempts { get; set; }

        public bool Used { get; set; }
    }

    public static class VerificationService
    {
        // =========================================================
        // GMAIL AYARLARI
        // =========================================================

        private const string SenderEmail =
            "erolaybuke1 @gmail.com";

        private const string AppPassword =
            "fnwdmkmojjnbizvv";

        private static readonly Dictionary<
            string,
            VerificationSession> Sessions =
            new Dictionary<
                string,
                VerificationSession>();

        // =========================================================
        // DOĞRULAMA KODU OLUŞTUR
        // =========================================================

        public static string CreateCode(
            UserAccount user)
        {
            string code =
                GenerateSixDigitCode();

            VerificationSession session =
                new VerificationSession();

            session.Username =
                user.Username;

            session.Code =
                code;

            session.ExpiresAt =
                DateTime.Now.AddMinutes(5);

            session.FailedAttempts =
                0;

            session.Used =
                false;

            Sessions[
                user.Username.ToLower()] =
                session;

            return code;
        }

        // =========================================================
        // DOĞRULAMA KODUNU KONTROL ET
        // =========================================================

        public static bool Verify(
            UserAccount user,
            string code,
            out string error)
        {
            error = "";

            string key =
                user.Username.ToLower();

            if (!Sessions.ContainsKey(key))
            {
                error =
                    "Aktif bir doğrulama kodu bulunamadı.";

                return false;
            }

            VerificationSession session =
                Sessions[key];

            if (session.Used)
            {
                error =
                    "Bu doğrulama kodu daha önce kullanılmış.";

                return false;
            }

            if (DateTime.Now >
                session.ExpiresAt)
            {
                Sessions.Remove(key);

                error =
                    "Doğrulama kodunun süresi dolmuş. Yeni bir kod isteyin.";

                return false;
            }

            if (session.FailedAttempts >= 5)
            {
                Sessions.Remove(key);

                error =
                    "Çok fazla hatalı deneme yapıldı. Yeni bir kod isteyin.";

                return false;
            }

            if (session.Code != code.Trim())
            {
                session.FailedAttempts++;

                int kalan =
                    5 -
                    session.FailedAttempts;

                if (kalan <= 0)
                {
                    Sessions.Remove(key);

                    error =
                        "Çok fazla hatalı deneme yapıldı. Yeni bir kod isteyin.";

                    return false;
                }

                error =
                    "Doğrulama kodu hatalı.\n\n" +
                    "Kalan deneme hakkı: " +
                    kalan;

                return false;
            }

            session.Used =
                true;

            return true;
        }

        // =========================================================
        // GERÇEK E-POSTA GÖNDERME
        // =========================================================

        public static void SendEmailCode(
            UserAccount user,
            string code)
        {
            try
            {
                if (user == null)
                {
                    MessageBox.Show(
                        "Kullanıcı bilgisi bulunamadı.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    user.Email))
                {
                    MessageBox.Show(
                        "Kullanıcının e-posta adresi bulunamadı.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                MailMessage mail =
                    new MailMessage();

                mail.From =
                    new MailAddress(
                        SenderEmail,
                        "E-Fatura Yönetim Sistemi");

                mail.To.Add(
                    user.Email);

                mail.Subject =
                    "Şifre Sıfırlama Doğrulama Kodunuz";

                mail.IsBodyHtml =
                    true;

                mail.Body =
                    MailGovdesiOlustur(
                        user,
                        code);

                SmtpClient smtp =
                    new SmtpClient(
                        "smtp.gmail.com",
                        587);

                smtp.EnableSsl =
                    true;

                smtp.UseDefaultCredentials =
                    false;

                smtp.Credentials =
                    new NetworkCredential(
                        SenderEmail,
                        AppPassword);

                smtp.Send(
                    mail);

                mail.Dispose();
                smtp.Dispose();

                MessageBox.Show(
                    "Doğrulama kodu e-posta adresinize gönderildi.\n\n" +
                    MaskEmail(
                        user.Email),
                    "E-posta Gönderildi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (SmtpException ex)
            {
                MessageBox.Show(
                    "E-posta gönderilemedi.\n\n" +
                    "SMTP Hatası:\n" +
                    ex.Message,
                    "E-posta Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "E-posta gönderilirken bir hata oluştu.\n\n" +
                    ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // E-POSTA TASARIMI
        // =========================================================

        private static string MailGovdesiOlustur(
            UserAccount user,
            string code)
        {
            string kullaniciAdi;

            if (string.IsNullOrWhiteSpace(
                user.FullName))
            {
                kullaniciAdi =
                    "Kullanıcı";
            }
            else
            {
                kullaniciAdi =
                    user.FullName;
            }

            return
                "<html>" +

                "<body style='" +
                "margin:0;" +
                "padding:30px;" +
                "background-color:#f4f7fb;" +
                "font-family:Segoe UI,Arial,sans-serif;" +
                "'>" +

                "<div style='" +
                "max-width:520px;" +
                "margin:0 auto;" +
                "background:#ffffff;" +
                "border:1px solid #e2e8f0;" +
                "border-radius:10px;" +
                "overflow:hidden;" +
                "'>" +

                "<div style='" +
                "background:#122642;" +
                "padding:25px 30px;" +
                "color:#ffffff;" +
                "'>" +

                "<h2 style='" +
                "margin:0;" +
                "font-size:22px;" +
                "'>" +

                "E-Fatura Yönetim Sistemi" +

                "</h2>" +

                "</div>" +

                "<div style='" +
                "padding:30px;" +
                "color:#374151;" +
                "'>" +

                "<p>" +
                "Merhaba <strong>" +
                kullaniciAdi +
                "</strong>," +
                "</p>" +

                "<p>" +
                "Şifre sıfırlama işleminiz için doğrulama kodunuz aşağıdadır." +
                "</p>" +

                "<div style='" +
                "margin:25px 0;" +
                "padding:20px;" +
                "background:#f4f7fb;" +
                "border-radius:8px;" +
                "text-align:center;" +
                "font-size:32px;" +
                "font-weight:bold;" +
                "letter-spacing:8px;" +
                "color:#2366d7;" +
                "'>" +

                code +

                "</div>" +

                "<p>" +
                "Bu kod <strong>5 dakika</strong> boyunca geçerlidir." +
                "</p>" +

                "<p>" +
                "Eğer bu şifre sıfırlama işlemini siz başlatmadıysanız bu e-postayı dikkate almayabilirsiniz." +
                "</p>" +

                "<hr style='" +
                "border:0;" +
                "border-top:1px solid #e5e7eb;" +
                "margin:25px 0;" +
                "' />" +

                "<p style='" +
                "font-size:12px;" +
                "color:#9ca3af;" +
                "'>" +

                "Bu e-posta otomatik olarak gönderilmiştir." +

                "</p>" +

                "</div>" +

                "</div>" +

                "</body>" +

                "</html>";
        }

        // =========================================================
        // 6 HANELİ DOĞRULAMA KODU
        // =========================================================

        private static string GenerateSixDigitCode()
        {
            byte[] bytes =
                new byte[4];

            using (var rng =
                new RNGCryptoServiceProvider())
            {
                rng.GetBytes(
                    bytes);
            }

            uint value =
                BitConverter.ToUInt32(
                    bytes,
                    0);

            int number =
                (int)(value % 900000) +
                100000;

            return number.ToString();
        }

        // =========================================================
        // E-POSTA ADRESİNİ GİZLE
        // =========================================================

        public static string MaskEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(
                email))
            {
                return "-";
            }

            int index =
                email.IndexOf('@');

            if (index <= 0)
            {
                return email;
            }

            string name =
                email.Substring(
                    0,
                    index);

            string domain =
                email.Substring(
                    index);

            if (name.Length == 1)
            {
                return name +
                       "***" +
                       domain;
            }

            return name.Substring(
                       0,
                       1) +
                   "***" +
                   domain;
        }
    }
}