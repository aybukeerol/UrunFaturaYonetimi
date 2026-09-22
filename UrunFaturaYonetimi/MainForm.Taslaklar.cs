using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
        private void Taslaklar_Click(object sender, EventArgs e)
        {
            TaslaklarAc();
        }

        private void TaslaklarAc()
        {
            SayfayiTemizle("Taslaklar");
            Label baslik = SayfaListeBasligi("Taslak Faturalar");
            pnlContent.Controls.Add(baslik);

            Label aciklama = new Label();
            aciklama.Text = "Taslaklarınızı görüntüleyin ve kaldığınız yerden devam edin";
            aciklama.Font = new Font("Segoe UI", 9F);
            aciklama.ForeColor = Color.FromArgb(105, 117, 132);
            aciklama.AutoSize = true;
            aciklama.Location = new Point(28, 62);
            pnlContent.Controls.Add(aciklama);

            DataGridView grid = TemelGrid();
            grid.Location = new Point(28, 95);
            grid.Size = new Size(
                Math.Max(1050, pnlContent.ClientSize.Width - 56),
                Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

            KolonEkle(grid, "FaturaNo", "Fatura No");
            KolonEkle(grid, "Tarih", "Tarih");
            KolonEkle(grid, "Cari", "Cari");
            KolonEkle(grid, "Tip", "Cari Tipi");
            KolonEkle(grid, "BelgeTipi", "Belge Tipi");
            KolonEkle(grid, "Tutar", "Genel Toplam");
            KolonEkle(grid, "Durum", "Durum");

            DataGridViewButtonColumn detay = new DataGridViewButtonColumn();
            detay.Name = "Detay";
            detay.HeaderText = "Görüntüle";
            detay.Text = "Detay";
            detay.UseColumnTextForButtonValue = true;
            detay.Width = 95;
            detay.MinimumWidth = 85;
            detay.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            detay.FlatStyle = FlatStyle.Flat;
            grid.Columns.Add(detay);

            DataGridViewButtonColumn duzenle = new DataGridViewButtonColumn();
            duzenle.Name = "Duzenle";
            duzenle.HeaderText = "Düzenle";
            duzenle.Text = "Düzenle";
            duzenle.UseColumnTextForButtonValue = true;
            duzenle.Width = 100;
            duzenle.MinimumWidth = 95;
            duzenle.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            duzenle.FlatStyle = FlatStyle.Flat;
            grid.Columns.Add(duzenle);

            // Uygulama yeniden açıldığında diskteki gerçek taslaklar listeye eklenir.
            try
            {
                foreach (TaslakBelgesi belge in TaslakDeposu.Listele())
                {
                    FaturaKaydi kayit = AppData.Faturalar.FirstOrDefault(x =>
                        string.Equals(x.FaturaNo, belge.FaturaNo, StringComparison.OrdinalIgnoreCase));
                    if (kayit == null)
                    {
                        kayit = new FaturaKaydi();
                        AppData.Faturalar.Add(kayit);
                    }
                    kayit.FaturaNo = belge.FaturaNo;
                    kayit.Tarih = belge.Tarih;
                    kayit.CariAdi = belge.Alici;
                    kayit.CariTipi = belge.AliciTipi == "Kurumsal Müşteri" ? "Kurumsal" : "Bireysel";
                    kayit.BelgeTipi = belge.Senaryo;
                    kayit.GenelToplam = belge.GenelToplam;
                    kayit.Durum = "Taslak";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Taslaklar okunamadı:\n" + ex.Message,
                    "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            foreach (FaturaKaydi fatura in AppData.Faturalar.Where(f =>
                string.Equals((f.Durum ?? "").Trim(), "Taslak", StringComparison.OrdinalIgnoreCase)))
            {
                grid.Rows.Add(
                    fatura.FaturaNo,
                    fatura.Tarih.ToShortDateString(),
                    fatura.CariAdi,
                    fatura.CariTipi,
                    fatura.BelgeTipi,
                    fatura.GenelToplam.ToString("N2") + " TL",
                    fatura.Durum,
                    "Detay",
                    "Düzenle");
            }

            grid.CellContentClick += delegate (object s, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                string no = Convert.ToString(grid.Rows[e.RowIndex].Cells["FaturaNo"].Value);
                string islem = grid.Columns[e.ColumnIndex].Name;
                if (islem == "Detay") TaslakDetayAc(no);
                if (islem == "Duzenle") TaslakDuzenleAc(no);
            };
            grid.CellDoubleClick += delegate (object s, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                TaslakDuzenleAc(Convert.ToString(grid.Rows[e.RowIndex].Cells["FaturaNo"].Value));
            };
            pnlContent.Controls.Add(grid);
        }

        private void TaslakDuzenleAc(string no)
        {
            if (TaslakDeposu.Bul(no) == null)
            {
                MessageBox.Show(
                    "Bu satır örnek/demo taslak olabilir. Kaydedilmiş alıcı ve kalem " +
                    "bilgileri bulunmadığından boş fatura formu açılmadı. " +
                    "Yeni Fatura ekranında Taslak Kaydet ile oluşturduğunuz " +
                    "taslakları buradan eksiksiz düzenleyebilirsiniz.",
                    "NEXORA - Taslak", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                using (YeniFaturaForm form = new YeniFaturaForm())
                {
                    form.TaslakYukle(no);
                    form.ShowDialog(this);
                }
                TaslaklarAc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Taslak açılamadı:\n" + ex.Message,
                    "NEXORA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Taslak detayı için eski FaturaDetayPenceresiAc kullanılmaz:
        // SQL'de kalemi olmayan demo taslaklar eski pencerede üst üste biniyordu.
        private void TaslakDetayAc(string no)
        {
            FaturaKaydi fatura = AppData.Faturalar.FirstOrDefault(x =>
                string.Equals(x.FaturaNo, no, StringComparison.OrdinalIgnoreCase));
            if (fatura == null) return;
            TaslakBelgesi belge = TaslakDeposu.Bul(no);

            using (Form pencere = new Form())
            {
                pencere.Text = "NEXORA • Taslak Detayı • " + no;
                pencere.StartPosition = FormStartPosition.CenterParent;
                pencere.Size = new Size(980, 690);
                pencere.MinimumSize = new Size(780, 540);
                pencere.BackColor = Color.FromArgb(244, 247, 251);
                pencere.Font = new Font("Segoe UI", 10F);

                TableLayoutPanel sayfa = new TableLayoutPanel();
                sayfa.Dock = DockStyle.Fill;
                sayfa.Padding = new Padding(24);
                sayfa.RowCount = 4;
                sayfa.ColumnCount = 1;
                sayfa.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
                sayfa.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
                sayfa.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                sayfa.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
                pencere.Controls.Add(sayfa);

                Label baslik = new Label();
                baslik.Text = "Taslak Detayı  •  " + no;
                baslik.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
                baslik.ForeColor = Color.FromArgb(28, 40, 57);
                baslik.Dock = DockStyle.Fill;
                baslik.TextAlign = ContentAlignment.MiddleLeft;
                sayfa.Controls.Add(baslik, 0, 0);

                TableLayoutPanel bilgi = new TableLayoutPanel();
                bilgi.BackColor = Color.White;
                bilgi.Dock = DockStyle.Fill;
                bilgi.Padding = new Padding(18);
                bilgi.ColumnCount = 2;
                bilgi.RowCount = 3;
                bilgi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                bilgi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                bilgi.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
                bilgi.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
                bilgi.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
                string[] alanlar = {
                    "CARİ / ALICI\n" + fatura.CariAdi,
                    "FATURA TARİHİ\n" + fatura.Tarih.ToString("dd.MM.yyyy"),
                    "BELGE TİPİ\n" + fatura.BelgeTipi,
                    "DURUM\nTaslak",
                    "GENEL TOPLAM\n" + fatura.GenelToplam.ToString("N2") + " TL",
                    "KALEM SAYISI\n" + (belge == null ? "Kayıt yok" : belge.Kalemler.Count.ToString())
                };
                for (int i = 0; i < alanlar.Length; i++)
                {
                    Label alan = new Label();
                    alan.Text = alanlar[i];
                    alan.Dock = DockStyle.Fill;
                    alan.AutoEllipsis = true;
                    alan.ForeColor = Color.FromArgb(35, 50, 70);
                    bilgi.Controls.Add(alan, i % 2, i / 2);
                }
                sayfa.Controls.Add(bilgi, 0, 1);

                DataGridView kalemler = TemelGrid();
                kalemler.Dock = DockStyle.Fill;
                kalemler.ReadOnly = true;
                kalemler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                kalemler.Columns.Add("Urun", "Ürün / Hizmet");
                kalemler.Columns.Add("Miktar", "Miktar");
                kalemler.Columns.Add("BirimFiyat", "Birim Fiyat");
                kalemler.Columns.Add("Kdv", "KDV %");
                if (belge != null)
                    foreach (TaslakKalemi k in belge.Kalemler)
                        kalemler.Rows.Add(k.Urun, k.Miktar, k.BirimFiyat, k.Kdv);
                sayfa.Controls.Add(kalemler, 0, 2);

                FlowLayoutPanel alt = new FlowLayoutPanel();
                alt.Dock = DockStyle.Fill;
                alt.FlowDirection = FlowDirection.RightToLeft;
                alt.Padding = new Padding(0, 12, 0, 0);
                Button kapat = new Button();
                kapat.Text = "Kapat";
                kapat.Size = new Size(110, 40);
                kapat.Click += delegate { pencere.Close(); };
                alt.Controls.Add(kapat);
                Button duzenle = new Button();
                duzenle.Text = "Düzenle";
                duzenle.Size = new Size(110, 40);
                duzenle.Enabled = belge != null;
                duzenle.Click += delegate { pencere.Close(); TaslakDuzenleAc(no); };
                alt.Controls.Add(duzenle);
                sayfa.Controls.Add(alt, 0, 3);
                pencere.ShowDialog(this);
            }
        }
    }
}
