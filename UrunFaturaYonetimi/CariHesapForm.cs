using System;
using System.Drawing;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Managers;

namespace UrunFaturaYonetimi
{
    public class CariHesapForm : Form
    {
        private TextBox txtCariKodu;
        private ComboBox cmbTip;
        private TextBox txtCariAdi;
        private TextBox txtYetkiliKisi;
        private TextBox txtFirmaNo;
        private TextBox txtFirmaAdi;
        private TextBox txtTelefon;
        private TextBox txtEmail;
        private TextBox txtMahalle;
        private TextBox txtSehir;
        private TextBox txtUlke;

        private DataGridView dgvCariler;

        private Button btnYeni;
        private Button btnKaydet;
        private Button btnSil;
        private Button btnTemizle;
        private Button btnKapat;

        private readonly ICariService _cariService;

        public CariHesapForm()
            : this(new CariManager())
        {
        }

        public CariHesapForm(ICariService cariService)
        {
            _cariService = cariService;
            FormuOlustur();
            CarileriYukle();
        }

        private void FormuOlustur()
        {
            Text = "Cari Hesaplar";

            StartPosition =
                FormStartPosition.CenterParent;

            Size =
                new Size(1180, 760);

            MinimumSize =
                new Size(1050, 700);

            BackColor =
                Color.FromArgb(243, 246, 250);

            Font =
                new Font("Segoe UI", 9.5F);

            // =====================================================
            // ÜST BAŞLIK
            // =====================================================

            Panel pnlHeader =
                new Panel();

            pnlHeader.Dock =
                DockStyle.Top;

            pnlHeader.Height =
                58;

            pnlHeader.BackColor =
                Color.FromArgb(24, 118, 193);

            Label lblBaslik =
                new Label();

            lblBaslik.Text =
                "Cari Hesap Kartı";

            lblBaslik.AutoSize =
                true;

            lblBaslik.Font =
                new Font(
                    "Segoe UI",
                    15F,
                    FontStyle.Bold);

            lblBaslik.ForeColor =
                Color.White;

            lblBaslik.Location =
                new Point(20, 16);

            pnlHeader.Controls.Add(
                lblBaslik);

            Controls.Add(
                pnlHeader);

            // =====================================================
            // ARAÇ ÇUBUĞU
            // =====================================================

            Panel pnlToolbar =
                new Panel();

            pnlToolbar.Dock =
                DockStyle.Top;

            pnlToolbar.Height =
                75;

            pnlToolbar.BackColor =
                Color.White;

            btnYeni =
                ToolbarButton(
                    "Yeni",
                    20);

            btnKaydet =
                ToolbarButton(
                    "Kaydet",
                    110);

            btnSil =
                ToolbarButton(
                    "Sil",
                    200);

            btnTemizle =
                ToolbarButton(
                    "Temizle",
                    290);

            btnKapat =
                ToolbarButton(
                    "Kapat",
                    390);

            btnYeni.Click +=
                BtnYeni_Click;

            btnKaydet.Click +=
                BtnKaydet_Click;

            btnSil.Click +=
                BtnSil_Click;

            btnTemizle.Click +=
                BtnTemizle_Click;

            btnKapat.Click +=
                BtnKapat_Click;

            pnlToolbar.Controls.Add(
                btnYeni);

            pnlToolbar.Controls.Add(
                btnKaydet);

            pnlToolbar.Controls.Add(
                btnSil);

            pnlToolbar.Controls.Add(
                btnTemizle);

            pnlToolbar.Controls.Add(
                btnKapat);

            Controls.Add(
                pnlToolbar);

            // =====================================================
            // CARİ BİLGİLERİ
            // =====================================================

            Panel pnlBilgi =
                new Panel();

            pnlBilgi.Dock =
                DockStyle.Top;

            pnlBilgi.Height =
                250;

            pnlBilgi.BackColor =
                Color.White;

            Label lblBolum =
                new Label();

            lblBolum.Text =
                "Cari Bilgileri";

            lblBolum.AutoSize =
                true;

            lblBolum.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            lblBolum.ForeColor =
                Color.FromArgb(
                    45,
                    58,
                    75);

            lblBolum.Location =
                new Point(25, 15);

            pnlBilgi.Controls.Add(
                lblBolum);

            // SATIR 1

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Cari Kodu",
                    25,
                    55));

            txtCariKodu =
                FormTextBox(
                    25,
                    78,
                    180);

            pnlBilgi.Controls.Add(
                txtCariKodu);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Tip",
                    225,
                    55));

            cmbTip =
                new ComboBox();

            cmbTip.Location =
                new Point(225, 78);

            cmbTip.Size =
                new Size(180, 30);

            cmbTip.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbTip.Items.Add(
                "Müşteri");

            cmbTip.Items.Add(
                "Tedarikçi");

            cmbTip.Items.Add(
                "Müşteri / Tedarikçi");

            cmbTip.SelectedIndex =
                0;

            pnlBilgi.Controls.Add(
                cmbTip);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Müşteri / Tedarikçi Adı",
                    425,
                    55));

            txtCariAdi =
                FormTextBox(
                    425,
                    78,
                    300);

            pnlBilgi.Controls.Add(
                txtCariAdi);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Yetkili Kişi",
                    745,
                    55));

            txtYetkiliKisi =
                FormTextBox(
                    745,
                    78,
                    260);

            pnlBilgi.Controls.Add(
                txtYetkiliKisi);

            // SATIR 2

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Firma No",
                    25,
                    125));

            txtFirmaNo =
                FormTextBox(
                    25,
                    148,
                    180);

            pnlBilgi.Controls.Add(
                txtFirmaNo);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Firma Adı",
                    225,
                    125));

            txtFirmaAdi =
                FormTextBox(
                    225,
                    148,
                    300);

            pnlBilgi.Controls.Add(
                txtFirmaAdi);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Telefon",
                    545,
                    125));

            txtTelefon =
                FormTextBox(
                    545,
                    148,
                    200);

            pnlBilgi.Controls.Add(
                txtTelefon);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "E-Posta",
                    765,
                    125));

            txtEmail =
                FormTextBox(
                    765,
                    148,
                    240);

            pnlBilgi.Controls.Add(
                txtEmail);

            // SATIR 3

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Mahalle",
                    25,
                    190));

            txtMahalle =
                FormTextBox(
                    25,
                    212,
                    300);

            pnlBilgi.Controls.Add(
                txtMahalle);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Şehir",
                    345,
                    190));

            txtSehir =
                FormTextBox(
                    345,
                    212,
                    220);

            pnlBilgi.Controls.Add(
                txtSehir);

            pnlBilgi.Controls.Add(
                FormLabel(
                    "Ülke",
                    585,
                    190));

            txtUlke =
                FormTextBox(
                    585,
                    212,
                    220);

            txtUlke.Text =
                "Türkiye";

            pnlBilgi.Controls.Add(
                txtUlke);

            Controls.Add(
                pnlBilgi);

            // =====================================================
            // LİSTE BAŞLIK
            // =====================================================

            Panel pnlListeBaslik =
                new Panel();

            pnlListeBaslik.Dock =
                DockStyle.Top;

            pnlListeBaslik.Height =
                50;

            pnlListeBaslik.BackColor =
                Color.FromArgb(
                    248,
                    249,
                    251);

            Label lblListe =
                new Label();

            lblListe.Text =
                "Cari Hesap Listesi";

            lblListe.AutoSize =
                true;

            lblListe.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            lblListe.ForeColor =
                Color.FromArgb(
                    45,
                    58,
                    75);

            lblListe.Location =
                new Point(25, 15);

            pnlListeBaslik.Controls.Add(
                lblListe);

            Controls.Add(
                pnlListeBaslik);

            // =====================================================
            // GRID
            // =====================================================

            dgvCariler =
                new DataGridView();

            dgvCariler.Dock =
                DockStyle.Fill;

            dgvCariler.BackgroundColor =
                Color.White;

            dgvCariler.BorderStyle =
                BorderStyle.None;

            dgvCariler.RowHeadersVisible =
                false;

            dgvCariler.AllowUserToAddRows =
                false;

            dgvCariler.AllowUserToDeleteRows =
                false;

            dgvCariler.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvCariler.MultiSelect =
                false;

            dgvCariler.ReadOnly =
                true;

            dgvCariler.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            dgvCariler.ScrollBars =
                ScrollBars.Both;

            dgvCariler.ColumnHeadersHeight =
                40;

            dgvCariler.RowTemplate.Height =
                34;

            dgvCariler.EnableHeadersVisualStyles =
                false;

            dgvCariler.GridColor =
                Color.FromArgb(
                    224,
                    228,
                    234);

            dgvCariler.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(
                    242,
                    245,
                    249);

            dgvCariler.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(
                    48,
                    61,
                    78);

            dgvCariler.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            dgvCariler.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    220,
                    235,
                    252);

            dgvCariler.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            dgvCariler.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(
                    250,
                    251,
                    253);

            KolonEkle(
                "CariKodu",
                "Cari Kodu",
                110);

            KolonEkle(
                "Tip",
                "Tip",
                120);

            KolonEkle(
                "CariAdi",
                "Müşteri / Tedarikçi Adı",
                210);

            KolonEkle(
                "YetkiliKisi",
                "Yetkili Kişi",
                160);

            KolonEkle(
                "FirmaNo",
                "Firma No",
                110);

            KolonEkle(
                "FirmaAdi",
                "Firma Adı",
                200);

            KolonEkle(
                "Telefon",
                "Telefon",
                140);

            KolonEkle(
                "Email",
                "E-Posta",
                200);

            KolonEkle(
                "Mahalle",
                "Mahalle",
                170);

            KolonEkle(
                "Sehir",
                "Şehir",
                120);

            KolonEkle(
                "Ulke",
                "Ülke",
                120);

            dgvCariler.CellDoubleClick +=
                DgvCariler_CellDoubleClick;

            Controls.Add(
                dgvCariler);


        }

        // =========================================================
        // ORTAK KONTROLLER
        // =====================================================

        private Button ToolbarButton(
            string text,
            int x)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Location =
                new Point(x, 18);

            button.Size =
                new Size(80, 40);

            button.FlatStyle =
                FlatStyle.Flat;

            button.BackColor =
                Color.White;

            button.ForeColor =
                Color.FromArgb(
                    55,
                    67,
                    82);

            button.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            button.Cursor =
                Cursors.Hand;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    205,
                    212,
                    220);

            return button;
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
                new Point(x, y);

            label.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    75,
                    85,
                    100);

            return label;
        }

        private TextBox FormTextBox(
            int x,
            int y,
            int width)
        {
            TextBox textBox =
                new TextBox();

            textBox.Location =
                new Point(x, y);

            textBox.Size =
                new Size(width, 30);

            textBox.Font =
                new Font(
                    "Segoe UI",
                    10F);

            return textBox;
        }

        private void KolonEkle(
            string name,
            string header,
            int width)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.Name =
                name;

            column.HeaderText =
                header;

            column.Width =
                width;

            column.SortMode =
                DataGridViewColumnSortMode.Automatic;

            dgvCariler.Columns.Add(
                column);
        }

        private void CarileriYukle()
        {
            dgvCariler.Rows.Clear();

            foreach (CariKaydi cari in
                _cariService.GetAll())
            {
                dgvCariler.Rows.Add(
                    cari.CariKodu,
                    cari.Tip,
                    cari.CariAdi,
                    cari.YetkiliKisi,
                    cari.FirmaNo,
                    cari.FirmaAdi,
                    cari.Telefon,
                    cari.Email,
                    cari.Mahalle,
                    cari.Sehir,
                    cari.Ulke);
            }
        }

        // =====================================================
        // BUTONLAR
        // =====================================================

        private void BtnYeni_Click(
            object sender,
            EventArgs e)
        {
            FormuTemizle();

            txtCariKodu.Focus();
        }

        private void BtnKaydet_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtCariKodu.Text))
            {
                MessageBox.Show(
                    "Cari kodu giriniz.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCariKodu.Focus();

                return;
            }

            if (string.IsNullOrWhiteSpace(
                txtCariAdi.Text))
            {
                MessageBox.Show(
                    "Müşteri / Tedarikçi adı giriniz.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCariAdi.Focus();

                return;
            }

            CariKaydi mevcut =
                _cariService.GetByCode(
                    txtCariKodu.Text.Trim());

            CariKaydi cari =
                new CariKaydi
                {
                    CariKodu = txtCariKodu.Text.Trim(),
                    Tip = cmbTip.Text,
                    CariAdi = txtCariAdi.Text.Trim(),
                    YetkiliKisi = txtYetkiliKisi.Text.Trim(),
                    FirmaNo = txtFirmaNo.Text.Trim(),
                    FirmaAdi = txtFirmaAdi.Text.Trim(),
                    Telefon = txtTelefon.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Mahalle = txtMahalle.Text.Trim(),
                    Sehir = txtSehir.Text.Trim(),
                    Ulke = txtUlke.Text.Trim(),
                    Favori = mevcut != null && mevcut.Favori
                };

            if (mevcut == null)
            {
                _cariService.Add(cari);
            }
            else
            {
                _cariService.Update(cari);
            }

            CarileriYukle();

            MessageBox.Show(
                "Cari hesap kaydedildi.",
                "Cari Hesap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            FormuTemizle();
        }

        private void BtnSil_Click(
            object sender,
            EventArgs e)
        {
            if (dgvCariler.CurrentRow == null)
            {
                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Seçili cari hesabı silmek istiyor musunuz?",
                    "Cari Hesap Sil",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result ==
                DialogResult.Yes)
            {
                string cariKodu =
                    Deger(
                        dgvCariler.CurrentRow,
                        "CariKodu");

                _cariService.Delete(
                    cariKodu);

                CarileriYukle();

                FormuTemizle();
            }
        }

        private void BtnTemizle_Click(
            object sender,
            EventArgs e)
        {
            FormuTemizle();
        }

        private void BtnKapat_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void FormuTemizle()
        {
            txtCariKodu.Clear();
            txtCariAdi.Clear();
            txtYetkiliKisi.Clear();
            txtFirmaNo.Clear();
            txtFirmaAdi.Clear();
            txtTelefon.Clear();
            txtEmail.Clear();
            txtMahalle.Clear();
            txtSehir.Clear();

            txtUlke.Text =
                "Türkiye";

            if (cmbTip.Items.Count > 0)
            {
                cmbTip.SelectedIndex =
                    0;
            }
        }

        // Gridde bir cariye çift tıklayınca
        // bilgileri üst forma getirir.
        private void DgvCariler_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row =
                dgvCariler.Rows[e.RowIndex];

            txtCariKodu.Text =
                Deger(row, "CariKodu");

            cmbTip.Text =
                Deger(row, "Tip");

            txtCariAdi.Text =
                Deger(row, "CariAdi");

            txtYetkiliKisi.Text =
                Deger(row, "YetkiliKisi");

            txtFirmaNo.Text =
                Deger(row, "FirmaNo");

            txtFirmaAdi.Text =
                Deger(row, "FirmaAdi");

            txtTelefon.Text =
                Deger(row, "Telefon");

            txtEmail.Text =
                Deger(row, "Email");

            txtMahalle.Text =
                Deger(row, "Mahalle");

            txtSehir.Text =
                Deger(row, "Sehir");

            txtUlke.Text =
                Deger(row, "Ulke");
        }

        private string Deger(
            DataGridViewRow row,
            string columnName)
        {
            object value =
                row.Cells[columnName].Value;

            if (value == null)
            {
                return "";
            }

            return value.ToString();
        }
    }
}