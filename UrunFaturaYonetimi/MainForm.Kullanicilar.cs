using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
        private void KullanicilarAc()
        {
            SayfayiTemizle("Kullanıcılar");

            pnlContent.AutoScroll = false;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.Padding = new Padding(24, 22, 24, 16);
            page.BackColor = Color.FromArgb(246, 249, 253);
            page.ColumnCount = 1;
            page.RowCount = 3;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 152F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlContent.Controls.Add(page);

            RoundedPanel header = new RoundedPanel();
            header.Dock = DockStyle.Fill;
            header.Margin = new Padding(0, 0, 0, 12);
            header.Radius = 10;
            header.FillColor = Color.White;
            header.BorderColor = _border;
            header.BorderThickness = 1;
            page.Controls.Add(header, 0, 0);

            Label title = new Label();
            title.Text = "Kullanıcı Yönetimi";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            title.ForeColor = _text;
            title.Location = new Point(20, 12);
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Sisteme erişebilen kullanıcı hesaplarını ve rollerini yönetin.";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9.5F);
            subtitle.ForeColor = _muted;
            subtitle.Location = new Point(22, 65);
            header.Controls.Add(subtitle);

            Button btnYeni = MaviButon("+ Yeni Kullanıcı");
            btnYeni.Size = new Size(150, 40);
            btnYeni.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnYeni.Click += delegate
            {
                using (RegisterForm form = new RegisterForm(_userService))
                {
                    form.ShowDialog(this);
                }

                KullanicilarAc();
            };
            header.Controls.Add(btnYeni);
            header.Resize += delegate
            {
                btnYeni.Location = new Point(
                    Math.Max(650, header.ClientSize.Width - btnYeni.Width - 20), 23);
            };

            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Dock = DockStyle.Fill;
            stats.Margin = new Padding(0, 0, 0, 12);
            stats.ColumnCount = 3;
            stats.RowCount = 1;
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            page.Controls.Add(stats, 0, 1);

            string[] st = { "AKTİF OTURUM", "SİSTEM ROLÜ", "HESAP DURUMU" };
            string[] sv = {
         string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Aktif Kullanıcı" : _kullaniciAdi,
         "Yönetici",
         "Aktif"
     };
            string[] sn = {
         "Şu anda uygulamada açık olan hesap",
         "Mevcut kullanıcı ekranındaki rol",
         "Sistem erişim durumu"
     };

            for (int i = 0; i < 3; i++)
            {
                RoundedPanel card = new RoundedPanel();
                card.Dock = DockStyle.Fill;
                card.Margin = i == 0
                    ? new Padding(0, 0, 6, 0)
                    : (i == 1 ? new Padding(3, 0, 3, 0) : new Padding(6, 0, 0, 0));
                card.Radius = 9;
                card.FillColor = Color.White;
                card.BorderColor = _border;
                card.BorderThickness = 1;

                Label cap = new Label();
                cap.Text = st[i];
                cap.AutoSize = true;
                cap.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                cap.ForeColor = _muted;
                cap.Location = new Point(18, 16);
                card.Controls.Add(cap);

                Label value = new Label();
                value.Text = sv[i];
                value.AutoSize = true;
                value.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
                value.ForeColor = i == 2 ? Color.FromArgb(21, 128, 61) : _text;
                value.Location = new Point(17, 43);
                card.Controls.Add(value);

                Label note = new Label();
                note.Text = sn[i];
                note.AutoSize = true;
                note.Font = new Font("Segoe UI", 8.5F);
                note.ForeColor = _muted;
                note.Location = new Point(19, 101);
                card.Controls.Add(note);

                stats.Controls.Add(card, i, 0);
            }

            RoundedPanel gridCard = new RoundedPanel();
            gridCard.Dock = DockStyle.Fill;
            gridCard.Margin = new Padding(0);
            gridCard.Padding = new Padding(12);
            gridCard.Radius = 10;
            gridCard.FillColor = Color.White;
            gridCard.BorderColor = _border;
            gridCard.BorderThickness = 1;
            page.Controls.Add(gridCard, 0, 2);

            DataGridView grid = TemelGrid();
            grid.Dock = DockStyle.Fill;
            grid.RowTemplate.Height = 44;
            grid.ColumnHeadersHeight = 42;

            KolonEkle(grid, "AdSoyad", "AD SOYAD");
            KolonEkle(grid, "KullaniciAdi", "KULLANICI ADI");
            KolonEkle(grid, "Rol", "ROL");
            KolonEkle(grid, "Durum", "DURUM");

            grid.Columns["AdSoyad"].FillWeight = 130;
            grid.Columns["KullaniciAdi"].FillWeight = 100;
            grid.Columns["Rol"].FillWeight = 80;
            grid.Columns["Durum"].FillWeight = 70;

            // Kullanıcılar, kayıt işleminin kullandığı users.xml dosyasından okunur.
            // Bu ekran yalnızca okuma yapar; mevcut kullanıcı kayıtlarını değiştirmez.
            string kullaniciDosyasi = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "users.xml");

            if (File.Exists(kullaniciDosyasi))
            {
                try
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(
                        typeof(List<UserAccount>));

                    using (FileStream stream = new FileStream(
                        kullaniciDosyasi, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var kullanicilar = serializer.Deserialize(stream) as List<UserAccount>;

                        if (kullanicilar != null)
                        {
                            foreach (UserAccount kullanici in kullanicilar)
                            {
                                grid.Rows.Add(
                                    kullanici.FullName,
                                    kullanici.Username,
                                    kullanici.Role,
                                    kullanici.IsActive ? "Aktif" : "Pasif");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Kullanıcı listesi okunamadı: " + ex.Message,
                        "Kullanıcılar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            gridCard.Controls.Add(grid);
        }

    }
}