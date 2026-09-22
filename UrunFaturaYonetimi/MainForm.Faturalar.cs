using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Services;

namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
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
            FaturalarAc(null, null, null);
        }

        private void FaturalarAc(
            string cariAdi,
            DateTime? baslangic,
            DateTime? bitis)
        {
            SayfayiTemizle("Faturalar");

            Label title = SayfaListeBasligi("Fatura Listesi");
            pnlContent.Controls.Add(title);

            Label filtreBilgisi = null;
            if (!string.IsNullOrWhiteSpace(cariAdi) || baslangic.HasValue || bitis.HasValue)
            {
                filtreBilgisi = new Label();
                filtreBilgisi.AutoSize = true;
                filtreBilgisi.ForeColor = _primary;
                filtreBilgisi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                string metin = "AI filtresi: ";
                if (!string.IsNullOrWhiteSpace(cariAdi)) metin += cariAdi;
                if (baslangic.HasValue)
                    metin += (metin.EndsWith(": ") ? "" : " • ") + baslangic.Value.ToString("dd.MM.yyyy");
                if (bitis.HasValue)
                    metin += " - " + bitis.Value.AddDays(-1).ToString("dd.MM.yyyy");

                filtreBilgisi.Text = metin;
                filtreBilgisi.Location = new Point(28, 62);
                pnlContent.Controls.Add(filtreBilgisi);
            }

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
            detay.HeaderText = "İşlem";
            detay.Text = "Detayı Aç";
            detay.UseColumnTextForButtonValue = true;
            detay.Width = 150;
            detay.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            detay.MinimumWidth = 140;
            detay.FlatStyle = FlatStyle.Flat;
            grid.Columns.Add(detay);

            foreach (FaturaKaydi fatura in AppData.Faturalar)
            {
                bool uygun = true;
                if (!string.IsNullOrWhiteSpace(cariAdi))
                    uygun = fatura.CariAdi.IndexOf(cariAdi, StringComparison.CurrentCultureIgnoreCase) >= 0;
                if (uygun && baslangic.HasValue) uygun = fatura.Tarih >= baslangic.Value;
                if (uygun && bitis.HasValue) uygun = fatura.Tarih < bitis.Value;
                if (!uygun) continue;

                grid.Rows.Add(
                    fatura.FaturaNo,
                    fatura.Tarih.ToShortDateString(),
                    fatura.CariAdi,
                    fatura.CariTipi,
                    fatura.BelgeTipi,
                    fatura.GenelToplam.ToString("N2") + " TL",
                    fatura.Durum,
                    "Detayı Aç");
            }

            grid.CellContentClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Detay") return;
                string no = Convert.ToString(grid.Rows[e.RowIndex].Cells["FaturaNo"].Value);
                FaturaDetayPenceresiAc(no);
            };

            grid.CellDoubleClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                string no = Convert.ToString(grid.Rows[e.RowIndex].Cells["FaturaNo"].Value);
                FaturaDetayPenceresiAc(no);
            };

            pnlContent.Controls.Add(grid);
        }

        private void FaturaDetayPenceresiAc(string faturaNo)
        {
            if (string.IsNullOrWhiteSpace(faturaNo)) return;

            FaturaKaydi appFatura = AppData.Faturalar.FirstOrDefault(
                f => string.Equals(f.FaturaNo, faturaNo, StringComparison.CurrentCultureIgnoreCase));

            string cari = appFatura == null ? "" : appFatura.CariAdi;
            DateTime tarih = appFatura == null ? DateTime.Today : appFatura.Tarih;
            decimal genelToplam = appFatura == null ? 0m : appFatura.GenelToplam;
            string belgeTipi = appFatura == null ? "" : appFatura.BelgeTipi;
            string durum = appFatura == null ? "" : appFatura.Durum;
            int invoiceId = 0;

            System.Collections.Generic.List<object[]> kalemler =
                new System.Collections.Generic.List<object[]>();

            try
            {
                const string cs = @"Server=(localdb)\MSSQLLocalDB;Database=UrunFaturaYonetimiDb;Trusted_Connection=True;TrustServerCertificate=True;";
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 Id, InvoiceDate, CustomerTitle, TotalAmount
FROM dbo.Invoices
WHERE InvoiceNumber = @no
ORDER BY Id DESC;", conn))
                    {
                        cmd.Parameters.AddWithValue("@no", faturaNo);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                invoiceId = Convert.ToInt32(rd["Id"]);
                                tarih = Convert.ToDateTime(rd["InvoiceDate"]);
                                cari = Convert.ToString(rd["CustomerTitle"]);
                                genelToplam = Convert.ToDecimal(rd["TotalAmount"]);
                            }
                        }
                    }

                    if (invoiceId > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
SELECT
    d.Id,
    ISNULL(p.Name, 'Ürün / Hizmet') AS ProductName,
    ISNULL(p.Category, '') AS Category,
    d.Quantity,
    d.UnitPrice,
    d.LineTotal
FROM dbo.InvoiceDetails d
LEFT JOIN dbo.Products p ON p.Id = d.ProductId
WHERE d.InvoiceId = @id
ORDER BY d.Id;", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", invoiceId);
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    kalemler.Add(new object[]
                                    {
                                        Convert.ToString(rd["ProductName"]),
                                        Convert.ToString(rd["Category"]),
                                        Convert.ToInt32(rd["Quantity"]),
                                        Convert.ToDecimal(rd["UnitPrice"]),
                                        Convert.ToDecimal(rd["LineTotal"])
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Fatura detayı veritabanından okunamadı.\n\n" + ex.Message,
                    "NEXORA - Fatura Detayı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            decimal kalemToplami = kalemler.Sum(k => Convert.ToDecimal(k[4]));

            Form detayForm = new Form();
            detayForm.Text = "NEXORA • Fatura Detayı • " + faturaNo;
            detayForm.StartPosition = FormStartPosition.CenterParent;
            detayForm.Size = new Size(1120, 760);
            detayForm.MinimumSize = new Size(940, 650);
            detayForm.BackColor = Color.FromArgb(244, 247, 251);
            detayForm.Font = new Font("Segoe UI", 9F);
            detayForm.FormBorderStyle = FormBorderStyle.Sizable;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 104;
            header.BackColor = Color.White;
            detayForm.Controls.Add(header);

            Label lblUst = new Label();
            lblUst.Text = "FATURALAR  /  FATURA DETAYI";
            lblUst.AutoSize = true;
            lblUst.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblUst.ForeColor = Color.FromArgb(111, 124, 141);
            lblUst.Location = new Point(30, 18);
            header.Controls.Add(lblUst);

            Label lblBaslik = new Label();
            lblBaslik.Text = "Fatura Detayı";
            lblBaslik.AutoSize = true;
            lblBaslik.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblBaslik.ForeColor = Color.FromArgb(25, 37, 55);
            lblBaslik.Location = new Point(27, 40);
            header.Controls.Add(lblBaslik);

            Label lblNo = new Label();
            lblNo.Text = faturaNo;
            lblNo.AutoSize = true;
            lblNo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblNo.ForeColor = _primary;
            lblNo.Location = new Point(205, 54);
            header.Controls.Add(lblNo);

            Label durumBadge = new Label();
            durumBadge.Text = "  " + (string.IsNullOrWhiteSpace(durum) ? "Kayıtlı" : durum) + "  ";
            durumBadge.AutoSize = true;
            durumBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            durumBadge.ForeColor = Color.FromArgb(21, 128, 61);
            durumBadge.BackColor = Color.FromArgb(232, 248, 238);
            durumBadge.Padding = new Padding(8, 5, 8, 5);
            durumBadge.Location = new Point(30, 78);
            header.Controls.Add(durumBadge);

            Button btnKapat = new Button();
            btnKapat.Text = "Kapat";
            btnKapat.Size = new Size(96, 36);
            btnKapat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnKapat.Location = new Point(detayForm.ClientSize.Width - 126, 32);
            btnKapat.FlatStyle = FlatStyle.Flat;
            btnKapat.FlatAppearance.BorderColor = Color.FromArgb(214, 221, 230);
            btnKapat.BackColor = Color.White;
            btnKapat.ForeColor = Color.FromArgb(50, 61, 76);
            btnKapat.Cursor = Cursors.Hand;
            btnKapat.Click += delegate { detayForm.Close(); };
            header.Controls.Add(btnKapat);

            Panel body = new Panel();
            body.Dock = DockStyle.Fill;
            body.AutoScroll = true;
            body.BackColor = Color.FromArgb(244, 247, 251);
            detayForm.Controls.Add(body);
            body.BringToFront();
            header.BringToFront();

            Panel bilgiCard = new Panel();
            bilgiCard.BackColor = Color.White;
            bilgiCard.Location = new Point(30, 26);
            bilgiCard.Size = new Size(1038, 142);
            bilgiCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            body.Controls.Add(bilgiCard);

            Label kartBaslik = new Label();
            kartBaslik.Text = "Fatura Bilgileri";
            kartBaslik.AutoSize = true;
            kartBaslik.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            kartBaslik.ForeColor = Color.FromArgb(28, 40, 57);
            kartBaslik.Location = new Point(22, 17);
            bilgiCard.Controls.Add(kartBaslik);

            Label lblCariEtiket = new Label();
            lblCariEtiket.Text = "CARİ / ALICI";
            lblCariEtiket.AutoSize = true;
            lblCariEtiket.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblCariEtiket.ForeColor = Color.FromArgb(120, 132, 148);
            lblCariEtiket.Location = new Point(22, 55);
            bilgiCard.Controls.Add(lblCariEtiket);

            Label lblCari = new Label();
            lblCari.Text = string.IsNullOrWhiteSpace(cari) ? "—" : cari;
            lblCari.AutoEllipsis = true;
            lblCari.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCari.ForeColor = Color.FromArgb(28, 40, 57);
            lblCari.Location = new Point(22, 76);
            lblCari.Size = new Size(370, 27);
            bilgiCard.Controls.Add(lblCari);

            Label lblTarihEtiket = new Label();
            lblTarihEtiket.Text = "FATURA TARİHİ";
            lblTarihEtiket.AutoSize = true;
            lblTarihEtiket.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblTarihEtiket.ForeColor = Color.FromArgb(120, 132, 148);
            lblTarihEtiket.Location = new Point(425, 55);
            bilgiCard.Controls.Add(lblTarihEtiket);

            Label lblTarih = new Label();
            lblTarih.Text = tarih.ToString("dd.MM.yyyy");
            lblTarih.AutoSize = true;
            lblTarih.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblTarih.ForeColor = Color.FromArgb(28, 40, 57);
            lblTarih.Location = new Point(425, 77);
            bilgiCard.Controls.Add(lblTarih);

            Label lblBelgeEtiket = new Label();
            lblBelgeEtiket.Text = "BELGE TİPİ";
            lblBelgeEtiket.AutoSize = true;
            lblBelgeEtiket.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblBelgeEtiket.ForeColor = Color.FromArgb(120, 132, 148);
            lblBelgeEtiket.Location = new Point(620, 55);
            bilgiCard.Controls.Add(lblBelgeEtiket);

            Label lblBelge = new Label();
            lblBelge.Text = string.IsNullOrWhiteSpace(belgeTipi) ? "E-Fatura" : belgeTipi;
            lblBelge.AutoSize = true;
            lblBelge.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblBelge.ForeColor = Color.FromArgb(28, 40, 57);
            lblBelge.Location = new Point(620, 77);
            bilgiCard.Controls.Add(lblBelge);

            Label lblToplamEtiket = new Label();
            lblToplamEtiket.Text = "GENEL TOPLAM";
            lblToplamEtiket.AutoSize = true;
            lblToplamEtiket.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblToplamEtiket.ForeColor = Color.FromArgb(120, 132, 148);
            lblToplamEtiket.Location = new Point(820, 55);
            bilgiCard.Controls.Add(lblToplamEtiket);

            Label lblToplam = new Label();
            lblToplam.Text = genelToplam.ToString("N2") + " TL";
            lblToplam.AutoSize = true;
            lblToplam.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblToplam.ForeColor = _primary;
            lblToplam.Location = new Point(820, 75);
            bilgiCard.Controls.Add(lblToplam);

            Panel kalemCard = new Panel();
            kalemCard.BackColor = Color.White;
            kalemCard.Location = new Point(30, 188);
            kalemCard.Size = new Size(1038, 365);
            kalemCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            body.Controls.Add(kalemCard);

            Label kalemBaslik = new Label();
            kalemBaslik.Text = "Fatura Kalemleri";
            kalemBaslik.AutoSize = true;
            kalemBaslik.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            kalemBaslik.ForeColor = Color.FromArgb(28, 40, 57);
            kalemBaslik.Location = new Point(22, 18);
            kalemCard.Controls.Add(kalemBaslik);

            Label kalemAdet = new Label();
            kalemAdet.Text = kalemler.Count + " kalem";
            kalemAdet.AutoSize = true;
            kalemAdet.Font = new Font("Segoe UI", 9F);
            kalemAdet.ForeColor = Color.FromArgb(111, 124, 141);
            kalemAdet.Location = new Point(22, 46);
            kalemCard.Controls.Add(kalemAdet);

            DataGridView dg = new DataGridView();
            dg.Location = new Point(22, 76);
            dg.Size = new Size(994, 265);
            dg.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dg.AllowUserToAddRows = false;
            dg.AllowUserToDeleteRows = false;
            dg.AllowUserToResizeRows = false;
            dg.ReadOnly = true;
            dg.RowHeadersVisible = false;
            dg.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dg.MultiSelect = false;
            dg.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dg.BackgroundColor = Color.White;
            dg.BorderStyle = BorderStyle.None;
            dg.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dg.GridColor = Color.FromArgb(232, 236, 242);
            dg.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dg.ColumnHeadersHeight = 42;
            dg.EnableHeadersVisualStyles = false;
            dg.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 248, 251);
            dg.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(70, 82, 98);
            dg.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dg.DefaultCellStyle.BackColor = Color.White;
            dg.DefaultCellStyle.ForeColor = Color.FromArgb(39, 50, 65);
            dg.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 243, 255);
            dg.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 70, 135);
            dg.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            dg.RowTemplate.Height = 40;

            dg.Columns.Add("Sira", "#");
            dg.Columns.Add("Urun", "Ürün / Hizmet");
            dg.Columns.Add("Kategori", "Kategori");
            dg.Columns.Add("Miktar", "Miktar");
            dg.Columns.Add("BirimFiyat", "Birim Fiyat");
            dg.Columns.Add("SatirToplam", "Satır Toplamı");
            dg.Columns["Sira"].FillWeight = 24;
            dg.Columns["Urun"].FillWeight = 155;
            dg.Columns["Kategori"].FillWeight = 80;
            dg.Columns["Miktar"].FillWeight = 45;
            dg.Columns["BirimFiyat"].FillWeight = 70;
            dg.Columns["SatirToplam"].FillWeight = 75;
            dg.Columns["Miktar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dg.Columns["BirimFiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dg.Columns["SatirToplam"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            for (int i = 0; i < kalemler.Count; i++)
            {
                object[] k = kalemler[i];
                dg.Rows.Add(
                    (i + 1).ToString(),
                    Convert.ToString(k[0]),
                    string.IsNullOrWhiteSpace(Convert.ToString(k[1])) ? "—" : Convert.ToString(k[1]),
                    Convert.ToInt32(k[2]).ToString(),
                    Convert.ToDecimal(k[3]).ToString("N2") + " TL",
                    Convert.ToDecimal(k[4]).ToString("N2") + " TL");
            }
            kalemCard.Controls.Add(dg);

            if (kalemler.Count == 0)
            {
                Panel empty = new Panel();
                empty.BackColor = Color.White;
                empty.Location = new Point(22, 76);
                empty.Size = new Size(994, 265);
                empty.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                kalemCard.Controls.Add(empty);
                empty.BringToFront();

                Label emptyTitle = new Label();
                emptyTitle.Text = "Bu faturanın kalem kaydı bulunamadı";
                emptyTitle.AutoSize = true;
                emptyTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                emptyTitle.ForeColor = Color.FromArgb(45, 56, 70);
                emptyTitle.Location = new Point(30, 72);
                empty.Controls.Add(emptyTitle);

                Label emptyText = new Label();
                emptyText.Text = "Bu kayıt, InvoiceDetails kaydı tutulmadan önce oluşturulmuş olabilir.\r\nYeni oluşturulan faturaların ürün/hizmet kalemleri burada ayrıntılı görünecek.";
                emptyText.AutoSize = true;
                emptyText.Font = new Font("Segoe UI", 9.5F);
                emptyText.ForeColor = Color.FromArgb(105, 117, 132);
                emptyText.Location = new Point(30, 105);
                empty.Controls.Add(emptyText);
            }

            Panel toplamCard = new Panel();
            toplamCard.BackColor = Color.White;
            toplamCard.Location = new Point(30, 573);
            toplamCard.Size = new Size(1038, 104);
            toplamCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            body.Controls.Add(toplamCard);

            Label toplamBaslik = new Label();
            toplamBaslik.Text = "Tutar Özeti";
            toplamBaslik.AutoSize = true;
            toplamBaslik.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            toplamBaslik.ForeColor = Color.FromArgb(28, 40, 57);
            toplamBaslik.Location = new Point(22, 18);
            toplamCard.Controls.Add(toplamBaslik);

            Label lblKalemToplami = new Label();
            lblKalemToplami.Text = "Kalem Toplamı\r\n" + (kalemler.Count == 0 ? "—" : kalemToplami.ToString("N2") + " TL");
            lblKalemToplami.AutoSize = true;
            lblKalemToplami.Font = new Font("Segoe UI", 9.5F);
            lblKalemToplami.ForeColor = Color.FromArgb(75, 88, 104);
            lblKalemToplami.Location = new Point(620, 18);
            toplamCard.Controls.Add(lblKalemToplami);

            Label lblGenel = new Label();
            lblGenel.Text = "GENEL TOPLAM\r\n" + genelToplam.ToString("N2") + " TL";
            lblGenel.AutoSize = true;
            lblGenel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGenel.ForeColor = _primary;
            lblGenel.Location = new Point(820, 18);
            toplamCard.Controls.Add(lblGenel);

            Action detayLayout = delegate
            {
                int w = Math.Max(850, body.ClientSize.Width - 60);
                bilgiCard.Width = w;
                kalemCard.Width = w;
                toplamCard.Width = w;
                dg.Width = Math.Max(760, kalemCard.ClientSize.Width - 44);
                lblToplamEtiket.Left = Math.Max(760, bilgiCard.ClientSize.Width - 218);
                lblToplam.Left = lblToplamEtiket.Left;
                lblBelgeEtiket.Left = Math.Max(575, bilgiCard.ClientSize.Width - 418);
                lblBelge.Left = lblBelgeEtiket.Left;
                lblKalemToplami.Left = Math.Max(570, toplamCard.ClientSize.Width - 418);
                lblGenel.Left = Math.Max(760, toplamCard.ClientSize.Width - 218);
            };
            body.Resize += delegate { detayLayout(); };
            detayLayout();

            detayForm.ShowDialog(this);
        }

        private void FaturaGridiniExcelCsvOlarakAktar(
            DataGridView grid)
        {
            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Faturaları Excel için dışa aktar";

                dialog.Filter =
                    "Excel uyumlu CSV (*.csv)|*.csv";

                dialog.FileName =
                    "NEXORA_Faturalar_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmm") +
                    ".csv";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                    return;

                StringBuilder sb =
                    new StringBuilder();

                var exportColumns =
                    grid.Columns
                        .Cast<DataGridViewColumn>()
                        .Where(c =>
                            c.Visible &&
                            c.Name != "Sec" &&
                            c.Name != "Goruntule" &&
                            c.Name != "Indir")
                        .ToList();

                sb.AppendLine(
                    string.Join(
                        ";",
                        exportColumns.Select(
                            c => CsvHucre(c.HeaderText))));

                foreach (DataGridViewRow row
                    in grid.Rows)
                {
                    if (row.IsNewRow ||
                        !row.Visible)
                        continue;

                    sb.AppendLine(
                        string.Join(
                            ";",
                            exportColumns.Select(
                                c => CsvHucre(
                                    Convert.ToString(
                                        row.Cells[c.Index].Value)))));
                }

                File.WriteAllText(
                    dialog.FileName,
                    sb.ToString(),
                    new UTF8Encoding(true));

                MessageBox.Show(
                    "Fatura listesi Excel uyumlu CSV olarak kaydedildi.",
                    "NEXORA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private string CsvHucre(
            string value)
        {
            value = value ?? "";

            if (value.Contains(";") ||
                value.Contains("\"") ||
                value.Contains("\r") ||
                value.Contains("\n"))
            {
                value =
                    "\"" +
                    value.Replace("\"", "\"\"") +
                    "\"";
            }

            return value;
        }

        private void FaturaSatiriniCsvOlarakKaydet(
            DataGridView grid,
            int rowIndex)
        {
            if (rowIndex < 0 ||
                rowIndex >= grid.Rows.Count)
                return;

            DataGridViewRow row =
                grid.Rows[rowIndex];

            string faturaNo =
                Convert.ToString(
                    row.Cells["FaturaNo"].Value);

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Faturayı kaydet";

                dialog.Filter =
                    "CSV Dosyası (*.csv)|*.csv";

                dialog.FileName =
                    (string.IsNullOrWhiteSpace(faturaNo)
                        ? "Fatura"
                        : faturaNo) +
                    ".csv";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                    return;

                StringBuilder sb =
                    new StringBuilder();

                foreach (DataGridViewColumn col
                    in grid.Columns)
                {
                    if (!col.Visible ||
                        col.Name == "Sec" ||
                        col.Name == "Goruntule" ||
                        col.Name == "Indir")
                        continue;

                    sb.AppendLine(
                        CsvHucre(col.HeaderText) +
                        ";" +
                        CsvHucre(
                            Convert.ToString(
                                row.Cells[col.Index].Value)));
                }

                File.WriteAllText(
                    dialog.FileName,
                    sb.ToString(),
                    new UTF8Encoding(true));

                MessageBox.Show(
                    "Fatura kaydedildi.",
                    "NEXORA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void FaturaGridiniYazdir(
            DataGridView grid)
        {
            PrintDocument document =
                new PrintDocument();

            document.DocumentName =
                "NEXORA - Fatura Listesi";

            document.DefaultPageSettings.Landscape =
                true;

            document.PrintPage +=
                delegate (object sender, PrintPageEventArgs e)
                {
                    float x =
                        e.MarginBounds.Left;

                    float y =
                        e.MarginBounds.Top;

                    using (Font titleFont =
                        new Font(
                            "Segoe UI",
                            16F,
                            FontStyle.Bold))
                    using (Font headerFont =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold))
                    using (Font bodyFont =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Regular))
                    {
                        e.Graphics.DrawString(
                            "NEXORA - Fatura Listesi",
                            titleFont,
                            Brushes.Black,
                            x,
                            y);

                        y += 38;

                        string[] headers =
                        {
                            "Fatura No",
                            "Tarih",
                            "Cari",
                            "Cari Tipi",
                            "Genel Toplam",
                            "Durum"
                        };

                        float[] widths =
                        {
                            115F,
                            80F,
                            190F,
                            105F,
                            110F,
                            100F
                        };

                        for (int i = 0;
                            i < headers.Length;
                            i++)
                        {
                            e.Graphics.DrawString(
                                headers[i],
                                headerFont,
                                Brushes.Black,
                                x,
                                y);

                            x += widths[i];
                        }

                        y += 26;

                        foreach (DataGridViewRow row
                            in grid.Rows)
                        {
                            if (row.IsNewRow ||
                                !row.Visible)
                                continue;

                            if (y >
                                e.MarginBounds.Bottom - 30)
                                break;

                            x =
                                e.MarginBounds.Left;

                            string[] values =
                            {
                                Convert.ToString(row.Cells["FaturaNo"].Value),
                                Convert.ToString(row.Cells["Tarih"].Value),
                                Convert.ToString(row.Cells["Cari"].Value),
                                Convert.ToString(row.Cells["Tip"].Value),
                                Convert.ToString(row.Cells["Toplam"].Value),
                                Convert.ToString(row.Cells["Durum"].Value)
                            };

                            for (int i = 0;
                                i < values.Length;
                                i++)
                            {
                                RectangleF rect =
                                    new RectangleF(
                                        x,
                                        y,
                                        widths[i] - 6,
                                        24);

                                e.Graphics.DrawString(
                                    values[i] ?? "",
                                    bodyFont,
                                    Brushes.Black,
                                    rect);

                                x += widths[i];
                            }

                            y += 25;
                        }
                    }

                    e.HasMorePages =
                        false;
                };

            using (PrintPreviewDialog preview =
                new PrintPreviewDialog())
            {
                preview.Document =
                    document;

                preview.Width =
                    1100;

                preview.Height =
                    750;

                preview.StartPosition =
                    FormStartPosition.CenterParent;

                preview.ShowDialog(this);
            }

            document.Dispose();
        }
    }
}
