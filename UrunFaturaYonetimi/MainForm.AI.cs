using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
        private Panel pnlAiAssistant;
        private FlowLayoutPanel flpAiMessages;
        private TextBox txtAiMessage;
        private Button btnAiFloating;
        private bool _aiPanelOpen;
        private void AiAsistaniOlustur()
        {
            btnAiFloating = new Button();
            btnAiFloating.Text = "✦  NEXORA AI";
            btnAiFloating.Size = new Size(150, 44);
            btnAiFloating.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnAiFloating.BackColor = _primary;
            btnAiFloating.ForeColor = Color.White;
            btnAiFloating.FlatStyle = FlatStyle.Flat;
            btnAiFloating.FlatAppearance.BorderSize = 0;
            btnAiFloating.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAiFloating.Cursor = Cursors.Hand;
            btnAiFloating.Click += delegate { AiPaneliniAcKapat(); };
            Controls.Add(btnAiFloating);

            pnlAiAssistant = new Panel();
            pnlAiAssistant.BackColor = Color.FromArgb(40, 44, 47);
            pnlAiAssistant.Visible = false;
            pnlAiAssistant.MinimumSize = new Size(410, 560);

            Label aiTitle = new Label();
            aiTitle.Text = "✦  NEXORA AI";
            aiTitle.ForeColor = Color.White;
            aiTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            aiTitle.AutoSize = true;
            aiTitle.Location = new Point(20, 17);
            pnlAiAssistant.Controls.Add(aiTitle);

            Label aiSub = new Label();
            aiSub.Text = "İşletme verilerinizi analiz edin ve doğal dille sorgulayın";
            aiSub.ForeColor = Color.FromArgb(185, 195, 205);
            aiSub.Font = new Font("Segoe UI", 9F);
            aiSub.AutoSize = true;
            aiSub.Location = new Point(22, 49);
            pnlAiAssistant.Controls.Add(aiSub);

            Button close = new Button();
            close.Text = "×";
            close.Size = new Size(40, 38);
            close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            close.FlatStyle = FlatStyle.Flat;
            close.FlatAppearance.BorderSize = 0;
            close.BackColor = Color.FromArgb(40, 44, 47);
            close.ForeColor = Color.White;
            close.Font = new Font("Segoe UI", 16F);
            close.Click += delegate { AiPaneliniAcKapat(); };
            pnlAiAssistant.Controls.Add(close);

            Panel line = new Panel();
            line.Height = 1;
            line.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            line.BackColor = Color.FromArgb(68, 74, 79);
            pnlAiAssistant.Controls.Add(line);

            flpAiMessages = new FlowLayoutPanel();
            flpAiMessages.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            flpAiMessages.FlowDirection = FlowDirection.TopDown;
            flpAiMessages.WrapContents = false;
            flpAiMessages.AutoScroll = true;
            flpAiMessages.HorizontalScroll.Enabled = false;
            flpAiMessages.HorizontalScroll.Visible = false;
            flpAiMessages.AutoScrollMinSize = new Size(0, 0);
            flpAiMessages.BackColor = Color.FromArgb(40, 44, 47);
            flpAiMessages.Padding = new Padding(4);
            pnlAiAssistant.Controls.Add(flpAiMessages);

            Panel inputHost = new Panel();
            inputHost.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            inputHost.BackColor = Color.FromArgb(52, 58, 63);
            pnlAiAssistant.Controls.Add(inputHost);

            txtAiMessage = new TextBox();
            txtAiMessage.BorderStyle = BorderStyle.None;
            txtAiMessage.BackColor = inputHost.BackColor;
            txtAiMessage.ForeColor = Color.White;
            txtAiMessage.Font = new Font("Segoe UI", 10F);
            txtAiMessage.Anchor =
                AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtAiMessage.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    AiMesajiniGonder();
                    e.SuppressKeyPress = true;
                }
            };
            inputHost.Controls.Add(txtAiMessage);

            Button send = new Button();
            send.Name = "btnAiSend";
            send.Text = "➤";
            send.Size = new Size(48, 42);
            send.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            send.BackColor = _primary;
            send.ForeColor = Color.White;
            send.FlatStyle = FlatStyle.Flat;
            send.FlatAppearance.BorderSize = 0;
            send.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            send.Click += delegate { AiMesajiniGonder(); };
            inputHost.Controls.Add(send);

            Controls.Add(pnlAiAssistant);

            Resize += delegate { AiKontrolleriniKonumlandir(); };
            pnlAiAssistant.Resize += delegate
            {
                AiPanelIciKonumlandir();
                AiMesajGenislikleriniGuncelle();
            };

            flpAiMessages.Resize += delegate
            {
                AiMesajGenislikleriniGuncelle();
            };

            AiKontrolleriniKonumlandir();
            AiPanelIciKonumlandir();

            AiMesajBalonuEkle(
                "Merhaba! Ben NEXORA AI.\r\n\r\n" +
                "Fatura verilerinizi analiz edebilirim. Örneğin:\r\n" +
                "• Bu ay işler nasıl?\r\n" +
                "• ABC Mobilya'nın bu ayki faturalarını getir\r\n" +
                "• ABC'nin son 3 faturasını göster\r\n" +
                "• Bu ay en çok hangi müşteriye fatura kestim?\r\n" +
                "• Bu ay ile geçen ayı karşılaştır\r\n" +
                "• 10.000 TL üzerindeki faturaları göster\r\n" +
                "• En yüksek faturam hangisi?",
                false);
        }
        private void AiKontrolleriniKonumlandir()
        {
            if (btnAiFloating != null)
            {
                btnAiFloating.Location = new Point(
                    Math.Max(10, ClientSize.Width - btnAiFloating.Width - 24),
                    Math.Max(10, ClientSize.Height - btnAiFloating.Height - 24));
                btnAiFloating.BringToFront();
            }

            if (pnlAiAssistant != null)
            {
                // Açıldığında ekranın yaklaşık dörtte biri genişliğinde,
                // yaklaşık %72 yüksekliğinde kullanışlı bir panel.
                int genislik = Math.Max(500, (int)(ClientSize.Width * 0.30));
                int yukseklik = Math.Max(590, (int)(ClientSize.Height * 0.72));

                genislik = Math.Min(genislik, Math.Max(430, ClientSize.Width - 60));
                yukseklik = Math.Min(yukseklik, Math.Max(500, ClientSize.Height - 110));

                pnlAiAssistant.Size = new Size(genislik, yukseklik);
                pnlAiAssistant.Location = new Point(
                    Math.Max(10, ClientSize.Width - genislik - 24),
                    Math.Max(10, ClientSize.Height - yukseklik - 82));

                AiPanelIciKonumlandir();

                if (pnlAiAssistant.Visible)
                    pnlAiAssistant.BringToFront();
            }
        }
        private void AiPanelIciKonumlandir()
        {
            if (pnlAiAssistant == null)
                return;

            foreach (Control c in pnlAiAssistant.Controls)
            {
                if (c is Label && c.Text == "✦  NEXORA AI")
                {
                    // title
                }
                else if (c is Button && c.Text == "×")
                {
                    c.Left = pnlAiAssistant.ClientSize.Width - c.Width - 12;
                    c.Top = 10;
                }
                else if (c is Panel && c.Height == 1)
                {
                    c.Location = new Point(0, 76);
                    c.Width = pnlAiAssistant.ClientSize.Width;
                }
            }

            if (flpAiMessages != null)
            {
                flpAiMessages.Location = new Point(14, 88);
                flpAiMessages.Size = new Size(
                    pnlAiAssistant.ClientSize.Width - 28,
                    pnlAiAssistant.ClientSize.Height - 168);
            }

            Panel inputHost = null;
            foreach (Control c in pnlAiAssistant.Controls)
            {
                Panel p = c as Panel;
                if (p != null && p != flpAiMessages && p.Height != 1 &&
                    p.BackColor == Color.FromArgb(52, 58, 63))
                {
                    inputHost = p;
                    break;
                }
            }

            if (inputHost != null)
            {
                inputHost.Location = new Point(
                    14,
                    pnlAiAssistant.ClientSize.Height - 66);
                inputHost.Size = new Size(
                    pnlAiAssistant.ClientSize.Width - 28,
                    52);

                if (txtAiMessage != null)
                {
                    txtAiMessage.Location = new Point(14, 17);
                    txtAiMessage.Width = inputHost.ClientSize.Width - 78;
                }

                Control[] sends = inputHost.Controls.Find("btnAiSend", true);
                if (sends.Length > 0)
                {
                    sends[0].Left = inputHost.ClientSize.Width - sends[0].Width - 5;
                    sends[0].Top = 5;
                }
            }
        }
        private void AiMesajGenislikleriniGuncelle()
        {
            if (flpAiMessages == null)
                return;

            int maxWidth =
                Math.Max(
                    320,
                    flpAiMessages.ClientSize.Width - 24);

            foreach (Control c in flpAiMessages.Controls)
            {
                FlowLayoutPanel raporSatiri = c as FlowLayoutPanel;
                if (raporSatiri != null && object.Equals(raporSatiri.Tag, "NEXORA_RAPOR_BUTONLARI"))
                {
                    int yeniGenislik = Math.Max(300, flpAiMessages.ClientSize.Width - 30);
                    if (raporSatiri.Width != yeniGenislik)
                        raporSatiri.Width = yeniGenislik;
                    continue;
                }

                Label bubble = c as Label;
                if (bubble == null)
                    continue;

                bool kullanici =
                    bubble.BackColor == _primary;

                bubble.MaximumSize =
                    new Size(
                        maxWidth,
                        1400);

                int metinGenisligi =
                    kullanici
                    ? maxWidth - 24
                    : Math.Max(280, maxWidth - 44);

                Size olcu =
                    TextRenderer.MeasureText(
                        bubble.Text,
                        bubble.Font,
                        new Size(
                            metinGenisligi,
                            1400),
                        TextFormatFlags.WordBreak);

                int genislik =
                    kullanici
                    ? Math.Min(
                        maxWidth,
                        Math.Max(
                            180,
                            olcu.Width + 28))
                    : Math.Max(
                        280,
                        maxWidth - 20);

                bubble.Size =
                    new Size(
                        genislik,
                        olcu.Height + 24);

                bubble.Margin =
                    kullanici
                    ? new Padding(42, 6, 6, 6)
                    : new Padding(6, 6, 12, 6);
            }

            flpAiMessages.HorizontalScroll.Enabled = false;
            flpAiMessages.HorizontalScroll.Visible = false;
        }
        private void AiPaneliniAcKapat()
        {
            _aiPanelOpen = !_aiPanelOpen;
            pnlAiAssistant.Visible = _aiPanelOpen;

            if (_aiPanelOpen)
            {
                AiKontrolleriniKonumlandir();
                pnlAiAssistant.BringToFront();
                txtAiMessage.Focus();
            }
        }

        private void AiMesajiniGonder()
        {
            if (txtAiMessage == null)
                return;

            string mesaj = txtAiMessage.Text.Trim();
            if (mesaj.Length == 0)
                return;

            txtAiMessage.Clear();
            AiMesajBalonuEkle(mesaj, true);

            string cevap = AiSorgusunuCalistir(mesaj);
            AiMesajBalonuEkle(cevap, false);
            if (!string.IsNullOrWhiteSpace(cevap) &&
                !cevap.StartsWith("Bu cümleyi henüz") &&
                !cevap.StartsWith("Şunları deneyebilirsiniz"))
            {
                AiRaporButonlariEkle(mesaj, cevap);
            }
        }

        private void AiMesajBalonuEkle(string mesaj, bool kullanici)
        {
            if (flpAiMessages == null)
                return;

            int maxWidth = Math.Max(320, flpAiMessages.ClientSize.Width - 24);

            Label bubble = new Label();
            bubble.AutoSize = false;
            bubble.MinimumSize = new Size(
                kullanici ? 180 : Math.Max(280, maxWidth - 20),
                0);
            bubble.MaximumSize = new Size(maxWidth, 1400);
            bubble.Font = new Font("Segoe UI", 9.5F);
            bubble.ForeColor = Color.White;
            bubble.BackColor =
                kullanici ? _primary : Color.FromArgb(56, 62, 68);
            bubble.Padding = new Padding(12, 10, 12, 10);
            bubble.Margin =
                kullanici
                ? new Padding(42, 6, 6, 6)
                : new Padding(6, 6, 12, 6);

            Size olcu = TextRenderer.MeasureText(
                mesaj,
                bubble.Font,
                new Size(maxWidth - 24, 1400),
                TextFormatFlags.WordBreak);

            int balonGenisligi =
                kullanici
                ? Math.Min(maxWidth, Math.Max(180, olcu.Width + 28))
                : Math.Max(280, maxWidth - 20);

            bubble.Size = new Size(
                balonGenisligi,
                olcu.Height + 24);

            bubble.Text = mesaj;
            flpAiMessages.Controls.Add(bubble);
            flpAiMessages.ScrollControlIntoView(bubble);
        }


        private void AiRaporButonlariEkle(string soru, string cevap)
        {
            if (flpAiMessages == null) return;

            FlowLayoutPanel satir = new FlowLayoutPanel();
            // Sabit ve yeterli yükseklik: Windows/Parallels DPI ölçeklemesinde
            // buton metinlerinin altının kesilmesini önler.
            satir.AutoSize = false;
            satir.WrapContents = false;
            satir.Tag = "NEXORA_RAPOR_BUTONLARI";
            satir.FlowDirection = FlowDirection.LeftToRight;
            satir.BackColor = Color.Transparent;
            satir.Margin = new Padding(6, 3, 6, 12);
            satir.Padding = new Padding(0);
            satir.Size = new Size(Math.Max(300, flpAiMessages.ClientSize.Width - 30), 56);

            string baslik = "NEXORA AI Raporu - " + soru;
            Button onizle = AiRaporButonu("Önizle");
            Button pdf = AiRaporButonu("PDF İndir");
            Button excel = AiRaporButonu("Excel İndir");

            onizle.Click += delegate
            {
                using (RaporOnizlemeForm form = new RaporOnizlemeForm(baslik, cevap))
                    form.ShowDialog(this);
            };
            pdf.Click += delegate { RaporOnizlemeForm.PdfKaydet(this, baslik, cevap); };
            excel.Click += delegate { RaporOnizlemeForm.ExcelKaydet(this, baslik, cevap); };

            satir.Controls.Add(onizle);
            satir.Controls.Add(pdf);
            satir.Controls.Add(excel);
            flpAiMessages.Controls.Add(satir);
            flpAiMessages.ScrollControlIntoView(satir);
        }

        private Button AiRaporButonu(string yazi)
        {
            Button b = new Button();
            b.Text = yazi;
            b.AutoSize = false;
            b.Size = new Size(yazi == "Önizle" ? 100 : 112, 44);
            b.Margin = new Padding(0, 0, 6, 0);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(100, 115, 130);
            b.BackColor = Color.FromArgb(65, 73, 82);
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.Padding = new Padding(0);
            b.UseCompatibleTextRendering = false;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            return b;
        }
    }
}