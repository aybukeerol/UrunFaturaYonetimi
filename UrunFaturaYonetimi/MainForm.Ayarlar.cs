using System;
using System.Drawing;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
        private void AyarlarAc()
        {
            SayfayiTemizle("Ayarlar");

            pnlContent.AutoScroll = true;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            Panel page = new Panel();
            page.Location = new Point(0, 0);
            page.Size = new Size(Math.Max(1040, pnlContent.ClientSize.Width), 790);
            page.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            page.BackColor = Color.FromArgb(246, 249, 253);
            pnlContent.Controls.Add(page);

            // ÜST BAŞLIK
            RoundedPanel header = new RoundedPanel();
            header.Location = new Point(24, 22);
            header.Size = new Size(Math.Max(970, page.ClientSize.Width - 48), 120);
            header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            header.Radius = 10;
            header.FillColor = Color.White;
            header.BorderColor = _border;
            header.BorderThickness = 1;
            page.Controls.Add(header);

            Label title = new Label();
            title.Text = "Sistem Ayarları";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            title.ForeColor = _text;
            title.Location = new Point(20, 13);
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Firma profilini ve NEXORA uygulama bilgilerini tek ekrandan görüntüleyin.";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9.5F);
            subtitle.ForeColor = _muted;
            subtitle.Location = new Point(22, 69);
            header.Controls.Add(subtitle);

            Label active = new Label();
            active.Text = "● Sistem Aktif";
            active.AutoSize = true;
            active.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            active.ForeColor = Color.FromArgb(22, 132, 75);
            active.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            header.Controls.Add(active);

            // ÖZET KARTLARI
            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Location = new Point(24, 160);
            stats.Size = new Size(Math.Max(970, page.ClientSize.Width - 48), 132);
            stats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            stats.ColumnCount = 4;
            stats.RowCount = 1;
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            page.Controls.Add(stats);

            string[] statBaslik = { "AKTİF KULLANICI", "CARİ", "ÜRÜN / HİZMET", "FATURA" };
            string[] statDeger =
            {
                string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Aktif" : _kullaniciAdi,
                AppData.Cariler.Count.ToString(),
                AppData.Urunler.Count.ToString(),
                AppData.Faturalar.Count.ToString()
            };

            for (int i = 0; i < 4; i++)
            {
                RoundedPanel stat = new RoundedPanel();
                stat.Dock = DockStyle.Fill;
                stat.Margin = i == 0
                    ? new Padding(0, 0, 6, 0)
                    : (i == 3 ? new Padding(6, 0, 0, 0) : new Padding(3, 0, 3, 0));
                stat.Radius = 9;
                stat.FillColor = Color.White;
                stat.BorderColor = _border;
                stat.BorderThickness = 1;
                stats.Controls.Add(stat, i, 0);

                Label cap = new Label();
                cap.Text = statBaslik[i];
                cap.AutoSize = true;
                cap.Font = new Font("Segoe UI", 8.3F, FontStyle.Bold);
                cap.ForeColor = _muted;
                cap.Location = new Point(17, 16);
                stat.Controls.Add(cap);

                Label val = new Label();
                val.Text = statDeger[i];
                val.AutoSize = true;
                val.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
                val.ForeColor = i == 0 ? _primary : _text;
                val.Location = new Point(16, 43);
                stat.Controls.Add(val);
            }

            // SOL: FİRMA PROFİLİ
            RoundedPanel firmaCard = new RoundedPanel();
            firmaCard.Location = new Point(24, 312);
            firmaCard.Size = new Size(Math.Max(610, page.ClientSize.Width - 388), 545);
            firmaCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            firmaCard.Radius = 10;
            firmaCard.FillColor = Color.White;
            firmaCard.BorderColor = _border;
            firmaCard.BorderThickness = 1;
            page.Controls.Add(firmaCard);

            Label firmaTitle = new Label();
            firmaTitle.Text = "Firma Profili";
            firmaTitle.AutoSize = true;
            firmaTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            firmaTitle.ForeColor = _text;
            firmaTitle.Location = new Point(22, 18);
            firmaCard.Controls.Add(firmaTitle);

            Label firmaSub = new Label();
            firmaSub.Text = "Fatura ve belgelerde kullanılacak temel firma bilgileri";
            firmaSub.AutoSize = true;
            firmaSub.Font = new Font("Segoe UI", 9F);
            firmaSub.ForeColor = _muted;
            firmaSub.Location = new Point(23, 58);
            firmaCard.Controls.Add(firmaSub);

            firmaCard.Controls.Add(FormLabel("Firma Ünvanı", 24, 112));
            TextBox txtFirma = new TextBox();
            txtFirma.Location = new Point(24, 145);
            txtFirma.Size = new Size(520, 30);
            txtFirma.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtFirma);

            firmaCard.Controls.Add(FormLabel("Vergi / T.C. No", 24, 190));
            TextBox txtVergi = new TextBox();
            txtVergi.Location = new Point(24, 223);
            txtVergi.Size = new Size(245, 30);
            txtVergi.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtVergi);

            firmaCard.Controls.Add(FormLabel("Vergi Dairesi", 299, 190));
            TextBox txtVergiDairesi = new TextBox();
            txtVergiDairesi.Location = new Point(299, 223);
            txtVergiDairesi.Size = new Size(245, 30);
            txtVergiDairesi.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtVergiDairesi);

            firmaCard.Controls.Add(FormLabel("E-Posta", 24, 268));
            TextBox txtMail = new TextBox();
            txtMail.Location = new Point(24, 301);
            txtMail.Size = new Size(245, 30);
            txtMail.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtMail);

            firmaCard.Controls.Add(FormLabel("Telefon", 299, 268));
            TextBox txtTelefon = new TextBox();
            txtTelefon.Location = new Point(299, 301);
            txtTelefon.Size = new Size(245, 30);
            txtTelefon.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtTelefon);

            firmaCard.Controls.Add(FormLabel("Adres", 24, 346));
            TextBox txtAdres = new TextBox();
            txtAdres.Location = new Point(24, 379);
            txtAdres.Size = new Size(520, 58);
            txtAdres.Multiline = true;
            txtAdres.Font = new Font("Segoe UI", 10F);
            firmaCard.Controls.Add(txtAdres);

            // SAĞ: UYGULAMA BİLGİLERİ
            RoundedPanel appCard = new RoundedPanel();
            appCard.Location = new Point(Math.Max(674, page.ClientSize.Width - 340), 312);
            appCard.Size = new Size(316, 545);
            appCard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            appCard.Radius = 10;
            appCard.FillColor = Color.White;
            appCard.BorderColor = _border;
            appCard.BorderThickness = 1;
            page.Controls.Add(appCard);

            Label appTitle = new Label();
            appTitle.Text = "NEXORA";
            appTitle.AutoSize = true;
            appTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            appTitle.ForeColor = _primary;
            appTitle.Location = new Point(22, 18);
            appCard.Controls.Add(appTitle);

            Label appSub = new Label();
            appSub.Text = "Uygulama Bilgileri";
            appSub.AutoSize = true;
            appSub.Font = new Font("Segoe UI", 9F);
            appSub.ForeColor = _muted;
            appSub.Location = new Point(23, 58);
            appCard.Controls.Add(appSub);

            string[] bilgiBaslik = { "Oturum", "Görünüm", "Veri Durumu", "Çalışma Alanı" };
            string[] bilgiDeger =
            {
                string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Aktif Kullanıcı" : _kullaniciAdi,
                _isDarkMode ? "Koyu Tema" : "Açık Tema",
                "Erişilebilir",
                "NEXORA ERP"
            };

            for (int i = 0; i < 4; i++)
            {
                Label b = new Label();
                b.Text = bilgiBaslik[i];
                b.AutoSize = true;
                b.Font = new Font("Segoe UI", 8.3F, FontStyle.Bold);
                b.ForeColor = _muted;
                b.Location = new Point(24, 104 + i * 88);
                appCard.Controls.Add(b);

                Label d = new Label();
                d.Text = bilgiDeger[i];
                d.AutoSize = true;
                d.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                d.ForeColor = i == 2 ? Color.FromArgb(22, 132, 75) : _text;
                d.Location = new Point(23, 132 + i * 88);
                appCard.Controls.Add(d);
            }

            Label note = new Label();
            note.Text = "Firma bilgileri için projede kalıcı bir ayar tablosu bulunmadığından bu ekran mevcut veritabanı yapısına müdahale etmez.";
            note.Size = new Size(266, 65);
            note.Font = new Font("Segoe UI", 8.2F);
            note.ForeColor = _muted;
            note.Location = new Point(24, 468);
            appCard.Controls.Add(note);

            // YERLEŞİM
            Action yerlesim = delegate
            {
                header.Width = Math.Max(970, page.ClientSize.Width - 48);
                active.Location = new Point(
                    Math.Max(700, header.ClientSize.Width - active.Width - 22), 46);

                stats.Width = Math.Max(970, page.ClientSize.Width - 48);

                firmaCard.Width = Math.Max(610, page.ClientSize.Width - 388);
                appCard.Left = Math.Max(674, page.ClientSize.Width - 340);
            };

            page.Resize += delegate { yerlesim(); };
            yerlesim();
        }

    }
}