using System;
using System.Drawing;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Services;

namespace UrunFaturaYonetimi
{
    public class ChangePasswordForm : Form
    {
        private readonly UserAccount _user;
        private readonly IUserService _userService;

        private TextBox txtYeniSifre;
        private TextBox txtYeniSifreTekrar;

        private CheckBox chkGoster;

        public bool PasswordChanged
        {
            get;
            private set;
        }

        public ChangePasswordForm(
            UserAccount user,
            IUserService userService)
        {
            _user = user;
            _userService = userService;

            FormuOlustur();
        }

        private void FormuOlustur()
        {
            Text =
                "Yeni Şifre Belirleyin";

            StartPosition =
                FormStartPosition.CenterParent;

            Size =
                new Size(
                    520,
                    490);

            MinimumSize =
                new Size(
                    520,
                    490);

            MaximumSize =
                new Size(
                    520,
                    490);

            BackColor =
                Color.FromArgb(
                    244,
                    247,
                    251);

            Font =
                new Font(
                    "Segoe UI",
                    10F);

            ControlBox =
                false;

            // =====================================================
            // HEADER
            // =====================================================

            Panel header =
                new Panel();

            header.Dock =
                DockStyle.Top;

            header.Height =
                95;

            header.BackColor =
                Color.FromArgb(
                    18,
                    38,
                    66);

            Label title =
                new Label();

            title.Text =
                "Yeni Şifre Belirleyin";

            title.AutoSize =
                true;

            title.Font =
                new Font(
                    "Segoe UI",
                    19F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.White;

            title.Location =
                new Point(
                    28,
                    18);

            header.Controls.Add(
                title);

            Label info =
                new Label();

            info.Text =
                "Devam etmek için hesabınıza yeni bir şifre tanımlayın.";

            info.AutoSize =
                true;

            info.ForeColor =
                Color.FromArgb(
                    190,
                    205,
                    225);

            info.Location =
                new Point(
                    31,
                    57);

            header.Controls.Add(
                info);

            Controls.Add(
                header);

            // =====================================================
            // KART
            // =====================================================

            Panel card =
                new Panel();

            card.Location =
                new Point(
                    28,
                    125);

            card.Size =
                new Size(
                    455,
                    295);

            card.BackColor =
                Color.White;

            card.BorderStyle =
                BorderStyle.FixedSingle;

            Controls.Add(
                card);

            // =====================================================
            // YENİ ŞİFRE
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Yeni Şifre",
                    25,
                    30));

            txtYeniSifre =
                new TextBox();

            txtYeniSifre.Location =
                new Point(
                    25,
                    58);

            txtYeniSifre.Size =
                new Size(
                    400,
                    30);

            txtYeniSifre.Font =
                new Font(
                    "Segoe UI",
                    11F);

            txtYeniSifre.UseSystemPasswordChar =
                true;

            card.Controls.Add(
                txtYeniSifre);

            // =====================================================
            // YENİ ŞİFRE TEKRAR
            // =====================================================

            card.Controls.Add(
                FormLabel(
                    "Yeni Şifre Tekrar",
                    25,
                    110));

            txtYeniSifreTekrar =
                new TextBox();

            txtYeniSifreTekrar.Location =
                new Point(
                    25,
                    138);

            txtYeniSifreTekrar.Size =
                new Size(
                    400,
                    30);

            txtYeniSifreTekrar.Font =
                new Font(
                    "Segoe UI",
                    11F);

            txtYeniSifreTekrar.UseSystemPasswordChar =
                true;

            card.Controls.Add(
                txtYeniSifreTekrar);

            // =====================================================
            // ŞİFRELERİ GÖSTER
            // =====================================================

            chkGoster =
                new CheckBox();

            chkGoster.Text =
                "Şifreleri göster";

            chkGoster.AutoSize =
                true;

            chkGoster.Location =
                new Point(
                    25,
                    185);

            chkGoster.CheckedChanged +=
                delegate
                {
                    bool gizle =
                        !chkGoster.Checked;

                    txtYeniSifre.UseSystemPasswordChar =
                        gizle;

                    txtYeniSifreTekrar.UseSystemPasswordChar =
                        gizle;
                };

            card.Controls.Add(
                chkGoster);

            Label passwordInfo =
                new Label();

            passwordInfo.Text =
                "Şifre en az 8 karakter olmalı ve en az bir harf ile bir rakam içermelidir.";

            passwordInfo.AutoSize =
                true;

            passwordInfo.Font =
                new Font(
                    "Segoe UI",
                    8F);

            passwordInfo.ForeColor =
                Color.Gray;

            passwordInfo.Location =
                new Point(
                    25,
                    215);

            card.Controls.Add(
                passwordInfo);

            // =====================================================
            // OTURUMU KAPAT
            // =====================================================

            Button btnCikis =
                new Button();

            btnCikis.Text =
                "Oturumu Kapat";

            btnCikis.Location =
                new Point(
                    145,
                    245);

            btnCikis.Size =
                new Size(
                    125,
                    38);

            btnCikis.FlatStyle =
                FlatStyle.Flat;

            btnCikis.BackColor =
                Color.White;

            btnCikis.Click +=
                delegate
                {
                    PasswordChanged =
                        false;

                    DialogResult =
                        DialogResult.Cancel;

                    Close();
                };

            card.Controls.Add(
                btnCikis);

            // =====================================================
            // ŞİFREYİ DEĞİŞTİR
            // =====================================================

            Button btnDegistir =
                new Button();

            btnDegistir.Text =
                "ŞİFREYİ DEĞİŞTİR";

            btnDegistir.Location =
                new Point(
                    280,
                    245);

            btnDegistir.Size =
                new Size(
                    145,
                    38);

            btnDegistir.FlatStyle =
                FlatStyle.Flat;

            btnDegistir.FlatAppearance.BorderSize =
                0;

            btnDegistir.BackColor =
                Color.FromArgb(
                    35,
                    102,
                    215);

            btnDegistir.ForeColor =
                Color.White;

            btnDegistir.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            btnDegistir.Cursor =
                Cursors.Hand;

            btnDegistir.Click +=
                BtnDegistir_Click;

            card.Controls.Add(
                btnDegistir);

            AcceptButton =
                btnDegistir;
        }

        private void BtnDegistir_Click(
            object sender,
            EventArgs e)
        {
            string password =
                txtYeniSifre.Text;

            string confirm =
                txtYeniSifreTekrar.Text;

            if (!PasswordService.IsPasswordValid(
                password))
            {
                MessageBox.Show(
                    "Yeni şifre en az 8 karakter olmalı ve en az bir harf ile bir rakam içermelidir.",
                    "Şifre Uygun Değil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtYeniSifre.Focus();

                return;
            }

            if (password != confirm)
            {
                MessageBox.Show(
                    "Girdiğiniz iki şifre birbiriyle eşleşmiyor.",
                    "Şifreler Eşleşmiyor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtYeniSifreTekrar.SelectAll();

                txtYeniSifreTekrar.Focus();

                return;
            }

            if (PasswordService.VerifyPassword(
                password,
                _user.PasswordHash,
                _user.PasswordSalt))
            {
                MessageBox.Show(
                    "Yeni şifreniz eski şifrenizle aynı olamaz.",
                    "Yeni Şifre Gerekli",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _userService.UpdatePassword(
                _user,
                password);

            PasswordChanged =
                true;

            MessageBox.Show(
                "Şifreniz başarıyla değiştirildi.",
                "Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult =
                DialogResult.OK;

            Close();
        }

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
                    9F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    70,
                    82,
                    98);

            return label;
        }
    }
}