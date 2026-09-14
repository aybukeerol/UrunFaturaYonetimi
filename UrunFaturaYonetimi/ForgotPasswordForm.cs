using System;
using System.Drawing;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;

namespace UrunFaturaYonetimi
{
    public class ForgotPasswordForm : Form
    {
        private readonly IUserService _userService;

        private TextBox txtEmail;

        public UserAccount VerifiedUser
        {
            get;
            private set;
        }

        public ForgotPasswordForm(
            IUserService userService)
        {
            _userService = userService;

            FormuOlustur();
        }

        private void FormuOlustur()
        {
            Text =
                "Şifremi Unuttum";

            StartPosition =
                FormStartPosition.CenterParent;

            Size =
                new Size(
                    530,
                    430);

            MinimumSize =
                new Size(
                    530,
                    430);

            MaximumSize =
                new Size(
                    530,
                    430);

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
                "Şifrenizi mi unuttunuz?";

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
                    17);

            header.Controls.Add(
                title);

            Label description =
                new Label();

            description.Text =
                "E-posta adresinize doğrulama kodu göndereceğiz.";

            description.AutoSize =
                true;

            description.ForeColor =
                Color.FromArgb(
                    190,
                    205,
                    225);

            description.Location =
                new Point(
                    31,
                    56);

            header.Controls.Add(
                description);

            Controls.Add(
                header);

            Panel card =
                new Panel();

            card.Location =
                new Point(
                    28,
                    125);

            card.Size =
                new Size(
                    460,
                    235);

            card.BackColor =
                Color.White;

            card.BorderStyle =
                BorderStyle.FixedSingle;

            Controls.Add(
                card);

            Label lblEmail =
                new Label();

            lblEmail.Text =
                "E-posta Adresiniz";

            lblEmail.AutoSize =
                true;

            lblEmail.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            lblEmail.ForeColor =
                Color.FromArgb(
                    70,
                    82,
                    98);

            lblEmail.Location =
                new Point(
                    25,
                    28);

            card.Controls.Add(
                lblEmail);

            txtEmail =
                new TextBox();

            txtEmail.Location =
                new Point(
                    25,
                    58);

            txtEmail.Size =
                new Size(
                    405,
                    32);

            txtEmail.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            card.Controls.Add(
                txtEmail);

            Label info =
                new Label();

            info.Text =
                "Kayıt sırasında kullandığınız e-posta adresini giriniz.";

            info.AutoSize =
                true;

            info.ForeColor =
                Color.Gray;

            info.Font =
                new Font(
                    "Segoe UI",
                    8.5F);

            info.Location =
                new Point(
                    25,
                    103);

            card.Controls.Add(
                info);

            Button btnIptal =
                new Button();

            btnIptal.Text =
                "İptal";

            btnIptal.Location =
                new Point(
                    185,
                    155);

            btnIptal.Size =
                new Size(
                    105,
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

            Button btnKodGonder =
                new Button();

            btnKodGonder.Text =
                "KOD GÖNDER";

            btnKodGonder.Location =
                new Point(
                    305,
                    155);

            btnKodGonder.Size =
                new Size(
                    125,
                    42);

            btnKodGonder.FlatStyle =
                FlatStyle.Flat;

            btnKodGonder.FlatAppearance.BorderSize =
                0;

            btnKodGonder.BackColor =
                Color.FromArgb(
                    35,
                    102,
                    215);

            btnKodGonder.ForeColor =
                Color.White;

            btnKodGonder.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            btnKodGonder.Cursor =
                Cursors.Hand;

            btnKodGonder.Click +=
                BtnKodGonder_Click;

            card.Controls.Add(
                btnKodGonder);

            AcceptButton =
                btnKodGonder;
        }

        private void BtnKodGonder_Click(
            object sender,
            EventArgs e)
        {
            string email =
                txtEmail.Text.Trim();

            if (email == "")
            {
                MessageBox.Show(
                    "E-posta adresinizi giriniz.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtEmail.Focus();

                return;
            }

            UserAccount user =
                _userService.FindByEmail(
                    email);

            if (user == null)
            {
                MessageBox.Show(
                    "Bu e-posta adresiyle kayıtlı bir kullanıcı hesabı bulunamadı.",
                    "Hesap Bulunamadı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtEmail.SelectAll();

                txtEmail.Focus();

                return;
            }

            if (!user.IsActive)
            {
                MessageBox.Show(
                    "Bu kullanıcı hesabı aktif değil.",
                    "Hesap Pasif",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string code =
                VerificationService.CreateCode(
                    user);

            VerificationService.SendEmailCode(
                user,
                code);

            VerificationCodeForm verificationForm =
                new VerificationCodeForm(
                    user);

            DialogResult result =
                verificationForm.ShowDialog(
                    this);

            if (result ==
                DialogResult.OK)
            {
                _userService.SetMustChangePassword(
                    user,
                    true);

                VerifiedUser =
                    user;

                DialogResult =
                    DialogResult.OK;

                Close();
            }
        }
    }
}