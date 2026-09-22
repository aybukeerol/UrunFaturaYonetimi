using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    // Bu dosya mevcut MainForm.AI.cs ile aynı kurucu ve statik metot imzalarını korur.
    public class RaporOnizlemeForm : Form
    {
        private readonly string _baslik;
        private readonly string _metin;
        private readonly DateTime _olusturmaZamani;
        private static readonly Color Koyu = Color.FromArgb(40, 44, 47);
        private static readonly Color Mavi = Color.FromArgb(25, 86, 171);

        public RaporOnizlemeForm(string baslik, string metin)
        {
            _baslik = baslik ?? "NEXORA AI Raporu";
            _metin = metin ?? "";
            _olusturmaZamani = DateTime.Now;
            Text = "NEXORA | Rapor Önizleme";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(680, 520);
            Size = new Size(900, 700);
            BackColor = Color.FromArgb(239, 242, 246);
            Font = new Font("Segoe UI", 10F);

            Panel ust = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Koyu };
            Label marka = new Label
            {
                Dock = DockStyle.Fill,
                Text = "✦  NEXORA  /  RAPOR ÖNİZLEME",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };
            ust.Controls.Add(marka);

            FlowLayoutPanel alt = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                Padding = new Padding(14, 16, 16, 12),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White
            };
            Button kapat = Dugme("Kapat", Koyu);
            Button excel = Dugme("Excel İndir", Koyu);
            Button pdf = Dugme("PDF İndir", Mavi);
            kapat.Click += delegate { Close(); };
            excel.Click += delegate { ExcelKaydet(this, _baslik, _metin); };
            pdf.Click += delegate { PdfKaydet(this, _baslik, _metin); };
            alt.Controls.Add(kapat);
            alt.Controls.Add(excel);
            alt.Controls.Add(pdf);

            // Önizleme: başlık, konu, tarih ve metin AYRI tablo satırlarında.
            // AutoSize sayesinde Windows/Parallels ekran ölçeklemesinde üst üste binmezler.
            Panel dis = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(22, 18, 22, 18),
                BackColor = Color.FromArgb(239, 242, 246)
            };
            Panel kagit = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24, 18, 24, 20)
            };

            TableLayoutPanel duzen = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            duzen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            duzen.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            duzen.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            TableLayoutPanel baslikAlani = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(0, 0, 0, 14),
                Padding = Padding.Empty,
                BackColor = Color.White
            };
            baslikAlani.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            baslikAlani.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            baslikAlani.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            baslikAlani.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            baslikAlani.RowStyles.Add(new RowStyle(SizeType.Absolute, 2F));

            Label ad = new Label
            {
                Text = "NEXORA AI RAPORU",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
                ForeColor = Mavi,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold)
            };
            Label konu = new Label
            {
                Text = _baslik,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 7),
                ForeColor = Koyu,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            Label tarih = new Label
            {
                Text = "Oluşturulma: " + _olusturmaZamani.ToString("dd.MM.yyyy HH:mm"),
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                ForeColor = Color.FromArgb(100, 110, 122),
                Font = new Font("Segoe UI", 9F)
            };
            Panel ayirici = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(225, 230, 237),
                Margin = Padding.Empty
            };
            baslikAlani.Controls.Add(ad, 0, 0);
            baslikAlani.Controls.Add(konu, 0, 1);
            baslikAlani.Controls.Add(tarih, 0, 2);
            baslikAlani.Controls.Add(ayirici, 0, 3);

            RichTextBox icerik = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Koyu,
                Font = new Font("Segoe UI", 11F),
                Text = _metin,
                Margin = Padding.Empty,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
                WordWrap = true
            };
            duzen.Controls.Add(baslikAlani, 0, 0);
            duzen.Controls.Add(icerik, 0, 1);
            kagit.Controls.Add(duzen);
            dis.Controls.Add(kagit);
            Controls.Add(dis);
            Controls.Add(alt);
            Controls.Add(ust);
        }

        private static Button Dugme(string yazi, Color renk)
        {
            Button b = new Button
            {
                Text = yazi,
                Width = 125,
                Height = 38,
                Margin = new Padding(6, 0, 0, 0),
                BackColor = renk,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // Yazıcıya bağımlı değildir: PDF sayfaları Windows GDI+ ile görüntü olarak
        // çizilir ve standart PDF dosyasına gömülür. Türkçe karakterleri korur.
        // Not: PDF içindeki metin görüntü tabanlıdır, seçilebilir metin değildir.
        public static void PdfKaydet(IWin32Window sahibi, string baslik, string metin)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "PDF raporunu kaydet";
                dlg.Filter = "PDF dosyası (*.pdf)|*.pdf";
                dlg.DefaultExt = "pdf";
                dlg.AddExtension = true;
                dlg.FileName = "NEXORA_Rapor_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                if (dlg.ShowDialog(sahibi) != DialogResult.OK) return;
                try
                {
                    List<byte[]> sayfalar = PdfSayfalariniOlustur(baslik ?? "NEXORA AI Raporu", metin ?? "");
                    string gecici = dlg.FileName + ".tmp";
                    try
                    {
                        using (FileStream dosya = new FileStream(gecici, FileMode.Create, FileAccess.Write))
                            PdfDosyasiniYaz(dosya, sayfalar);
                        if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);
                        File.Move(gecici, dlg.FileName);
                    }
                    finally
                    {
                        if (File.Exists(gecici)) File.Delete(gecici);
                    }
                    MessageBox.Show(sahibi, "PDF kaydedildi:\n" + dlg.FileName, "NEXORA",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(sahibi, "PDF oluşturulamadı:\n" + ex.Message, "NEXORA",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static List<byte[]> PdfSayfalariniOlustur(string baslik, string metin)
        {
            const int genislik = 1240;  // A4 oranında yaklaşık 150 dpi
            const int yukseklik = 1754;
            const float sol = 100F;
            const float sag = 1140F;
            const float icerikUst = 255F;
            const float icerikAlt = 1640F;
            List<byte[]> sayfalar = new List<byte[]>();
            string[] satirlar = metin.Replace("\r", "").Split('\n');
            DateTime zaman = DateTime.Now;
            using (Font govde = new Font("Segoe UI", 17F, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Font baslikFont = new Font("Segoe UI", 30F, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font konuFont = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font kucuk = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                using (Bitmap olcuResmi = new Bitmap(1, 1))
                using (Graphics olcu = Graphics.FromImage(olcuResmi))
                {
                    List<string> gorunenSatirlar = new List<string>();
                    foreach (string satir in satirlar)
                        gorunenSatirlar.AddRange(SatiriBol(olcu, satir, govde, sag - sol));
                    if (gorunenSatirlar.Count == 0) gorunenSatirlar.Add("");
                    int konum = 0;
                    do
                    {
                        using (Bitmap resim = new Bitmap(genislik, yukseklik))
                        using (Graphics g = Graphics.FromImage(resim))
                        {
                            g.Clear(Color.White);
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                            using (Brush koyu = new SolidBrush(Koyu))
                            using (Brush mavi = new SolidBrush(Mavi))
                            using (Brush gri = new SolidBrush(Color.FromArgb(105, 115, 125)))
                            {
                                g.FillRectangle(koyu, 0, 0, genislik, 105);
                                g.DrawString("NEXORA  |  İŞLETME YÖNETİM PLATFORMU", konuFont,
                                    Brushes.White, sol, 36F);
                                g.DrawString("NEXORA AI RAPORU", baslikFont, mavi, sol, 138F);
                                g.DrawString(baslik, konuFont, koyu,
                                    new RectangleF(sol, 187F, sag - sol, 36F));
                                g.DrawString("Oluşturulma: " + zaman.ToString("dd.MM.yyyy HH:mm"),
                                    kucuk, gri, sol, 230F);
                                using (Pen cizgi = new Pen(Color.FromArgb(220, 226, 233), 2F))
                                    g.DrawLine(cizgi, sol, 249F, sag, 249F);
                                float y = icerikUst;
                                while (konum < gorunenSatirlar.Count)
                                {
                                    string satir = gorunenSatirlar[konum];
                                    float yuk = string.IsNullOrEmpty(satir) ? 20F : 31F;
                                    if (y + yuk > icerikAlt) break;
                                    if (satir.Length > 0)
                                        g.DrawString(satir, govde, koyu, new PointF(sol, y));
                                    y += yuk;
                                    konum++;
                                }
                                g.DrawString("NEXORA  •  Sayfa " + (sayfalar.Count + 1),
                                    kucuk, gri, sol, 1690F);
                            }
                            using (MemoryStream jpg = new MemoryStream())
                            {
                                resim.Save(jpg, ImageFormat.Jpeg);
                                sayfalar.Add(jpg.ToArray());
                            }
                        }
                    } while (konum < gorunenSatirlar.Count);
                }
            }
            return sayfalar;
        }

        private static List<string> SatiriBol(Graphics g, string satir, Font font, float genislik)
        {
            List<string> sonuc = new List<string>();
            if (string.IsNullOrEmpty(satir)) { sonuc.Add(""); return sonuc; }
            string kalan = satir;
            while (kalan.Length > 0)
            {
                if (g.MeasureString(kalan, font).Width <= genislik)
                {
                    sonuc.Add(kalan);
                    break;
                }
                int kes = 1;
                while (kes < kalan.Length && g.MeasureString(kalan.Substring(0, kes + 1), font).Width <= genislik)
                    kes++;
                int bosluk = kalan.LastIndexOf(' ', Math.Min(kes - 1, kalan.Length - 1));
                if (bosluk > 0) kes = bosluk;
                sonuc.Add(kalan.Substring(0, kes).TrimEnd());
                kalan = kalan.Substring(kes).TrimStart();
            }
            return sonuc;
        }

        private static void PdfDosyasiniYaz(Stream hedef, List<byte[]> sayfalar)
        {
            // PDF 1.4: catalog=1, pages=2, her sayfa için page/image/content nesneleri.
            int nesneSayisi = 2 + sayfalar.Count * 3;
            long[] konumlar = new long[nesneSayisi + 1];
            using (BinaryWriter w = new BinaryWriter(hedef, Encoding.ASCII, true))
            {
                PdfAscii(w, "%PDF-1.4\n");
                PdfNesne(w, konumlar, 1, "<< /Type /Catalog /Pages 2 0 R >>");
                StringBuilder cocuklar = new StringBuilder();
                for (int i = 0; i < sayfalar.Count; i++)
                    cocuklar.Append(3 + i * 3).Append(" 0 R ");
                PdfNesne(w, konumlar, 2, "<< /Type /Pages /Kids [ " + cocuklar + "] /Count " + sayfalar.Count + " >>");
                for (int i = 0; i < sayfalar.Count; i++)
                {
                    int sayfa = 3 + i * 3;
                    int resim = sayfa + 1;
                    int icerik = sayfa + 2;
                    PdfNesne(w, konumlar, sayfa,
                        "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595.28 841.89] " +
                        "/Resources << /XObject << /Im0 " + resim + " 0 R >> >> " +
                        "/Contents " + icerik + " 0 R >>");
                    konumlar[resim] = hedef.Position;
                    PdfAscii(w, resim + " 0 obj\n<< /Type /XObject /Subtype /Image /Width 1240 /Height 1754 " +
                        "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " +
                        sayfalar[i].Length + " >>\nstream\n");
                    w.Write(sayfalar[i]);
                    PdfAscii(w, "\nendstream\nendobj\n");
                    byte[] komut = Encoding.ASCII.GetBytes("q\n595.28 0 0 841.89 0 0 cm\n/Im0 Do\nQ\n");
                    konumlar[icerik] = hedef.Position;
                    PdfAscii(w, icerik + " 0 obj\n<< /Length " + komut.Length + " >>\nstream\n");
                    w.Write(komut);
                    PdfAscii(w, "endstream\nendobj\n");
                }
                long xref = hedef.Position;
                PdfAscii(w, "xref\n0 " + (nesneSayisi + 1) + "\n0000000000 65535 f \n");
                for (int i = 1; i <= nesneSayisi; i++)
                    PdfAscii(w, konumlar[i].ToString("D10") + " 00000 n \n");
                PdfAscii(w, "trailer\n<< /Size " + (nesneSayisi + 1) + " /Root 1 0 R >>\n" +
                    "startxref\n" + xref + "\n%%EOF\n");
            }
        }

        private static void PdfNesne(BinaryWriter w, long[] konumlar, int no, string govde)
        {
            konumlar[no] = w.BaseStream.Position;
            PdfAscii(w, no + " 0 obj\n" + govde + "\nendobj\n");
        }

        private static void PdfAscii(BinaryWriter w, string s)
        {
            w.Write(Encoding.ASCII.GetBytes(s));
        }

        // Çalışan Excel dışa aktarma kodu korunmuştur.
        public static void ExcelKaydet(IWin32Window sahibi, string baslik, string metin)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Excel raporunu kaydet";
                dlg.Filter = "Excel çalışma kitabı (*.xlsx)|*.xlsx";
                dlg.FileName = "NEXORA_Rapor_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                if (dlg.ShowDialog(sahibi) != DialogResult.OK) return;
                try
                {
                    using (FileStream dosya = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                    using (ZipArchive zip = new ZipArchive(dosya, ZipArchiveMode.Create))
                    {
                        Ekle(zip, "[Content_Types].xml",
                            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                            "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                            "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                            "</Types>");
                        Ekle(zip, "_rels/.rels",
                            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                            "</Relationships>");
                        Ekle(zip, "xl/workbook.xml",
                            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                            "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                            "<sheets><sheet name=\"NEXORA Rapor\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                        Ekle(zip, "xl/_rels/workbook.xml.rels",
                            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                            "</Relationships>");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><cols><col min=\"1\" max=\"1\" width=\"100\" customWidth=\"1\"/></cols><sheetData>");
                        string[] satirlar = (baslik + "\n" + DateTime.Now.ToString("dd.MM.yyyy HH:mm") +
                            "\n\n" + metin).Replace("\r", "").Split('\n');
                        for (int i = 0; i < satirlar.Length; i++)
                        {
                            sb.Append("<row r=\"").Append(i + 1).Append("\"><c r=\"A")
                              .Append(i + 1).Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">")
                              .Append(XmlGuvenli(satirlar[i])).Append("</t></is></c></row>");
                        }
                        sb.Append("</sheetData></worksheet>");
                        Ekle(zip, "xl/worksheets/sheet1.xml", sb.ToString());
                    }
                    MessageBox.Show(sahibi, "Excel kaydedildi:\n" + dlg.FileName, "NEXORA");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(sahibi, "Excel oluşturulamadı:\n" + ex.Message, "NEXORA",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void Ekle(ZipArchive zip, string ad, string icerik)
        {
            ZipArchiveEntry entry = zip.CreateEntry(ad, CompressionLevel.Optimal);
            using (StreamWriter yazar = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
                yazar.Write(icerik);
        }

        private static string XmlGuvenli(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
                if (c == '\t' || c == '\n' || c == '\r' || c >= 32)
                    sb.Append(c);
            return System.Security.SecurityElement.Escape(sb.ToString());
        }
    }
}
