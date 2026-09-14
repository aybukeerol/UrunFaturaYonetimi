using System;
using System.Drawing;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public class VerificationCodeForm : Form
    {
        private readonly UserAccount _user;

        private TextBox txtCode;

        public VerificationCodeForm(
            UserAccount user)
        {
            _user = user;

            FormuOlustur();
        }

        private void FormuOlustur()
        {
            Text =
                "E-posta Doğrulama";

            StartPosition =
                FormStartPosition.CenterParent;

            Size =
                new Size(
                    500,
                    430);

            MinimumSize =
                new Size(
                    500,
                    430);

            MaximumSize =
                new Size(
                    500,
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
                "E-posta Doğrulama";

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
                "E-postanıza gönderilen 6 haneli kodu giriniz.";

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
                    425,
                    235);

            card.BackColor =
                Color.White;

            card.BorderStyle =
                BorderStyle.FixedSingle;

            Controls.Add(
                card);

            Label sent =
                new Label();

            sent.Text =
                VerificationService.MaskEmail(
                    _user.Email) +
                "\nadresine gönderilen kodu giriniz.";

            sent.AutoSize =
                true;

            sent.ForeColor =
                Color.Gray;

            sent.Location =
                new Point(
                    25,
                    25);

            card.Controls.Add(
                sent);

            // =====================================================
            // KOD
            // =====================================================

            txtCode =
                new TextBox();

            txtCode.Location =
                new Point(
                    25,
                    78);

            txtCode.Size =
                new Size(
                    375,
                    38);

            txtCode.Font =
                new Font(
                    "Segoe UI",
                    18F,
                    FontStyle.Bold);

            txtCode.TextAlign =
                HorizontalAlignment.Center;

            txtCode.MaxLength =
                6;

            txtCode.KeyPress +=
                delegate (
                    object sender,
                    KeyPressEventArgs e)
                {
                    if (!char.IsControl(e.KeyChar) &&
                        !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                };

            card.Controls.Add(
                txtCode);

            // =====================================================
            // İPTAL
            // =====================================================

            Button btnIptal =
                new Button();

            btnIptal.Text =
                "İptal";

            btnIptal.Location =
                new Point(
                    105,
                    155);

            btnIptal.Size =
                new Size(
                    100,
                    42);

            btnIptal.FlatStyle =
                FlatStyle.Flat;

            btnIptal.BackColor =
                Color.White;

            btnIptal.Click +=
                delegate
                {
                    Close();
                };

            card.Controls.Add(
                btnIptal);

            // =====================================================
            // DOĞRULA
            // =====================================================

            Button btnDogrula =
                new Button();

            btnDogrula.Text =
                "KODU DOĞRULA";

            btnDogrula.Location =
                new Point(
                    220,
                    155);

            btnDogrula.Size =
                new Size(
                    180,
                    42);

            btnDogrula.FlatStyle =
                FlatStyle.Flat;

            btnDogrula.FlatAppearance.BorderSize =
                0;

            btnDogrula.BackColor =
                Color.FromArgb(
                    35,
                    102,
                    215);

            btnDogrula.ForeColor =
                Color.White;

            btnDogrula.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            btnDogrula.Cursor =
                Cursors.Hand;

            btnDogrula.Click +=
                BtnDogrula_Click;

            card.Controls.Add(
                btnDogrula);

            AcceptButton =
                btnDogrula;
        }

        private void BtnDogrula_Click(
            object sender,
            EventArgs e)
        {
            string code =
                txtCode.Text.Trim();

            if (code.Length != 6)
            {
                MessageBox.Show(
                    "Lütfen 6 haneli doğrulama kodunu giriniz.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCode.Focus();

                return;
            }

            string error;

            bool success =
                VerificationService.Verify(
                    _user,
                    code,
                    out error);

            if (!success)
            {
                MessageBox.Show(
                    error,
                    "Doğrulama Başarısız",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCode.SelectAll();
                txtCode.Focus();

                return;
            }

            MessageBox.Show(
                "E-posta adresiniz başarıyla doğrulandı.\n\n" +
                "Sisteme giriş yapıldıktan sonra yeni bir şifre belirlemeniz istenecek.",
                "Doğrulama Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult =
                DialogResult.OK;

            Close();
        }
    }
}