using Microsoft.Data.SqlClient;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
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

        private bool hesaplamaYapiliyor =
            false;

        // SQL Server'a kaydedilmiş mevcut fatura. 0 ise henüz INSERT yapılmamıştır.
        private int kayitliFaturaId = 0;

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

            Panel header =
                UstBaslikOlustur();

            TableLayoutPanel anaLayout =
                new TableLayoutPanel();

            anaLayout.Dock =
                DockStyle.Fill;

            anaLayout.ColumnCount =
                2;

            anaLayout.RowCount =
                1;

            anaLayout.BackColor =
                Color.FromArgb(225, 230, 236);

            // Sol formu biraz daha geniş tutuyoruz.
            // Sağdaki e-Fatura önizlemesi aynen korunuyor.
            anaLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    54F));

            anaLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    46F));

            anaLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            Panel sol =
                SolTarafiOlustur();

            Panel sag =
                SagTarafiOlustur();

            anaLayout.Controls.Add(
                sol,
                0,
                0);

            anaLayout.Controls.Add(
                sag,
                1,
                0);

            Controls.Add(
                anaLayout);

            // Dock sırası: Fill önce, Top header sonra.
            // Böylece içerik başlığın altından başlar ve üst üste binmez.
            Controls.Add(
                header);

            header.BringToFront();

            Shown +=
                delegate
                {
                    FavorileriYukle();

                    OnizlemeyiGuncelle();

                    TemayiUygula();

                    // Form ilk açıldığında AutoScroll odaklanan kontrole kaymasın.
                    BeginInvoke(
                        new Action(
                            delegate
                            {
                                if (solContainer != null)
                                {
                                    solContainer.AutoScrollPosition =
                                        new Point(0, 0);
                                }

                                if (sagBackground != null)
                                {
                                    sagBackground.AutoScrollPosition =
                                        new Point(0, 0);
                                }

                                ActiveControl = null;
                            }));
                };
        }

        // =========================================================
        // HEADER
        // =========================================================

        private Panel UstBaslikOlustur()
        {
            Panel header = new Panel();
            headerPanel = header;
            header.Dock = DockStyle.Top;
            header.Height = 92;
            header.BackColor = Color.White;
            header.Padding = new Padding(18, 10, 18, 10);

            Label crumb = new Label();
            crumb.Text = "Faturalar   ›   Yeni Fatura Düzenle";
            crumb.AutoSize = true;
            crumb.Font = new Font("Segoe UI", 8.5F);
            crumb.ForeColor = Color.FromArgb(105, 116, 130);
            crumb.Location = new Point(20, 12);

            Label title = new Label();
            title.Text = "Yeni Fatura Düzenle & GİB Canlı Önizleme";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(23, 35, 50);
            title.Location = new Point(20, 37);

            Label status = new Label();
            status.Text = "●  GİB Portal Canlı Senkron";
            status.AutoSize = true;
            status.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            status.ForeColor = Color.FromArgb(16, 150, 105);
            status.BackColor = Color.FromArgb(231, 250, 242);
            status.Padding = new Padding(8, 4, 8, 4);
            status.Location = new Point(390, 38);

            Button btnKapat = BeyazButon("Kapat");
            btnKapat.Size = new Size(82, 34);
            btnKapat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnKapat.Click += delegate { Close(); };

            btnTema = BeyazButon("☾  Koyu mod");
            btnTema.Size = new Size(112, 34);
            btnTema.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTema.Click += delegate { koyuMod = !koyuMod; TemayiUygula(); };

            Button btnOnay = MaviButon("Faturayı Onayla ve GİB'e Gönder");
            btnOnay.Size = new Size(245, 36);
            btnOnay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOnay.Click += delegate
            {
                MessageBox.Show("Bu buton GİB gönderim entegrasyonuna bağlanacak.", "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            header.Controls.Add(crumb);
            header.Controls.Add(title);
            header.Controls.Add(status);
            header.Controls.Add(btnTema);
            header.Controls.Add(btnKapat);
            header.Controls.Add(btnOnay);

            header.Resize += delegate
            {
                btnKapat.Left = header.ClientSize.Width - btnKapat.Width - 18;
                btnKapat.Top = 12;
                btnTema.Left = btnKapat.Left - btnTema.Width - 8;
                btnTema.Top = 12;
                btnOnay.Left = header.ClientSize.Width - btnOnay.Width - 18;
                btnOnay.Top = 50;
            };

            return header;
        }


        // =========================================================
        // SOL TARAF
        // =========================================================

        private Panel SolTarafiOlustur()
        {
            Panel container = new Panel();
            solContainer = container;
            container.Dock = DockStyle.Fill;
            container.AutoScroll = true;
            container.BackColor = Color.FromArgb(244, 247, 250);
            container.Padding = new Padding(14);

            // 1) FATURA & BELGE BİLGİLERİ
            Panel faturaCard = KartOlustur();
            faturaCard.Location = new Point(14, 14);
            faturaCard.Size = new Size(650, 245);
            faturaCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Label faturaBaslik = Baslik("▣  Fatura & Belge Bilgileri", 18, 14);
            faturaCard.Controls.Add(faturaBaslik);
            Label ubl = new Label();
            ubl.Text = "UBL-TR 2.1";
            ubl.AutoSize = true;
            ubl.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            ubl.ForeColor = Color.FromArgb(100, 110, 122);
            ubl.BackColor = Color.FromArgb(238, 241, 245);
            ubl.Padding = new Padding(7, 4, 7, 4);
            ubl.Location = new Point(540, 13);
            faturaCard.Controls.Add(ubl);

            faturaCard.Controls.Add(FormLabel("Belge Türü", 18, 48));
            Panel belgeSecim = new Panel();
            belgeSecim.Location = new Point(18, 68);
            belgeSecim.Size = new Size(610, 40);
            belgeSecim.BackColor = Color.FromArgb(246, 249, 255);
            belgeSecim.BorderStyle = BorderStyle.FixedSingle;
            Label ef = new Label();
            ef.Text = "●  e-Fatura                 GİB Kayıtlı";
            ef.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            ef.ForeColor = Color.FromArgb(25, 103, 210);
            ef.AutoSize = true;
            ef.Location = new Point(12, 11);
            Label ea = new Label();
            ea.Text = "○  e-Arşiv Fatura        Bireysel / Diğer";
            ea.Font = new Font("Segoe UI", 9F);
            ea.ForeColor = Color.FromArgb(75, 83, 95);
            ea.AutoSize = true;
            ea.Location = new Point(305, 11);
            belgeSecim.Controls.Add(ef); belgeSecim.Controls.Add(ea);
            faturaCard.Controls.Add(belgeSecim);

            faturaCard.Controls.Add(FormLabel("Senaryo", 18, 116));
            cmbSenaryo = new ComboBox(); cmbSenaryo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSenaryo.Items.Add("Temel Fatura"); cmbSenaryo.Items.Add("Ticari Fatura"); cmbSenaryo.SelectedIndex = 0;
            cmbSenaryo.Location = new Point(18, 136); cmbSenaryo.Size = new Size(285, 30); cmbSenaryo.SelectedIndexChanged += VeriDegisti;
            faturaCard.Controls.Add(cmbSenaryo);

            faturaCard.Controls.Add(FormLabel("Fatura Tipi", 323, 116));
            cmbFaturaTipi = new ComboBox(); cmbFaturaTipi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFaturaTipi.Items.Add("Satış"); cmbFaturaTipi.Items.Add("İade"); cmbFaturaTipi.SelectedIndex = 0;
            cmbFaturaTipi.Location = new Point(323, 136); cmbFaturaTipi.Size = new Size(305, 30); cmbFaturaTipi.SelectedIndexChanged += VeriDegisti;
            faturaCard.Controls.Add(cmbFaturaTipi);

            faturaCard.Controls.Add(FormLabel("Fatura No", 18, 174));
            txtFaturaNo = new TextBox(); txtFaturaNo.Location = new Point(18, 194); txtFaturaNo.Size = new Size(190, 30);
            txtFaturaNo.Text = "SF" + DateTime.Now.Year + "000001"; txtFaturaNo.TextChanged += VeriDegisti;
            faturaCard.Controls.Add(txtFaturaNo);

            faturaCard.Controls.Add(FormLabel("Fatura Tarihi", 222, 174));
            dtpTarih = new DateTimePicker(); dtpTarih.Location = new Point(222, 194); dtpTarih.Size = new Size(190, 30); dtpTarih.Format = DateTimePickerFormat.Short; dtpTarih.ValueChanged += VeriDegisti;
            faturaCard.Controls.Add(dtpTarih);

            faturaCard.Controls.Add(FormLabel("Para Birimi", 426, 174));
            cmbParaBirimi = new ComboBox(); cmbParaBirimi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParaBirimi.Items.Add("TRY - Türk Lirası"); cmbParaBirimi.Items.Add("USD - Amerikan Doları"); cmbParaBirimi.Items.Add("EUR - Euro"); cmbParaBirimi.SelectedIndex = 0;
            cmbParaBirimi.Location = new Point(426, 194); cmbParaBirimi.Size = new Size(202, 30); cmbParaBirimi.SelectedIndexChanged += VeriDegisti;
            faturaCard.Controls.Add(cmbParaBirimi);
            container.Controls.Add(faturaCard);

            // 2) ALICI / MÜŞTERİ BİLGİLERİ
            Panel aliciCard = KartOlustur();
            aliciCard.Location = new Point(14, 273); aliciCard.Size = new Size(650, 205);
            aliciCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            aliciCard.Controls.Add(Baslik("▦  Alıcı / Müşteri Bilgileri", 18, 14));

            Button btnCariSec = BeyazButon("+ Yeni Cari / Cari Seç");
            btnCariSec.Size = new Size(165, 32); btnCariSec.Location = new Point(463, 10);
            btnCariSec.Click += delegate { CariHesapForm form = new CariHesapForm(); form.ShowDialog(this); };
            aliciCard.Controls.Add(btnCariSec);

            aliciCard.Controls.Add(FormLabel("Alıcı Tipi", 18, 52));
            cmbAliciTipi = new ComboBox(); cmbAliciTipi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAliciTipi.Items.Add("Bireysel Müşteri"); cmbAliciTipi.Items.Add("Kurumsal Müşteri"); cmbAliciTipi.SelectedIndex = 0;
            cmbAliciTipi.Location = new Point(18, 72); cmbAliciTipi.Size = new Size(190, 30);
            cmbAliciTipi.SelectedIndexChanged += delegate { KimlikBasliginiDegistir(); OnizlemeyiGuncelle(); };
            aliciCard.Controls.Add(cmbAliciTipi);

            aliciCard.Controls.Add(FormLabel("Cari / Firma Unvanı", 222, 52));
            txtAlici = new TextBox(); txtAlici.Location = new Point(222, 72); txtAlici.Size = new Size(406, 30); txtAlici.TextChanged += VeriDegisti;
            aliciCard.Controls.Add(txtAlici);

            lblKimlik = FormLabel("T.C. Kimlik No", 18, 112); aliciCard.Controls.Add(lblKimlik);
            txtKimlikNo = new TextBox(); txtKimlikNo.Location = new Point(18, 132); txtKimlikNo.Size = new Size(285, 30); txtKimlikNo.TextChanged += VeriDegisti;
            aliciCard.Controls.Add(txtKimlikNo);

            aliciCard.Controls.Add(FormLabel("Vergi Dairesi / İletişim", 323, 112));
            TextBox txtBilgi = new TextBox(); txtBilgi.Location = new Point(323, 132); txtBilgi.Size = new Size(305, 30); txtBilgi.Text = "Cari kartından otomatik gelir"; txtBilgi.ForeColor = Color.Gray; txtBilgi.ReadOnly = true;
            aliciCard.Controls.Add(txtBilgi);

            // Favoriler işlevini koru, modern ince şerit olarak göster.
            pnlFavoriCariler = new FlowLayoutPanel(); pnlFavoriCariler.Location = new Point(18, 168); pnlFavoriCariler.Size = new Size(295, 28); pnlFavoriCariler.AutoScroll = true;
            pnlFavoriUrunler = new FlowLayoutPanel(); pnlFavoriUrunler.Location = new Point(323, 168); pnlFavoriUrunler.Size = new Size(305, 28); pnlFavoriUrunler.AutoScroll = true;
            aliciCard.Controls.Add(pnlFavoriCariler); aliciCard.Controls.Add(pnlFavoriUrunler);
            container.Controls.Add(aliciCard);

            // 3) FATURA SATIR KALEMLERİ
            Panel kalemCard = KartOlustur();
            kalemCard.Location = new Point(14, 492); kalemCard.Size = new Size(650, 310);
            kalemCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            kalemCard.Controls.Add(Baslik("▤  Fatura Satır Kalemleri", 18, 14));
            Label adet = new Label(); adet.Text = "Kalemler"; adet.AutoSize = true; adet.Location = new Point(210, 18); adet.ForeColor = Color.FromArgb(25, 103, 210); adet.BackColor = Color.FromArgb(232, 241, 255); adet.Padding = new Padding(6, 2, 6, 2); kalemCard.Controls.Add(adet);

            Button btnKalemEkle = MaviButon("+  Kalem Ekle"); btnKalemEkle.Size = new Size(120, 34); btnKalemEkle.Location = new Point(508, 10); btnKalemEkle.Click += delegate { BosKalemEkle(); }; kalemCard.Controls.Add(btnKalemEkle);
            Button btnKalemSil = BeyazButon("Kalem Sil"); btnKalemSil.Size = new Size(95, 32); btnKalemSil.Location = new Point(403, 11);
            btnKalemSil.Click += delegate { if (dgvKalemler.CurrentRow != null) { dgvKalemler.Rows.Remove(dgvKalemler.CurrentRow); OnizlemeyiGuncelle(); } };
            kalemCard.Controls.Add(btnKalemSil);

            dgvKalemler = KalemGridOlustur(); dgvKalemler.Location = new Point(18, 58); dgvKalemler.Size = new Size(610, 235);
            dgvKalemler.BackgroundColor = Color.White; dgvKalemler.BorderStyle = BorderStyle.FixedSingle; dgvKalemler.RowTemplate.Height = 36;
            kalemCard.Controls.Add(dgvKalemler); container.Controls.Add(kalemCard);

            Panel buttons = new Panel(); buttons.Location = new Point(14, 816); buttons.Size = new Size(650, 62); buttons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Button btnTaslak = BeyazButon("Taslak Kaydet"); btnTaslak.Size = new Size(145, 40);
            Button btnKaydet = MaviButon("FATURAYI KAYDET"); btnKaydet.Size = new Size(180, 40);
            btnKaydet.Click += delegate { VeritabaninaKaydet(true); };
            buttons.Controls.Add(btnTaslak); buttons.Controls.Add(btnKaydet); container.Controls.Add(buttons);

            BosKalemEkle();

            EventHandler layout = delegate
            {
                int w = Math.Max(620, container.ClientSize.Width - 28);
                faturaCard.Width = w; aliciCard.Width = w; kalemCard.Width = w; buttons.Width = w;
                int inner = w - 36;
                belgeSecim.Width = inner;
                int half = (inner - 18) / 2;
                cmbSenaryo.Width = half; cmbFaturaTipi.Left = 18 + half + 18; cmbFaturaTipi.Width = half;
                foreach (Control c in faturaCard.Controls) if (c is Label && c.Text == "Fatura Tipi") c.Left = cmbFaturaTipi.Left;
                int third = (inner - 28) / 3;
                txtFaturaNo.Width = third; dtpTarih.Left = 18 + third + 14; dtpTarih.Width = third; cmbParaBirimi.Left = 18 + (third + 14) * 2; cmbParaBirimi.Width = third;
                foreach (Control c in faturaCard.Controls) if (c is Label) { if (c.Text == "Fatura Tarihi") c.Left = dtpTarih.Left; if (c.Text == "Para Birimi") c.Left = cmbParaBirimi.Left; }
                btnCariSec.Left = w - btnCariSec.Width - 18;
                txtAlici.Width = w - txtAlici.Left - 18;
                txtKimlikNo.Width = half; txtBilgi.Left = 18 + half + 18; txtBilgi.Width = half;
                foreach (Control c in aliciCard.Controls) if (c is Label && c.Text == "Vergi Dairesi / İletişim") c.Left = txtBilgi.Left;
                pnlFavoriCariler.Width = half; pnlFavoriUrunler.Left = 18 + half + 18; pnlFavoriUrunler.Width = half;
                btnKalemEkle.Left = w - btnKalemEkle.Width - 18; btnKalemSil.Left = btnKalemEkle.Left - btnKalemSil.Width - 10;
                dgvKalemler.Width = w - 36;
                btnKaydet.Left = w - btnKaydet.Width - 18; btnTaslak.Left = btnKaydet.Left - btnTaslak.Width - 10; btnTaslak.Top = btnKaydet.Top = 8;
            };
            container.Resize += layout; layout(container, EventArgs.Empty);
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
                38;

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
            Panel background = new Panel();
            sagBackground = background;
            background.Dock = DockStyle.Fill;
            background.AutoScroll = true;
            background.BackColor = Color.FromArgb(236, 241, 247);

            Panel toolbar = new Panel();
            toolbar.Height = 52; toolbar.BackColor = Color.White; toolbar.BorderStyle = BorderStyle.FixedSingle;
            toolbar.Location = new Point(14, 14); toolbar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Label live = new Label(); live.Text = "●  GİB Standart E-Fatura Çıktısı"; live.AutoSize = true; live.Font = new Font("Segoe UI", 9F, FontStyle.Bold); live.ForeColor = Color.FromArgb(30, 45, 60); live.Location = new Point(14, 17);
            Label zoom = new Label(); zoom.Text = "⌕    100%    ⊕    ⛶"; zoom.AutoSize = true; zoom.ForeColor = Color.FromArgb(90, 105, 120); zoom.Location = new Point(420, 17);
            toolbar.Controls.Add(live); toolbar.Controls.Add(zoom); background.Controls.Add(toolbar);

            Panel paper = new Panel(); previewPaper = paper;
            paper.Location = new Point(14, 78); paper.Size = new Size(720, 900); paper.BackColor = Color.White; paper.BorderStyle = BorderStyle.FixedSingle;
            background.Controls.Add(paper);

            Label gib = new Label(); gib.Text = "GİB"; gib.Size = new Size(52, 52); gib.TextAlign = ContentAlignment.MiddleCenter; gib.Font = new Font("Segoe UI", 12F, FontStyle.Bold); gib.ForeColor = Color.White; gib.BackColor = Color.FromArgb(190, 25, 25); gib.Location = new Point(30, 30); paper.Controls.Add(gib);
            Label kurum = new Label(); kurum.Text = "GELİR İDARESİ\nBAŞKANLIĞI"; kurum.AutoSize = true; kurum.Font = new Font("Segoe UI", 11F, FontStyle.Bold); kurum.Location = new Point(95, 30); paper.Controls.Add(kurum);
            Label portal = new Label(); portal.Text = "e-Fatura / e-Arşiv Portal\nUBL-TR 2.1  •  Doğrulandı"; portal.AutoSize = true; portal.Font = new Font("Segoe UI", 8F); portal.ForeColor = Color.FromArgb(100, 110, 122); portal.Location = new Point(95, 67); paper.Controls.Add(portal);
            Label title = new Label(); title.Text = "E-FATURA"; title.AutoSize = true; title.Font = new Font("Segoe UI", 17F, FontStyle.Bold); title.ForeColor = Color.FromArgb(190, 25, 25); title.Location = new Point(555, 30); paper.Controls.Add(title);

            Panel meta = new Panel(); meta.Location = new Point(430, 70); meta.Size = new Size(255, 120); meta.BackColor = Color.White;
            prvFaturaNo = PreviewLabel(0, 0); prvTarih = PreviewLabel(0, 27); prvSenaryo = PreviewLabel(0, 54); prvTip = PreviewLabel(0, 81);
            meta.Controls.Add(prvFaturaNo); meta.Controls.Add(prvTarih); meta.Controls.Add(prvSenaryo); meta.Controls.Add(prvTip); paper.Controls.Add(meta);
            paper.Controls.Add(Cizgi(30, 205, 655));

            Panel ettn = new Panel(); ettn.Location = new Point(30, 220); ettn.Size = new Size(655, 34); ettn.BackColor = Color.FromArgb(247, 249, 252);
            Label ettnText = new Label(); ettnText.Text = "ETTN (UUID):  doğrulanabilir GİB imzalı belge"; ettnText.AutoSize = true; ettnText.Font = new Font("Segoe UI", 8F); ettnText.ForeColor = Color.FromArgb(75, 88, 104); ettnText.Location = new Point(10, 9); ettn.Controls.Add(ettnText); paper.Controls.Add(ettn);

            Panel satici = new Panel(); satici.Location = new Point(30, 270); satici.Size = new Size(315, 155); satici.BorderStyle = BorderStyle.FixedSingle;
            Label sl = new Label(); sl.Text = "SAYIN SATICI (GÖNDERİCİ)\n\nNEXORA TEKNOLOJİ VE YAZILIM A.Ş.\nMaslak / İstanbul\nVergi Dairesi: Maslak V.D.\nE-Posta: muhasebe@nexora.com.tr"; sl.Size = new Size(285, 135); sl.Location = new Point(12, 10); sl.Font = new Font("Segoe UI", 8.5F); satici.Controls.Add(sl); paper.Controls.Add(satici);
            Panel alici = new Panel(); alici.Location = new Point(370, 270); alici.Size = new Size(315, 155); alici.BorderStyle = BorderStyle.FixedSingle;
            Label al = new Label(); al.Text = "SAYIN ALICI (MÜŞTERİ)"; al.AutoSize = true; al.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold); al.ForeColor = Color.FromArgb(25, 83, 160); al.Location = new Point(12, 10); alici.Controls.Add(al);
            prvAlici = new Label(); prvAlici.Text = "-"; prvAlici.Size = new Size(285, 45); prvAlici.Font = new Font("Segoe UI", 10F, FontStyle.Bold); prvAlici.Location = new Point(12, 38); alici.Controls.Add(prvAlici);
            prvKimlik = new Label(); prvKimlik.Size = new Size(285, 45); prvKimlik.Location = new Point(12, 88); alici.Controls.Add(prvKimlik); paper.Controls.Add(alici);

            dgvOnizleme = new DataGridView(); dgvOnizleme.Location = new Point(30, 445); dgvOnizleme.Size = new Size(655, 285); dgvOnizleme.BackgroundColor = Color.White; dgvOnizleme.BorderStyle = BorderStyle.FixedSingle; dgvOnizleme.RowHeadersVisible = false; dgvOnizleme.AllowUserToAddRows = false; dgvOnizleme.ReadOnly = true; dgvOnizleme.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvOnizleme.EnableHeadersVisualStyles = false; dgvOnizleme.ColumnHeadersHeight = 40; dgvOnizleme.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 244, 250); dgvOnizleme.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 45, 60); dgvOnizleme.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvOnizleme.Columns.Add("Urun", "Mal / Hizmet Açıklaması"); dgvOnizleme.Columns.Add("Miktar", "Miktar"); dgvOnizleme.Columns.Add("Fiyat", "Birim Fiyat"); dgvOnizleme.Columns.Add("Kdv", "KDV"); dgvOnizleme.Columns.Add("Toplam", "Mal Hizmet Tutarı"); paper.Controls.Add(dgvOnizleme);

            Panel toplam = new Panel(); toplam.Location = new Point(420, 750); toplam.Size = new Size(265, 125); toplam.BackColor = Color.FromArgb(247, 249, 252);
            prvAraToplam = ToplamLabel(10, 12); prvKdv = ToplamLabel(10, 45); prvGenelToplam = ToplamLabel(10, 82); prvGenelToplam.Font = new Font("Segoe UI", 11F, FontStyle.Bold); prvGenelToplam.ForeColor = Color.FromArgb(20, 78, 130);
            toplam.Controls.Add(prvAraToplam); toplam.Controls.Add(prvKdv); toplam.Controls.Add(prvGenelToplam); paper.Controls.Add(toplam);

            EventHandler layout = delegate
            {
                toolbar.Width = Math.Max(540, background.ClientSize.Width - 28);
                zoom.Left = toolbar.ClientSize.Width - zoom.Width - 18;
                int available = Math.Max(620, background.ClientSize.Width - 28);
                paper.Width = Math.Min(790, available);
                paper.Height = Math.Max(900, background.ClientSize.Height - 92);
                paper.Left = Math.Max(14, (background.ClientSize.Width - paper.Width) / 2);
                paper.Top = 78;
                int pw = paper.ClientSize.Width;
                title.Left = pw - title.Width - 30;
                meta.Left = pw - meta.Width - 30;
                int contentW = pw - 60;
                ettn.Width = contentW;
                int boxGap = 16; int boxW = (contentW - boxGap) / 2;
                satici.Width = boxW; alici.Left = 30 + boxW + boxGap; alici.Width = boxW;
                dgvOnizleme.Width = contentW;
                toplam.Left = pw - toplam.Width - 30;
            };
            background.Resize += layout; layout(background, EventArgs.Empty);
            return background;
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
        // GERÇEK SQL SERVER FATURA KAYDI
        // =========================================================
        public bool VeritabaninaKaydet(bool mesajGoster)
        {
            try
            {
                string faturaNo = txtFaturaNo == null ? "" : txtFaturaNo.Text.Trim();
                string alici = txtAlici == null ? "" : txtAlici.Text.Trim();

                if (string.IsNullOrWhiteSpace(faturaNo))
                    throw new Exception("Fatura numarası boş olamaz.");
                if (string.IsNullOrWhiteSpace(alici))
                    throw new Exception("Alıcı / cari seçilmelidir.");

                var gecerliSatirlar = dgvKalemler.Rows.Cast<DataGridViewRow>()
                    .Where(r => !string.IsNullOrWhiteSpace(Deger(r, "Urun")) && DecimalDeger(r.Cells["Miktar"].Value) > 0)
                    .ToList();

                if (gecerliSatirlar.Count == 0)
                    throw new Exception("Faturada en az bir ürün / hizmet kalemi olmalıdır.");

                decimal genelToplam = 0m;
                foreach (DataGridViewRow row in gecerliSatirlar)
                {
                    decimal miktar = DecimalDeger(row.Cells["Miktar"].Value);
                    decimal fiyat = DecimalDeger(row.Cells["BirimFiyat"].Value);
                    decimal kdv = DecimalDeger(row.Cells["Kdv"].Value);
                    decimal ara = miktar * fiyat;
                    genelToplam += ara + (ara * kdv / 100m);
                }

                string cs = @"Server=(localdb)\MSSQLLocalDB;Database=UrunFaturaYonetimiDb;Trusted_Connection=True;TrustServerCertificate=True;";

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlTransaction tr = conn.BeginTransaction())
                    {
                        // Projede veritabanı daha önce initialize edilmemiş olsa bile gerekli tabloları hazırla.
                        string schema = @"
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Category NVARCHAR(150) NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        Image VARBINARY(MAX) NULL
    );
END;
IF OBJECT_ID(N'dbo.Invoices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invoices
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(100) NOT NULL,
        InvoiceDate DATETIME2 NOT NULL,
        CustomerTitle NVARCHAR(MAX) NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL
    );
END;
IF OBJECT_ID(N'dbo.InvoiceDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InvoiceDetails
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_InvoiceDetails_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(Id) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceDetails_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END;";
                        using (SqlCommand cmd = new SqlCommand(schema, conn, tr)) cmd.ExecuteNonQuery();

                        if (kayitliFaturaId == 0)
                        {
                            string insertInvoice = @"
INSERT INTO dbo.Invoices (InvoiceNumber, InvoiceDate, CustomerTitle, TotalAmount)
VALUES (@no, @tarih, @alici, @toplam);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                            using (SqlCommand cmd = new SqlCommand(insertInvoice, conn, tr))
                            {
                                cmd.Parameters.AddWithValue("@no", faturaNo);
                                cmd.Parameters.AddWithValue("@tarih", dtpTarih.Value);
                                cmd.Parameters.AddWithValue("@alici", alici);
                                cmd.Parameters.AddWithValue("@toplam", genelToplam);
                                kayitliFaturaId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            string updateInvoice = @"
UPDATE dbo.Invoices
SET InvoiceNumber=@no, InvoiceDate=@tarih, CustomerTitle=@alici, TotalAmount=@toplam
WHERE Id=@id;
DELETE FROM dbo.InvoiceDetails WHERE InvoiceId=@id;";
                            using (SqlCommand cmd = new SqlCommand(updateInvoice, conn, tr))
                            {
                                cmd.Parameters.AddWithValue("@no", faturaNo);
                                cmd.Parameters.AddWithValue("@tarih", dtpTarih.Value);
                                cmd.Parameters.AddWithValue("@alici", alici);
                                cmd.Parameters.AddWithValue("@toplam", genelToplam);
                                cmd.Parameters.AddWithValue("@id", kayitliFaturaId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (DataGridViewRow row in gecerliSatirlar)
                        {
                            string urunAdi = Deger(row, "Urun");
                            decimal miktarDecimal = DecimalDeger(row.Cells["Miktar"].Value);
                            int miktar = Math.Max(1, Convert.ToInt32(Math.Round(miktarDecimal, MidpointRounding.AwayFromZero)));
                            decimal fiyat = DecimalDeger(row.Cells["BirimFiyat"].Value);
                            decimal kdv = DecimalDeger(row.Cells["Kdv"].Value);
                            decimal ara = miktarDecimal * fiyat;
                            decimal satirToplam = ara + (ara * kdv / 100m);

                            int productId;
                            using (SqlCommand find = new SqlCommand("SELECT TOP 1 Id FROM dbo.Products WHERE Name=@name ORDER BY Id", conn, tr))
                            {
                                find.Parameters.AddWithValue("@name", urunAdi);
                                object found = find.ExecuteScalar();
                                if (found != null && found != DBNull.Value)
                                    productId = Convert.ToInt32(found);
                                else
                                {
                                    using (SqlCommand add = new SqlCommand(@"
INSERT INTO dbo.Products(Name, Category, UnitPrice, StockQuantity)
VALUES(@name, @category, @price, @stock);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tr))
                                    {
                                        UrunKaydi appUrun = AppData.Urunler.FirstOrDefault(u => u != null && string.Equals(u.UrunAdi, urunAdi, StringComparison.CurrentCultureIgnoreCase));
                                        add.Parameters.AddWithValue("@name", urunAdi);
                                        add.Parameters.AddWithValue("@category", appUrun == null ? "" : (appUrun.Kategori ?? ""));
                                        add.Parameters.AddWithValue("@price", fiyat);
                                        add.Parameters.AddWithValue("@stock", appUrun == null ? 0 : appUrun.Stok);
                                        productId = Convert.ToInt32(add.ExecuteScalar());
                                    }
                                }
                            }

                            using (SqlCommand detail = new SqlCommand(@"
INSERT INTO dbo.InvoiceDetails(InvoiceId, ProductId, Quantity, UnitPrice, LineTotal)
VALUES(@invoiceId, @productId, @quantity, @unitPrice, @lineTotal);", conn, tr))
                            {
                                detail.Parameters.AddWithValue("@invoiceId", kayitliFaturaId);
                                detail.Parameters.AddWithValue("@productId", productId);
                                detail.Parameters.AddWithValue("@quantity", miktar);
                                detail.Parameters.AddWithValue("@unitPrice", fiyat);
                                detail.Parameters.AddWithValue("@lineTotal", satirToplam);
                                detail.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();
                    }
                }

                // Mevcut NEXORA ekranları AppData.Faturalar kullandığı için aynı kaydı bellekte de güncel tut.
                FaturaKaydi appFatura = AppData.Faturalar.FirstOrDefault(f => f.FaturaNo == faturaNo);
                if (appFatura == null)
                {
                    appFatura = new FaturaKaydi();
                    AppData.Faturalar.Add(appFatura);
                }
                appFatura.FaturaNo = faturaNo;
                appFatura.Tarih = dtpTarih.Value;
                appFatura.CariAdi = alici;
                appFatura.CariTipi = cmbAliciTipi != null && cmbAliciTipi.SelectedIndex == 1 ? "Kurumsal" : "Bireysel";
                appFatura.BelgeTipi = cmbSenaryo != null ? cmbSenaryo.Text : "E-Fatura";
                appFatura.GenelToplam = genelToplam;
                appFatura.Durum = "Kaydedildi";

                OnizlemeyiGuncelle();
                if (mesajGoster)
                    MessageBox.Show("Fatura SQL Server veritabanına kaydedildi.\n\nFatura No: " + faturaNo + "\nToplam: " + genelToplam.ToString("N2") + " TL", "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                if (mesajGoster)
                    MessageBox.Show("Fatura kaydedilemedi:\n\n" + ex.Message, "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

            button.Size =
                new Size(150, 34);

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

            button.AutoSize =
                true;

            button.Height =
                30;

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
                new Size(255, 23);

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
                new Size(320, 30);

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
    }
}