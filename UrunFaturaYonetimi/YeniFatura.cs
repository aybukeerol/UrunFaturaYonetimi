using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public class YeniFaturaForm : Form
    {
        private ComboBox cmbAliciTipi;
        private TextBox txtAlici;
        private TextBox txtKimlikNo;
        private Label lblKimlik;

        private TextBox txtFaturaNo;
        private DateTimePicker dtpTarih;
        private ComboBox cmbSenaryo;
        private ComboBox cmbFaturaTipi;
        private ComboBox cmbParaBirimi;

        private DataGridView dgvKalemler;
        private DataGridView dgvOnizleme;

        private Label prvAlici;
        private Label prvKimlik;
        private Label prvFaturaNo;
        private Label prvTarih;
        private Label prvSenaryo;
        private Label prvTip;

        private Label prvAraToplam;
        private Label prvKdv;
        private Label prvGenelToplam;

        private FlowLayoutPanel pnlFavoriCariler;
        private FlowLayoutPanel pnlFavoriUrunler;

        private string duzenlenenTaslakNo;

        private bool hesaplamaYapiliyor =
            false;

        // Tema ve responsive yerleşim
        private bool koyuMod = false;
        private Button btnTema;
        private Panel headerPanel;
        private Panel solContainer;
        private Panel sagBackground;
        private Panel previewPaper;

        public YeniFaturaForm()
        {
            FormuOlustur();
        }

        // =========================================================
        // FORM
        // =========================================================

        private void FormuOlustur()
        {
            Text =
                "Yeni Satış Faturası";

            StartPosition =
                FormStartPosition.CenterParent;

            WindowState =
                FormWindowState.Maximized;

            MinimumSize =
                new Size(1200, 720);

            BackColor =
                Color.FromArgb(242, 245, 249);

            Font =
                new Font("Segoe UI", 9.5F);

            // Windows DPI / yazı ölçeklemesinin kontrolleri üst üste bindirmesini engeller.
            AutoScaleMode =
                AutoScaleMode.None;

            Panel header = UstBaslikOlustur();

            TableLayoutPanel anaLayout = new TableLayoutPanel();
            anaLayout.Dock = DockStyle.Fill;
            anaLayout.Margin = Padding.Empty;
            anaLayout.Padding = Padding.Empty;
            anaLayout.ColumnCount = 2;
            anaLayout.RowCount = 1;
            anaLayout.BackColor = Color.FromArgb(225, 230, 236);
            anaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            anaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            anaLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel sol = SolTarafiOlustur();
            Panel sag = SagTarafiOlustur();
            sol.Margin = Padding.Empty;
            sag.Margin = Padding.Empty;
            anaLayout.Controls.Add(sol, 0, 0);
            anaLayout.Controls.Add(sag, 1, 0);

            // Başlık ve içerik ayrı satırlarda: içerik başlığın altına kayamaz.
            TableLayoutPanel formLayout = new TableLayoutPanel();
            formLayout.Dock = DockStyle.Fill;
            formLayout.Margin = Padding.Empty;
            formLayout.Padding = Padding.Empty;
            formLayout.ColumnCount = 1;
            formLayout.RowCount = 2;
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            header.Dock = DockStyle.Fill;
            header.Margin = Padding.Empty;
            formLayout.Controls.Add(header, 0, 0);
            formLayout.Controls.Add(anaLayout, 0, 1);
            Controls.Add(formLayout);

            Shown +=
                delegate
                {
                    FavorileriYukle();

                    OnizlemeyiGuncelle();

                    TemayiUygula();
                };
        }

        // =========================================================
        // HEADER
        // =========================================================

        private Panel UstBaslikOlustur()
        {
            Panel header =
                new Panel();

            headerPanel =
                header;

            header.Dock =
                DockStyle.Top;

            header.Height =
                65;

            header.BackColor =
                Color.FromArgb(18, 38, 66);

            Label title =
                new Label();

            title.Text =
                "Yeni Satış Faturası";

            title.AutoSize =
                true;

            title.Font =
                new Font(
                    "Segoe UI",
                    17F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.White;

            title.Location =
                new Point(22, 17);

            Label status =
                new Label();

            status.Text =
                "● TASLAK";

            status.AutoSize =
                true;

            status.ForeColor =
                Color.FromArgb(190, 210, 235);

            status.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            status.Location =
                new Point(270, 25);

            btnTema =
                new Button();

            btnTema.Text =
                "☾  Koyu mod";

            btnTema.Size =
                new Size(145, 39);

            btnTema.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            btnTema.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnTema.FlatStyle =
                FlatStyle.Flat;

            btnTema.FlatAppearance.BorderSize =
                0;

            btnTema.BackColor =
                Color.FromArgb(30, 55, 85);

            btnTema.ForeColor =
                Color.White;

            btnTema.Cursor =
                Cursors.Hand;

            btnTema.Click +=
                delegate
                {
                    koyuMod =
                        !koyuMod;

                    TemayiUygula();
                };

            header.Controls.Add(
                btnTema);

            Button btnKapat =
                new Button();

            btnKapat.Text =
                "Kapat";

            btnKapat.Size =
                new Size(95, 39);

            btnKapat.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            btnKapat.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnKapat.FlatStyle =
                FlatStyle.Flat;

            btnKapat.FlatAppearance.BorderSize =
                0;

            btnKapat.BackColor =
                Color.FromArgb(30, 55, 85);

            btnKapat.ForeColor =
                Color.White;

            btnKapat.Location =
                new Point(1000, 15);

            btnKapat.Click +=
                delegate
                {
                    Close();
                };

            header.Resize +=
                delegate
                {
                    btnKapat.Left =
                        header.Width -
                        btnKapat.Width -
                        20;

                    btnTema.Left =
                        btnKapat.Left -
                        btnTema.Width -
                        10;

                    btnTema.Top =
                        13;

                    btnKapat.Top = 13;
                };

            header.Controls.Add(
                title);

            header.Controls.Add(
                status);

            header.Controls.Add(
                btnKapat);

            return header;
        }

        // =========================================================
        // SOL TARAF
        // =========================================================

        private Panel SolTarafiOlustur()
        {
            Panel container =
                new Panel();

            solContainer =
                container;

            container.Dock =
                DockStyle.Fill;

            container.AutoScroll =
                true;

            container.BackColor =
                Color.FromArgb(244, 247, 250);

            // =====================================================
            // FAVORİLER
            // =====================================================

            Panel favoriCard =
                KartOlustur();

            favoriCard.Location =
                new Point(15, 15);

            favoriCard.Size =
                new Size(500, 255);

            favoriCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            favoriCard.Controls.Add(
                Baslik(
                    "Hızlı Erişim",
                    18,
                    12));

            Label lblCari =
                FormLabel(
                    "Favori Cariler",
                    18,
                    56);

            favoriCard.Controls.Add(
                lblCari);

            pnlFavoriCariler =
                new FlowLayoutPanel();

            pnlFavoriCariler.Location =
                new Point(18, 86);

            pnlFavoriCariler.Size =
                new Size(460, 48);

            pnlFavoriCariler.AutoScroll =
                true;
            pnlFavoriCariler.WrapContents = false;

            favoriCard.Controls.Add(
                pnlFavoriCariler);

            Label lblUrun =
                FormLabel(
                    "Favori Ürünler",
                    18,
                    153);

            favoriCard.Controls.Add(
                lblUrun);

            pnlFavoriUrunler =
                new FlowLayoutPanel();

            pnlFavoriUrunler.Location =
                new Point(18, 185);

            pnlFavoriUrunler.Size =
                new Size(460, 50);

            pnlFavoriUrunler.AutoScroll =
                true;
            pnlFavoriUrunler.WrapContents = false;

            favoriCard.Controls.Add(
                pnlFavoriUrunler);

            container.Controls.Add(
                favoriCard);

            // =====================================================
            // ALICI
            // =====================================================

            Panel aliciCard =
                KartOlustur();

            aliciCard.Location =
                new Point(15, 280);

            aliciCard.Size =
                new Size(500, 255);

            aliciCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            aliciCard.Controls.Add(
                Baslik(
                    "Alıcı Bilgileri",
                    18,
                    14));

            aliciCard.Controls.Add(
                FormLabel(
                    "Alıcı Tipi",
                    18,
                    60));

            cmbAliciTipi =
                new ComboBox();

            cmbAliciTipi.Location =
                new Point(18, 91);

            cmbAliciTipi.Size =
                new Size(205, 30);

            cmbAliciTipi.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbAliciTipi.Items.Add(
                "Bireysel Müşteri");

            cmbAliciTipi.Items.Add(
                "Kurumsal Müşteri");

            cmbAliciTipi.SelectedIndex =
                0;

            cmbAliciTipi.SelectedIndexChanged +=
                delegate
                {
                    KimlikBasliginiDegistir();

                    OnizlemeyiGuncelle();
                };

            aliciCard.Controls.Add(
                cmbAliciTipi);

            aliciCard.Controls.Add(
                FormLabel(
                    "Cari / Alıcı",
                    245,
                    60));

            txtAlici =
                new TextBox();

            txtAlici.Location =
                new Point(245, 91);

            txtAlici.Size =
                new Size(225, 30);

            txtAlici.TextChanged +=
                VeriDegisti;

            aliciCard.Controls.Add(
                txtAlici);

            lblKimlik =
                FormLabel(
                    "T.C. Kimlik No",
                    18,
                    145);

            aliciCard.Controls.Add(
                lblKimlik);

            txtKimlikNo =
                new TextBox();

            txtKimlikNo.Location =
                new Point(18, 178);

            txtKimlikNo.Size =
                new Size(205, 30);

            txtKimlikNo.TextChanged +=
                VeriDegisti;

            aliciCard.Controls.Add(
                txtKimlikNo);

            Button btnCariSec =
                BeyazButon(
                    "Cari Hesap Seç");

            btnCariSec.Location =
                new Point(245, 175);

            btnCariSec.Click +=
                delegate
                {
                    CariHesapForm form =
                        new CariHesapForm();

                    form.ShowDialog(this);
                };

            aliciCard.Controls.Add(
                btnCariSec);

            container.Controls.Add(
                aliciCard);

            // =====================================================
            // FATURA BİLGİLERİ
            // =====================================================

            Panel faturaCard =
                KartOlustur();

            faturaCard.Location =
                new Point(15, 550);

            faturaCard.Size =
                new Size(500, 315);

            faturaCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            faturaCard.Controls.Add(
                Baslik(
                    "Fatura Bilgileri",
                    18,
                    14));

            faturaCard.Controls.Add(
                FormLabel(
                    "Fatura No",
                    18,
                    60));

            txtFaturaNo =
                new TextBox();

            txtFaturaNo.Location =
                new Point(18, 91);

            txtFaturaNo.Size =
                new Size(205, 30);

            txtFaturaNo.Text =
                "SF" +
                DateTime.Now.Year +
                "000001";

            txtFaturaNo.TextChanged +=
                VeriDegisti;

            faturaCard.Controls.Add(
                txtFaturaNo);

            faturaCard.Controls.Add(
                FormLabel(
                    "Tarih",
                    245,
                    60));

            dtpTarih =
                new DateTimePicker();

            dtpTarih.Location =
                new Point(245, 91);

            dtpTarih.Size =
                new Size(205, 30);

            dtpTarih.Format =
                DateTimePickerFormat.Short;

            dtpTarih.ValueChanged +=
                VeriDegisti;

            faturaCard.Controls.Add(
                dtpTarih);

            faturaCard.Controls.Add(
                FormLabel(
                    "Senaryo",
                    18,
                    145));

            cmbSenaryo =
                new ComboBox();

            cmbSenaryo.Location =
                new Point(18, 178);

            cmbSenaryo.Size =
                new Size(205, 30);

            cmbSenaryo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbSenaryo.Items.Add(
                "Temel Fatura");

            cmbSenaryo.Items.Add(
                "Ticari Fatura");

            cmbSenaryo.SelectedIndex =
                0;

            cmbSenaryo.SelectedIndexChanged +=
                VeriDegisti;

            faturaCard.Controls.Add(
                cmbSenaryo);

            faturaCard.Controls.Add(
                FormLabel(
                    "Fatura Tipi",
                    245,
                    145));

            cmbFaturaTipi =
                new ComboBox();

            cmbFaturaTipi.Location =
                new Point(245, 178);

            cmbFaturaTipi.Size =
                new Size(205, 30);

            cmbFaturaTipi.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbFaturaTipi.Items.Add(
                "Satış");

            cmbFaturaTipi.Items.Add(
                "İade");

            cmbFaturaTipi.SelectedIndex =
                0;

            cmbFaturaTipi.SelectedIndexChanged +=
                VeriDegisti;

            faturaCard.Controls.Add(
                cmbFaturaTipi);

            faturaCard.Controls.Add(
                FormLabel(
                    "Para Birimi",
                    18,
                    235));

            cmbParaBirimi =
                new ComboBox();

            cmbParaBirimi.Location =
                new Point(18, 265);

            cmbParaBirimi.Size =
                new Size(205, 30);

            cmbParaBirimi.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbParaBirimi.Items.Add(
                "TRY - Türk Lirası");

            cmbParaBirimi.Items.Add(
                "USD - Amerikan Doları");

            cmbParaBirimi.Items.Add(
                "EUR - Euro");

            cmbParaBirimi.SelectedIndex =
                0;

            cmbParaBirimi.SelectedIndexChanged +=
                VeriDegisti;

            faturaCard.Controls.Add(
                cmbParaBirimi);

            container.Controls.Add(
                faturaCard);

            // =====================================================
            // KALEMLER
            // =====================================================

            Panel kalemCard =
                KartOlustur();

            kalemCard.Location =
                new Point(15, 880);

            kalemCard.Size =
                new Size(500, 365);

            kalemCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            kalemCard.Controls.Add(
                Baslik(
                    "Ürün / Hizmet Kalemleri",
                    18,
                    14));

            Button btnKalemEkle =
                MaviButon(
                    "+ Kalem Ekle");

            btnKalemEkle.Location =
                new Point(255, 9);

            btnKalemEkle.Size =
                new Size(110, 40);

            btnKalemEkle.Click +=
                delegate
                {
                    BosKalemEkle();
                };

            kalemCard.Controls.Add(
                btnKalemEkle);

            Button btnKalemSil =
                BeyazButon(
                    "Kalem Sil");

            btnKalemSil.Location =
                new Point(375, 9);

            btnKalemSil.Size =
                new Size(95, 40);

            btnKalemSil.Click +=
                delegate
                {
                    if (dgvKalemler.CurrentRow != null)
                    {
                        dgvKalemler.Rows.Remove(
                            dgvKalemler.CurrentRow);

                        OnizlemeyiGuncelle();
                    }
                };

            kalemCard.Controls.Add(
                btnKalemSil);

            dgvKalemler =
                KalemGridOlustur();

            dgvKalemler.Location =
                new Point(18, 66);

            dgvKalemler.Size =
                new Size(455, 277);

            kalemCard.Controls.Add(
                dgvKalemler);

            container.Controls.Add(
                kalemCard);

            // =====================================================
            // KAYDET
            // =====================================================

            Panel buttons =
                new Panel();

            buttons.Location =
                new Point(15, 1260);

            buttons.Size =
                new Size(500, 80);

            Button btnTaslak =
                BeyazButon(
                    "Taslak Kaydet");

            btnTaslak.Location =
                new Point(130, 15);

            btnTaslak.Size =
                new Size(150, 42);

            btnTaslak.Click += delegate { TaslagiKaydet(); };

            buttons.Controls.Add(
                btnTaslak);

            Button btnKaydet =
                MaviButon(
                    "FATURAYI KAYDET");

            btnKaydet.Location =
                new Point(295, 15);

            btnKaydet.Size =
                new Size(175, 42);

            btnKaydet.Click +=
                delegate
                {
                    MessageBox.Show(
                        "Arayüz hazır.\n\n" +
                        "SQL Server bağlantısında gerçek fatura kaydı yapılacak.",
                        "Fatura",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                };

            buttons.Controls.Add(
                btnKaydet);

            container.Controls.Add(
                buttons);

            BosKalemEkle();

            // ---------------------------------------------------------
            // RESPONSIVE YERLEŞİM
            // ---------------------------------------------------------
            // İçerik ve alan adları değişmeden yalnızca genişlikleri,
            // ikinci kolon konumlarını ve butonları ekran boyutuna göre
            // yeniden hizalar.
            EventHandler layoutGuncelle =
                delegate
                {
                    int kullanilabilir =
                        Math.Max(
                            520,
                            container.ClientSize.Width - 40);

                    int kartGenislik =
                        kullanilabilir;

                    favoriCard.Width =
                        kartGenislik;

                    aliciCard.Width =
                        kartGenislik;

                    faturaCard.Width =
                        kartGenislik;

                    kalemCard.Width =
                        kartGenislik;

                    buttons.Width =
                        kartGenislik;

                    int icBosluk =
                        18;

                    int kolonArasi =
                        22;

                    int kolonGenisligi =
                        Math.Max(
                            205,
                            (kartGenislik -
                             (icBosluk * 2) -
                             kolonArasi) / 2);

                    int ikinciKolon =
                        icBosluk +
                        kolonGenisligi +
                        kolonArasi;

                    // Favoriler
                    pnlFavoriCariler.Width =
                        Math.Max(
                            200,
                            kartGenislik - 36);

                    pnlFavoriUrunler.Width =
                        Math.Max(
                            200,
                            kartGenislik - 36);

                    // Alıcı Bilgileri
                    cmbAliciTipi.Left =
                        icBosluk;

                    cmbAliciTipi.Width =
                        kolonGenisligi;

                    txtAlici.Left =
                        ikinciKolon;

                    txtAlici.Width =
                        kolonGenisligi;

                    txtKimlikNo.Left =
                        icBosluk;

                    txtKimlikNo.Width =
                        kolonGenisligi;

                    btnCariSec.Left =
                        ikinciKolon;

                    btnCariSec.Width =
                        Math.Min(
                            170,
                            kolonGenisligi);

                    // Fatura Bilgileri
                    txtFaturaNo.Left =
                        icBosluk;

                    txtFaturaNo.Width =
                        kolonGenisligi;

                    dtpTarih.Left =
                        ikinciKolon;

                    dtpTarih.Width =
                        kolonGenisligi;

                    cmbSenaryo.Left =
                        icBosluk;

                    cmbSenaryo.Width =
                        kolonGenisligi;

                    cmbFaturaTipi.Left =
                        ikinciKolon;

                    cmbFaturaTipi.Width =
                        kolonGenisligi;

                    cmbParaBirimi.Left =
                        icBosluk;

                    cmbParaBirimi.Width =
                        kolonGenisligi;

                    // Etiketlerin ikinci kolon başlangıcını düzelt
                    foreach (Control control in aliciCard.Controls)
                    {
                        Label label =
                            control as Label;

                        if (label != null &&
                            label.Text == "Cari / Alıcı")
                        {
                            label.Left =
                                ikinciKolon;
                        }
                    }

                    foreach (Control control in faturaCard.Controls)
                    {
                        Label label =
                            control as Label;

                        if (label == null)
                            continue;

                        if (label.Text == "Tarih" ||
                            label.Text == "Fatura Tipi")
                        {
                            label.Left =
                                ikinciKolon;
                        }
                    }

                    // Kalemler
                    btnKalemSil.Left =
                        kartGenislik -
                        btnKalemSil.Width -
                        18;

                    btnKalemEkle.Left =
                        btnKalemSil.Left -
                        btnKalemEkle.Width -
                        10;

                    dgvKalemler.Width =
                        Math.Max(
                            320,
                            kartGenislik - 36);

                    // Alt butonları sağa hizala
                    btnKaydet.Left =
                        kartGenislik -
                        btnKaydet.Width -
                        18;

                    btnTaslak.Left =
                        btnKaydet.Left -
                        btnTaslak.Width -
                        12;
                };

            container.Resize +=
                layoutGuncelle;

            // Kontrol ilk oluştuğunda da uygula.
            layoutGuncelle(
                container,
                EventArgs.Empty);

            return container;
        }

        // =========================================================
        // FAVORİLERİ YÜKLE
        // =========================================================

        private void FavorileriYukle()
        {
            if (pnlFavoriCariler == null ||
                pnlFavoriUrunler == null)
            {
                return;
            }

            pnlFavoriCariler.Controls.Clear();
            pnlFavoriUrunler.Controls.Clear();

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                if (!cari.Favori)
                    continue;

                Button button =
                    MiniFavoriButton(
                        "★ " +
                        cari.CariAdi);

                button.Tag =
                    cari;

                button.Click +=
                    FavoriCari_Click;

                pnlFavoriCariler.Controls.Add(
                    button);
            }

            foreach (
                UrunKaydi urun
                in AppData.Urunler)
            {
                if (!urun.Favori)
                    continue;

                Button button =
                    MiniFavoriButton(
                        "★ " +
                        urun.UrunAdi);

                button.Tag =
                    urun;

                button.Click +=
                    FavoriUrun_Click;

                pnlFavoriUrunler.Controls.Add(
                    button);
            }
        }

        private void FavoriCari_Click(
            object sender,
            EventArgs e)
        {
            Button button =
                sender as Button;

            if (button == null)
                return;

            CariKaydi cari =
                button.Tag as CariKaydi;

            if (cari == null)
                return;

            txtAlici.Text =
                cari.CariAdi;

            txtKimlikNo.Text =
                cari.KimlikNo;

            if (!string.IsNullOrWhiteSpace(
                cari.FirmaAdi))
            {
                cmbAliciTipi.SelectedIndex =
                    1;
            }
            else
            {
                cmbAliciTipi.SelectedIndex =
                    0;
            }

            OnizlemeyiGuncelle();
        }

        private void FavoriUrun_Click(
            object sender,
            EventArgs e)
        {
            Button button =
                sender as Button;

            if (button == null)
                return;

            UrunKaydi urun =
                button.Tag as UrunKaydi;

            if (urun == null)
                return;

            UrunKalemiEkle(
                urun);
        }

        // =========================================================
        // KALEM GRID
        // =========================================================

        private DataGridView KalemGridOlustur()
        {
            DataGridView grid =
                new DataGridView();

            grid.BackgroundColor =
                Color.White;

            grid.BorderStyle =
                BorderStyle.FixedSingle;

            grid.RowHeadersVisible =
                false;

            grid.AllowUserToAddRows =
                false;

            grid.AllowUserToDeleteRows =
                false;

            grid.MultiSelect =
                false;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.EditMode =
                DataGridViewEditMode.EditOnEnter;

            grid.ColumnHeadersHeight =
                42;

            grid.RowTemplate.Height =
                32;

            grid.EnableHeadersVisualStyles =
                false;

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(240, 244, 248);

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(50, 62, 78);

            grid.Columns.Add(
                "StokKodu",
                "Kod");

            grid.Columns.Add(
                "Urun",
                "Ürün");

            grid.Columns.Add(
                "Miktar",
                "Miktar");

            grid.Columns.Add(
                "BirimFiyat",
                "Fiyat");

            DataGridViewComboBoxColumn kdv =
                new DataGridViewComboBoxColumn();

            kdv.Name =
                "Kdv";

            kdv.HeaderText =
                "KDV";

            kdv.Items.Add("0");
            kdv.Items.Add("1");
            kdv.Items.Add("10");
            kdv.Items.Add("20");

            grid.Columns.Add(
                kdv);

            grid.Columns.Add(
                "Toplam",
                "Toplam");

            grid.Columns["Toplam"].ReadOnly =
                true;

            grid.CellValueChanged +=
                delegate (
                    object sender,
                    DataGridViewCellEventArgs e)
                {
                    if (hesaplamaYapiliyor)
                        return;

                    if (e.RowIndex < 0)
                        return;

                    SatirHesapla(
                        e.RowIndex);

                    OnizlemeyiGuncelle();
                };

            grid.CurrentCellDirtyStateChanged +=
                delegate
                {
                    if (grid.IsCurrentCellDirty)
                    {
                        grid.CommitEdit(
                            DataGridViewDataErrorContexts.Commit);
                    }
                };

            grid.DataError +=
                delegate
                {
                };

            return grid;
        }

        private void BosKalemEkle()
        {
            if (dgvKalemler == null)
                return;

            int index =
                dgvKalemler.Rows.Add();

            DataGridViewRow row =
                dgvKalemler.Rows[index];

            row.Cells["Miktar"].Value =
                "1";

            row.Cells["BirimFiyat"].Value =
                "0";

            row.Cells["Kdv"].Value =
                "20";

            row.Cells["Toplam"].Value =
                "0,00";

            OnizlemeyiGuncelle();
        }

        private void UrunKalemiEkle(
            UrunKaydi urun)
        {
            int index =
                dgvKalemler.Rows.Add();

            DataGridViewRow row =
                dgvKalemler.Rows[index];

            row.Cells["StokKodu"].Value =
                urun.StokKodu;

            row.Cells["Urun"].Value =
                urun.UrunAdi;

            row.Cells["Miktar"].Value =
                "1";

            row.Cells["BirimFiyat"].Value =
                urun.BirimFiyat.ToString("0.00");

            row.Cells["Kdv"].Value =
                urun.KdvOrani.ToString("0");

            SatirHesapla(
                index);

            OnizlemeyiGuncelle();
        }

        private void SatirHesapla(
            int index)
        {
            if (index < 0 ||
                index >= dgvKalemler.Rows.Count)
            {
                return;
            }

            hesaplamaYapiliyor =
                true;

            try
            {
                DataGridViewRow row =
                    dgvKalemler.Rows[index];

                decimal miktar =
                    DecimalDeger(
                        row.Cells["Miktar"].Value);

                decimal fiyat =
                    DecimalDeger(
                        row.Cells["BirimFiyat"].Value);

                decimal kdv =
                    DecimalDeger(
                        row.Cells["Kdv"].Value);

                decimal ara =
                    miktar * fiyat;

                decimal kdvTutar =
                    ara * kdv / 100m;

                decimal toplam =
                    ara + kdvTutar;

                row.Cells["Toplam"].Value =
                    toplam.ToString("N2");
            }
            finally
            {
                hesaplamaYapiliyor =
                    false;
            }
        }

        // =========================================================
        // SAĞ ÖNİZLEME
        // =========================================================

        private Panel SagTarafiOlustur()
        {
            Panel background =
                new Panel();

            sagBackground =
                background;

            background.Dock =
                DockStyle.Fill;

            background.AutoScroll =
                true;

            background.BackColor =
                Color.FromArgb(225, 230, 236);

            Panel paper =
                new Panel();

            previewPaper =
                paper;

            paper.Location =
                new Point(20, 20);

            paper.Size =
                new Size(760, 900);

            paper.BackColor =
                Color.White;

            paper.BorderStyle =
                BorderStyle.FixedSingle;

            background.Controls.Add(
                paper);

            // Önizleme paneli ekran genişliğine göre daralır; sağ kenarı kesilmez.
            // İç kontroller oluşturulduktan sonra Shown/Resize sırasında hizalanır.
            background.Resize += delegate { OnizlemeYerlesiminiGuncelle(); };

            Label title =
                new Label();

            title.Text =
                "e-FATURA";

            title.AutoSize =
                true;

            title.Font =
                new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.FromArgb(29, 73, 105);

            title.Location =
                new Point(35, 30);

            paper.Controls.Add(
                title);

            Label firma =
                new Label();

            firma.Text =
                "ŞİRKET ÜNVANI";

            firma.Size =
                new Size(300, 32);

            firma.TextAlign =
                ContentAlignment.MiddleRight;

            firma.Font =
                new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold);

            firma.Location =
                new Point(420, 28);

            paper.Controls.Add(
                firma);

            Label firmaBilgi =
                new Label();

            firmaBilgi.Text =
                "Firma adresi\n" +
                "Telefon / E-posta\n" +
                "Vergi Dairesi / Vergi No";

            firmaBilgi.Size =
                new Size(300, 68);

            firmaBilgi.Font = new Font("Segoe UI", 8F, FontStyle.Regular);

            firmaBilgi.TextAlign =
                ContentAlignment.TopRight;

            firmaBilgi.ForeColor =
                Color.Gray;

            firmaBilgi.Location =
                new Point(420, 72);

            paper.Controls.Add(
                firmaBilgi);

            paper.Controls.Add(
                Cizgi(
                    30,
                    148,
                    700));

            Label sayin =
                new Label();

            sayin.Text =
                "SAYIN";

            sayin.AutoSize =
                true;

            sayin.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            sayin.ForeColor =
                Color.FromArgb(49, 91, 121);

            sayin.Location =
                new Point(35, 165);

            paper.Controls.Add(
                sayin);

            prvAlici =
                new Label();

            prvAlici.Text =
                "-";

            prvAlici.Size =
                new Size(380, 28);

            prvAlici.Font =
                new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold);

            prvAlici.Location =
                new Point(35, 192);

            paper.Controls.Add(
                prvAlici);

            prvKimlik =
                new Label();

            prvKimlik.Size =
                new Size(380, 25);

            prvKimlik.Location =
                new Point(35, 225);

            paper.Controls.Add(
                prvKimlik);

            Panel bilgi =
                new Panel();

            bilgi.Location =
                new Point(440, 170);

            bilgi.Size =
                new Size(280, 120);

            bilgi.BackColor =
                Color.FromArgb(247, 249, 251);

            prvFaturaNo =
                PreviewLabel(10, 5);

            prvTarih =
                PreviewLabel(10, 33);

            prvSenaryo =
                PreviewLabel(10, 61);

            prvTip =
                PreviewLabel(10, 89);

            bilgi.Controls.Add(
                prvFaturaNo);

            bilgi.Controls.Add(
                prvTarih);

            bilgi.Controls.Add(
                prvSenaryo);

            bilgi.Controls.Add(
                prvTip);

            paper.Controls.Add(
                bilgi);

            paper.Controls.Add(
                Cizgi(
                    30,
                    325,
                    700));

            dgvOnizleme =
                new DataGridView();

            dgvOnizleme.Location =
                new Point(30, 348);

            dgvOnizleme.Size =
                new Size(700, 282);

            dgvOnizleme.BackgroundColor =
                Color.White;

            dgvOnizleme.BorderStyle =
                BorderStyle.None;

            dgvOnizleme.RowHeadersVisible =
                false;

            dgvOnizleme.AllowUserToAddRows =
                false;

            dgvOnizleme.ReadOnly =
                true;

            dgvOnizleme.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvOnizleme.EnableHeadersVisualStyles =
                false;

            dgvOnizleme.ColumnHeadersHeight =
                38;

            dgvOnizleme.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(67, 108, 137);

            dgvOnizleme.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvOnizleme.Columns.Add(
                "Urun",
                "Ürün / Hizmet");

            dgvOnizleme.Columns.Add(
                "Miktar",
                "Miktar");

            dgvOnizleme.Columns.Add(
                "Fiyat",
                "Birim Fiyat");

            dgvOnizleme.Columns.Add(
                "Kdv",
                "KDV");

            dgvOnizleme.Columns.Add(
                "Toplam",
                "Toplam");

            dgvOnizleme.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 8.5F, FontStyle.Regular);
            dgvOnizleme.ColumnHeadersDefaultCellStyle.WrapMode =
                DataGridViewTriState.False;
            dgvOnizleme.Columns["Urun"].FillWeight = 25F;
            dgvOnizleme.Columns["Miktar"].FillWeight = 15F;
            dgvOnizleme.Columns["Fiyat"].FillWeight = 22F;
            dgvOnizleme.Columns["Kdv"].FillWeight = 13F;
            dgvOnizleme.Columns["Toplam"].FillWeight = 25F;

            paper.Controls.Add(
                dgvOnizleme);

            Panel toplam =
                new Panel();

            toplam.Location =
                new Point(385, 650);

            toplam.Size =
                new Size(345, 165);

            toplam.BackColor =
                Color.FromArgb(247, 249, 251);

            prvAraToplam =
                ToplamLabel(
                    10,
                    15);

            prvKdv =
                ToplamLabel(
                    10,
                    59);

            prvGenelToplam =
                ToplamLabel(
                    10,
                    100);

            prvGenelToplam.Font =
                new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold);

            prvGenelToplam.ForeColor =
                Color.FromArgb(24, 75, 110);

            toplam.Controls.Add(
                prvAraToplam);

            toplam.Controls.Add(
                prvKdv);

            toplam.Controls.Add(
                prvGenelToplam);

            paper.Controls.Add(
                toplam);

            background.HandleCreated += delegate
            {
                BeginInvoke(new Action(OnizlemeYerlesiminiGuncelle));
            };
            return background;
        }

        // Önizleme kâğıdının yatay taşmasını engeller. Veri alanları değişmez.
        private void OnizlemeYerlesiminiGuncelle()
        {
            if (sagBackground == null || previewPaper == null || previewPaper.IsDisposed)
                return;
            Panel paper = previewPaper;
            int width = Math.Max(420, Math.Min(760, sagBackground.ClientSize.Width - 36));
            paper.Width = width;
            paper.Left = Math.Max(12, (sagBackground.ClientSize.Width - width) / 2);
            int inner = width - 60;
            foreach (Control control in paper.Controls)
            {
                if (control is DataGridView)
                {
                    control.Left = 30;
                    control.Width = inner;
                }
                else if (control is Panel && control.Height == 1)
                {
                    control.Left = 30;
                    control.Width = inner;
                }
            }
            // Üst şirket bilgileri ve sağ fatura bilgi kutusu
            // dar ekranlarda sol alıcı bilgileriyle çakışmaz.
            foreach (Control control in paper.Controls)
            {
                Label label = control as Label;
                if (label != null && label.Text == "ŞİRKET ÜNVANI")
                {
                    label.Width = Math.Max(145, (width - 85) / 2);
                    label.Left = width - label.Width - 25;
                    label.AutoEllipsis = true;
                }
                else if (label != null && label.Text.StartsWith("Firma adresi"))
                {
                    label.Width = Math.Max(145, (width - 85) / 2);
                    label.Left = width - label.Width - 25;
                    label.AutoEllipsis = true;
                }
                else if (control is Panel && (control.Height == 120 || control.Height == 115 || control.Height == 125 || control.Height == 148))
                {
                    control.Width = Math.Max(175, (width - 85) / 2);
                    control.Left = width - control.Width - 25;
                    control.Top = 170;
                    control.Height = 120;
                    foreach (Control child in control.Controls)
                        child.Width = control.Width - 18;
                }
                else if (control is Panel && control.Height == 165)
                {
                    control.Width = Math.Min(400, inner);
                    control.Left = width - control.Width - 30;
                    foreach (Control child in control.Controls)
                        child.Width = control.Width - 20;
                }
            }
            if (prvAlici != null) { prvAlici.Width = Math.Max(145, (width - 95) / 2); prvAlici.AutoEllipsis = true; }
            if (prvKimlik != null) { prvKimlik.Width = Math.Max(160, (width - 95) / 2); prvKimlik.AutoEllipsis = true; }
        }

        // =========================================================
        // CANLI ÖNİZLEME
        // =========================================================

        private void VeriDegisti(
            object sender,
            EventArgs e)
        {
            OnizlemeyiGuncelle();
        }

        private void KimlikBasliginiDegistir()
        {
            if (cmbAliciTipi.SelectedIndex == 0)
            {
                lblKimlik.Text =
                    "T.C. Kimlik No";
            }
            else
            {
                lblKimlik.Text =
                    "Vergi No";
            }
        }

        private void OnizlemeyiGuncelle()
        {
            if (prvAlici == null ||
                dgvOnizleme == null)
            {
                return;
            }

            string alici =
                txtAlici.Text.Trim();

            if (alici == "")
                alici = "-";

            prvAlici.Text =
                alici;

            string kimlikBaslik =
                cmbAliciTipi.SelectedIndex == 0
                ? "T.C. Kimlik No"
                : "Vergi No";

            string kimlik =
                txtKimlikNo.Text.Trim();

            if (kimlik == "")
                kimlik = "-";

            prvKimlik.Text =
                kimlikBaslik +
                ": " +
                kimlik;

            prvFaturaNo.Text =
                "Fatura No : " +
                txtFaturaNo.Text;

            prvTarih.Text =
                "Tarih : " +
                dtpTarih.Value.ToShortDateString();

            prvSenaryo.Text =
                "Senaryo : " +
                cmbSenaryo.Text;

            prvTip.Text =
                "Fatura Tipi : " +
                cmbFaturaTipi.Text;

            dgvOnizleme.Rows.Clear();

            decimal araToplam = 0;
            decimal toplamKdv = 0;
            decimal genelToplam = 0;

            foreach (
                DataGridViewRow row
                in dgvKalemler.Rows)
            {
                string urun =
                    Deger(
                        row,
                        "Urun");

                decimal miktar =
                    DecimalDeger(
                        row.Cells["Miktar"].Value);

                decimal fiyat =
                    DecimalDeger(
                        row.Cells["BirimFiyat"].Value);

                decimal kdv =
                    DecimalDeger(
                        row.Cells["Kdv"].Value);

                decimal ara =
                    miktar * fiyat;

                decimal kdvTutar =
                    ara * kdv / 100m;

                decimal toplam =
                    ara + kdvTutar;

                if (urun != "" ||
                    fiyat > 0)
                {
                    dgvOnizleme.Rows.Add(
                        urun,
                        miktar.ToString("N2"),
                        fiyat.ToString("N2"),
                        "%" + kdv.ToString("N0"),
                        toplam.ToString("N2"));
                }

                araToplam +=
                    ara;

                toplamKdv +=
                    kdvTutar;

                genelToplam +=
                    toplam;
            }

            string para =
                ParaBirimi();

            prvAraToplam.Text =
                "Mal / Hizmet Toplamı : " +
                araToplam.ToString("N2") +
                " " +
                para;

            prvKdv.Text =
                "KDV Toplamı : " +
                toplamKdv.ToString("N2") +
                " " +
                para;

            prvGenelToplam.Text =
                "GENEL TOPLAM : " +
                genelToplam.ToString("N2") +
                " " +
                para;
        }

        private string ParaBirimi()
        {
            if (cmbParaBirimi.SelectedIndex == 1)
                return "USD";

            if (cmbParaBirimi.SelectedIndex == 2)
                return "EUR";

            return "TL";
        }

        // =========================================================
        // TEMA
        // =========================================================

        private void TemayiUygula()
        {
            if (btnTema != null)
            {
                btnTema.Text =
                    koyuMod
                    ? "☀  Açık mod"
                    : "☾  Koyu mod";
            }

            Color formBack =
                koyuMod
                ? Color.FromArgb(16, 29, 46)
                : Color.FromArgb(242, 245, 249);

            Color solBack =
                koyuMod
                ? Color.FromArgb(20, 36, 57)
                : Color.FromArgb(244, 247, 250);

            Color kartBack =
                koyuMod
                ? Color.FromArgb(27, 47, 72)
                : Color.White;

            Color anaYazi =
                koyuMod
                ? Color.FromArgb(238, 244, 250)
                : Color.FromArgb(40, 53, 71);

            Color ikincilYazi =
                koyuMod
                ? Color.FromArgb(180, 197, 217)
                : Color.FromArgb(75, 85, 100);

            Color inputBack =
                koyuMod
                ? Color.FromArgb(22, 41, 64)
                : Color.White;

            Color inputYazi =
                koyuMod
                ? Color.White
                : Color.FromArgb(35, 48, 63);

            BackColor =
                formBack;

            if (headerPanel != null)
            {
                headerPanel.BackColor =
                    koyuMod
                    ? Color.FromArgb(8, 24, 43)
                    : Color.FromArgb(18, 38, 66);
            }

            if (solContainer != null)
            {
                solContainer.BackColor =
                    solBack;

                TemaKontrolleriniUygula(
                    solContainer,
                    kartBack,
                    anaYazi,
                    ikincilYazi,
                    inputBack,
                    inputYazi);
            }

            if (sagBackground != null)
            {
                sagBackground.BackColor =
                    koyuMod
                    ? Color.FromArgb(13, 27, 44)
                    : Color.FromArgb(225, 230, 236);
            }

            // e-Fatura kağıdı çıktı gibi göründüğü için daima beyaz kalır.
            if (previewPaper != null)
            {
                previewPaper.BackColor =
                    Color.White;
            }
        }

        private void TemaKontrolleriniUygula(
            Control parent,
            Color kartBack,
            Color anaYazi,
            Color ikincilYazi,
            Color inputBack,
            Color inputYazi)
        {
            foreach (Control control in parent.Controls)
            {
                Panel panel =
                    control as Panel;

                if (panel != null &&
                    panel != solContainer)
                {
                    if (panel.BorderStyle ==
                        BorderStyle.FixedSingle)
                    {
                        panel.BackColor =
                            kartBack;
                    }
                }

                Label label =
                    control as Label;

                if (label != null)
                {
                    label.ForeColor =
                        label.Font.Bold
                        ? anaYazi
                        : ikincilYazi;
                }

                TextBox textBox =
                    control as TextBox;

                if (textBox != null)
                {
                    textBox.BackColor =
                        inputBack;

                    textBox.ForeColor =
                        inputYazi;
                }

                ComboBox combo =
                    control as ComboBox;

                if (combo != null)
                {
                    combo.BackColor =
                        inputBack;

                    combo.ForeColor =
                        inputYazi;
                }

                DateTimePicker date =
                    control as DateTimePicker;

                if (date != null)
                {
                    date.CalendarMonthBackground =
                        inputBack;

                    date.CalendarForeColor =
                        inputYazi;
                }

                DataGridView grid =
                    control as DataGridView;

                if (grid != null)
                {
                    grid.BackgroundColor =
                        inputBack;

                    grid.DefaultCellStyle.BackColor =
                        inputBack;

                    grid.DefaultCellStyle.ForeColor =
                        inputYazi;

                    grid.DefaultCellStyle.SelectionBackColor =
                        koyuMod
                        ? Color.FromArgb(38, 78, 120)
                        : Color.FromArgb(220, 235, 250);

                    grid.DefaultCellStyle.SelectionForeColor =
                        inputYazi;
                }

                if (control.HasChildren)
                {
                    TemaKontrolleriniUygula(
                        control,
                        kartBack,
                        anaYazi,
                        ikincilYazi,
                        inputBack,
                        inputYazi);
                }
            }
        }

        // =========================================================
        // UI HELPERS
        // =========================================================

        private Panel KartOlustur()
        {
            Panel panel =
                new Panel();

            panel.BackColor =
                Color.White;

            panel.BorderStyle =
                BorderStyle.FixedSingle;

            return panel;
        }

        private Label Baslik(
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

            label.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(40, 53, 71);

            label.Location =
                new Point(x, y);

            return label;
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

            label.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(75, 85, 100);

            label.Location =
                new Point(x, y);

            return label;
        }

        private Button MaviButon(
            string text)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleCenter;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.BackColor =
                Color.FromArgb(35, 102, 215);

            button.ForeColor =
                Color.White;

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        private Button BeyazButon(
            string text)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleCenter;

            button.Size =
                new Size(150, 40);

            button.FlatStyle =
                FlatStyle.Flat;

            button.BackColor =
                Color.White;

            button.ForeColor =
                Color.FromArgb(35, 102, 215);

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        private Button MiniFavoriButton(
            string text)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.AutoSize = false;
            button.AutoEllipsis = true;
            button.Size = new Size(150, 36);
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(210, 218, 228);

            button.BackColor =
                Color.White;

            button.ForeColor =
                Color.FromArgb(45, 58, 75);

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        private Label PreviewLabel(
            int x,
            int y)
        {
            Label label =
                new Label();

            label.Location =
                new Point(x, y);

            label.Size =
                new Size(255, 26);

            label.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoEllipsis = true;

            return label;
        }

        private Label ToplamLabel(
            int x,
            int y)
        {
            Label label =
                new Label();

            label.Location =
                new Point(x, y);

            label.Size =
                new Size(320, 42);

            label.TextAlign =
                ContentAlignment.MiddleRight;

            return label;
        }

        private Panel Cizgi(
            int x,
            int y,
            int width)
        {
            Panel line =
                new Panel();

            line.Location =
                new Point(x, y);

            line.Size =
                new Size(width, 1);

            line.BackColor =
                Color.FromArgb(180, 188, 197);

            return line;
        }

        private decimal DecimalDeger(
            object value)
        {
            if (value == null)
                return 0;

            string text =
                value.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return 0;

            decimal result;

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            text =
                text.Replace(",", ".");

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result))
            {
                return result;
            }

            return 0;
        }

        private string Deger(
            DataGridViewRow row,
            string column)
        {
            object value =
                row.Cells[column].Value;

            if (value == null)
                return "";

            return value.ToString();
        }

        // Mevcut formun kontrollerine taslağın kaydedilmiş alanlarını yükler.
        public void TaslakYukle(string faturaNo)
        {
            TaslakBelgesi belge = TaslakDeposu.Bul(faturaNo);
            if (belge == null)
                throw new InvalidOperationException("Düzenlenebilir taslak verisi bulunamadı: " + faturaNo);

            duzenlenenTaslakNo = belge.FaturaNo;
            txtFaturaNo.Text = belge.FaturaNo;
            dtpTarih.Value = belge.Tarih >= dtpTarih.MinDate && belge.Tarih <= dtpTarih.MaxDate
                ? belge.Tarih : DateTime.Today;
            cmbAliciTipi.SelectedIndex = belge.AliciTipi == "Kurumsal Müşteri" ? 1 : 0;
            txtAlici.Text = belge.Alici ?? "";
            txtKimlikNo.Text = belge.KimlikNo ?? "";
            TaslakComboSec(cmbSenaryo, belge.Senaryo);
            TaslakComboSec(cmbFaturaTipi, belge.FaturaTipi);
            TaslakComboSec(cmbParaBirimi, belge.ParaBirimi);

            dgvKalemler.Rows.Clear();
            if (belge.Kalemler != null)
                foreach (TaslakKalemi kalem in belge.Kalemler)
                {
                    int i = dgvKalemler.Rows.Add();
                    DataGridViewRow row = dgvKalemler.Rows[i];
                    row.Cells["StokKodu"].Value = kalem.StokKodu;
                    row.Cells["Urun"].Value = kalem.Urun;
                    row.Cells["Miktar"].Value = kalem.Miktar;
                    row.Cells["BirimFiyat"].Value = kalem.BirimFiyat;
                    row.Cells["Kdv"].Value = kalem.Kdv;
                    SatirHesapla(i);
                }
            if (dgvKalemler.Rows.Count == 0) BosKalemEkle();
            OnizlemeyiGuncelle();
            Text = "Taslak Düzenle • " + belge.FaturaNo;
        }

        private void TaslakComboSec(ComboBox combo, string deger)
        {
            if (string.IsNullOrWhiteSpace(deger)) return;
            int index = combo.FindStringExact(deger);
            if (index >= 0) combo.SelectedIndex = index;
        }

        private void TaslagiKaydet()
        {
            try
            {
                // Var olan taslağın numarası değiştirilirse eski kaydın kaybolmasını önle.
                string no = txtFaturaNo.Text.Trim();
                if (string.IsNullOrWhiteSpace(no))
                {
                    MessageBox.Show("Fatura numarası boş olamaz.", "NEXORA");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(duzenlenenTaslakNo) &&
                    !string.Equals(no, duzenlenenTaslakNo, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Düzenlenen taslağın fatura numarasını değiştirmeyin.", "NEXORA");
                    return;
                }
                foreach (FaturaKaydi mevcut in AppData.Faturalar)
                {
                    if (string.Equals(mevcut.FaturaNo, no, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals((mevcut.Durum ?? "").Trim(), "Taslak", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Bu numara tamamlanmış bir faturada kullanılıyor. Başka numara girin.", "NEXORA");
                        return;
                    }
                }

                TaslakBelgesi belge = new TaslakBelgesi
                {
                    FaturaNo = no,
                    Tarih = dtpTarih.Value.Date,
                    AliciTipi = cmbAliciTipi.Text,
                    Alici = txtAlici.Text.Trim(),
                    KimlikNo = txtKimlikNo.Text.Trim(),
                    Senaryo = cmbSenaryo.Text,
                    FaturaTipi = cmbFaturaTipi.Text,
                    ParaBirimi = cmbParaBirimi.Text
                };
                decimal toplam = 0m;
                dgvKalemler.EndEdit();
                foreach (DataGridViewRow row in dgvKalemler.Rows)
                {
                    if (row.IsNewRow) continue;
                    string urun = Deger(row, "Urun");
                    string kod = Deger(row, "StokKodu");
                    if (string.IsNullOrWhiteSpace(urun) && string.IsNullOrWhiteSpace(kod)) continue;
                    decimal miktar = DecimalDeger(row.Cells["Miktar"].Value);
                    decimal fiyat = DecimalDeger(row.Cells["BirimFiyat"].Value);
                    decimal kdv = DecimalDeger(row.Cells["Kdv"].Value);
                    toplam += miktar * fiyat * (1m + kdv / 100m);
                    belge.Kalemler.Add(new TaslakKalemi
                    {
                        StokKodu = kod,
                        Urun = urun,
                        Miktar = Deger(row, "Miktar"),
                        BirimFiyat = Deger(row, "BirimFiyat"),
                        Kdv = Deger(row, "Kdv")
                    });
                }
                belge.GenelToplam = toplam;
                TaslakDeposu.Kaydet(belge);
                duzenlenenTaslakNo = no;

                FaturaKaydi kayit = null;
                foreach (FaturaKaydi item in AppData.Faturalar)
                    if (string.Equals(item.FaturaNo, no, StringComparison.OrdinalIgnoreCase))
                    { kayit = item; break; }
                if (kayit == null)
                {
                    kayit = new FaturaKaydi();
                    AppData.Faturalar.Add(kayit);
                }
                kayit.FaturaNo = no;
                kayit.Tarih = belge.Tarih;
                kayit.CariAdi = belge.Alici;
                kayit.CariTipi = belge.AliciTipi == "Kurumsal Müşteri" ? "Kurumsal" : "Bireysel";
                kayit.BelgeTipi = belge.Senaryo;
                kayit.GenelToplam = belge.GenelToplam;
                kayit.Durum = "Taslak";
                MessageBox.Show("Taslak kaydedildi. Taslaklar menüsünden düzenleyebilirsiniz.",
                    "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Taslak kaydedilemedi:\n" + ex.Message,
                    "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}