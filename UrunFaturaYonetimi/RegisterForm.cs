using System;
using System.Drawing;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Services;

namespace UrunFaturaYonetimi
{
    public class RegisterForm : Form
    {
        private readonly IUserService _userService;

        private TextBox txtAdSoyad;
        private TextBox txtKullaniciAdi;
        private TextBox txtEmail;
        private TextBox txtTelefon;
        private TextBox txtSifre;
        private TextBox txtSifreTekrar;

        private CheckBox chkSifreGoster;

        public RegisterForm(IUserService userService)
        {
            _userService = userService;

            FormuOlustur();
        }

        private void FormuOlustur()
        {
            Text = "Hesap Oluştur";

            StartPosition =
                FormStartPosition.CenterParent;

            Size =
                new Size(570, 730);

            MinimumSize =
                new Size(570, 730);

            MaximumSize =
                new Size(570, 730);

            BackColor =
                Color.FromArgb(
                    244,
                    247,
                    251);

            Font =
                new Font(
                    "Segoe UI",
                    10F);

            Panel header =
                new Panel();

            header.Dock =
                DockStyle.Top;

            header.Height = 95;

            header.BackColor =
                Color.FromArgb(
                    18,
                    38,
                    66);

            Label title =
                new Label();

            title.Text =
                "Hesap Oluştur";

            title.AutoSize = true;

            title.Font =
                new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.White;

            title.Location =
                new Point(
                    30,
                    20);

            header.Controls.Add(title);

            Label description =
                new Label();

            description.Text =
                "Yeni kullanıcı hesabınızı oluşturun";

            description.AutoSize =
                true;

            description.ForeColor =
                Color.FromArgb(
                    190,
                    205,
                    225);

            description.Location =
                new Point(
                    34,
                    59);

            header.Controls.Add(
                description);

            Controls.Add(header);

            Panel card =
                new Panel();

            card.Location =
                new Point(
                    30,
                    120);

            card.Size =
                new Size(
                    495,
                    535);

            card.BackColor =
                Color.White;

            card.BorderStyle =
                BorderStyle.FixedSingle;

            Controls.Add(card);

            // =====================================================
            // AD SOYAD
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Ad Soyad",
                    25,
                    25));

            txtAdSoyad =
                FormTextBox(
                    25,
                    50,
                    440);

            card.Controls.Add(
                txtAdSoyad);

            // =====================================================
            // KULLANICI ADI
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Kullanıcı Adı",
                    25,
                    100));

            txtKullaniciAdi =
                FormTextBox(
                    25,
                    125,
                    440);

            card.Controls.Add(
                txtKullaniciAdi);

            // =====================================================
            // E-POSTA
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "E-posta",
                    25,
                    175));

            txtEmail =
                FormTextBox(
                    25,
                    200,
                    440);

            card.Controls.Add(
                txtEmail);

            // =====================================================
            // TELEFON
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Telefon",
                    25,
                    250));

            txtTelefon =
                FormTextBox(
                    25,
                    275,
                    440);

            card.Controls.Add(
                txtTelefon);

            // =====================================================
            // ŞİFRE
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Şifre",
                    25,
                    325));

            txtSifre =
                FormTextBox(
                    25,
                    350,
                    210);

            txtSifre.UseSystemPasswordChar =
                true;

            card.Controls.Add(
                txtSifre);

            // =====================================================
            // ŞİFRE TEKRAR
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Şifre Tekrar",
                    255,
                    325));

            txtSifreTekrar =
                FormTextBox(
                    255,
                    350,
                    210);

            txtSifreTekrar.UseSystemPasswordChar =
                true;

            card.Controls.Add(
                txtSifreTekrar);

            // =====================================================
            // ŞİFRE GÖSTER
            // =====================================================

            chkSifreGoster =
                new CheckBox();

            chkSifreGoster.Text =
                "Şifreleri göster";

            chkSifreGoster.AutoSize =
                true;

            chkSifreGoster.Location =
                new Point(
                    25,
                    400);

            chkSifreGoster.ForeColor =
                Color.Gray;

            chkSifreGoster.CheckedChanged +=
                delegate
                {
                    bool gizle =
                        !chkSifreGoster.Checked;

                    txtSifre.UseSystemPasswordChar =
                        gizle;

                    txtSifreTekrar.UseSystemPasswordChar =
                        gizle;
                };

            card.Controls.Add(
                chkSifreGoster);

            Label passwordInfo =
                new Label();

            passwordInfo.Text =
                "Şifre en az 8 karakter olmalı ve harf ile rakam içermelidir.";

            passwordInfo.AutoSize =
                true;

            passwordInfo.ForeColor =
                Color.Gray;

            passwordInfo.Font =
                new Font(
                    "Segoe UI",
                    8F);

            passwordInfo.Location =
                new Point(
                    25,
                    430);

            card.Controls.Add(
                passwordInfo);

            // =====================================================
            // İPTAL
            // =====================================================

            Button btnIptal =
                new Button();

            btnIptal.Text =
                "İptal";

            btnIptal.Location =
                new Point(
                    205,
                    470);

            btnIptal.Size =
                new Size(
                    110,
                    42);

            btnIptal.FlatStyle =
                FlatStyle.Flat;

            btnIptal.BackColor =
                Color.White;

            btnIptal.Cursor =
                Cursors.Hand;

            btnIptal.Click +=
                delegate
                {
                    Close();
                };

            card.Controls.Add(
                btnIptal);

            // =====================================================
            // KAYIT OL
            // =====================================================

            Button btnKayit =
                new Button();

            btnKayit.Text =
                "KAYIT OL";

            btnKayit.Location =
                new Point(
                    330,
                    470);

            btnKayit.Size =
                new Size(
                    135,
                    42);

            btnKayit.FlatStyle =
                FlatStyle.Flat;

            btnKayit.FlatAppearance.BorderSize =
                0;

            btnKayit.BackColor =
                Color.FromArgb(
                    35,
                    102,
                    215);

            btnKayit.ForeColor =
                Color.White;

            btnKayit.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            btnKayit.Cursor =
                Cursors.Hand;

            btnKayit.Click +=
                BtnKayit_Click;

            card.Controls.Add(
                btnKayit);
        }

        // =========================================================
        // KAYIT İŞLEMİ
        // =========================================================

        private void BtnKayit_Click(
            object sender,
            EventArgs e)
        {
            string fullName =
                txtAdSoyad.Text.Trim();

            string username =
                txtKullaniciAdi.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string phone =
                txtTelefon.Text.Trim();

            string password =
                txtSifre.Text;

            string confirm =
                txtSifreTekrar.Text;

            // =====================================================
            // AD SOYAD KONTROL
            // =====================================================

            if (fullName == "")
            {
                Uyari(
                    "Ad soyad giriniz.");

                txtAdSoyad.Focus();

                return;
            }

            // =====================================================
            // KULLANICI ADI KONTROL
            // =====================================================

            if (username == "")
            {
                Uyari(
                    "Kullanıcı adı giriniz.");

                txtKullaniciAdi.Focus();

                return;
            }

            if (username.Length < 3)
            {
                Uyari(
                    "Kullanıcı adı en az 3 karakter olmalıdır.");

                txtKullaniciAdi.Focus();

                return;
            }

            // =====================================================
            // E-POSTA KONTROL
            // =====================================================

            if (!EmailGecerliMi(email))
            {
                Uyari(
                    "Geçerli bir e-posta adresi giriniz.");

                txtEmail.Focus();

                return;
            }

            // =====================================================
            // TELEFON KONTROL
            // =====================================================

            if (phone == "")
            {
                Uyari(
                    "Telefon numarası giriniz.");

                txtTelefon.Focus();

                return;
            }

            // =====================================================
            // AYNI KULLANICI ADI VAR MI?
            // BUSINESS KATMANI
            // =====================================================

            if (_userService.UsernameExists(
                username))
            {
                Uyari(
                    "Bu kullanıcı adı zaten kullanılıyor.");

                txtKullaniciAdi.Focus();

                return;
            }

            // =====================================================
            // AYNI E-POSTA VAR MI?
            // BUSINESS KATMANI
            // =====================================================

            if (_userService.EmailExists(
                email))
            {
                Uyari(
                    "Bu e-posta adresi zaten kayıtlı.");

                txtEmail.Focus();

                return;
            }

            // =====================================================
            // AYNI TELEFON VAR MI?
            // BUSINESS KATMANI
            // =====================================================

            if (_userService.PhoneExists(
                phone))
            {
                Uyari(
                    "Bu telefon numarası zaten kayıtlı.");

                txtTelefon.Focus();

                return;
            }

            // =====================================================
            // ŞİFRE KONTROL
            // =====================================================

            if (!PasswordService.IsPasswordValid(
                password))
            {
                Uyari(
                    "Şifre en az 8 karakter olmalı ve en az bir harf ile bir rakam içermelidir.");

                txtSifre.Focus();

                return;
            }

            // =====================================================
            // ŞİFRE TEKRAR KONTROL
            // =====================================================

            if (password != confirm)
            {
                Uyari(
                    "Şifreler eşleşmiyor.");

                txtSifreTekrar.Focus();

                return;
            }

            // =====================================================
            // USER NESNESİ
            // =====================================================

            UserAccount user =
                new UserAccount();

            user.FullName =
                fullName;

            user.Username =
                username;

            user.Email =
                email;

            user.Phone =
                phone;

            // =====================================================
            // KAYDI BUSINESS KATMANINA GÖNDER
            // PasswordHash, PasswordSalt, Role, IsActive,
            // CreatedAt işlemlerini UserManager yapacak.
            // =====================================================

            try
            {
                _userService.Register(
                    user,
                    password);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Kullanıcı kaydedilirken bir hata oluştu.\n\n" +
                    ex.Message,
                    "Kayıt Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Hesabınız başarıyla oluşturuldu.\n\n" +
                "Artık kullanıcı adınız veya e-posta adresiniz ile giriş yapabilirsiniz.",
                "Kayıt Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult =
                DialogResult.OK;

            Close();
        }

        // =========================================================
        // E-POSTA KONTROLÜ
        // =========================================================

        private bool EmailGecerliMi(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            email =
                email.Trim();

            int atIndex =
                email.IndexOf('@');

            int dotIndex =
                email.LastIndexOf('.');

            return atIndex > 0 &&
                   dotIndex > atIndex + 1 &&
                   dotIndex < email.Length - 1;
        }

        // =========================================================
        // LABEL OLUŞTUR
        // =========================================================

        private Label FormLabel(
            string text,
            int x,
            int y)
        {
            Label label =
                new Label();

            label.Text =
                text;

            label.AutoSize =
                true;

            label.Location =
                new Point(
                    x,
                    y);

            label.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    70,
                    82,
                    98);

            return label;
        }

        // =========================================================
        // TEXTBOX OLUŞTUR
        // =========================================================

        private TextBox FormTextBox(
            int x,
            int y,
            int width)
        {
            TextBox textBox =
                new TextBox();

            textBox.Location =
                new Point(
                    x,
                    y);

            textBox.Size =
                new Size(
                    width,
                    30);

            textBox.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            return textBox;
        }

        // =========================================================
        // UYARI
        // =========================================================

        private void Uyari(
            string message)
        {
            MessageBox.Show(
                message,
                "Uyarı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}