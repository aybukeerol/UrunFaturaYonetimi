using System;
using System.Drawing;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;

namespace UrunFaturaYonetimi
{
    public partial class MainForm : Form
    {
        private readonly string _kullaniciAdi;
        private readonly IUserService _userService;

        private Panel pnlMenu;
        private Panel pnlMain;
        private Panel pnlTop;
        private Panel pnlContent;

        private Label lblSayfaBaslik;
        private Label lblKullanici;

        private TextBox txtAkilliArama;
        private ListBox lstAramaSonuclari;

        // =========================================================
        // ARAMA SONUCU MODELİ
        // =========================================================

        private class AramaSonucu
        {
            public string Tur { get; set; }

            public string Gosterim { get; set; }

            public object Kayit { get; set; }

            public override string ToString()
            {
                return Gosterim;
            }
        }

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainForm(string kullaniciAdi, IUserService userService)
        {
            _kullaniciAdi = kullaniciAdi;
            _userService = userService;

            FormuHazirla();

            SolMenuOlustur();

            AnaAlanOlustur();

            UstPanelOlustur();

            IcerikPaneliOlustur();

            AramaSonucListesiniOlustur();

            DashboardAc();
        }

        // =========================================================
        // FORM
        // =========================================================

        private void FormuHazirla()
        {
            Text =
                "NEXORA - İşletme Yönetim Platformu";

            WindowState =
                FormWindowState.Maximized;

            StartPosition =
                FormStartPosition.CenterScreen;

            MinimumSize =
                new Size(1200, 760);

            BackColor =
                Color.FromArgb(
                    244,
                    247,
                    251);

            Font =
                new Font(
                    "Segoe UI",
                    10F);

            Click += delegate
            {
                AramaSonuclariniKapat();
            };
        }

        // =========================================================
        // SOL MENÜ
        // =========================================================

        private void SolMenuOlustur()
        {
            pnlMenu =
                new Panel();

            pnlMenu.Dock =
                DockStyle.Left;

            pnlMenu.Width =
                250;

            pnlMenu.BackColor =
                Color.FromArgb(
                    18,
                    38,
                    66);

            // -----------------------------------------------------
            // LOGO
            // -----------------------------------------------------

            Panel logoPanel =
                new Panel();

            logoPanel.Dock =
                DockStyle.Top;

            logoPanel.Height =
                105;

            logoPanel.BackColor =
                Color.FromArgb(
                    15,
                    33,
                    58);

            Label logo =
                new Label();

            logo.Text =
                "NEXORA";

            logo.AutoSize =
                true;

            logo.Font =
                new Font(
                    "Segoe UI",
                    21F,
                    FontStyle.Bold);

            logo.ForeColor =
                Color.White;

            logo.Location =
                new Point(
                    24,
                    20);

            logoPanel.Controls.Add(
                logo);

            Label logoSubtitle =
                new Label();

            logoSubtitle.Text =
                "İşletme Yönetim Platformu";

            logoSubtitle.AutoSize =
                true;

            logoSubtitle.Font =
                new Font(
                    "Segoe UI",
                    8.5F);

            logoSubtitle.ForeColor =
                Color.FromArgb(
                    145,
                    166,
                    193);

            logoSubtitle.Location =
                new Point(
                    25,
                    60);

            logoPanel.Controls.Add(
                logoSubtitle);

            // -----------------------------------------------------
            // MENÜ
            // -----------------------------------------------------

            FlowLayoutPanel menu =
                new FlowLayoutPanel();

            menu.Dock =
                DockStyle.Fill;

            menu.FlowDirection =
                FlowDirection.TopDown;

            menu.WrapContents =
                false;

            menu.AutoScroll =
                true;

            menu.Padding =
                new Padding(
                    0,
                    15,
                    0,
                    0);

            menu.BackColor =
                Color.FromArgb(
                    18,
                    38,
                    66);

            menu.Controls.Add(
                MenuButton(
                    "Genel Bakış",
                    Dashboard_Click));

            menu.Controls.Add(
                MenuButton(
                    "Müşteriler",
                    Musteriler_Click));

            menu.Controls.Add(
                MenuButton(
                    "Kurumsal Müşteriler",
                    Kurumsal_Click));

            menu.Controls.Add(
                MenuButton(
                    "Cari Hesaplar",
                    Cari_Click));

            menu.Controls.Add(
                MenuButton(
                    "Ürün / Hizmetler",
                    Urunler_Click));

            menu.Controls.Add(
                MenuButton(
                    "Yeni Fatura",
                    YeniFatura_Click));

            menu.Controls.Add(
                MenuButton(
                    "Faturalar",
                    Faturalar_Click));

            menu.Controls.Add(
                MenuButton(
                    "Kullanıcılar",
                    Kullanicilar_Click));

            menu.Controls.Add(
                MenuButton(
                    "Ayarlar",
                    Ayarlar_Click));

            pnlMenu.Controls.Add(
                menu);

            pnlMenu.Controls.Add(
                logoPanel);

            Controls.Add(
                pnlMenu);
        }

        private Button MenuButton(
            string text,
            EventHandler click)
        {
            Button button =
                new Button();

            button.Text =
                "   " + text;

            button.Size =
                new Size(
                    250,
                    50);

            button.Margin =
                new Padding(
                    0,
                    0,
                    0,
                    2);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.BackColor =
                Color.FromArgb(
                    18,
                    38,
                    66);

            button.ForeColor =
                Color.FromArgb(
                    220,
                    229,
                    240);

            button.Font =
                new Font(
                    "Segoe UI",
                    10F);

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.Cursor =
                Cursors.Hand;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(
                    31,
                    58,
                    91);

            button.Click += click;

            return button;
        }

        // =========================================================
        // ANA İÇ ALAN
        // =========================================================

        private void AnaAlanOlustur()
        {
            pnlMain = new Panel();
            pnlMain.BackColor = Color.FromArgb(244, 247, 251);
            pnlMain.Location = new Point(pnlMenu.Width, 0);
            pnlMain.Size = new Size(
                Math.Max(0, ClientSize.Width - pnlMenu.Width),
                ClientSize.Height);
            pnlMain.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            Controls.Add(pnlMain);

            // Sol menü her zaman kendi 250 px alanında kalır.
            // pnlMain artık menünün ALTINA girmez.
            pnlMenu.BringToFront();

            Resize += delegate
            {
                AnaAlaniBoyutlandir();
            };

            AnaAlaniBoyutlandir();
        }

        private void AnaAlaniBoyutlandir()
        {
            if (pnlMain == null || pnlMenu == null)
                return;

            pnlMain.Left = pnlMenu.Width;
            pnlMain.Top = 0;
            pnlMain.Width = Math.Max(0, ClientSize.Width - pnlMenu.Width);
            pnlMain.Height = ClientSize.Height;
        }

        // =========================================================
        // ÜST PANEL
        // =========================================================

        private void UstPanelOlustur()
        {
            pnlTop =
                new Panel();

            pnlTop.Dock =
                DockStyle.Top;

            pnlTop.Height =
                78;

            pnlTop.BackColor =
                Color.White;

            // -----------------------------------------------------
            // SAYFA BAŞLIĞI
            // -----------------------------------------------------

            lblSayfaBaslik =
                new Label();

            lblSayfaBaslik.Text =
                "Genel Bakış";

            lblSayfaBaslik.AutoSize =
                true;

            lblSayfaBaslik.Font =
                new Font(
                    "Segoe UI",
                    17F,
                    FontStyle.Bold);

            lblSayfaBaslik.ForeColor =
                Color.FromArgb(
                    34,
                    47,
                    64);

            lblSayfaBaslik.Location =
                new Point(
                    32,
                    22);

            pnlTop.Controls.Add(
                lblSayfaBaslik);

            // -----------------------------------------------------
            // ARAMA KUTUSU
            // "Akıllı Arama" yazısı YOK
            // -----------------------------------------------------

            txtAkilliArama =
                new TextBox();

            txtAkilliArama.Location =
                new Point(
                    330,
                    22);

            txtAkilliArama.Size =
                new Size(
                    440,
                    34);

            txtAkilliArama.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            txtAkilliArama.BorderStyle =
                BorderStyle.FixedSingle;

            txtAkilliArama.BackColor =
                Color.FromArgb(248, 250, 253);

            txtAkilliArama.ForeColor =
                Color.FromArgb(37, 50, 68);

            txtAkilliArama.TextChanged +=
                AkilliArama_TextChanged;

            txtAkilliArama.KeyDown +=
                AkilliArama_KeyDown;

            txtAkilliArama.Enter +=
                delegate
                {
                    if (!string.IsNullOrWhiteSpace(
                        txtAkilliArama.Text))
                    {
                        AkilliAramayiYap();
                    }
                };

            pnlTop.Controls.Add(
                txtAkilliArama);

            // -----------------------------------------------------
            // KULLANICI
            // -----------------------------------------------------

            lblKullanici =
                new Label();

            lblKullanici.Text =
                _kullaniciAdi +
                "  |  Çıkış";

            lblKullanici.AutoSize =
                true;

            lblKullanici.Font =
                new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Bold);

            lblKullanici.ForeColor =
                Color.FromArgb(
                    75,
                    86,
                    100);

            lblKullanici.Cursor =
                Cursors.Hand;

            lblKullanici.Click +=
                delegate
                {
                    Close();
                };

            pnlTop.Controls.Add(
                lblKullanici);

            pnlTop.Resize +=
                delegate
                {
                    lblKullanici.Left =
                        pnlTop.Width -
                        lblKullanici.Width -
                        35;

                    lblKullanici.Top =
                        29;

                    int aramaGenislik =
                        Math.Min(520, Math.Max(300, pnlTop.Width / 3));

                    txtAkilliArama.Width =
                        aramaGenislik;

                    txtAkilliArama.Left =
                        Math.Max(250, (pnlTop.Width - aramaGenislik) / 2);

                    AramaListesiKonumlandir();
                };

            pnlMain.Controls.Add(
                pnlTop);
        }

        // =========================================================
        // İÇERİK PANELİ
        // =========================================================

        private void IcerikPaneliOlustur()
        {
            pnlContent = new Panel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.AutoScroll = false;
            pnlContent.BackColor = Color.FromArgb(244, 247, 251);
            pnlContent.Padding = new Padding(0);

            pnlContent.Click += delegate
            {
                AramaSonuclariniKapat();
            };

            // İçerik artık doğrudan Form'a değil, sağdaki ana alana ekleniyor.
            // Böylece sol menünün altına girmesi fiziksel olarak mümkün değil.
            pnlMain.Controls.Add(pnlContent);

            // Üst bar 78 px alanını korusun.
            pnlTop.BringToFront();
        }

        // =========================================================
        // ARAMA SONUÇ KUTUSU
        // =========================================================

        private void AramaSonucListesiniOlustur()
        {
            lstAramaSonuclari =
                new ListBox();

            lstAramaSonuclari.Visible =
                false;

            lstAramaSonuclari.Size =
                new Size(
                    420,
                    210);

            lstAramaSonuclari.Font =
                new Font(
                    "Segoe UI",
                    10F);

            lstAramaSonuclari.BorderStyle =
                BorderStyle.FixedSingle;

            lstAramaSonuclari.BackColor =
                Color.FromArgb(255, 255, 255);

            lstAramaSonuclari.ForeColor =
                Color.FromArgb(
                    45,
                    55,
                    70);

            lstAramaSonuclari.IntegralHeight =
                false;

            lstAramaSonuclari.ItemHeight =
                28;

            lstAramaSonuclari.Click +=
                AramaSonucuSecildi;

            lstAramaSonuclari.DoubleClick +=
                AramaSonucuSecildi;

            pnlMain.Controls.Add(
                lstAramaSonuclari);

            AramaListesiKonumlandir();

            lstAramaSonuclari.BringToFront();
        }

        // =========================================================
        // SAYFA TEMİZLE
        // =========================================================

        private void SayfayiTemizle(
            string baslik)
        {
            pnlContent.Controls.Clear();

            lblSayfaBaslik.Text =
                baslik;

            AramaSonuclariniKapat();
        }

        // =========================================================
        // AKILLI ARAMA
        // =========================================================

        private void AkilliArama_TextChanged(
            object sender,
            EventArgs e)
        {
            AkilliAramayiYap();
        }

        private void AkilliAramayiYap()
        {
            if (txtAkilliArama == null ||
                lstAramaSonuclari == null)
            {
                return;
            }

            string aranan =
                txtAkilliArama.Text
                    .Trim()
                    .ToLower();

            lstAramaSonuclari.Items.Clear();

            if (aranan.Length == 0)
            {
                lstAramaSonuclari.Visible =
                    false;

                return;
            }

            // =====================================================
            // CARİLERDE ARA
            // =====================================================

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                string tumBilgi =
                    (
                        cari.CariKodu + " " +
                        cari.CariAdi + " " +
                        cari.Tip + " " +
                        cari.YetkiliKisi + " " +
                        cari.FirmaNo + " " +
                        cari.FirmaAdi + " " +
                        cari.KimlikNo + " " +
                        cari.Telefon + " " +
                        cari.Email + " " +
                        cari.Mahalle + " " +
                        cari.Sehir + " " +
                        cari.Ulke
                    ).ToLower();

                if (tumBilgi.Contains(
                    aranan))
                {
                    AramaSonucu sonuc =
                        new AramaSonucu();

                    sonuc.Tur =
                        "Cari";

                    sonuc.Kayit =
                        cari;

                    sonuc.Gosterim =
                        "CARİ   " +
                        (
                            cari.Favori
                            ? "★ "
                            : ""
                        ) +
                        cari.CariKodu +
                        "   " +
                        cari.CariAdi;

                    lstAramaSonuclari.Items.Add(
                        sonuc);
                }
            }

            // =====================================================
            // ÜRÜNLERDE ARA
            // =====================================================

            foreach (
                UrunKaydi urun
                in AppData.Urunler)
            {
                string tumBilgi =
                    (
                        urun.StokKodu + " " +
                        urun.UrunAdi + " " +
                        urun.Kategori + " " +
                        urun.Birim + " " +
                        urun.BirimFiyat.ToString() + " " +
                        urun.Stok.ToString()
                    ).ToLower();

                if (tumBilgi.Contains(
                    aranan))
                {
                    AramaSonucu sonuc =
                        new AramaSonucu();

                    sonuc.Tur =
                        "Urun";

                    sonuc.Kayit =
                        urun;

                    sonuc.Gosterim =
                        "ÜRÜN   " +
                        (
                            urun.Favori
                            ? "★ "
                            : ""
                        ) +
                        urun.StokKodu +
                        "   " +
                        urun.UrunAdi;

                    lstAramaSonuclari.Items.Add(
                        sonuc);
                }
            }

            // =====================================================
            // FATURALARDA ARA
            // =====================================================

            foreach (
                FaturaKaydi fatura
                in AppData.Faturalar)
            {
                string tumBilgi =
                    (
                        fatura.FaturaNo + " " +
                        fatura.CariAdi + " " +
                        fatura.CariTipi + " " +
                        fatura.BelgeTipi + " " +
                        fatura.Durum + " " +
                        fatura.GenelToplam.ToString()
                    ).ToLower();

                if (tumBilgi.Contains(
                    aranan))
                {
                    AramaSonucu sonuc =
                        new AramaSonucu();

                    sonuc.Tur =
                        "Fatura";

                    sonuc.Kayit =
                        fatura;

                    sonuc.Gosterim =
                        "FATURA   " +
                        fatura.FaturaNo +
                        "   " +
                        fatura.CariAdi;

                    lstAramaSonuclari.Items.Add(
                        sonuc);
                }
            }

            // =====================================================
            // SONUÇ YOKSA
            // =====================================================

            if (lstAramaSonuclari.Items.Count == 0)
            {
                lstAramaSonuclari.Items.Add(
                    "Sonuç bulunamadı");
            }

            // Sonuç sayısına göre yükseklik
            int sonucSayisi =
                lstAramaSonuclari.Items.Count;

            int yukseklik =
                sonucSayisi * 30 + 4;

            if (yukseklik > 220)
            {
                yukseklik =
                    220;
            }

            if (yukseklik < 45)
            {
                yukseklik =
                    45;
            }

            lstAramaSonuclari.Height =
                yukseklik;

            AramaListesiKonumlandir();

            lstAramaSonuclari.Visible =
                true;

            lstAramaSonuclari.BringToFront();

            // =====================================================
            // ÖNEMLİ:
            // ODAK ARAMA KUTUSUNDA KALIYOR
            // =====================================================

            if (!txtAkilliArama.Focused)
            {
                txtAkilliArama.Focus();
            }

            txtAkilliArama.SelectionStart =
                txtAkilliArama.Text.Length;

            txtAkilliArama.SelectionLength =
                0;
        }

        // =========================================================
        // ARAMA LİSTESİNİN KONUMU
        // =========================================================

        private void AramaListesiKonumlandir()
        {
            if (txtAkilliArama == null ||
                lstAramaSonuclari == null ||
                pnlMain == null)
            {
                return;
            }

            Point ekranNoktasi =
                txtAkilliArama.PointToScreen(
                    new Point(
                        0,
                        txtAkilliArama.Height));

            Point anaAlanNoktasi =
                pnlMain.PointToClient(
                    ekranNoktasi);

            lstAramaSonuclari.Left =
                anaAlanNoktasi.X;

            lstAramaSonuclari.Top =
                anaAlanNoktasi.Y + 2;

            lstAramaSonuclari.Width =
                txtAkilliArama.Width;

            lstAramaSonuclari.BringToFront();
        }

        // =========================================================
        // ARAMA KLAVYE KONTROLÜ
        // =========================================================

        private void AkilliArama_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (lstAramaSonuclari == null)
            {
                return;
            }

            // -----------------------------------------------------
            // ESC
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Escape)
            {
                lstAramaSonuclari.Visible =
                    false;

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (!lstAramaSonuclari.Visible)
            {
                return;
            }

            // -----------------------------------------------------
            // AŞAĞI OK
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Down)
            {
                if (lstAramaSonuclari.Items.Count >
                    0)
                {
                    int index =
                        lstAramaSonuclari.SelectedIndex;

                    index++;

                    if (index >=
                        lstAramaSonuclari.Items.Count)
                    {
                        index =
                            lstAramaSonuclari.Items.Count -
                            1;
                    }

                    lstAramaSonuclari.SelectedIndex =
                        index;
                }

                e.SuppressKeyPress =
                    true;
            }

            // -----------------------------------------------------
            // YUKARI OK
            // -----------------------------------------------------

            else if (e.KeyCode ==
                     Keys.Up)
            {
                if (lstAramaSonuclari.Items.Count >
                    0)
                {
                    int index =
                        lstAramaSonuclari.SelectedIndex;

                    index--;

                    if (index < 0)
                    {
                        index = 0;
                    }

                    lstAramaSonuclari.SelectedIndex =
                        index;
                }

                e.SuppressKeyPress =
                    true;
            }

            // -----------------------------------------------------
            // ENTER
            // -----------------------------------------------------

            else if (e.KeyCode ==
                     Keys.Enter)
            {
                if (lstAramaSonuclari.SelectedItem !=
                    null)
                {
                    AramaSonucunuAc(
                        lstAramaSonuclari.SelectedItem);
                }

                e.SuppressKeyPress =
                    true;
            }
        }

        // =========================================================
        // ARAMA SONUCU TIKLAMA
        // =========================================================

        private void AramaSonucuSecildi(
            object sender,
            EventArgs e)
        {
            if (lstAramaSonuclari.SelectedItem ==
                null)
            {
                return;
            }

            AramaSonucunuAc(
                lstAramaSonuclari.SelectedItem);
        }

        private void AramaSonucunuAc(
            object secilen)
        {
            AramaSonucu sonuc =
                secilen as AramaSonucu;

            if (sonuc == null)
            {
                return;
            }

            lstAramaSonuclari.Visible =
                false;

            // =====================================================
            // CARİ
            // =====================================================

            if (sonuc.Tur ==
                "Cari")
            {
                CariKaydi cari =
                    sonuc.Kayit
                    as CariKaydi;

                CariAc();

                if (cari != null)
                {
                    CariSatiriniSec(
                        cari.CariKodu);
                }
            }

            // =====================================================
            // ÜRÜN
            // =====================================================

            else if (sonuc.Tur ==
                     "Urun")
            {
                UrunKaydi urun =
                    sonuc.Kayit
                    as UrunKaydi;

                UrunlerAc();

                if (urun != null)
                {
                    UrunSatiriniSec(
                        urun.StokKodu);
                }
            }

            // =====================================================
            // FATURA
            // =====================================================

            else if (sonuc.Tur ==
                     "Fatura")
            {
                FaturaKaydi fatura =
                    sonuc.Kayit
                    as FaturaKaydi;

                FaturalarAc();

                if (fatura != null)
                {
                    FaturaSatiriniSec(
                        fatura.FaturaNo);
                }
            }

            txtAkilliArama.Clear();

            txtAkilliArama.Focus();
        }

        private void AramaSonuclariniKapat()
        {
            if (lstAramaSonuclari != null)
            {
                lstAramaSonuclari.Visible =
                    false;
            }
        }

        // =========================================================
        // ARAMA SONUCU GELİNCE SATIRI SEÇ
        // =========================================================

        private void CariSatiriniSec(
            string cariKodu)
        {
            foreach (
                Control control
                in pnlContent.Controls)
            {
                DataGridView grid =
                    control as DataGridView;

                if (grid == null)
                    continue;

                if (!grid.Columns.Contains(
                    "CariKodu"))
                {
                    continue;
                }

                foreach (
                    DataGridViewRow row
                    in grid.Rows)
                {
                    object value =
                        row.Cells[
                            "CariKodu"].Value;

                    if (value != null &&
                        value.ToString() ==
                        cariKodu)
                    {
                        row.Selected =
                            true;

                        grid.CurrentCell =
                            row.Cells["CariKodu"];

                        if (row.Index >= 0)
                        {
                            grid.FirstDisplayedScrollingRowIndex =
                                row.Index;
                        }

                        return;
                    }
                }
            }
        }

        private void UrunSatiriniSec(
            string stokKodu)
        {
            foreach (
                Control control
                in pnlContent.Controls)
            {
                DataGridView grid =
                    control as DataGridView;

                if (grid == null)
                    continue;

                if (!grid.Columns.Contains(
                    "StokKodu"))
                {
                    continue;
                }

                foreach (
                    DataGridViewRow row
                    in grid.Rows)
                {
                    object value =
                        row.Cells[
                            "StokKodu"].Value;

                    if (value != null &&
                        value.ToString() ==
                        stokKodu)
                    {
                        row.Selected =
                            true;

                        grid.CurrentCell =
                            row.Cells["StokKodu"];

                        grid.FirstDisplayedScrollingRowIndex =
                            row.Index;

                        return;
                    }
                }
            }
        }

        private void FaturaSatiriniSec(
            string faturaNo)
        {
            foreach (
                Control control
                in pnlContent.Controls)
            {
                DataGridView grid =
                    control as DataGridView;

                if (grid == null)
                    continue;

                if (!grid.Columns.Contains(
                    "FaturaNo"))
                {
                    continue;
                }

                foreach (
                    DataGridViewRow row
                    in grid.Rows)
                {
                    object value =
                        row.Cells[
                            "FaturaNo"].Value;

                    if (value != null &&
                        value.ToString() ==
                        faturaNo)
                    {
                        row.Selected =
                            true;

                        grid.CurrentCell =
                            row.Cells["FaturaNo"];

                        grid.FirstDisplayedScrollingRowIndex =
                            row.Index;

                        return;
                    }
                }
            }
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        private void DashboardAc()
        {
            SayfayiTemizle("Genel Bakış");

            // Ana dashboard kabı: sabit X/Y koordinatları yerine tamamen
            // Dock + TableLayoutPanel kullanıyoruz. Böylece DPI ve ekran
            // çözünürlüğü değiştiğinde başlıklar/kartlar birbirine girmez.
            Panel dashboardHost = new Panel();
            dashboardHost.Dock = DockStyle.Fill;
            dashboardHost.BackColor = Color.FromArgb(244, 247, 251);
            dashboardHost.Padding = new Padding(28, 22, 28, 24);
            pnlContent.Controls.Add(dashboardHost);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 7;
            layout.BackColor = Color.Transparent;
            layout.Margin = new Padding(0);
            layout.Padding = new Padding(0);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));   // karşılama
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));  // özet kartları
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));   // hızlı erişim başlığı
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));   // favoriler
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));   // son faturalar başlığı
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // grid
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));

            dashboardHost.Controls.Add(layout);

            // -----------------------------------------------------
            // KARŞILAMA
            // -----------------------------------------------------
            Panel welcomePanel = new Panel();
            welcomePanel.Dock = DockStyle.Fill;
            welcomePanel.BackColor = Color.Transparent;
            welcomePanel.Margin = new Padding(0);

            Label welcome = new Label();
            welcome.Text = "Hoş geldiniz, " + _kullaniciAdi;
            welcome.AutoSize = true;
            welcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            welcome.ForeColor = Color.FromArgb(24, 39, 61);
            welcome.Location = new Point(0, 0);
            welcomePanel.Controls.Add(welcome);

            Label description = new Label();
            description.Text = "İşletmenizin genel durumunu ve sık kullandığınız kayıtları tek ekrandan takip edin.";
            description.AutoSize = true;
            description.Font = new Font("Segoe UI", 10F);
            description.ForeColor = Color.FromArgb(101, 117, 139);
            description.Location = new Point(2, 45);
            welcomePanel.Controls.Add(description);

            layout.Controls.Add(welcomePanel, 0, 0);

            // -----------------------------------------------------
            // ÖZET KARTLARI
            // -----------------------------------------------------
            TableLayoutPanel cards = new TableLayoutPanel();
            cards.Dock = DockStyle.Fill;
            cards.ColumnCount = 4;
            cards.RowCount = 1;
            cards.BackColor = Color.Transparent;
            cards.Margin = new Padding(0, 0, 0, 14);
            cards.Padding = new Padding(0);

            for (int i = 0; i < 4; i++)
            {
                cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            Panel card1 = DashboardCard(
                "Bugünkü Faturalar",
                AppData.Faturalar.Count.ToString(),
                0, 0);

            Panel card2 = DashboardCard(
                "Toplam Cari",
                AppData.Cariler.Count.ToString(),
                0, 0);

            Panel card3 = DashboardCard(
                "Toplam Ürün",
                AppData.Urunler.Count.ToString(),
                0, 0);

            Panel card4 = DashboardCard(
                "Favori Kayıtlar",
                FavoriSayisi().ToString(),
                0, 0);

            card1.Dock = DockStyle.Fill;
            card2.Dock = DockStyle.Fill;
            card3.Dock = DockStyle.Fill;
            card4.Dock = DockStyle.Fill;

            card1.Margin = new Padding(0, 0, 9, 0);
            card2.Margin = new Padding(3, 0, 6, 0);
            card3.Margin = new Padding(6, 0, 3, 0);
            card4.Margin = new Padding(9, 0, 0, 0);

            cards.Controls.Add(card1, 0, 0);
            cards.Controls.Add(card2, 1, 0);
            cards.Controls.Add(card3, 2, 0);
            cards.Controls.Add(card4, 3, 0);

            layout.Controls.Add(cards, 0, 1);

            // -----------------------------------------------------
            // HIZLI ERİŞİM BAŞLIĞI
            // -----------------------------------------------------
            Panel quickHeader = new Panel();
            quickHeader.Dock = DockStyle.Fill;
            quickHeader.BackColor = Color.Transparent;
            quickHeader.Margin = new Padding(0);

            Label lblFavoriler = new Label();
            lblFavoriler.Text = "Hızlı Erişim";
            lblFavoriler.AutoSize = true;
            lblFavoriler.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            lblFavoriler.ForeColor = Color.FromArgb(30, 46, 68);
            lblFavoriler.Location = new Point(0, 2);
            quickHeader.Controls.Add(lblFavoriler);

            Label favoriAciklama = new Label();
            favoriAciklama.Text = "Favori cari ve ürünlerinize hızlıca ulaşın.";
            favoriAciklama.AutoSize = true;
            favoriAciklama.Font = new Font("Segoe UI", 9F);
            favoriAciklama.ForeColor = Color.FromArgb(111, 124, 142);
            favoriAciklama.Location = new Point(2, 35);
            quickHeader.Controls.Add(favoriAciklama);

            layout.Controls.Add(quickHeader, 0, 2);

            // -----------------------------------------------------
            // FAVORİLER
            // -----------------------------------------------------
            FlowLayoutPanel favoriPanel = new FlowLayoutPanel();
            favoriPanel.Dock = DockStyle.Fill;
            favoriPanel.BackColor = Color.White;
            favoriPanel.Padding = new Padding(10);
            favoriPanel.Margin = new Padding(0, 0, 0, 8);
            favoriPanel.AutoScroll = true;
            favoriPanel.WrapContents = false;
            favoriPanel.FlowDirection = FlowDirection.LeftToRight;

            foreach (CariKaydi cari in AppData.Cariler)
            {
                if (!cari.Favori)
                    continue;

                Button btn = FavoriButton(
                    "★  Cari   " + cari.CariAdi);

                CariKaydi secilenCari = cari;

                btn.Click += delegate
                {
                    CariAc();
                    CariSatiriniSec(secilenCari.CariKodu);
                };

                favoriPanel.Controls.Add(btn);
            }

            foreach (UrunKaydi urun in AppData.Urunler)
            {
                if (!urun.Favori)
                    continue;

                Button btn = FavoriButton(
                    "★  Ürün   " + urun.UrunAdi);

                UrunKaydi secilenUrun = urun;

                btn.Click += delegate
                {
                    UrunlerAc();
                    UrunSatiriniSec(secilenUrun.StokKodu);
                };

                favoriPanel.Controls.Add(btn);
            }

            layout.Controls.Add(favoriPanel, 0, 3);

            // -----------------------------------------------------
            // SON FATURALAR BAŞLIĞI
            // -----------------------------------------------------
            Panel invoiceHeader = new Panel();
            invoiceHeader.Dock = DockStyle.Fill;
            invoiceHeader.BackColor = Color.Transparent;
            invoiceHeader.Margin = new Padding(0);

            Label son = new Label();
            son.Text = "Son Faturalar";
            son.AutoSize = true;
            son.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            son.ForeColor = Color.FromArgb(30, 46, 68);
            son.Location = new Point(0, 22);
            invoiceHeader.Controls.Add(son);

            layout.Controls.Add(invoiceHeader, 0, 4);

            // -----------------------------------------------------
            // FATURA GRID
            // -----------------------------------------------------
            DataGridView grid = TemelGrid();
            grid.Dock = DockStyle.Fill;
            grid.Margin = new Padding(0);

            KolonEkle(grid, "FaturaNo", "Fatura No");
            KolonEkle(grid, "Tarih", "Tarih");
            KolonEkle(grid, "Cari", "Cari");
            KolonEkle(grid, "Tip", "Cari Tipi");
            KolonEkle(grid, "Toplam", "Genel Toplam");
            KolonEkle(grid, "Durum", "Durum");

            foreach (FaturaKaydi fatura in AppData.Faturalar)
            {
                grid.Rows.Add(
                    fatura.FaturaNo,
                    fatura.Tarih.ToShortDateString(),
                    fatura.CariAdi,
                    fatura.CariTipi,
                    fatura.GenelToplam.ToString("N2") + " TL",
                    fatura.Durum);
            }

            layout.Controls.Add(grid, 0, 5);
        }

        private int FavoriSayisi()
        {
            int toplam = 0;

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                if (cari.Favori)
                {
                    toplam++;
                }
            }

            foreach (
                UrunKaydi urun
                in AppData.Urunler)
            {
                if (urun.Favori)
                {
                    toplam++;
                }
            }

            return toplam;
        }

        private Panel DashboardCard(
            string title,
            string value,
            int x,
            int y)
        {
            Panel panel = KartOlustur();
            panel.Location = new Point(x, y);
            panel.Size = new Size(245, 112);
            panel.Padding = new Padding(18);

            Panel accent = new Panel();
            accent.Dock = DockStyle.Left;
            accent.Width = 4;
            accent.BackColor = Color.FromArgb(47, 141, 255);
            panel.Controls.Add(accent);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            lblTitle.ForeColor = Color.FromArgb(105, 119, 138);
            lblTitle.Location = new Point(20, 18);

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.AutoSize = true;
            lblValue.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblValue.ForeColor = Color.FromArgb(22, 39, 63);
            lblValue.Location = new Point(18, 48);

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblValue);
            return panel;
        }

        private Button FavoriButton(
            string text)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(210, 58);
            button.Margin = new Padding(5);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(222, 231, 242);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 248, 255);
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(40, 57, 79);
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 8, 0);
            return button;
        }

        // =========================================================
        // BİREYSEL MÜŞTERİLER
        // =========================================================

        private void MusterilerAc()
        {
            SayfayiTemizle(
                "Müşteriler");

            Label baslik =
                SayfaListeBasligi(
                    "Bireysel Müşteriler");

            pnlContent.Controls.Add(
                baslik);

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    85);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(420, pnlContent.ClientSize.Height - 125));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            KolonEkle(
                grid,
                "Kod",
                "Cari Kodu");

            KolonEkle(
                grid,
                "Ad",
                "Ad Soyad");

            KolonEkle(
                grid,
                "Kimlik",
                "T.C. Kimlik No");

            KolonEkle(
                grid,
                "Telefon",
                "Telefon");

            KolonEkle(
                grid,
                "Email",
                "E-Posta");

            KolonEkle(
                grid,
                "Sehir",
                "Şehir");

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                if (cari.Tip !=
                    "Müşteri")
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(
                    cari.FirmaAdi))
                {
                    continue;
                }

                grid.Rows.Add(
                    cari.CariKodu,
                    cari.CariAdi,
                    cari.KimlikNo,
                    cari.Telefon,
                    cari.Email,
                    cari.Sehir);
            }

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // KURUMSAL MÜŞTERİLER
        // =========================================================

        private void KurumsalAc()
        {
            SayfayiTemizle(
                "Kurumsal Müşteriler");

            Label baslik =
                SayfaListeBasligi(
                    "Kurumsal Müşteriler");

            pnlContent.Controls.Add(
                baslik);

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    85);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(420, pnlContent.ClientSize.Height - 125));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            KolonEkle(
                grid,
                "Kod",
                "Cari Kodu");

            KolonEkle(
                grid,
                "FirmaAdi",
                "Firma Adı");

            KolonEkle(
                grid,
                "VergiNo",
                "Vergi No");

            KolonEkle(
                grid,
                "Yetkili",
                "Yetkili Kişi");

            KolonEkle(
                grid,
                "Telefon",
                "Telefon");

            KolonEkle(
                grid,
                "Email",
                "E-Posta");

            KolonEkle(
                grid,
                "Sehir",
                "Şehir");

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                if (string.IsNullOrWhiteSpace(
                    cari.FirmaAdi))
                {
                    continue;
                }

                grid.Rows.Add(
                    cari.CariKodu,
                    cari.FirmaAdi,
                    cari.KimlikNo,
                    cari.YetkiliKisi,
                    cari.Telefon,
                    cari.Email,
                    cari.Sehir);
            }

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // CARİ HESAPLAR
        // =========================================================

        private void CariAc()
        {
            SayfayiTemizle(
                "Cari Hesaplar");

            Label title =
                SayfaListeBasligi(
                    "Cari Hesap Listesi");

            pnlContent.Controls.Add(
                title);

            // -----------------------------------------------------
            // LİSTE ARAMA
            // -----------------------------------------------------

            Label lblAra =
                new Label();

            lblAra.Text =
                "Liste içinde ara";

            lblAra.AutoSize =
                true;

            lblAra.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            lblAra.ForeColor =
                Color.Gray;

            lblAra.Location =
                new Point(
                    28,
                    65);

            pnlContent.Controls.Add(
                lblAra);

            TextBox txtAra =
                new TextBox();

            txtAra.Location =
                new Point(
                    28,
                    88);

            txtAra.Size =
                new Size(
                    380,
                    30);

            pnlContent.Controls.Add(
                txtAra);

            // -----------------------------------------------------
            // YENİ CARİ
            // -----------------------------------------------------

            Button btnYeniCari =
                MaviButon(
                    "+ Yeni Cari");

            btnYeniCari.Location =
                new Point(
                    Math.Max(680, pnlContent.ClientSize.Width - 182),
                    82);

            btnYeniCari.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnYeniCari.Click +=
                delegate
                {
                    CariHesapForm form =
                        new CariHesapForm();

                    form.ShowDialog(
                        this);

                    CariAc();
                };

            pnlContent.Controls.Add(
                btnYeniCari);

            // -----------------------------------------------------
            // GRID
            // -----------------------------------------------------

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    140);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(400, pnlContent.ClientSize.Height - 180));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            grid.ScrollBars =
                ScrollBars.Both;

            grid.ReadOnly =
                true;

            // -----------------------------------------------------
            // FAVORİ
            // -----------------------------------------------------

            DataGridViewTextBoxColumn favoriColumn =
                new DataGridViewTextBoxColumn();

            favoriColumn.Name =
                "Favori";

            favoriColumn.HeaderText =
                "★";

            favoriColumn.Width =
                55;

            favoriColumn.SortMode =
                DataGridViewColumnSortMode.NotSortable;

            grid.Columns.Add(
                favoriColumn);

            // -----------------------------------------------------
            // DİĞER KOLONLAR
            // -----------------------------------------------------

            KolonEkle(
                grid,
                "CariKodu",
                "Cari Kodu");

            KolonEkle(
                grid,
                "Tip",
                "Tip");

            KolonEkle(
                grid,
                "CariAdi",
                "Müşteri / Tedarikçi Adı");

            KolonEkle(
                grid,
                "YetkiliKisi",
                "Yetkili Kişi");

            KolonEkle(
                grid,
                "FirmaNo",
                "Firma No");

            KolonEkle(
                grid,
                "FirmaAdi",
                "Firma Adı");

            KolonEkle(
                grid,
                "Telefon",
                "Telefon");

            KolonEkle(
                grid,
                "Email",
                "E-Posta");

            KolonEkle(
                grid,
                "Mahalle",
                "Mahalle");

            KolonEkle(
                grid,
                "Sehir",
                "Şehir");

            KolonEkle(
                grid,
                "Ulke",
                "Ülke");

            grid.Columns["CariKodu"].Width =
                115;

            grid.Columns["Tip"].Width =
                110;

            grid.Columns["CariAdi"].Width =
                200;

            grid.Columns["YetkiliKisi"].Width =
                160;

            grid.Columns["FirmaNo"].Width =
                110;

            grid.Columns["FirmaAdi"].Width =
                190;

            grid.Columns["Telefon"].Width =
                140;

            grid.Columns["Email"].Width =
                190;

            grid.Columns["Mahalle"].Width =
                165;

            grid.Columns["Sehir"].Width =
                110;

            grid.Columns["Ulke"].Width =
                110;

            // -----------------------------------------------------
            // VERİLER
            // -----------------------------------------------------

            foreach (
                CariKaydi cari
                in AppData.Cariler)
            {
                int index =
                    grid.Rows.Add(
                        cari.Favori
                            ? "★"
                            : "☆",
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

                grid.Rows[index].Tag =
                    cari;
            }

            // -----------------------------------------------------
            // FAVORİ TIKLAMA
            // -----------------------------------------------------

            grid.CellClick +=
                delegate (
                    object sender,
                    DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex < 0)
                    {
                        return;
                    }

                    if (e.ColumnIndex !=
                        grid.Columns[
                            "Favori"].Index)
                    {
                        return;
                    }

                    CariKaydi cari =
                        grid.Rows[
                            e.RowIndex]
                            .Tag
                        as CariKaydi;

                    if (cari == null)
                    {
                        return;
                    }

                    cari.Favori =
                        !cari.Favori;

                    grid.Rows[
                        e.RowIndex]
                        .Cells[
                            "Favori"]
                        .Value =
                        cari.Favori
                            ? "★"
                            : "☆";
                };

            // -----------------------------------------------------
            // FİLTRELE
            // -----------------------------------------------------

            txtAra.TextChanged +=
                delegate
                {
                    ListeFiltrele(
                        grid,
                        txtAra.Text);
                };

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // ÜRÜNLER
        // =========================================================

        private void UrunlerAc()
        {
            SayfayiTemizle(
                "Ürün / Hizmetler");

            Label title =
                SayfaListeBasligi(
                    "Ürün ve Hizmet Listesi");

            pnlContent.Controls.Add(
                title);

            Label lblAra =
                new Label();

            lblAra.Text =
                "Liste içinde ara";

            lblAra.AutoSize =
                true;

            lblAra.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            lblAra.ForeColor =
                Color.Gray;

            lblAra.Location =
                new Point(
                    28,
                    65);

            pnlContent.Controls.Add(
                lblAra);

            TextBox txtAra =
                new TextBox();

            txtAra.Location =
                new Point(
                    28,
                    88);

            txtAra.Size =
                new Size(
                    380,
                    30);

            pnlContent.Controls.Add(
                txtAra);

            Button btnYeniUrun =
                MaviButon(
                    "+ Yeni Ürün");

            btnYeniUrun.Location =
                new Point(
                    Math.Max(680, pnlContent.ClientSize.Width - 182),
                    82);

            btnYeniUrun.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            pnlContent.Controls.Add(
                btnYeniUrun);

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    140);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(400, pnlContent.ClientSize.Height - 180));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            DataGridViewTextBoxColumn favoriColumn =
                new DataGridViewTextBoxColumn();

            favoriColumn.Name =
                "Favori";

            favoriColumn.HeaderText =
                "★";

            favoriColumn.Width =
                55;

            favoriColumn.SortMode =
                DataGridViewColumnSortMode.NotSortable;

            grid.Columns.Add(
                favoriColumn);

            KolonEkle(
                grid,
                "StokKodu",
                "Stok Kodu");

            KolonEkle(
                grid,
                "Urun",
                "Ürün / Hizmet");

            KolonEkle(
                grid,
                "Kategori",
                "Kategori");

            KolonEkle(
                grid,
                "Birim",
                "Birim");

            KolonEkle(
                grid,
                "Fiyat",
                "Satış Fiyatı");

            KolonEkle(
                grid,
                "Kdv",
                "KDV");

            KolonEkle(
                grid,
                "Stok",
                "Stok");

            foreach (
                UrunKaydi urun
                in AppData.Urunler)
            {
                int index =
                    grid.Rows.Add(
                        urun.Favori
                            ? "★"
                            : "☆",
                        urun.StokKodu,
                        urun.UrunAdi,
                        urun.Kategori,
                        urun.Birim,
                        urun.BirimFiyat.ToString("N2") +
                        " TL",
                        "%" +
                        urun.KdvOrani.ToString("N0"),
                        urun.Stok);

                grid.Rows[index].Tag =
                    urun;
            }

            // -----------------------------------------------------
            // FAVORİ TIKLAMA
            // -----------------------------------------------------

            grid.CellClick +=
                delegate (
                    object sender,
                    DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex < 0)
                    {
                        return;
                    }

                    if (e.ColumnIndex !=
                        grid.Columns[
                            "Favori"].Index)
                    {
                        return;
                    }

                    UrunKaydi urun =
                        grid.Rows[
                            e.RowIndex]
                            .Tag
                        as UrunKaydi;

                    if (urun == null)
                    {
                        return;
                    }

                    urun.Favori =
                        !urun.Favori;

                    grid.Rows[
                        e.RowIndex]
                        .Cells[
                            "Favori"]
                        .Value =
                        urun.Favori
                            ? "★"
                            : "☆";
                };

            txtAra.TextChanged +=
                delegate
                {
                    ListeFiltrele(
                        grid,
                        txtAra.Text);
                };

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // YENİ FATURA
        // =========================================================

        private void YeniFaturaAc()
        {
            YeniFaturaForm faturaForm =
                new YeniFaturaForm();

            faturaForm.ShowDialog(
                this);
        }

        // =========================================================
        // FATURALAR
        // =========================================================

        private void FaturalarAc()
        {
            SayfayiTemizle(
                "Faturalar");

            Label title =
                SayfaListeBasligi(
                    "Fatura Listesi");

            pnlContent.Controls.Add(
                title);

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    85);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(420, pnlContent.ClientSize.Height - 125));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            KolonEkle(
                grid,
                "FaturaNo",
                "Fatura No");

            KolonEkle(
                grid,
                "Tarih",
                "Tarih");

            KolonEkle(
                grid,
                "Cari",
                "Cari");

            KolonEkle(
                grid,
                "Tip",
                "Cari Tipi");

            KolonEkle(
                grid,
                "BelgeTipi",
                "Belge Tipi");

            KolonEkle(
                grid,
                "Tutar",
                "Genel Toplam");

            KolonEkle(
                grid,
                "Durum",
                "Durum");

            foreach (
                FaturaKaydi fatura
                in AppData.Faturalar)
            {
                grid.Rows.Add(
                    fatura.FaturaNo,
                    fatura.Tarih.ToShortDateString(),
                    fatura.CariAdi,
                    fatura.CariTipi,
                    fatura.BelgeTipi,
                    fatura.GenelToplam.ToString("N2") +
                    " TL",
                    fatura.Durum);
            }

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // KULLANICILAR
        // =========================================================

        private void KullanicilarAc()
        {
            SayfayiTemizle(
                "Kullanıcılar");

            Label title =
                SayfaListeBasligi(
                    "Sistem Kullanıcıları");

            pnlContent.Controls.Add(
                title);

            Button btnYeni =
                MaviButon(
                    "+ Yeni Kullanıcı");

            btnYeni.Location =
                new Point(
                    Math.Max(680, pnlContent.ClientSize.Width - 182),
                    30);

            btnYeni.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnYeni.Click +=
                delegate
                {
                    RegisterForm form =
                        new RegisterForm(_userService);

                    form.ShowDialog(
                        this);
                };

            pnlContent.Controls.Add(
                btnYeni);

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    90);

            grid.Size =
                new Size(
                    Math.Max(800, pnlContent.ClientSize.Width - 64),
                    Math.Max(400, pnlContent.ClientSize.Height - 180));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            KolonEkle(
                grid,
                "AdSoyad",
                "Ad Soyad");

            KolonEkle(
                grid,
                "KullaniciAdi",
                "Kullanıcı Adı");

            KolonEkle(
                grid,
                "Rol",
                "Rol");

            KolonEkle(
                grid,
                "Durum",
                "Durum");

            grid.Rows.Add(
                "Sistem Yöneticisi",
                "admin",
                "Yönetici",
                "Aktif");

            pnlContent.Controls.Add(
                grid);
        }

        // =========================================================
        // AYARLAR
        // =========================================================

        private void AyarlarAc()
        {
            SayfayiTemizle(
                "Ayarlar");

            Panel card =
                KartOlustur();

            card.Location =
                new Point(
                    28,
                    25);

            card.Size =
                new Size(
                    Math.Min(980, Math.Max(700, pnlContent.ClientSize.Width - 64)),
                    345);

            Label title =
                new Label();

            title.Text =
                "Firma Bilgileri";

            title.AutoSize =
                true;

            title.Font =
                new Font(
                    "Segoe UI",
                    15F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.FromArgb(
                    35,
                    48,
                    65);

            title.Location =
                new Point(
                    20,
                    20);

            card.Controls.Add(
                title);

            // Firma Ünvanı
            card.Controls.Add(
                FormLabel(
                    "Firma Ünvanı",
                    20,
                    75));

            TextBox txtFirma =
                new TextBox();

            txtFirma.Location =
                new Point(
                    20,
                    100);

            txtFirma.Size =
                new Size(
                    500,
                    30);

            card.Controls.Add(
                txtFirma);

            // Vergi No
            card.Controls.Add(
                FormLabel(
                    "Vergi No",
                    20,
                    155));

            TextBox txtVergi =
                new TextBox();

            txtVergi.Location =
                new Point(
                    20,
                    180);

            txtVergi.Size =
                new Size(
                    235,
                    30);

            card.Controls.Add(
                txtVergi);

            // Vergi Dairesi
            card.Controls.Add(
                FormLabel(
                    "Vergi Dairesi",
                    280,
                    155));

            TextBox txtVergiDairesi =
                new TextBox();

            txtVergiDairesi.Location =
                new Point(
                    280,
                    180);

            txtVergiDairesi.Size =
                new Size(
                    240,
                    30);

            card.Controls.Add(
                txtVergiDairesi);

            // Adres
            card.Controls.Add(
                FormLabel(
                    "Adres",
                    20,
                    235));

            TextBox txtAdres =
                new TextBox();

            txtAdres.Location =
                new Point(
                    20,
                    260);

            txtAdres.Size =
                new Size(
                    500,
                    55);

            txtAdres.Multiline =
                true;

            card.Controls.Add(
                txtAdres);

            pnlContent.Controls.Add(
                card);
        }

        // =========================================================
        // LİSTE FİLTRELE
        // =========================================================

        private void ListeFiltrele(
            DataGridView grid,
            string aranan)
        {
            aranan =
                aranan
                    .Trim()
                    .ToLower();

            foreach (
                DataGridViewRow row
                in grid.Rows)
            {
                if (aranan == "")
                {
                    row.Visible =
                        true;

                    continue;
                }

                bool bulundu =
                    false;

                foreach (
                    DataGridViewCell cell
                    in row.Cells)
                {
                    if (cell.Value ==
                        null)
                    {
                        continue;
                    }

                    string value =
                        cell.Value
                            .ToString()
                            .ToLower();

                    if (value.Contains(
                        aranan))
                    {
                        bulundu =
                            true;

                        break;
                    }
                }

                try
                {
                    row.Visible =
                        bulundu;
                }
                catch
                {
                    // Seçili satır gizlenemiyorsa
                    // uygulama hata vermesin.
                }
            }
        }

        // =========================================================
        // UI YARDIMCI METOTLARI
        // =========================================================

        private Label SayfaListeBasligi(
            string text)
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
                    15F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    29,
                    45,
                    67);

            label.Location =
                new Point(
                    32,
                    24);

            return label;
        }

        private Button MaviButon(
            string text)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Size =
                new Size(
                    150,
                    38);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.BackColor =
                Color.FromArgb(
                    47,
                    126,
                    245);

            button.ForeColor =
                Color.White;

            button.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            button.Cursor =
                Cursors.Hand;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(38, 111, 226);

            button.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(31, 94, 197);

            return button;
        }

        private DataGridView TemelGrid()
        {
            DataGridView grid = new DataGridView();
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.ColumnHeadersHeight = 46;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 42;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(232, 237, 244);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 253);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 66, 86);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(246, 249, 253);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(49, 62, 80);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 242, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(23, 49, 82);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 252, 254);
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            return grid;
        }

        private void KolonEkle(
            DataGridView grid,
            string name,
            string header)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.Name =
                name;

            column.HeaderText =
                header;

            column.SortMode =
                DataGridViewColumnSortMode.Automatic;

            grid.Columns.Add(
                column);
        }

        private Panel KartOlustur()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
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
                    8.5F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    78,
                    89,
                    105);

            return label;
        }

        // =========================================================
        // MENÜ EVENTLERİ
        // =========================================================

        private void Dashboard_Click(
            object sender,
            EventArgs e)
        {
            DashboardAc();
        }

        private void Musteriler_Click(
            object sender,
            EventArgs e)
        {
            MusterilerAc();
        }

        private void Kurumsal_Click(
            object sender,
            EventArgs e)
        {
            KurumsalAc();
        }

        private void Cari_Click(
            object sender,
            EventArgs e)
        {
            CariAc();
        }

        private void Urunler_Click(
            object sender,
            EventArgs e)
        {
            UrunlerAc();
        }

        private void YeniFatura_Click(
            object sender,
            EventArgs e)
        {
            YeniFaturaAc();
        }

        private void Faturalar_Click(
            object sender,
            EventArgs e)
        {
            FaturalarAc();
        }

        private void Kullanicilar_Click(
            object sender,
            EventArgs e)
        {
            KullanicilarAc();
        }

        private void Ayarlar_Click(
            object sender,
            EventArgs e)
        {
            AyarlarAc();
        }
    }
}