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
    public partial class MainForm : Form
    {
        private readonly string _kullaniciAdi;
        private readonly IUserService _userService;

        private Panel pnlMenu;
        private Panel pnlTop;
        private Panel pnlContent;

        private Label lblSayfaBaslik;
        private Label lblKullanici;

        private TextBox txtAkilliArama;
        private ListBox lstAramaSonuclari;

        // Referans dashboard yerleşimi
        private TableLayoutPanel rootLayout;
        private TableLayoutPanel rightLayout;
        private Button btnTema;
        private bool _isDarkMode = false;
        // Sadece arayüz renklerinin ilk hâli saklanır; veri/işlem kodlarına dokunulmaz.
        private readonly Dictionary<Control, Color[]> _temaOrijinalRenkleri =
            new Dictionary<Control, Color[]>();
        private readonly Dictionary<DataGridView, Color[]> _temaGridRenkleri =
            new Dictionary<DataGridView, Color[]>();
        private readonly Dictionary<RoundedPanel, Color[]> _temaKartRenkleri =
            new Dictionary<RoundedPanel, Color[]>();
        private EventHandler _dashboardResizeHandler;

        // =========================================================
        // NEXORA AI - UYGULAMA İÇİ ASİSTAN
        // =========================================================
        private Panel pnlAiAssistant;
        private FlowLayoutPanel flpAiMessages;
        private TextBox txtAiMessage;
        private Button btnAiFloating;
        private bool _aiPanelOpen;

        // AI fatura hazırlama konuşma durumu.
        // Kullanıcıdan yalnızca veritabanında bulunmayan zorunlu bilgi istenir.

        

        private readonly Color _sidebar = Color.FromArgb(43, 48, 52);
        private readonly Color _sidebarHover = Color.FromArgb(55, 61, 66);
        private readonly Color _page = Color.FromArgb(246, 249, 253);
        private readonly Color _card = Color.White;
        private readonly Color _primary = Color.FromArgb(0, 88, 189);
        private readonly Color _text = Color.FromArgb(27, 31, 36);
        private readonly Color _muted = Color.FromArgb(94, 105, 118);
        private readonly Color _border = Color.FromArgb(224, 230, 237);

        private const int EmSetCueBanner = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam);

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

            UstPanelOlustur();

            IcerikPaneliOlustur();

            AramaSonucListesiniOlustur();

            DashboardAc();

            AiAsistaniOlustur();
            pnlContent.Resize +=
                delegate
                {
                    ListeGridleriniBoyutlandir();
                };

            // WinForms AutoScaleMode.Dpi ile ölçeklemeyi kendisi uygular.
            // Burada PerformAutoScale() çağırmıyoruz; ikinci kez ölçekleme
            // sidebar, kart ve yazıları orantısız küçültüyordu.
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
                _page;

            Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Regular,
                    GraphicsUnit.Point);

            // Tasarım 96 DPI bazında hazırlandı. Windows %125 / %150 ekran
            // ölçeğinde yalnızca yazıların değil bütün yerleşimin birlikte
            // büyümesini sağlar; kesilme ve üst üste binmeyi önler.
            AutoScaleDimensions =
                new SizeF(96F, 96F);

            AutoScaleMode =
                AutoScaleMode.Dpi;

            DoubleBuffered =
                true;

            rootLayout =
                new TableLayoutPanel();

            rootLayout.Dock =
                DockStyle.Fill;

            rootLayout.Margin =
                new Padding(0);

            rootLayout.Padding =
                new Padding(0);

            rootLayout.ColumnCount =
                2;

            rootLayout.RowCount =
                1;

            rootLayout.BackColor =
                _page;

            rootLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 19F));

            rootLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    81F));

            rootLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            rightLayout =
                new TableLayoutPanel();

            rightLayout.Dock =
                DockStyle.Fill;

            rightLayout.Margin =
                new Padding(0);

            rightLayout.Padding =
                new Padding(0);

            rightLayout.ColumnCount =
                1;

            rightLayout.RowCount =
                2;

            rightLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            rightLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    78F));

            rightLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            rightLayout.BackColor =
                _page;

            rootLayout.Controls.Add(
                rightLayout,
                1,
                0);

            Controls.Add(
                rootLayout);

            Click +=
                delegate
                {
                    AramaSonuclariniKapat();
                };
        }

        // =========================================================
        // SOL MENÜ
        // =========================================================

        private void SolMenuOlustur()
        {
            pnlMenu = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = _sidebar
            };

            TableLayoutPanel side = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0),
                ColumnCount = 1,
                RowCount = 3,
                BackColor = _sidebar
            };
            side.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 125F));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));

            // Logo ve alt başlık ayrı satırlarda: pencere/DPI değişince çakışmaz.
            Panel logoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = _sidebar
            };
            Label logo = new Label
            {
                Text = "NEXORA",
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(17, 14)
            };
            Label erp = new Label
            {
                Text = "ERP",
                AutoSize = false,
                Size = new Size(36, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = _primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            Label subtitle = new Label
            {
                Text = "İşletme Yönetim Platformu",
                AutoSize = false,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(190, 196, 202),
                Location = new Point(19, 74),
                Height = 35
            };
            Panel logoLine = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(66, 72, 77)
            };
            logoPanel.Controls.Add(logo);
            logoPanel.Controls.Add(erp);
            logoPanel.Controls.Add(subtitle);
            logoPanel.Controls.Add(logoLine);
            Action logoYerlesimi = delegate
            {
                erp.Left = logo.Right + 9;
                erp.Top = logo.Top + Math.Max(0, (logo.Height - erp.Height) / 2);
                subtitle.Top = logo.Bottom + 8;
                erp.Visible = erp.Right < logoPanel.ClientSize.Width - 12;
                subtitle.Width = Math.Max(80, logoPanel.ClientSize.Width - 36);
            };
            logoPanel.Resize += delegate { logoYerlesimi(); };
            logoYerlesimi();

            // Standart Windows simge fontuna bağımlı olmayan, çizilerek oluşturulan
            // sade kısaltmalar: eksik ikon/bozuk karakter sorunu oluşturmaz.
            FlowLayoutPanel menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0),
                Padding = new Padding(12, 12, 12, 8),
                BackColor = _sidebar
            };
            menu.Controls.Add(MenuButton("home", "Genel Bakış", Dashboard_Click));
            menu.Controls.Add(MenuButton("people", "Müşteriler", Musteriler_Click));
            menu.Controls.Add(MenuButton("building", "Kurumsal Müşteriler", Kurumsal_Click));
            menu.Controls.Add(MenuButton("wallet", "Cari Hesaplar", Cari_Click));
            menu.Controls.Add(MenuButton("box", "Ürün / Hizmetler", Urunler_Click));
            menu.Controls.Add(MenuButton("plus", "Yeni Fatura", YeniFatura_Click));
            menu.Controls.Add(MenuButton("invoice", "Faturalar", Faturalar_Click));
            menu.Controls.Add(MenuButton("user", "Kullanıcılar", Kullanicilar_Click));
            menu.Controls.Add(MenuButton("gear", "Ayarlar", Ayarlar_Click));
            Action menuYerlesimi = delegate
            {
                int width = Math.Max(120, menu.ClientSize.Width -
                    menu.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 4);
                foreach (Control item in menu.Controls)
                    item.Width = width;
            };
            menu.Resize += delegate { menuYerlesimi(); };

            Panel accountHost = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(9, 8, 9, 9),
                BackColor = _sidebar
            };
            RoundedPanel account = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 6,
                FillColor = Color.FromArgb(46, 52, 57),
                BorderColor = Color.FromArgb(72, 78, 83),
                BorderThickness = 1
            };
            PictureBox avatar = new PictureBox
            {
                Size = new Size(34, 34),
                Location = new Point(13, 23),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = IconBitmap("\uE77B", 15, Color.White, 34, _primary)
            };
            Label accountName = new Label
            {
                Text = string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Kullanıcı" : _kullaniciAdi,
                AutoSize = false,
                AutoEllipsis = true,
                Location = new Point(58, 18),
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label accountCaption = new Label
            {
                Text = "Hesabım",
                AutoSize = false,
                Location = new Point(58, 49),
                Height = 30,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(196, 201, 207),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Button logout = new Button
            {
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 52, 57),
                ForeColor = Color.White,
                Text = "↪",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            logout.FlatAppearance.BorderSize = 0;
            logout.AccessibleName = "Oturumu kapat";
            logout.Text = "";
            logout.Paint += delegate (object sender, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(232, 239, 247), 1.8F))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    e.Graphics.DrawLines(pen, new PointF[] {
                        new PointF(10, 8), new PointF(5, 8),
                        new PointF(5, 25), new PointF(10, 25) });
                    e.Graphics.DrawLine(pen, 11, 16, 26, 16);
                    e.Graphics.DrawLines(pen, new PointF[] {
                        new PointF(20, 10), new PointF(26, 16),
                        new PointF(20, 22) });
                }
            };
            logout.Click += delegate
            {
                DialogResult cevap = MessageBox.Show(this,
                    "Çıkmak istediğinize emin misiniz?",
                    "NEXORA - Oturumu Kapat",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (cevap == DialogResult.Yes) Close();
            };
            Action hesabiAc = delegate { HesabimAc(); };
            account.Cursor = Cursors.Hand;
            avatar.Cursor = Cursors.Hand;
            accountName.Cursor = Cursors.Hand;
            accountCaption.Cursor = Cursors.Hand;
            account.Click += delegate { hesabiAc(); };
            avatar.Click += delegate { hesabiAc(); };
            accountName.Click += delegate { hesabiAc(); };
            accountCaption.Click += delegate { hesabiAc(); };
            account.Controls.Add(avatar);
            account.Controls.Add(accountName);
            account.Controls.Add(accountCaption);
            account.Controls.Add(logout);
            Action hesapYerlesimi = delegate
            {
                logout.Left = account.ClientSize.Width - logout.Width - 8;
                logout.Top = Math.Max(7, (account.ClientSize.Height - logout.Height) / 2);
                accountName.Top = Math.Max(9, (account.ClientSize.Height - 64) / 2);
                accountCaption.Top = accountName.Bottom + 1;
                int metinGenisligi = Math.Max(45, logout.Left - accountName.Left - 7);
                accountName.Width = metinGenisligi;
                accountCaption.Width = metinGenisligi;
                avatar.Top = Math.Max(8, (account.ClientSize.Height - avatar.Height) / 2);
            };
            account.Resize += delegate { hesapYerlesimi(); };
            accountHost.Controls.Add(account);

            side.Controls.Add(logoPanel, 0, 0);
            side.Controls.Add(menu, 0, 1);
            side.Controls.Add(accountHost, 0, 2);
            pnlMenu.Controls.Add(side);
            rootLayout.Controls.Add(pnlMenu, 0, 0);
            menuYerlesimi();
            hesapYerlesimi();
        }

        // Menü simgeleri metin/font glifleri yerine GDI+ ile çizilir;
        // böylece Windows dilinde veya farklı DPI değerlerinde bozulmaz.
        private Button MenuButton(string glyph, string text, EventHandler click)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(230, 48),
                Margin = new Padding(0, 0, 0, 4),
                Padding = new Padding(46, 0, 5, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = _sidebar,
                ForeColor = Color.FromArgb(226, 230, 234),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = _sidebarHover;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 40, 44);
            button.Paint += delegate (object sender, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float x = 14F;
                float y = (button.Height - 20F) / 2F;
                using (Pen pen = new Pen(Color.FromArgb(223, 232, 241), 1.65F))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    switch (glyph)
                    {
                        case "home":
                            e.Graphics.DrawLines(pen, new PointF[] { new PointF(x, y + 9), new PointF(x + 10, y + 1), new PointF(x + 20, y + 9) });
                            e.Graphics.DrawLines(pen, new PointF[] { new PointF(x + 3, y + 9), new PointF(x + 3, y + 19), new PointF(x + 17, y + 19), new PointF(x + 17, y + 9) });
                            e.Graphics.DrawRectangle(pen, x + 8, y + 13, 4, 6);
                            break;
                        case "people":
                            e.Graphics.DrawEllipse(pen, x + 7, y + 1, 7, 7);
                            e.Graphics.DrawArc(pen, x + 4, y + 10, 14, 11, 185, 170);
                            e.Graphics.DrawEllipse(pen, x + 1, y + 6, 4, 4);
                            e.Graphics.DrawArc(pen, x, y + 12, 7, 7, 185, 120);
                            break;
                        case "building":
                            e.Graphics.DrawRectangle(pen, x + 2, y + 7, 16, 12);
                            e.Graphics.DrawRectangle(pen, x + 6, y + 2, 8, 5);
                            e.Graphics.DrawLine(pen, x, y + 10, x + 20, y + 10);
                            e.Graphics.DrawLine(pen, x + 6, y + 14, x + 8, y + 14);
                            e.Graphics.DrawLine(pen, x + 12, y + 14, x + 14, y + 14);
                            break;
                        case "wallet":
                            e.Graphics.DrawRectangle(pen, x + 1, y + 5, 18, 14);
                            e.Graphics.DrawRectangle(pen, x + 12, y + 10, 8, 5);
                            e.Graphics.DrawLine(pen, x + 4, y + 2, x + 16, y + 2);
                            break;
                        case "box":
                            e.Graphics.DrawLines(pen, new PointF[] { new PointF(x + 1, y + 6), new PointF(x + 10, y + 1), new PointF(x + 19, y + 6), new PointF(x + 10, y + 11), new PointF(x + 1, y + 6) });
                            e.Graphics.DrawLines(pen, new PointF[] { new PointF(x + 1, y + 6), new PointF(x + 1, y + 16), new PointF(x + 10, y + 21), new PointF(x + 19, y + 16), new PointF(x + 19, y + 6) });
                            e.Graphics.DrawLine(pen, x + 10, y + 11, x + 10, y + 21);
                            break;
                        case "plus":
                            e.Graphics.DrawRectangle(pen, x + 2, y + 2, 16, 17);
                            e.Graphics.DrawLine(pen, x + 10, y + 6, x + 10, y + 15);
                            e.Graphics.DrawLine(pen, x + 6, y + 10.5F, x + 14, y + 10.5F);
                            break;
                        case "invoice":
                            e.Graphics.DrawLines(pen, new PointF[] { new PointF(x + 3, y + 1), new PointF(x + 14, y + 1), new PointF(x + 18, y + 5), new PointF(x + 18, y + 19), new PointF(x + 3, y + 19), new PointF(x + 3, y + 1) });
                            e.Graphics.DrawLine(pen, x + 6, y + 9, x + 15, y + 9);
                            e.Graphics.DrawLine(pen, x + 6, y + 13, x + 15, y + 13);
                            break;
                        case "user":
                            e.Graphics.DrawEllipse(pen, x + 6, y + 1, 8, 8);
                            e.Graphics.DrawArc(pen, x + 2, y + 11, 16, 10, 185, 170);
                            break;
                        case "gear":
                            e.Graphics.DrawEllipse(pen, x + 4, y + 4, 12, 12);
                            e.Graphics.DrawEllipse(pen, x + 8, y + 8, 4, 4);
                            for (int i = 0; i < 8; i++)
                            {
                                double angle = i * Math.PI / 4;
                                float dx = (float)Math.Cos(angle);
                                float dy = (float)Math.Sin(angle);
                                e.Graphics.DrawLine(pen, x + 10 + dx * 7, y + 10 + dy * 7, x + 10 + dx * 10, y + 10 + dy * 10);
                            }
                            break;
                    }
                }
            };
            button.Click += click;
            return button;
        }

        // =========================================================
        // HESABIM - ana içerikte açılır; mevcut oturum işlemleri korunur.
        // =========================================================
        private void HesabimAc()
        {
            SayfayiTemizle("Hesabım");
            pnlContent.AutoScroll = true;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            Panel page = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(Math.Max(740, pnlContent.ClientSize.Width), 1030),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(246, 249, 253)
            };
            pnlContent.Controls.Add(page);

            RoundedPanel header = new RoundedPanel
            {
                Location = new Point(24, 22),
                Size = new Size(Math.Max(690, page.Width - 48), 118),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Radius = 9,
                FillColor = Color.White,
                BorderColor = _border,
                BorderThickness = 1
            };
            page.Controls.Add(header);
            Label title = new Label
            {
                Text = "Hesabım",
                Location = new Point(22, 19),
                AutoSize = true,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = _text
            };
            header.Controls.Add(title);
            header.Controls.Add(new Label
            {
                Text = "Profiliniz ve oturum ayarlarınız",
                Location = new Point(24, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = _muted
            });

            RoundedPanel profile = new RoundedPanel
            {
                Location = new Point(24, 156),
                Size = new Size(Math.Max(690, page.Width - 48), 208),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Radius = 9,
                FillColor = Color.White,
                BorderColor = _border,
                BorderThickness = 1
            };
            page.Controls.Add(profile);
            profile.Controls.Add(new Label
            {
                Text = "Profil Bilgileri",
                Location = new Point(22, 17),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = _text
            });
            profile.Controls.Add(new Label
            {
                Text = "Kullanıcı adı",
                Location = new Point(24, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = _muted
            });
            TextBox username = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Kullanıcı" : _kullaniciAdi,
                Location = new Point(24, 100),
                Size = new Size(360, 32),
                ReadOnly = true,
                BackColor = Color.FromArgb(247, 249, 252),
                ForeColor = _text,
                Font = new Font("Segoe UI", 10F)
            };
            profile.Controls.Add(username);
            profile.Controls.Add(new Label
            {
                Text = "Oturum durumu: Aktif",
                Location = new Point(24, 153),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(22, 132, 75)
            });

            RoundedPanel security = new RoundedPanel
            {
                Location = new Point(24, 380),
                Size = new Size(Math.Max(690, page.Width - 48), 370),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Radius = 9,
                FillColor = Color.White,
                BorderColor = _border,
                BorderThickness = 1
            };
            page.Controls.Add(security);
            security.Controls.Add(new Label
            {
                Text = "Şifremi Değiştir",
                Location = new Point(22, 17),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = _text
            });
            security.Controls.Add(new Label
            {
                Text = "Mevcut şifrenizi doğrulayarak yeni şifre belirleyin.",
                Location = new Point(24, 54),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = _muted
            });
            Func<string, int, TextBox> sifreAlani = delegate (string baslik, int y)
            {
                security.Controls.Add(new Label
                {
                    Text = baslik,
                    Location = new Point(24, y),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = _muted
                });
                TextBox alan = new TextBox
                {
                    Location = new Point(24, y + 23),
                    Size = new Size(350, 30),
                    UseSystemPasswordChar = true,
                    Font = new Font("Segoe UI", 10F)
                };
                security.Controls.Add(alan);
                return alan;
            };
            TextBox mevcutSifre = sifreAlani("Mevcut şifre", 91);
            TextBox yeniSifre = sifreAlani("Yeni şifre", 163);
            TextBox tekrarSifre = sifreAlani("Yeni şifre tekrar", 235);
            Button sifreKaydet = new Button
            {
                Text = "Şifreyi Güncelle",
                Location = new Point(405, 286),
                Size = new Size(190, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = _primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            sifreKaydet.FlatAppearance.BorderSize = 0;
            security.Controls.Add(sifreKaydet);
            sifreKaydet.Click += delegate
            {
                if (string.IsNullOrWhiteSpace(mevcutSifre.Text) ||
                    string.IsNullOrWhiteSpace(yeniSifre.Text) ||
                    string.IsNullOrWhiteSpace(tekrarSifre.Text))
                {
                    MessageBox.Show(this, "Lütfen üç şifre alanını da doldurun.",
                        "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (yeniSifre.Text != tekrarSifre.Text)
                {
                    MessageBox.Show(this, "Yeni şifreler birbiriyle eşleşmiyor.",
                        "Şifre Kontrolü", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!PasswordService.IsPasswordValid(yeniSifre.Text))
                {
                    MessageBox.Show(this, "Yeni şifre en az 8 karakter, harf ve rakam içermelidir.",
                        "Şifre Kontrolü", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    UserAccount oturum = _userService.Login(_kullaniciAdi, mevcutSifre.Text);
                    if (oturum == null)
                    {
                        MessageBox.Show(this, "Mevcut şifre doğru değil.",
                            "Kimlik Doğrulama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (PasswordService.VerifyPassword(yeniSifre.Text,
                        oturum.PasswordHash, oturum.PasswordSalt))
                    {
                        MessageBox.Show(this, "Yeni şifre mevcut şifreden farklı olmalıdır.",
                            "Şifre Kontrolü", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _userService.UpdatePassword(oturum, yeniSifre.Text);
                    mevcutSifre.Clear(); yeniSifre.Clear(); tekrarSifre.Clear();
                    MessageBox.Show(this, "Şifreniz başarıyla güncellendi.",
                        "Hesabım", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Şifre güncellenemedi: " + ex.Message,
                        "Hesabım", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            // Hesabım ekranında tema seçimi bulunmaz; tema üst çubuktan yönetilir.
            RoundedPanel session = new RoundedPanel
            {
                Location = new Point(24, 778),
                Size = new Size(Math.Max(690, page.Width - 48), 156),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Radius = 9,
                FillColor = Color.White,
                BorderColor = _border,
                BorderThickness = 1
            };
            page.Controls.Add(session);
            session.Controls.Add(new Label
            {
                Text = "Oturum İşlemleri",
                Location = new Point(22, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = _text
            });
            session.Controls.Add(new Label
            {
                Text = "Hesabınızdan güvenli şekilde çıkış yapabilirsiniz.",
                Location = new Point(24, 53),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = _muted
            });
            Button oturumuKapat = new Button
            {
                Text = "Çıkış Yap",
                Location = new Point(24, 93),
                Size = new Size(170, 42),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(190, 44, 54),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            oturumuKapat.FlatAppearance.BorderSize = 0;
            oturumuKapat.Click += delegate
            {
                if (MessageBox.Show(this, "Çıkmak istediğinize emin misiniz?",
                    "NEXORA - Oturumu Kapat", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                    == DialogResult.Yes) Close();
            };
            session.Controls.Add(oturumuKapat);
            if (_isDarkMode) TemaSayfaKontrolleriniUygula(pnlContent);
        }

        // =========================================================
        // ÜST PANEL
        // =========================================================

        private void UstPanelOlustur()
        {
            pnlTop =
                new Panel();

            pnlTop.Dock =
                DockStyle.Fill;

            pnlTop.Margin =
                new Padding(0);

            pnlTop.BackColor =
                Color.White;

            lblSayfaBaslik =
                new Label();

            lblSayfaBaslik.Text =
                "Genel Bakış";

            lblSayfaBaslik.Visible =
                false;

            pnlTop.Controls.Add(
                lblSayfaBaslik);

            RoundedPanel searchBox =
                new RoundedPanel();

            searchBox.Size =
                new Size(
                    610,
                    40);

            searchBox.Radius =
                4;

            searchBox.FillColor =
                Color.FromArgb(
                    247,
                    249,
                    252);

            searchBox.BorderColor =
                _border;

            searchBox.BorderThickness =
                1;

            PictureBox searchIcon =
                new PictureBox();

            searchIcon.Size =
                new Size(
                    28,
                    28);

            searchIcon.Location =
                new Point(
                    7,
                    6);

            searchIcon.SizeMode =
                PictureBoxSizeMode.CenterImage;

            searchIcon.Image =
                IconBitmap(
                    "\uE721",
                    13,
                    Color.FromArgb(
                        88,
                        100,
                        114),
                    22,
                    Color.Transparent);

            searchBox.Controls.Add(
                searchIcon);

            txtAkilliArama =
                new TextBox();

            txtAkilliArama.Location = new Point(36, 10);

            txtAkilliArama.Size = new Size(510, 24);

            txtAkilliArama.BorderStyle =
                BorderStyle.None;

            txtAkilliArama.BackColor =
                Color.FromArgb(
                    247,
                    249,
                    252);

            txtAkilliArama.ForeColor =
                Color.FromArgb(
                    55,
                    65,
                    77);

            txtAkilliArama.Font =
                new Font(
                    "Segoe UI",
                    15.5F,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

            txtAkilliArama.TextChanged +=
                AkilliArama_TextChanged;

            txtAkilliArama.KeyDown +=
                AkilliArama_KeyDown;

            CueBanner(
                txtAkilliArama,
                "Cari, ürün veya fatura ara (Ctrl + K)...");

            txtAkilliArama.Enter +=
                delegate
                {
                    if (!string.IsNullOrWhiteSpace(
                        txtAkilliArama.Text))
                    {
                        AkilliAramayiYap();
                    }
                };

            searchBox.Controls.Add(
                txtAkilliArama);

            Label shortcut =
                new Label();

            shortcut.Text =
                "Ctrl K";

            shortcut.AutoSize =
                false;

            shortcut.Size =
                new Size(
                    48,
                    22);

            shortcut.TextAlign =
                ContentAlignment.MiddleCenter;

            shortcut.BackColor =
                Color.FromArgb(
                    235,
                    239,
                    244);

            shortcut.ForeColor =
                Color.FromArgb(
                    67,
                    76,
                    87);

            shortcut.Font =
                new Font(
                    "Segoe UI",
                    13.5F,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

            searchBox.Controls.Add(
                shortcut);

            pnlTop.Controls.Add(
                searchBox);

            btnTema =
                new Button();

            btnTema.Text =
                "Koyu mod";

            btnTema.Size =
                new Size(
                    110,
                    36);

            btnTema.FlatStyle =
                FlatStyle.Flat;

            btnTema.FlatAppearance.BorderColor =
                _border;

            btnTema.FlatAppearance.BorderSize =
                1;

            btnTema.BackColor =
                Color.White;

            btnTema.ForeColor =
                _text;

            btnTema.Font =
                new Font(
                    "Segoe UI",
                    14.5F,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

            btnTema.Image =
                IconBitmap(
                    "\uE708",
                    12,
                    _text,
                    20,
                    Color.Transparent);

            btnTema.ImageAlign =
                ContentAlignment.MiddleLeft;

            btnTema.TextImageRelation =
                TextImageRelation.ImageBeforeText;

            btnTema.Cursor =
                Cursors.Hand;

            btnTema.Click +=
                delegate
                {
                    _isDarkMode =
                        !_isDarkMode;

                    TemaUygula();
                };

            pnlTop.Controls.Add(
                btnTema);

            Label divider =
                new Label();

            divider.Text =
                "|";

            divider.AutoSize =
                true;

            divider.ForeColor =
                Color.FromArgb(
                    206,
                    212,
                    220);

            divider.Font =
                new Font(
                    "Segoe UI",
                    24F,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

            pnlTop.Controls.Add(
                divider);

            lblKullanici =
                new Label();

            lblKullanici.Text =
                _kullaniciAdi;

            lblKullanici.AutoSize =
                false;

            lblKullanici.Size =
                new Size(
                    125,
                    20);

            lblKullanici.TextAlign =
                ContentAlignment.MiddleRight;

            lblKullanici.Font =
                new Font(
                    "Segoe UI",
                    14.5F,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

            lblKullanici.ForeColor =
                _text;

            pnlTop.Controls.Add(
                lblKullanici);

            Label userRole =
                new Label();

            userRole.Text =
                "Sistem Yöneticisi";

            userRole.AutoSize =
                false;

            userRole.Size =
                new Size(
                    125,
                    18);

            userRole.TextAlign =
                ContentAlignment.MiddleRight;

            userRole.Font =
                new Font(
                    "Segoe UI",
                    12.5F,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

            userRole.ForeColor =
                _muted;

            pnlTop.Controls.Add(
                userRole);

            PictureBox userAvatar =
                new PictureBox();

            userAvatar.Size =
                new Size(
                    38,
                    38);

            userAvatar.SizeMode =
                PictureBoxSizeMode.CenterImage;

            userAvatar.Image =
                IconBitmap(
                    "\uE77B",
                    17,
                    Color.White,
                    38,
                    _primary);

            pnlTop.Controls.Add(
                userAvatar);

            Panel bottom =
                new Panel();

            bottom.Dock =
                DockStyle.Bottom;

            bottom.Height =
                1;

            bottom.BackColor =
                _border;

            pnlTop.Controls.Add(
                bottom);

            pnlTop.Resize +=
                delegate
                {
                    int right =
                        pnlTop.ClientSize.Width -
                        24;

                    userAvatar.Left =
                        right -
                        userAvatar.Width;

                    userAvatar.Top =
                        20;

                    lblKullanici.Left =
                        userAvatar.Left -
                        lblKullanici.Width -
                        9;

                    lblKullanici.Top =
                        16;

                    userRole.Left =
                        lblKullanici.Left;

                    userRole.Top =
                        37;

                    divider.Left =
                        lblKullanici.Left -
                        14;

                    divider.Top =
                        26;

                    btnTema.Left =
                        divider.Left -
                        btnTema.Width -
                        13;

                    btnTema.Top =
                        21;

                    searchBox.Left =
                        24;

                    searchBox.Top =
                        19;

                    int maxWidth =
                        btnTema.Left -
                        searchBox.Left -
                        28;

                    searchBox.Width =
                        Math.Max(
                            240,
                            Math.Min(
                                610,
                                maxWidth));

                    shortcut.Left =
                        searchBox.Width -
                        shortcut.Width -
                        8;

                    shortcut.Top =
                        8;

                    txtAkilliArama.Width =
                        Math.Max(
                            110,
                            shortcut.Left -
                            txtAkilliArama.Left -
                            10);

                    AramaListesiKonumlandir();
                };

            rightLayout.Controls.Add(
                pnlTop,
                0,
                0);
        }

        // =========================================================
        // İÇERİK PANELİ
        // =========================================================

        private void IcerikPaneliOlustur()
        {
            pnlContent =
                new Panel();

            pnlContent.Dock =
                DockStyle.Fill;

            pnlContent.Margin =
                new Padding(0);

            pnlContent.AutoScroll =
                true;

            pnlContent.BackColor =
                _page;

            pnlContent.Click +=
                delegate
                {
                    AramaSonuclariniKapat();
                };

            rightLayout.Controls.Add(
                pnlContent,
                0,
                1);
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
                    11.5F);

            lstAramaSonuclari.BorderStyle =
                BorderStyle.FixedSingle;

            lstAramaSonuclari.BackColor =
                Color.White;

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

            Controls.Add(
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
            // Dashboard her yenilendiğinde eski canvas'ı yakalayan Resize
            // olayını kaldır. Aksi halde yenileme/tema değişimi sonrasında
            // kullanılmayan dashboard örnekleri bellekte kalır.
            if (_dashboardResizeHandler != null)
            {
                pnlContent.Resize -=
                    _dashboardResizeHandler;

                _dashboardResizeHandler =
                    null;
            }

            pnlContent.Controls.Clear();

            lblSayfaBaslik.Text =
                baslik;

            AramaSonuclariniKapat();
            _temaOrijinalRenkleri.Clear();
            _temaGridRenkleri.Clear();
            _temaKartRenkleri.Clear();
            // Yeni sayfanın kontrolleri oluşturulduktan sonra koyu temayı uygula.
            if (_isDarkMode)
            {
                BeginInvoke(new Action(delegate
                {
                    if (!IsDisposed && pnlContent != null && !pnlContent.IsDisposed)
                        TemaSayfaKontrolleriniUygula(pnlContent);
                }));
            }
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
                lstAramaSonuclari == null)
            {
                return;
            }

            Point ekranNoktasi =
                txtAkilliArama.PointToScreen(
                    new Point(
                        0,
                        txtAkilliArama.Height));

            Point formNoktasi =
                PointToClient(
                    ekranNoktasi);

            lstAramaSonuclari.Left =
                formNoktasi.X;

            lstAramaSonuclari.Top =
                formNoktasi.Y + 2;

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

            pnlContent.BackColor =
                _isDarkMode
                ? Color.FromArgb(15, 23, 42)
                : Color.FromArgb(246, 249, 252);

            Panel canvas = new Panel();
            canvas.Location = new Point(0, 0);
            canvas.Size = new Size(
                Math.Max(1100, pnlContent.ClientSize.Width),
                1540);
            canvas.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            canvas.BackColor = Color.Transparent;
            pnlContent.Controls.Add(canvas);

            int margin = 24;
            int gap = 22;

            // =====================================================
            // GİB DURUM ŞERİDİ
            // =====================================================
            RoundedPanel statusBar = new RoundedPanel();
            statusBar.Location = new Point(margin, 16);
            statusBar.Size = new Size(canvas.Width - margin * 2, 78);
            statusBar.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            statusBar.Radius = 5;
            statusBar.FillColor = CurrentCardColor();
            statusBar.BorderColor = CurrentBorderColor();
            statusBar.BorderThickness = 1;

            PictureBox statusDot = new PictureBox();
            statusDot.Size = new Size(18, 18);
            statusDot.Location = new Point(14, 30);
            statusDot.SizeMode = PictureBoxSizeMode.CenterImage;
            statusDot.Image = DotBitmap(_primary, 8, 18);
            statusBar.Controls.Add(statusDot);

            Label statusTitle = new Label();
            statusTitle.Text = "GİB Entegrasyon Servisi:";
            statusTitle.AutoSize = true;
            statusTitle.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            statusTitle.ForeColor = CurrentPrimaryText();
            statusTitle.Location = new Point(42, 29);
            statusBar.Controls.Add(statusTitle);

            Label statusBadge =
                Badge(
                    "Aktif (Portal V2.4)",
                    Color.FromArgb(226, 238, 255),
                    _primary);
            statusBadge.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            statusBar.Controls.Add(statusBadge);

            Label statusMeta = new Label();
            statusMeta.Text =
                "•  Son Senkronizasyon: Bugün, " +
                DateTime.Now.ToString("HH:mm:ss") +
                "   •   Mali Dönem: " +
                DateTime.Now.Year +
                " / Q" +
                (((DateTime.Now.Month - 1) / 3) + 1);
            statusMeta.AutoSize = true;
            statusMeta.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            statusMeta.ForeColor = CurrentMutedText();
            statusBar.Controls.Add(statusMeta);

            Button refresh =
                SmallActionButton("\uE72C", "Verileri Yenile", false);
            refresh.Size = new Size(245, 40);

            Button quickInvoice =
                SmallActionButton("\uE710", "Hızlı e-Fatura Düzenle", true);
            quickInvoice.Size = new Size(335, 40);

            statusBar.Controls.Add(refresh);
            statusBar.Controls.Add(quickInvoice);

            refresh.Click += delegate { DashboardAc(); };
            quickInvoice.Click += delegate { YeniFaturaAc(); };

            Action layoutStatus = delegate
            {
                statusBadge.Left = statusTitle.Right + 8;
                statusBadge.Top = 25;
                statusMeta.Left = statusBadge.Right + 12;
                statusMeta.Top = 29;

                quickInvoice.Left =
                    statusBar.ClientSize.Width - quickInvoice.Width - 12;
                quickInvoice.Top = 23;

                refresh.Left =
                    quickInvoice.Left - refresh.Width - 8;
                refresh.Top = 23;
            };

            statusBar.Resize += delegate { layoutStatus(); };
            layoutStatus();
            canvas.Controls.Add(statusBar);

            // =====================================================
            // KARŞILAMA
            // =====================================================
            RoundedPanel welcome = new RoundedPanel();
            welcome.Location = new Point(margin, statusBar.Bottom + gap);
            welcome.Size = new Size(canvas.Width - margin * 2, 292);
            welcome.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            welcome.Radius = 5;
            welcome.FillColor = CurrentCardColor();
            welcome.BorderColor = CurrentBorderColor();
            welcome.BorderThickness = 1;

            Panel accent = new Panel();
            accent.Dock = DockStyle.Left;
            accent.Width = 5;
            accent.BackColor = _primary;
            welcome.Controls.Add(accent);

            Label hello = new Label();
            hello.Text = "Hoş geldiniz, " + _kullaniciAdi;
            hello.AutoSize = true;
            hello.Font =
                new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            hello.ForeColor = CurrentPrimaryText();
            hello.Location = new Point(34, 92);
            welcome.Controls.Add(hello);

            Label adminBadge =
                Badge(
                    "KURUMSAL YÖNETİCİ",
                    Color.FromArgb(218, 230, 250),
                    Color.FromArgb(90, 108, 132));
            adminBadge.Font =
                new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            welcome.Controls.Add(adminBadge);

            Label helloText = new Label();
            helloText.Text =
                "İşletmenizin genel durumunu ve sık kullandığınız kayıtları tek ekrandan takip edin.";
            helloText.AutoSize = true;
            helloText.Font =
                new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            helloText.ForeColor = CurrentMutedText();
            helloText.Location = new Point(35, 151);
            welcome.Controls.Add(helloText);

            RoundedPanel searchPreview = new RoundedPanel();
            searchPreview.Size = new Size(570, 236);
            searchPreview.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;
            searchPreview.Radius = 5;
            searchPreview.FillColor =
                _isDarkMode
                ? Color.FromArgb(37, 49, 68)
                : Color.FromArgb(247, 249, 252);
            searchPreview.BorderColor = CurrentBorderColor();
            searchPreview.BorderThickness = 1;
            welcome.Controls.Add(searchPreview);

            PictureBox miniSearchIcon = new PictureBox();
            miniSearchIcon.Image =
                IconBitmap("\uE721", 9F, _primary, 16, Color.Transparent);
            miniSearchIcon.Size = new Size(16, 16);
            miniSearchIcon.Location = new Point(12, 11);
            miniSearchIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            searchPreview.Controls.Add(miniSearchIcon);

            Label previewTitle = new Label();
            previewTitle.Text = "ARAMA SONUÇLARI ÖNİZLEME";
            previewTitle.AutoSize = true;
            previewTitle.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            previewTitle.ForeColor = CurrentPrimaryText();
            previewTitle.Location = new Point(34, 11);
            searchPreview.Controls.Add(previewTitle);

            Label matches = new Label();
            matches.Text = "3 Eşleşme";
            matches.AutoSize = true;
            matches.Font =
                new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            matches.ForeColor = _primary;
            matches.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchPreview.Controls.Add(matches);

            AddPreviewRow(
                searchPreview, 50, "CARİ",
                AppData.Cariler.Count > 0
                    ? AppData.Cariler[0].CariAdi
                    : "Anadolu Lojistik",
                "\uE77B");

            AddPreviewRow(
                searchPreview, 111, "ÜRÜN",
                AppData.Urunler.Count > 0
                    ? AppData.Urunler[0].UrunAdi
                    : "ERP Yazılım Lisansı V3",
                "\uE7C3");

            AddPreviewRow(
                searchPreview, 172, "FATURA",
                AppData.Faturalar.Count > 0
                    ? AppData.Faturalar[0].FaturaNo
                    : "FTR-2025-001",
                "\uE8A5");

            Action layoutWelcome = delegate
            {
                adminBadge.Left = hello.Right + 10;
                adminBadge.Top = hello.Top + 9;

                searchPreview.Left =
                    welcome.ClientSize.Width - searchPreview.Width - 24;
                searchPreview.Top = 28;

                matches.Left =
                    searchPreview.ClientSize.Width - matches.Width - 10;
                matches.Top = 12;
            };

            welcome.Resize += delegate { layoutWelcome(); };
            layoutWelcome();
            canvas.Controls.Add(welcome);

            // =====================================================
            // KPI KARTLARI
            // =====================================================
            TableLayoutPanel kpis = new TableLayoutPanel();
            kpis.Location = new Point(margin, welcome.Bottom + gap);
            kpis.Size = new Size(canvas.Width - margin * 2, 210);
            kpis.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            kpis.ColumnCount = 4;
            kpis.RowCount = 1;
            kpis.Margin = new Padding(0);
            kpis.Padding = new Padding(0);

            for (int i = 0; i < 4; i++)
                kpis.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25F));

            Panel[] cards =
            {
                DashboardCard(
                    "BUGÜNKÜ FATURALAR",
                    "Son kesilen belgeler",
                    AppData.Faturalar.Count.ToString(),
                    "\uE8A5",
                    "↗  +4 Düne göre",
                    true),

                DashboardCard(
                    "TOPLAM CARİ",
                    "Aktif müşteri ve tedarikçiler",
                    AppData.Cariler.Count.ToString(),
                    "\uE716",
                    "94 Kurumsal",
                    false),

                DashboardCard(
                    "TOPLAM ÜRÜN",
                    "Stok ve hizmet kalemleri",
                    AppData.Urunler.Count.ToString(),
                    "\uE7C3",
                    "12 Kritik Stok",
                    false),

                DashboardCard(
                    "FAVORİ KAYITLAR",
                    "İşaretli hızlı erişimler",
                    (AppData.Cariler.Count(c => c.Favori) +
                     AppData.Urunler.Count(u => u.Favori)).ToString(),
                    "\uE734",
                    "6 Cari / 6 Kalem",
                    false)
            };

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Dock = DockStyle.Fill;
                cards[i].Margin =
                    new Padding(
                        i == 0 ? 0 : 6,
                        0,
                        i == cards.Length - 1 ? 0 : 6,
                        0);
                kpis.Controls.Add(cards[i], i, 0);
            }

            canvas.Controls.Add(kpis);

            // =====================================================
            // HIZLI ERİŞİM
            // =====================================================
            RoundedPanel quick = new RoundedPanel();
            quick.Location = new Point(margin, kpis.Bottom + gap);
            quick.Size = new Size(canvas.Width - margin * 2, 286);
            quick.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            quick.Radius = 5;
            quick.FillColor = CurrentCardColor();
            quick.BorderColor = CurrentBorderColor();
            quick.BorderThickness = 1;

            PictureBox quickIcon = new PictureBox();
            quickIcon.Image =
                IconBitmap("\uE734", 11F, _primary, 20, Color.Transparent);
            quickIcon.Size = new Size(20, 20);
            quickIcon.Location = new Point(20, 26);
            quickIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            quick.Controls.Add(quickIcon);

            Label quickTitle = new Label();
            quickTitle.Text = "Hızlı Erişim";
            quickTitle.AutoSize = true;
            quickTitle.Font =
                new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
            quickTitle.ForeColor = CurrentPrimaryText();
            quickTitle.Location = new Point(50, 23);
            quick.Controls.Add(quickTitle);

            Label quickText = new Label();
            quickText.Text =
                "Favori cari ve ürünlerinize hızlıca ulaşın.";
            quickText.AutoSize = true;
            quickText.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            quickText.ForeColor = CurrentMutedText();
            quickText.Location = new Point(20, 62);
            quick.Controls.Add(quickText);

            Button arrange =
                SmallActionButton("\uE8FD", "Sırala & Düzenle", false);
            arrange.Size = new Size(270, 40);
            arrange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            quick.Controls.Add(arrange);

            FlowLayoutPanel favs = new FlowLayoutPanel();
            favs.Location = new Point(16, 100);
            favs.Size = new Size(quick.ClientSize.Width - 32, 164);
            favs.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            favs.FlowDirection = FlowDirection.LeftToRight;
            favs.WrapContents = true;
            favs.AutoScroll = false;
            favs.BackColor = Color.Transparent;
            favs.Padding = new Padding(0);

            foreach (CariKaydi cari in AppData.Cariler)
            {
                if (!cari.Favori)
                    continue;

                Button b =
                    FavoriButton("CARİ", cari.CariAdi, "\uE734");
                b.Height = 40;
                CariKaydi secilen = cari;
                b.Click += delegate
                {
                    CariAc();
                    CariSatiriniSec(secilen.CariKodu);
                };
                favs.Controls.Add(b);
            }

            foreach (UrunKaydi urun in AppData.Urunler)
            {
                if (!urun.Favori)
                    continue;

                Button b =
                    FavoriButton("ÜRÜN", urun.UrunAdi, "\uE734");
                b.Height = 40;
                UrunKaydi secilen = urun;
                b.Click += delegate
                {
                    UrunlerAc();
                    UrunSatiriniSec(secilen.StokKodu);
                };
                favs.Controls.Add(b);
            }

            Action layoutQuick = delegate
            {
                arrange.Left =
                    quick.ClientSize.Width - arrange.Width - 20;
                arrange.Top = 22;
                favs.Width =
                    quick.ClientSize.Width - 32;
            };

            quick.Resize += delegate { layoutQuick(); };
            layoutQuick();
            quick.Controls.Add(favs);
            canvas.Controls.Add(quick);

            // =====================================================
            // SON FATURALAR
            // =====================================================
            RoundedPanel invoiceCard = new RoundedPanel();
            invoiceCard.Location =
                new Point(margin, quick.Bottom + gap);
            invoiceCard.Size =
                new Size(canvas.Width - margin * 2, 520);
            invoiceCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            invoiceCard.Radius = 5;
            invoiceCard.FillColor = CurrentCardColor();
            invoiceCard.BorderColor = CurrentBorderColor();
            invoiceCard.BorderThickness = 1;

            PictureBox invoiceIcon = new PictureBox();
            invoiceIcon.Image =
                IconBitmap("\uE8A5", 11F, _primary, 20, Color.Transparent);
            invoiceIcon.Size = new Size(20, 20);
            invoiceIcon.Location = new Point(20, 27);
            invoiceIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            invoiceCard.Controls.Add(invoiceIcon);

            Label invoiceTitle = new Label();
            invoiceTitle.Text = "Son Faturalar";
            invoiceTitle.AutoSize = true;
            invoiceTitle.Font =
                new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
            invoiceTitle.ForeColor = CurrentPrimaryText();
            invoiceTitle.Location = new Point(50, 24);
            invoiceCard.Controls.Add(invoiceTitle);

            Label invoiceCount =
                Badge(
                    "Bugün oluşturulan " +
                    AppData.Faturalar.Count +
                    " kayıt listeleniyor",
                    _isDarkMode
                        ? Color.FromArgb(45, 57, 75)
                        : Color.FromArgb(239, 242, 246),
                    CurrentMutedText());
            invoiceCount.Font =
                new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
            invoiceCard.Controls.Add(invoiceCount);

            FlowLayoutPanel invoiceActions = new FlowLayoutPanel();
            invoiceActions.FlowDirection = FlowDirection.LeftToRight;
            invoiceActions.WrapContents = false;
            invoiceActions.AutoSize = true;
            invoiceActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            invoiceActions.BackColor = Color.Transparent;
            invoiceActions.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;
            invoiceActions.Padding = new Padding(0);
            invoiceActions.Margin = new Padding(0);

            TextBox filter = new TextBox();
            filter.Size = new Size(300, 38);
            filter.BorderStyle = BorderStyle.FixedSingle;
            filter.BackColor =
                _isDarkMode
                ? Color.FromArgb(35, 47, 64)
                : Color.FromArgb(247, 249, 252);
            filter.ForeColor = CurrentPrimaryText();
            filter.Font =
                new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CueBanner(filter, "Fatura veya cari filtrele...");
            filter.Margin = new Padding(0, 0, 8, 0);

            Button excel =
                SmallActionButton("\uE896", "Excel", false);
            excel.Size = new Size(142, 38);
            excel.Margin = new Padding(0, 0, 8, 0);

            Button print =
                SmallActionButton("\uE749", "Yazdır", false);
            print.Size = new Size(142, 38);
            print.Margin = new Padding(0);

            invoiceActions.Controls.Add(filter);
            invoiceActions.Controls.Add(excel);
            invoiceActions.Controls.Add(print);
            invoiceCard.Controls.Add(invoiceActions);

            DataGridView grid = TemelGrid();
            grid.Location = new Point(20, 82);
            grid.Size =
                new Size(invoiceCard.ClientSize.Width - 40, 330);
            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;
            grid.BackgroundColor = CurrentCardColor();
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.RowTemplate.Height = 36;
            grid.ColumnHeadersHeight = 38;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor =
                _isDarkMode
                ? Color.FromArgb(44, 55, 71)
                : Color.FromArgb(232, 236, 241);
            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                CurrentPrimaryText();
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            grid.DefaultCellStyle.BackColor = CurrentCardColor();
            grid.DefaultCellStyle.ForeColor = CurrentPrimaryText();
            grid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                _isDarkMode
                ? Color.FromArgb(31, 43, 59)
                : Color.FromArgb(248, 250, 252);

            DataGridViewCheckBoxColumn sec =
                new DataGridViewCheckBoxColumn();
            sec.Name = "Sec";
            sec.HeaderText = "";
            sec.Width = 38;
            sec.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;
            grid.Columns.Add(sec);

            KolonEkle(grid, "FaturaNo", "Fatura No");
            KolonEkle(grid, "Tarih", "Tarih");
            KolonEkle(grid, "Cari", "Cari");
            KolonEkle(grid, "Tip", "Cari Tipi");
            KolonEkle(grid, "Toplam", "Genel Toplam");
            KolonEkle(grid, "Durum", "Durum");

            DataGridViewButtonColumn viewCol =
                new DataGridViewButtonColumn();
            viewCol.Name = "Goruntule";
            viewCol.HeaderText = "İşlem";
            viewCol.Text = "Aç";
            viewCol.UseColumnTextForButtonValue = true;
            viewCol.Width = 105;
            viewCol.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;
            viewCol.FlatStyle = FlatStyle.Flat;
            viewCol.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            viewCol.DefaultCellStyle.Padding = new Padding(3);
            grid.Columns.Add(viewCol);

            DataGridViewButtonColumn downloadCol =
                new DataGridViewButtonColumn();
            downloadCol.Name = "Indir";
            downloadCol.HeaderText = "";
            downloadCol.Text = "İndir";
            downloadCol.UseColumnTextForButtonValue = true;
            downloadCol.Width = 105;
            downloadCol.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;
            downloadCol.FlatStyle = FlatStyle.Flat;
            downloadCol.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            downloadCol.DefaultCellStyle.Padding = new Padding(3);
            grid.Columns.Add(downloadCol);

            foreach (FaturaKaydi fatura in AppData.Faturalar)
            {
                int index =
                    grid.Rows.Add(
                        false,
                        fatura.FaturaNo,
                        fatura.Tarih.ToString("dd.MM.yyyy"),
                        fatura.CariAdi,
                        fatura.CariTipi,
                        fatura.GenelToplam.ToString("N2") + " TL",
                        fatura.Durum,
                        null,
                        null);

                grid.Rows[index].Cells["FaturaNo"].Style.ForeColor =
                    _primary;
                grid.Rows[index].Cells["FaturaNo"].Style.Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                ApplyStatusStyle(
                    grid.Rows[index].Cells["Durum"],
                    fatura.Durum);
            }

            excel.Click += delegate
            {
                FaturaGridiniExcelCsvOlarakAktar(grid);
            };

            print.Click += delegate
            {
                FaturaGridiniYazdir(grid);
            };

            grid.CellClick +=
                delegate (object sender, DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex < 0)
                        return;

                    string columnName =
                        grid.Columns[e.ColumnIndex].Name;

                    string faturaNo =
                        Convert.ToString(
                            grid.Rows[e.RowIndex]
                                .Cells["FaturaNo"].Value);

                    if (columnName == "Goruntule")
                    {
                        FaturalarAc();

                        BeginInvoke(
                            new Action(
                                delegate
                                {
                                    FaturaSatiriniSec(faturaNo);
                                }));
                    }
                    else if (columnName == "Indir")
                    {
                        FaturaSatiriniCsvOlarakKaydet(
                            grid,
                            e.RowIndex);
                    }
                };

            filter.TextChanged +=
                delegate
                {
                    string q =
                        filter.Text.Trim().ToLowerInvariant();

                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        bool visible = q.Length == 0;

                        if (!visible)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                string value =
                                    cell.Value == null
                                    ? ""
                                    : cell.Value
                                        .ToString()
                                        .ToLowerInvariant();

                                if (value.Contains(q))
                                {
                                    visible = true;
                                    break;
                                }
                            }
                        }

                        row.Visible = visible;
                    }
                };

            invoiceCard.Controls.Add(grid);

            Label footer = new Label();
            footer.Text =
                "Toplam " +
                AppData.Faturalar.Count +
                " faturadan 1-" +
                Math.Min(5, AppData.Faturalar.Count) +
                " arası gösteriliyor     •     Sayfa Başına: 10";
            footer.AutoSize = true;
            footer.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            footer.ForeColor = CurrentMutedText();
            footer.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Left;
            invoiceCard.Controls.Add(footer);

            FlowLayoutPanel pages = new FlowLayoutPanel();
            pages.AutoSize = true;
            pages.FlowDirection = FlowDirection.LeftToRight;
            pages.WrapContents = false;
            pages.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Right;
            pages.BackColor = CurrentCardColor();
            pages.Controls.Add(PageButton("‹", false));
            pages.Controls.Add(PageButton("1", true));
            pages.Controls.Add(PageButton("2", false));
            pages.Controls.Add(PageButton("3", false));
            pages.Controls.Add(PageButton("›", false));
            invoiceCard.Controls.Add(pages);

            Action layoutInvoices = delegate
            {
                invoiceCount.Left = invoiceTitle.Right + 12;
                invoiceCount.Top = 29;

                invoiceActions.Left =
                    invoiceCard.ClientSize.Width -
                    invoiceActions.Width - 20;
                invoiceActions.Top = 17;

                grid.Width =
                    invoiceCard.ClientSize.Width - 40;
                grid.Height =
                    invoiceCard.ClientSize.Height - 150;

                footer.Left = 20;
                footer.Top =
                    invoiceCard.ClientSize.Height - footer.Height - 15;

                pages.Left =
                    invoiceCard.ClientSize.Width -
                    pages.Width - 15;
                pages.Top =
                    invoiceCard.ClientSize.Height -
                    pages.Height - 10;
            };

            invoiceCard.Resize += delegate { layoutInvoices(); };
            layoutInvoices();
            canvas.Controls.Add(invoiceCard);

            Action layoutCanvas = delegate
            {
                int w = canvas.ClientSize.Width - margin * 2;
                statusBar.Width = w;
                welcome.Width = w;
                kpis.Width = w;
                quick.Width = w;
                invoiceCard.Width = w;
            };

            canvas.Resize += delegate { layoutCanvas(); };
            layoutCanvas();

            _dashboardResizeHandler =
                delegate
                {
                    canvas.Width =
                        Math.Max(
                            1100,
                            pnlContent.ClientSize.Width);
                };

            pnlContent.Resize +=
                _dashboardResizeHandler;
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
            string subtitle,
            string value,
            string glyph,
            string microText,
            bool blueMicro)
        {
            RoundedPanel panel =
                new RoundedPanel();

            panel.Radius =
                5;

            panel.FillColor =
                CurrentCardColor();

            panel.BorderColor =
                CurrentBorderColor();

            panel.BorderThickness =
                1;

            Panel accent =
                new Panel();

            accent.Dock =
                DockStyle.Left;

            accent.Width =
                4;

            accent.BackColor =
                _primary;

            panel.Controls.Add(
                accent);

            Label titleLabel =
                new Label();

            titleLabel.Text =
                title;

            titleLabel.AutoSize =
                true;

            titleLabel.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            titleLabel.ForeColor =
                CurrentPrimaryText();

            titleLabel.Location =
                new Point(20, 28);

            panel.Controls.Add(
                titleLabel);

            Label subtitleLabel =
                new Label();

            subtitleLabel.Text =
                subtitle;

            subtitleLabel.AutoSize =
                true;

            subtitleLabel.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            subtitleLabel.ForeColor =
                CurrentMutedText();

            subtitleLabel.Location =
                new Point(20, 61);

            panel.Controls.Add(
                subtitleLabel);

            PictureBox icon =
                new PictureBox();

            icon.Size = new Size(36, 36);

            icon.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            icon.SizeMode =
                PictureBoxSizeMode.CenterImage;

            icon.Image =
                IconBitmap(glyph, 13F, _primary, 32, Color.Transparent);

            panel.Controls.Add(
                icon);

            Label valueLabel =
                new Label();

            valueLabel.Text =
                value;

            valueLabel.AutoSize =
                true;

            valueLabel.Font =
                new Font("Segoe UI", 21F, FontStyle.Bold, GraphicsUnit.Point);

            valueLabel.ForeColor =
                CurrentPrimaryText();

            valueLabel.Location =
                new Point(20, 112);

            panel.Controls.Add(
                valueLabel);

            Label micro =
                Badge(
                    microText,
                    blueMicro
                        ? Color.FromArgb(226, 238, 255)
                        : _isDarkMode
                            ? Color.FromArgb(44, 56, 73)
                            : Color.FromArgb(240, 243, 247),
                    blueMicro
                        ? _primary
                        : CurrentMutedText());

            micro.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);

            micro.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Right;

            panel.Controls.Add(
                micro);

            panel.Resize +=
                delegate
                {
                    icon.Left =
                        panel.ClientSize.Width -
                        icon.Width -
                        14;

                    icon.Top =
                        26;

                    micro.Left =
                        panel.ClientSize.Width -
                        micro.Width -
                        14;

                    micro.Top =
                        panel.ClientSize.Height -
                        micro.Height -
                        22;
                };

            return panel;
        }

        private Button FavoriButton(
            string type,
            string title,
            string glyph)
        {
            Button button =
                new Button();

            button.AutoSize =
                true;

            button.Height = 48;

            button.Margin =
                new Padding(5, 5, 5, 5);

            button.Padding =
                new Padding(14, 0, 16, 0);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.FlatAppearance.MouseOverBackColor =
                _isDarkMode
                ? Color.FromArgb(
                    51,
                    64,
                    82)
                : Color.FromArgb(
                    236,
                    241,
                    247);

            button.BackColor =
                _isDarkMode
                ? Color.FromArgb(
                    37,
                    49,
                    66)
                : Color.FromArgb(
                    247,
                    249,
                    252);

            button.ForeColor =
                CurrentPrimaryText();

            button.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);

            button.Text =
                type + "  •  " + title;

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        // =========================================================
        // BİREYSEL MÜŞTERİLER
        // =========================================================

        private void MusterilerAc()
        {
            SayfayiTemizle("Müşteriler");

            pnlContent.SuspendLayout();
            pnlContent.AutoScroll = false;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            // Bu ekran TableLayoutPanel ile kurulmuştur. Sabit koordinatlarla
            // birbirinin üstüne binen kontroller yerine satırlar birbirinden
            // tamamen ayrıdır ve pencere büyüyüp küçüldükçe düzen korunur.
            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.Margin = new Padding(0);
            page.Padding = new Padding(24, 24, 24, 16);
            page.BackColor = Color.FromArgb(246, 249, 253);
            page.ColumnCount = 1;
            page.RowCount = 5;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 156F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            pnlContent.Controls.Add(page);

            // ========================= BAŞLIK KARTI =========================
            RoundedPanel headerCard = new RoundedPanel();
            headerCard.Dock = DockStyle.Fill;
            headerCard.Margin = new Padding(0, 0, 0, 14);
            headerCard.Radius = 10;
            headerCard.FillColor = Color.White;
            headerCard.BorderColor = Color.FromArgb(232, 237, 243);
            headerCard.BorderThickness = 1;

            TableLayoutPanel header = new TableLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.Transparent;
            header.Padding = new Padding(22, 16, 18, 12);
            header.ColumnCount = 2;
            header.RowCount = 1;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            headerCard.Controls.Add(header);

            Panel titleArea = new Panel();
            titleArea.Dock = DockStyle.Fill;
            titleArea.BackColor = Color.Transparent;

            Label title = new Label();
            title.Text = "♙  Bireysel Müşteriler";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 19F, FontStyle.Bold, GraphicsUnit.Point);
            title.ForeColor = Color.FromArgb(20, 25, 31);
            title.Location = new Point(0, 0);
            titleArea.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Kayıtlı bireysel alıcılar, T.C. Kimlik doğrulamaları ve müşteri iletişim kartları";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            subtitle.ForeColor = Color.FromArgb(112, 123, 139);
            subtitle.Location = new Point(2, 54);
            titleArea.Controls.Add(subtitle);
            header.Controls.Add(titleArea, 0, 0);

            FlowLayoutPanel headerButtons = new FlowLayoutPanel();
            headerButtons.Dock = DockStyle.Fill;
            headerButtons.FlowDirection = FlowDirection.RightToLeft;
            headerButtons.WrapContents = false;
            headerButtons.Padding = new Padding(0, 12, 0, 0);
            headerButtons.BackColor = Color.Transparent;

            Button btnYeni = new Button();
            btnYeni.Text = "+  Yeni Bireysel Müşteri";
            btnYeni.Size = new Size(196, 42);
            btnYeni.Margin = new Padding(8, 0, 0, 0);
            btnYeni.FlatStyle = FlatStyle.Flat;
            btnYeni.FlatAppearance.BorderSize = 0;
            btnYeni.BackColor = Color.FromArgb(18, 93, 203);
            btnYeni.ForeColor = Color.White;
            btnYeni.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnYeni.Cursor = Cursors.Hand;

            Button btnYazdir = new Button();
            btnYazdir.Text = "▣  Yazdır";
            btnYazdir.Size = new Size(96, 42);
            btnYazdir.Margin = new Padding(8, 0, 0, 0);
            btnYazdir.FlatStyle = FlatStyle.Flat;
            btnYazdir.FlatAppearance.BorderSize = 0;
            btnYazdir.BackColor = Color.FromArgb(245, 247, 250);
            btnYazdir.ForeColor = Color.FromArgb(45, 53, 64);
            btnYazdir.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnYazdir.Cursor = Cursors.Hand;

            Button btnExcel = new Button();
            btnExcel.Text = "⇩  Dışa Aktar";
            btnExcel.Size = new Size(128, 42);
            btnExcel.Margin = new Padding(8, 0, 0, 0);
            btnExcel.FlatStyle = FlatStyle.Flat;
            btnExcel.FlatAppearance.BorderSize = 0;
            btnExcel.BackColor = Color.FromArgb(245, 247, 250);
            btnExcel.ForeColor = Color.FromArgb(45, 53, 64);
            btnExcel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnExcel.Cursor = Cursors.Hand;

            headerButtons.Controls.Add(btnYeni);
            headerButtons.Controls.Add(btnYazdir);
            headerButtons.Controls.Add(btnExcel);
            header.Controls.Add(headerButtons, 1, 0);
            page.Controls.Add(headerCard, 0, 0);

            var bireysel = AppData.Cariler
                .Where(c => c.Tip == "Müşteri" && string.IsNullOrWhiteSpace(c.FirmaAdi))
                .ToList();

            // ========================= ÖZET KARTLARI =========================
            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Dock = DockStyle.Fill;
            stats.Margin = new Padding(0, 0, 0, 14);
            stats.ColumnCount = 3;
            stats.RowCount = 1;
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));

            string[] statTitles = { "TOPLAM BİREYSEL MÜŞTERİ", "AKTİF BAKİYE / AÇIK HESAP", "GİB E-ARŞİV / TCKN DOĞRULANMIŞ" };
            string[] statValues = { bireysel.Count + " Kayıt", "₺ 0,00", "%98,5" };
            string[] statNotes = { "Aktif çalışan portföy hacmi", "Bakiye alanı mevcut modelde yok", "Kimlik doğrulama görünümü" };
            string[] statIcons = { "◯", "▥", "♢" };

            for (int i = 0; i < 3; i++)
            {
                RoundedPanel card = new RoundedPanel();
                card.Dock = DockStyle.Fill;
                card.Margin = i == 0 ? new Padding(0, 0, 6, 0) : (i == 1 ? new Padding(3, 0, 3, 0) : new Padding(6, 0, 0, 0));
                card.Radius = 10;
                card.FillColor = Color.White;
                card.BorderColor = Color.FromArgb(232, 237, 243);
                card.BorderThickness = 1;

                Label cap = new Label();
                cap.Text = statTitles[i];
                cap.AutoSize = true;
                cap.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
                cap.ForeColor = Color.FromArgb(105, 116, 132);
                cap.Location = new Point(20, 18);
                card.Controls.Add(cap);

                Label value = new Label();
                value.Text = statValues[i];
                value.AutoSize = true;
                value.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
                value.ForeColor = Color.FromArgb(23, 28, 35);
                value.Location = new Point(20, 44);
                card.Controls.Add(value);

                Label note = new Label();
                note.Text = statNotes[i];
                note.AutoSize = true;
                note.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
                note.ForeColor = Color.FromArgb(125, 136, 151);
                note.Location = new Point(21, 108);
                card.Controls.Add(note);

                Label icon = new Label();
                icon.Text = statIcons[i];
                icon.AutoSize = false;
                icon.Size = new Size(60, 60);
                icon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                icon.Location = new Point(card.Width - 78, 30);
                icon.Font = new Font("Segoe UI Symbol", 26F, FontStyle.Bold, GraphicsUnit.Point);
                icon.TextAlign = ContentAlignment.MiddleCenter;
                icon.ForeColor = Color.FromArgb(17, 91, 199);
                icon.BackColor = Color.FromArgb(241, 246, 253);
                card.Controls.Add(icon);
                card.Resize += delegate { icon.Location = new Point(card.ClientSize.Width - 78, 30); };

                stats.Controls.Add(card, i, 0);
            }
            page.Controls.Add(stats, 0, 1);

            // ========================= FİLTRELER =========================
            RoundedPanel filterCard = new RoundedPanel();
            filterCard.Dock = DockStyle.Fill;
            filterCard.Margin = new Padding(0, 0, 0, 14);
            filterCard.Radius = 10;
            filterCard.FillColor = Color.White;
            filterCard.BorderColor = Color.FromArgb(232, 237, 243);
            filterCard.BorderThickness = 1;

            TableLayoutPanel filters = new TableLayoutPanel();
            filters.Dock = DockStyle.Fill;
            filters.Padding = new Padding(14, 13, 14, 12);
            filters.ColumnCount = 5;
            filters.RowCount = 1;
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21F));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            filterCard.Controls.Add(filters);

            TextBox txtAra = new TextBox();
            txtAra.Dock = DockStyle.Fill;
            txtAra.Margin = new Padding(0, 1, 8, 1);
            txtAra.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtAra.BorderStyle = BorderStyle.FixedSingle;
            txtAra.ForeColor = Color.FromArgb(85, 96, 111);
            txtAra.Text = "Ad soyad, TCKN veya telefon...";
            filters.Controls.Add(txtAra, 0, 0);

            ComboBox cmbSehir = new ComboBox();
            cmbSehir.Dock = DockStyle.Fill;
            cmbSehir.Margin = new Padding(0, 1, 8, 1);
            cmbSehir.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSehir.FlatStyle = FlatStyle.Standard;
            cmbSehir.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            cmbSehir.Items.Add("Şehir: Tümü");
            foreach (string s in bireysel.Where(x => !string.IsNullOrWhiteSpace(x.Sehir)).Select(x => x.Sehir).Distinct().OrderBy(x => x))
                cmbSehir.Items.Add(s);
            cmbSehir.SelectedIndex = 0;
            filters.Controls.Add(cmbSehir, 1, 0);

            ComboBox cmbBakiye = new ComboBox();
            cmbBakiye.Dock = DockStyle.Fill;
            cmbBakiye.Margin = new Padding(0, 1, 8, 1);
            cmbBakiye.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBakiye.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            cmbBakiye.Items.AddRange(new object[] { "Bakiye Durumu: Tümü", "Borçlu", "Dengede" });
            cmbBakiye.SelectedIndex = 0;
            filters.Controls.Add(cmbBakiye, 2, 0);

            Button btnSifirla = new Button();
            btnSifirla.Dock = DockStyle.Fill;
            btnSifirla.Margin = new Padding(0, 0, 8, 0);
            btnSifirla.Text = "Filtreleri Sıfırla";
            btnSifirla.FlatStyle = FlatStyle.Flat;
            btnSifirla.FlatAppearance.BorderSize = 0;
            btnSifirla.BackColor = Color.FromArgb(235, 239, 244);
            btnSifirla.ForeColor = Color.FromArgb(55, 64, 76);
            btnSifirla.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold, GraphicsUnit.Point);
            filters.Controls.Add(btnSifirla, 3, 0);

            Label secim = new Label();
            secim.Text = "Seçili: 0     □   ✉   ⇩";
            secim.Dock = DockStyle.Fill;
            secim.TextAlign = ContentAlignment.MiddleRight;
            secim.Font = new Font("Segoe UI Symbol", 9F, FontStyle.Regular, GraphicsUnit.Point);
            secim.ForeColor = Color.FromArgb(83, 94, 109);
            filters.Controls.Add(secim, 4, 0);
            page.Controls.Add(filterCard, 0, 2);

            // ========================= TABLO =========================
            RoundedPanel tableCard = new RoundedPanel();
            tableCard.Dock = DockStyle.Fill;
            tableCard.Margin = new Padding(0);
            tableCard.Radius = 10;
            tableCard.FillColor = Color.White;
            tableCard.BorderColor = Color.FromArgb(232, 237, 243);
            tableCard.BorderThickness = 1;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.Margin = new Padding(0);
            tableLayout.Padding = new Padding(0);
            tableLayout.ColumnCount = 1;
            tableLayout.RowCount = 2;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableCard.Controls.Add(tableLayout);

            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.Margin = new Padding(0);
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 50;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 74;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(231, 236, 242);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(49, 59, 72);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 4, 0);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(38, 46, 57);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.6F, FontStyle.Regular, GraphicsUnit.Point);
            grid.DefaultCellStyle.Padding = new Padding(8, 2, 4, 2);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 33, 43);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 253);

            Action<string, string, float> addCol = delegate (string name, string text, float weight)
            {
                DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                c.Name = name;
                c.HeaderText = text;
                c.FillWeight = weight;
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(c);
            };

            addCol("CariNo", "CARİ NO", 72F);
            addCol("AdSoyad", "AD SOYAD", 120F);
            addCol("Tckn", "T.C. KİMLİK NO", 100F);
            addCol("Iletisim", "İLETİŞİM & TELEFON", 145F);
            addCol("Sehir", "ŞEHİR / İLÇE", 95F);
            addCol("Bakiye", "GÜNCEL BAKİYE", 92F);
            addCol("SonIslem", "SON İŞLEM", 100F);
            addCol("Islemler", "İŞLEMLER", 105F);

            tableLayout.Controls.Add(grid, 0, 0);

            TableLayoutPanel footer = new TableLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.Padding = new Padding(14, 4, 12, 4);
            footer.ColumnCount = 2;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            Label lblCount = new Label();
            lblCount.Dock = DockStyle.Fill;
            lblCount.TextAlign = ContentAlignment.MiddleLeft;
            lblCount.Font = new Font("Segoe UI", 8.8F, FontStyle.Regular, GraphicsUnit.Point);
            lblCount.ForeColor = Color.FromArgb(105, 116, 132);
            footer.Controls.Add(lblCount, 0, 0);

            Label pages = new Label();
            pages.Dock = DockStyle.Fill;
            pages.Text = "‹     1     2     3     ...     ›";
            pages.TextAlign = ContentAlignment.MiddleRight;
            pages.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            pages.ForeColor = Color.FromArgb(27, 82, 170);
            footer.Controls.Add(pages, 1, 0);
            tableLayout.Controls.Add(footer, 0, 1);
            page.Controls.Add(tableCard, 0, 3);

            // ========================= ALT DURUM =========================
            Label status = new Label();
            status.Dock = DockStyle.Fill;
            status.Margin = new Padding(4, 4, 0, 0);
            status.Text = "●  Veri kaynağı: uygulamanın mevcut cari kayıtları     •     TCKN doğrulama görünümü aktif";
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = new Font("Segoe UI", 8.3F, FontStyle.Regular, GraphicsUnit.Point);
            status.ForeColor = Color.FromArgb(98, 109, 124);
            page.Controls.Add(status, 0, 4);

            // ========================= VERİ DOLDURMA =========================
            Action doldur = null;
            doldur = delegate
            {
                string q = txtAra.Text == "Ad soyad, TCKN veya telefon..." ? "" : txtAra.Text.Trim();
                string city = cmbSehir.SelectedItem == null ? "Şehir: Tümü" : cmbSehir.SelectedItem.ToString();

                var list = AppData.Cariler
                    .Where(c => c.Tip == "Müşteri" && string.IsNullOrWhiteSpace(c.FirmaAdi))
                    .Where(c => city == "Şehir: Tümü" || string.Equals(c.Sehir, city, StringComparison.OrdinalIgnoreCase))
                    .Where(c => string.IsNullOrWhiteSpace(q)
                        || (!string.IsNullOrWhiteSpace(c.CariAdi) && c.CariAdi.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (!string.IsNullOrWhiteSpace(c.KimlikNo) && c.KimlikNo.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (!string.IsNullOrWhiteSpace(c.Telefon) && c.Telefon.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (!string.IsNullOrWhiteSpace(c.Email) && c.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                grid.Rows.Clear();
                foreach (CariKaydi cari in list)
                {
                    string kimlik = string.IsNullOrWhiteSpace(cari.KimlikNo) ? "—" : cari.KimlikNo;
                    string telefon = string.IsNullOrWhiteSpace(cari.Telefon) ? "—" : cari.Telefon;
                    string email = string.IsNullOrWhiteSpace(cari.Email) ? "—" : cari.Email;
                    string sehir = string.IsNullOrWhiteSpace(cari.Sehir) ? "—" : cari.Sehir;
                    string ad = string.IsNullOrWhiteSpace(cari.CariAdi) ? "—" : cari.CariAdi;
                    string kod = string.IsNullOrWhiteSpace(cari.CariKodu) ? "—" : cari.CariKodu;

                    int r = grid.Rows.Add(
                        kod,
                        ad + Environment.NewLine + "Bireysel Müşteri",
                        kimlik,
                        telefon + Environment.NewLine + email,
                        sehir,
                        "₺ 0,00" + Environment.NewLine + "Dengede",
                        "—" + Environment.NewLine + "Kayıt",
                        "◉    ✎    Fatura Kes");

                    grid.Rows[r].Height = 74;
                    grid.Rows[r].Tag = cari;
                    grid.Rows[r].Cells[0].Style.ForeColor = Color.FromArgb(12, 91, 199);
                    grid.Rows[r].Cells[1].Style.Font = new Font("Segoe UI", 9.7F, FontStyle.Bold, GraphicsUnit.Point);
                    grid.Rows[r].Cells[5].Style.ForeColor = Color.FromArgb(95, 106, 121);
                    grid.Rows[r].Cells[7].Style.ForeColor = Color.FromArgb(20, 78, 165);
                }

                lblCount.Text = "Toplam " + list.Count + " müşteriden " + (list.Count == 0 ? "0" : "1-" + Math.Min(10, list.Count)) + " arası gösteriliyor";
            };

            txtAra.GotFocus += delegate
            {
                if (txtAra.Text == "Ad soyad, TCKN veya telefon...")
                {
                    txtAra.Text = "";
                    txtAra.ForeColor = Color.FromArgb(45, 55, 68);
                }
            };
            txtAra.LostFocus += delegate
            {
                if (string.IsNullOrWhiteSpace(txtAra.Text))
                {
                    txtAra.Text = "Ad soyad, TCKN veya telefon...";
                    txtAra.ForeColor = Color.FromArgb(85, 96, 111);
                }
            };
            txtAra.TextChanged += delegate { if (doldur != null && txtAra.Text != "Ad soyad, TCKN veya telefon...") doldur(); };
            cmbSehir.SelectedIndexChanged += delegate { if (doldur != null) doldur(); };
            btnSifirla.Click += delegate
            {
                txtAra.Text = "Ad soyad, TCKN veya telefon...";
                txtAra.ForeColor = Color.FromArgb(85, 96, 111);
                cmbSehir.SelectedIndex = 0;
                cmbBakiye.SelectedIndex = 0;
                doldur();
            };

            btnYeni.Click += delegate { CariHesapForm f = new CariHesapForm(); f.ShowDialog(this); MusterilerAc(); };
            btnYazdir.Click += delegate { MessageBox.Show("Yazdırma işlemi bu müşteri listesi için hazırlanabilir.", "Yazdır", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            btnExcel.Click += delegate { MessageBox.Show("Dışa aktarma düğmesi hazır. Excel aktarımı veri katmanına göre bağlanabilir.", "Dışa Aktar", MessageBoxButtons.OK, MessageBoxIcon.Information); };

            grid.CellClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Islemler"].Index) return;
                CariKaydi cari = grid.Rows[e.RowIndex].Tag as CariKaydi;
                if (cari == null) return;
                Rectangle rect = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                int localX = grid.PointToClient(Cursor.Position).X - rect.Left;
                if (localX < rect.Width * 0.32)
                {
                    MessageBox.Show("Cari No: " + cari.CariKodu + Environment.NewLine + "Ad Soyad: " + cari.CariAdi + Environment.NewLine + "TCKN: " + cari.KimlikNo + Environment.NewLine + "Telefon: " + cari.Telefon + Environment.NewLine + "E-posta: " + cari.Email + Environment.NewLine + "Şehir: " + cari.Sehir, "Müşteri Bilgileri", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (localX >= rect.Width * 0.60)
                {
                    YeniFaturaForm f = new YeniFaturaForm();
                    f.ShowDialog(this);
                }
            };

            doldur();
            pnlContent.ResumeLayout(true);
        }


        // =========================================================
        // KURUMSAL MÜŞTERİLER
        // =========================================================

        private void KurumsalAc()
        {
            SayfayiTemizle("Kurumsal Müşteriler");
            pnlContent.SuspendLayout();
            pnlContent.AutoScroll = false;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            var kurumsal = AppData.Cariler
                .Where(c => !string.IsNullOrWhiteSpace(c.FirmaAdi))
                .ToList();

            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill; page.Margin = new Padding(0); page.Padding = new Padding(24, 18, 24, 16);
            page.BackColor = Color.FromArgb(246, 249, 253); page.ColumnCount = 1; page.RowCount = 5;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 156F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            pnlContent.Controls.Add(page);

            Panel header = new Panel(); header.Dock = DockStyle.Fill; header.BackColor = Color.Transparent;
            Label title = new Label(); title.Text = "Kurumsal Müşteriler"; title.AutoSize = true; title.Font = new Font("Segoe UI", 18F, FontStyle.Bold); title.Location = new Point(0, 2); header.Controls.Add(title);
            Label tag = Badge("TÜZEL CARİ MODÜLÜ", Color.FromArgb(231, 239, 250), _primary); tag.Location = new Point(0, 92); header.Controls.Add(tag);
            Label sub = new Label(); sub.Text = "Kayıtlı tüzel firmalar, VKN / Vergi daireleri ve kurumsal cari sözleşmeler"; sub.AutoSize = true; sub.Font = new Font("Segoe UI", 9.5F); sub.ForeColor = _muted; sub.Location = new Point(2, 54); header.Controls.Add(sub);
            Button btnYeni = SmallActionButton("\uE710", "+ Yeni Kurumsal Müşteri", true); btnYeni.Size = new Size(210, 38); btnYeni.Anchor = AnchorStyles.Top | AnchorStyles.Right; header.Controls.Add(btnYeni);
            Button btnExcel = SmallActionButton("\uE896", "Excel İndir", false); btnExcel.Size = new Size(118, 38); btnExcel.Anchor = AnchorStyles.Top | AnchorStyles.Right; header.Controls.Add(btnExcel);
            Button btnGib = SmallActionButton("\uE72C", "Toplu GİB Sorgula", false); btnGib.Size = new Size(170, 38); btnGib.Anchor = AnchorStyles.Top | AnchorStyles.Right; header.Controls.Add(btnGib);
            Action layoutHeader = delegate { btnYeni.Left = header.ClientSize.Width - btnYeni.Width; btnYeni.Top = 12; btnExcel.Left = btnYeni.Left - btnExcel.Width - 8; btnExcel.Top = 0; btnGib.Left = btnExcel.Left - btnGib.Width - 8; btnGib.Top = 0; }; header.Resize += delegate { layoutHeader(); }; layoutHeader(); page.Controls.Add(header, 0, 0);

            TableLayoutPanel stats = new TableLayoutPanel(); stats.Dock = DockStyle.Fill; stats.Margin = new Padding(0, 0, 0, 12); stats.ColumnCount = 4; stats.RowCount = 1;
            for (int i = 0; i < 4; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            string[] st = { "TOPLAM KURUMSAL FİRMA", "GİB E-FATURA MÜKELLEFİ", "TOPLAM KURUMSAL ALACAK", "ORTALAMA VADE" };
            string[] sv = { kurumsal.Count + " Şirket", "—", "₺ 0,00", "— Gün" };
            string[] sn = { "Aktif kurumsal cari kayıtları", "Mükellefiyet alanı modelde yok", "Bakiye alanı modelde yok", "Vade alanı modelde yok" };
            for (int i = 0; i < 4; i++) { RoundedPanel c = new RoundedPanel(); c.Dock = DockStyle.Fill; c.Margin = new Padding(i == 0 ? 0 : 5, 0, i == 3 ? 0 : 5, 0); c.Radius = 8; c.FillColor = Color.White; c.BorderColor = _border; c.BorderThickness = 1; Label a = new Label() { Text = st[i], AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = _muted, Location = new Point(14, 17) }; Label v = new Label() { Text = sv[i], AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = _text, Location = new Point(14, 47) }; Label n = new Label() { Text = sn[i], AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = _muted, Location = new Point(14, 108) }; c.Controls.Add(a); c.Controls.Add(v); c.Controls.Add(n); stats.Controls.Add(c, i, 0); }
            page.Controls.Add(stats, 0, 1);

            RoundedPanel filterCard = new RoundedPanel(); filterCard.Dock = DockStyle.Fill; filterCard.Margin = new Padding(0, 0, 0, 12); filterCard.Radius = 7; filterCard.FillColor = Color.White; filterCard.BorderColor = _border; filterCard.BorderThickness = 1;
            TableLayoutPanel filters = new TableLayoutPanel(); filters.Dock = DockStyle.Fill; filters.Padding = new Padding(10, 10, 10, 9); filters.ColumnCount = 4; filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); filterCard.Controls.Add(filters);
            TextBox txt = new TextBox() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Text = "Firma unvanı, VKN veya cari kodu..." }; txt.Margin = new Padding(0, 0, 8, 0); filters.Controls.Add(txt, 0, 0);
            ComboBox city = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; city.Margin = new Padding(0, 0, 8, 0); city.Items.Add("Şehir: Tümü"); foreach (string x in kurumsal.Where(x => !string.IsNullOrWhiteSpace(x.Sehir)).Select(x => x.Sehir).Distinct().OrderBy(x => x)) city.Items.Add(x); city.SelectedIndex = 0; filters.Controls.Add(city, 1, 0);
            ComboBox gib = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; gib.Margin = new Padding(0, 0, 8, 0); gib.Items.AddRange(new object[] { "GİB Durumu: Tümü", "e-Fatura", "e-Arşiv" }); gib.SelectedIndex = 0; filters.Controls.Add(gib, 2, 0); Button clear = new Button() { Text = "Temizle", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(241, 244, 248) }; clear.FlatAppearance.BorderSize = 0; filters.Controls.Add(clear, 3, 0); page.Controls.Add(filterCard, 0, 2);

            RoundedPanel tableCard = new RoundedPanel(); tableCard.Dock = DockStyle.Fill; tableCard.Radius = 7; tableCard.FillColor = Color.White; tableCard.BorderColor = _border; tableCard.BorderThickness = 1; TableLayoutPanel tl = new TableLayoutPanel() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 }; tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F)); tableCard.Controls.Add(tl);
            DataGridView grid = new DataGridView(); grid.Dock = DockStyle.Fill; grid.BorderStyle = BorderStyle.None; grid.BackgroundColor = Color.White; grid.AllowUserToAddRows = false; grid.RowHeadersVisible = false; grid.ReadOnly = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersHeight = 42; grid.RowTemplate.Height = 58; grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; grid.GridColor = _border; grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 245, 249); grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.2F); grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 246, 255); grid.DefaultCellStyle.SelectionForeColor = _text;
            Action<string, string, float> col = delegate (string n, string h, float w) { var c = new DataGridViewTextBoxColumn(); c.Name = n; c.HeaderText = h; c.FillWeight = w; c.SortMode = DataGridViewColumnSortMode.NotSortable; grid.Columns.Add(c); }; col("CariKodu", "Cari Kodu", 75); col("Firma", "Firma Unvanı", 190); col("VKN", "Vergi No (VKN)", 100); col("Yetkili", "Yetkili Kişi", 105); col("Telefon", "Telefon", 100); col("Sehir", "Şehir", 80); col("Islem", "Hızlı İşlemler", 95); tl.Controls.Add(grid, 0, 0);
            Label count = new Label() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), ForeColor = _muted, Font = new Font("Segoe UI", 8.7F) }; tl.Controls.Add(count, 0, 1); page.Controls.Add(tableCard, 0, 3);
            Action fill = null; fill = delegate { string q = txt.Text == "Firma unvanı, VKN veya cari kodu..." ? "" : txt.Text.Trim(); string cs = city.SelectedItem == null ? "Şehir: Tümü" : city.SelectedItem.ToString(); var list = AppData.Cariler.Where(c => !string.IsNullOrWhiteSpace(c.FirmaAdi)).Where(c => cs == "Şehir: Tümü" || string.Equals(c.Sehir, cs, StringComparison.OrdinalIgnoreCase)).Where(c => string.IsNullOrWhiteSpace(q) || (c.FirmaAdi ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (c.KimlikNo ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (c.CariKodu ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList(); grid.Rows.Clear(); foreach (var c in list) { int r = grid.Rows.Add(c.CariKodu, c.FirmaAdi, c.KimlikNo, c.YetkiliKisi, c.Telefon, c.Sehir, "Görüntüle   Düzenle"); grid.Rows[r].Tag = c; grid.Rows[r].Cells[0].Style.ForeColor = _primary; grid.Rows[r].Cells[1].Style.Font = new Font("Segoe UI", 9.4F, FontStyle.Bold); grid.Rows[r].Cells[6].Style.ForeColor = _primary; } count.Text = "Toplam " + list.Count + " kurumsal kayıt gösteriliyor"; };
            txt.GotFocus += delegate { if (txt.Text == "Firma unvanı, VKN veya cari kodu...") txt.Text = ""; }; txt.LostFocus += delegate { if (string.IsNullOrWhiteSpace(txt.Text)) txt.Text = "Firma unvanı, VKN veya cari kodu..."; }; txt.TextChanged += delegate { if (fill != null && txt.Text != "Firma unvanı, VKN veya cari kodu...") fill(); }; city.SelectedIndexChanged += delegate { if (fill != null) fill(); }; clear.Click += delegate { txt.Text = "Firma unvanı, VKN veya cari kodu..."; city.SelectedIndex = 0; gib.SelectedIndex = 0; fill(); }; btnYeni.Click += delegate { CariHesapForm f = new CariHesapForm(); f.ShowDialog(this); KurumsalAc(); }; btnExcel.Click += delegate { MessageBox.Show("Excel aktarımı mevcut veri katmanına göre bağlanabilir.", "Excel", MessageBoxButtons.OK, MessageBoxIcon.Information); }; btnGib.Click += delegate { MessageBox.Show("GİB mükellefiyet alanı mevcut CariKaydi modelinde bulunmadığı için sorgu görünümü henüz veriyle bağlanmadı.", "GİB", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            TableLayoutPanel info = new TableLayoutPanel() { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 12, 0, 0) }; for (int i = 0; i < 3; i++) info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F)); string[] it = { "VKN & GİB Otomatik Eşleme", "Dinamik Risk ve Kredi Limitleri", "WinForms ERP Entegrasyonu" }; string[] ib = { "Kurumsal cari kayıtları mevcut VKN ve firma bilgileriyle listelenir.", "Risk ve limit alanları veri modeline eklendiğinde bu kart gerçek veriye bağlanabilir.", "Bu ekran doğrudan AppData.Cariler kayıtlarını kullanır." }; for (int i = 0; i < 3; i++) { RoundedPanel c = new RoundedPanel(); c.Dock = DockStyle.Fill; c.Margin = new Padding(i == 0 ? 0 : 5, 0, i == 2 ? 0 : 5, 0); c.Radius = 7; c.FillColor = Color.White; c.BorderColor = _border; c.BorderThickness = 1; Label a = new Label() { Text = it[i], AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(16, 15) }; Label b = new Label() { Text = ib[i], AutoSize = false, Size = new Size(330, 45), Font = new Font("Segoe UI", 8.5F), ForeColor = _muted, Location = new Point(16, 43) }; c.Controls.Add(a); c.Controls.Add(b); info.Controls.Add(c, i, 0); }
            page.Controls.Add(info, 0, 4); fill(); pnlContent.ResumeLayout(true);
        }

        // =========================================================
        // CARİ HESAPLAR
        // =========================================================

        private void CariAc()
        {
            SayfayiTemizle("Cari Hesaplar");
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            Panel canvas = new Panel();
            canvas.Location = new Point(0, 0);
            canvas.Size = new Size(Math.Max(1080, pnlContent.ClientSize.Width), 1040);
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            canvas.BackColor = Color.Transparent;
            pnlContent.Controls.Add(canvas);

            int margin = 24;
            int gap = 12;

            // BAŞLIK
            Label eyebrow = new Label();
            eyebrow.Text = "FİNANS & MUHASEBE / CARİ HESAPLAR";
            eyebrow.AutoSize = true;
            eyebrow.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            eyebrow.ForeColor = _primary;
            eyebrow.Location = new Point(margin, 18);
            canvas.Controls.Add(eyebrow);

            Label title = new Label();
            title.Text = "Cari Hesap Yönetimi & Bakiye Takibi";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            title.ForeColor = _text;
            title.Location = new Point(margin, 38);
            canvas.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Müşteri ve tedarikçi cari kartlarını, iletişim bilgilerini ve kayıt durumlarını tek ekrandan yönetin.";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9.5F);
            subtitle.ForeColor = _muted;
            subtitle.Location = new Point(margin, 89);
            canvas.Controls.Add(subtitle);

            Button btnYeniCari = MaviButon("+ Yeni Cari Hesap");
            btnYeniCari.Size = new Size(170, 40);
            btnYeniCari.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnYeniCari.Click += delegate
            {
                CariHesapForm form = new CariHesapForm();
                form.ShowDialog(this);
                CariAc();
            };
            canvas.Controls.Add(btnYeniCari);

            Button btnYenile = new Button();
            btnYenile.Text = "↻  Verileri Yenile";
            btnYenile.Size = new Size(145, 40);
            btnYenile.FlatStyle = FlatStyle.Flat;
            btnYenile.FlatAppearance.BorderColor = _border;
            btnYenile.FlatAppearance.BorderSize = 1;
            btnYenile.BackColor = Color.White;
            btnYenile.ForeColor = _text;
            btnYenile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnYenile.Cursor = Cursors.Hand;
            btnYenile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnYenile.Click += delegate { CariAc(); };
            canvas.Controls.Add(btnYenile);

            // KPI KARTLARI - mevcut AppData.Cariler üzerinden
            int toplamCari = AppData.Cariler.Count;
            int musteriSayisi = AppData.Cariler.Count(c =>
                !string.IsNullOrWhiteSpace(c.Tip) &&
                c.Tip.IndexOf("müşteri", StringComparison.OrdinalIgnoreCase) >= 0);
            int tedarikciSayisi = AppData.Cariler.Count(c =>
                !string.IsNullOrWhiteSpace(c.Tip) &&
                c.Tip.IndexOf("tedarik", StringComparison.OrdinalIgnoreCase) >= 0);
            int favoriSayisi = AppData.Cariler.Count(c => c.Favori);

            TableLayoutPanel kpis = new TableLayoutPanel();
            kpis.Location = new Point(margin, 142);
            kpis.Size = new Size(canvas.Width - margin * 2, 160);
            kpis.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            kpis.ColumnCount = 4;
            kpis.RowCount = 1;
            kpis.Margin = new Padding(0);
            kpis.Padding = new Padding(0);
            for (int i = 0; i < 4; i++)
                kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            string[] cardTitles = { "TOPLAM CARİ HESAP", "MÜŞTERİ HESAPLARI", "TEDARİKÇİ HESAPLARI", "FAVORİ CARİLER" };
            string[] cardValues = { toplamCari.ToString() + " Kayıt", musteriSayisi.ToString() + " Müşteri", tedarikciSayisi.ToString() + " Tedarikçi", favoriSayisi.ToString() + " Favori" };
            string[] cardNotes = { "Tüm cari kartları", "Kayıtlı müşteri carileri", "Kayıtlı tedarikçi carileri", "Hızlı erişim için işaretlenenler" };
            string[] cardGlyphs = { "▣", "●", "◆", "★" };

            for (int i = 0; i < 4; i++)
            {
                RoundedPanel card = new RoundedPanel();
                card.Dock = DockStyle.Fill;
                card.Margin = new Padding(i == 0 ? 0 : 6, 0, i == 3 ? 0 : 6, 0);
                card.Radius = 6;
                card.FillColor = Color.White;
                card.BorderColor = _border;
                card.BorderThickness = 1;

                Label ct = new Label();
                ct.Text = cardTitles[i];
                ct.AutoSize = true;
                ct.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                ct.ForeColor = _muted;
                ct.Location = new Point(14, 16);
                card.Controls.Add(ct);

                Label cv = new Label();
                cv.Text = cardValues[i];
                cv.AutoSize = true;
                cv.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
                cv.ForeColor = _text;
                cv.Location = new Point(14, 44);
                card.Controls.Add(cv);

                Label cn = new Label();
                cn.Text = cardNotes[i];
                cn.AutoSize = true;
                cn.Font = new Font("Segoe UI", 8.5F);
                cn.ForeColor = _muted;
                cn.Location = new Point(14, 108);
                card.Controls.Add(cn);

                Label icon = new Label();
                icon.Text = cardGlyphs[i];
                icon.AutoSize = false;
                icon.Size = new Size(34, 34);
                icon.TextAlign = ContentAlignment.MiddleCenter;
                icon.Font = new Font("Segoe UI Symbol", 14F, FontStyle.Bold);
                icon.ForeColor = _primary;
                icon.BackColor = Color.FromArgb(237, 244, 255);
                icon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                card.Controls.Add(icon);
                card.Resize += delegate (object sender, EventArgs e)
                {
                    Panel cp = sender as Panel;
                    if (cp != null) icon.Left = cp.ClientSize.Width - icon.Width - 14;
                };
                icon.Left = Math.Max(14, card.Width - icon.Width - 14);
                icon.Top = 14;
                kpis.Controls.Add(card, i, 0);
            }
            canvas.Controls.Add(kpis);

            // FİLTRE KARTI
            RoundedPanel filterCard = new RoundedPanel();
            filterCard.Location = new Point(margin, kpis.Bottom + gap);
            filterCard.Size = new Size(canvas.Width - margin * 2, 78);
            filterCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterCard.Radius = 6;
            filterCard.FillColor = Color.White;
            filterCard.BorderColor = _border;
            filterCard.BorderThickness = 1;
            canvas.Controls.Add(filterCard);

            TextBox txtAra = new TextBox();
            txtAra.Location = new Point(18, 23);
            txtAra.Size = new Size(420, 30);
            txtAra.Font = new Font("Segoe UI", 10F);
            CueBanner(txtAra, "Cari kodu, unvan, yetkili, telefon veya e-posta ara...");
            filterCard.Controls.Add(txtAra);

            ComboBox cmbTip = new ComboBox();
            cmbTip.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTip.Items.AddRange(new object[] { "Cari Türü: Tümü", "Müşteri", "Tedarikçi" });
            cmbTip.SelectedIndex = 0;
            cmbTip.Font = new Font("Segoe UI", 9.5F);
            cmbTip.Size = new Size(190, 30);
            cmbTip.Location = new Point(452, 22);
            filterCard.Controls.Add(cmbTip);

            ComboBox cmbSehir = new ComboBox();
            cmbSehir.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSehir.Items.Add("Şehir: Tümü");
            foreach (string sehir in AppData.Cariler.Select(c => c.Sehir).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x))
                cmbSehir.Items.Add(sehir);
            cmbSehir.SelectedIndex = 0;
            cmbSehir.Font = new Font("Segoe UI", 9.5F);
            cmbSehir.Size = new Size(180, 30);
            cmbSehir.Location = new Point(654, 22);
            filterCard.Controls.Add(cmbSehir);

            Button btnTemizle = new Button();
            btnTemizle.Text = "Filtreleri Temizle";
            btnTemizle.Size = new Size(135, 32);
            btnTemizle.FlatStyle = FlatStyle.Flat;
            btnTemizle.FlatAppearance.BorderColor = _border;
            btnTemizle.BackColor = Color.White;
            btnTemizle.ForeColor = _text;
            btnTemizle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTemizle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterCard.Controls.Add(btnTemizle);

            // TABLO KARTI
            RoundedPanel tableCard = new RoundedPanel();
            tableCard.Location = new Point(margin, filterCard.Bottom + gap);
            tableCard.Size = new Size(canvas.Width - margin * 2, 475);
            tableCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableCard.Radius = 6;
            tableCard.FillColor = Color.White;
            tableCard.BorderColor = _border;
            tableCard.BorderThickness = 1;
            canvas.Controls.Add(tableCard);

            DataGridView grid = TemelGrid();
            grid.Location = new Point(1, 1);
            grid.Size = new Size(tableCard.Width - 2, 410);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars = ScrollBars.Vertical;
            grid.ReadOnly = true;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 52;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 246, 250);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(50, 58, 68);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = _text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 242, 255);
            grid.DefaultCellStyle.SelectionForeColor = _text;
            grid.GridColor = Color.FromArgb(232, 236, 241);

            DataGridViewTextBoxColumn favoriColumn = new DataGridViewTextBoxColumn();
            favoriColumn.Name = "Favori";
            favoriColumn.HeaderText = "★";
            favoriColumn.FillWeight = 35;
            favoriColumn.MinimumWidth = 42;
            favoriColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.Columns.Add(favoriColumn);

            KolonEkle(grid, "CariKodu", "CARİ KODU");
            KolonEkle(grid, "Tip", "TÜR");
            KolonEkle(grid, "CariAdi", "CARİ ÜNVANI / FİRMA ADI");
            KolonEkle(grid, "YetkiliKisi", "YETKİLİ KİŞİ");
            KolonEkle(grid, "Iletisim", "İLETİŞİM");
            KolonEkle(grid, "Konum", "ŞEHİR / ÜLKE");
            KolonEkle(grid, "Hizli", "HIZLI İŞLEMLER");

            grid.Columns["CariKodu"].FillWeight = 85;
            grid.Columns["Tip"].FillWeight = 75;
            grid.Columns["CariAdi"].FillWeight = 210;
            grid.Columns["YetkiliKisi"].FillWeight = 110;
            grid.Columns["Iletisim"].FillWeight = 160;
            grid.Columns["Konum"].FillWeight = 100;
            grid.Columns["Hizli"].FillWeight = 85;

            Action listeyiDoldur = delegate
            {
                grid.Rows.Clear();
                string ara = txtAra.Text.Trim().ToLowerInvariant();
                string tip = cmbTip.SelectedIndex > 0 ? cmbTip.SelectedItem.ToString() : "";
                string sehir = cmbSehir.SelectedIndex > 0 ? cmbSehir.SelectedItem.ToString() : "";

                foreach (CariKaydi cari in AppData.Cariler)
                {
                    string tum = (cari.CariKodu + " " + cari.CariAdi + " " + cari.FirmaAdi + " " + cari.YetkiliKisi + " " + cari.Telefon + " " + cari.Email + " " + cari.Sehir + " " + cari.Ulke).ToLowerInvariant();
                    if (ara.Length > 0 && !tum.Contains(ara)) continue;
                    if (tip.Length > 0 && (cari.Tip == null || cari.Tip.IndexOf(tip, StringComparison.OrdinalIgnoreCase) < 0)) continue;
                    if (sehir.Length > 0 && !string.Equals(cari.Sehir, sehir, StringComparison.OrdinalIgnoreCase)) continue;

                    string unvan = !string.IsNullOrWhiteSpace(cari.CariAdi) ? cari.CariAdi : cari.FirmaAdi;
                    string iletisim = !string.IsNullOrWhiteSpace(cari.Telefon) ? cari.Telefon : cari.Email;
                    if (!string.IsNullOrWhiteSpace(cari.Telefon) && !string.IsNullOrWhiteSpace(cari.Email))
                        iletisim = cari.Telefon + "  •  " + cari.Email;
                    string konum = (cari.Sehir + " / " + cari.Ulke).Trim(' ', '/');

                    int index = grid.Rows.Add(
                        cari.Favori ? "★" : "☆",
                        cari.CariKodu,
                        cari.Tip,
                        unvan,
                        cari.YetkiliKisi,
                        iletisim,
                        konum,
                        "Görüntüle   Düzenle");
                    grid.Rows[index].Tag = cari;
                }
            };

            listeyiDoldur();
            tableCard.Controls.Add(grid);

            Label tableSummary = new Label();
            tableSummary.AutoSize = true;
            tableSummary.Font = new Font("Segoe UI", 9F);
            tableSummary.ForeColor = _muted;
            tableSummary.Location = new Point(14, 432);
            tableCard.Controls.Add(tableSummary);

            Label sourceInfo = new Label();
            sourceInfo.Text = "Veri kaynağı: AppData.Cariler";
            sourceInfo.AutoSize = true;
            sourceInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            sourceInfo.ForeColor = _primary;
            sourceInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            tableCard.Controls.Add(sourceInfo);

            Action ozetGuncelle = delegate
            {
                tableSummary.Text = "Toplam " + AppData.Cariler.Count + " cari kaydından " + grid.Rows.Count + " kayıt listeleniyor";
                sourceInfo.Left = tableCard.ClientSize.Width - sourceInfo.Width - 16;
                sourceInfo.Top = 432;
            };
            ozetGuncelle();

            txtAra.TextChanged += delegate { listeyiDoldur(); ozetGuncelle(); };
            cmbTip.SelectedIndexChanged += delegate { listeyiDoldur(); ozetGuncelle(); };
            cmbSehir.SelectedIndexChanged += delegate { listeyiDoldur(); ozetGuncelle(); };
            btnTemizle.Click += delegate
            {
                txtAra.Clear();
                cmbTip.SelectedIndex = 0;
                cmbSehir.SelectedIndex = 0;
                listeyiDoldur();
                ozetGuncelle();
            };

            grid.CellClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                CariKaydi cari = grid.Rows[e.RowIndex].Tag as CariKaydi;
                if (cari == null) return;

                if (e.ColumnIndex == grid.Columns["Favori"].Index)
                {
                    cari.Favori = !cari.Favori;
                    grid.Rows[e.RowIndex].Cells["Favori"].Value = cari.Favori ? "★" : "☆";
                }
            };

            Action layout = delegate
            {
                canvas.Width = Math.Max(1080, pnlContent.ClientSize.Width);
                btnYeniCari.Left = canvas.ClientSize.Width - margin - btnYeniCari.Width;
                btnYeniCari.Top = 32;
                btnYenile.Left = btnYeniCari.Left - btnYenile.Width - 8;
                btnYenile.Top = 32;

                kpis.Width = canvas.ClientSize.Width - margin * 2;
                filterCard.Width = canvas.ClientSize.Width - margin * 2;
                btnTemizle.Left = filterCard.ClientSize.Width - btnTemizle.Width - 16;
                btnTemizle.Top = 21;

                int available = btnTemizle.Left - 18;
                if (available < 850)
                {
                    txtAra.Width = Math.Max(280, available - 390);
                    cmbTip.Left = txtAra.Right + 12;
                    cmbSehir.Left = cmbTip.Right + 12;
                }

                tableCard.Width = canvas.ClientSize.Width - margin * 2;
                grid.Width = tableCard.ClientSize.Width - 2;
                sourceInfo.Left = tableCard.ClientSize.Width - sourceInfo.Width - 16;
            };

            canvas.Resize += delegate { layout(); };
            pnlContent.Resize += delegate { layout(); };
            layout();
        }

        // =========================================================
        // ÜRÜNLER
        // =========================================================

        private void UrunlerAc()
        {
            SayfayiTemizle("Ürün / Hizmetler");

            pnlContent.SuspendLayout();
            pnlContent.AutoScroll = false;
            pnlContent.BackColor = Color.FromArgb(246, 249, 253);

            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.Padding = new Padding(24, 22, 24, 16);
            page.BackColor = Color.FromArgb(246, 249, 253);
            page.ColumnCount = 1;
            page.RowCount = 4;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 152F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlContent.Controls.Add(page);

            RoundedPanel headerCard = new RoundedPanel();
            headerCard.Dock = DockStyle.Fill;
            headerCard.Margin = new Padding(0, 0, 0, 12);
            headerCard.Radius = 10;
            headerCard.FillColor = Color.White;
            headerCard.BorderColor = _border;
            headerCard.BorderThickness = 1;
            page.Controls.Add(headerCard, 0, 0);

            Label title = new Label();
            title.Text = "Ürün & Hizmet Yönetimi";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            title.ForeColor = _text;
            title.Location = new Point(20, 12);
            headerCard.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Ürün kartlarını, satış fiyatlarını, KDV oranlarını ve stok durumunu tek ekrandan yönetin.";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 9.5F);
            subtitle.ForeColor = _muted;
            subtitle.Location = new Point(22, 65);
            headerCard.Controls.Add(subtitle);

            Button btnYeniUrun = MaviButon("+ Yeni Ürün");
            btnYeniUrun.Size = new Size(142, 40);
            btnYeniUrun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerCard.Controls.Add(btnYeniUrun);
            headerCard.Resize += delegate
            {
                btnYeniUrun.Location = new Point(
                    Math.Max(650, headerCard.ClientSize.Width - btnYeniUrun.Width - 20), 23);
            };

            int toplamUrun = AppData.Urunler.Count;
            int favori = AppData.Urunler.Count(x => x.Favori);
            decimal ortalamaFiyat = toplamUrun == 0
                ? 0M
                : AppData.Urunler.Average(x => x.BirimFiyat);

            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Dock = DockStyle.Fill;
            stats.Margin = new Padding(0, 0, 0, 12);
            stats.ColumnCount = 3;
            stats.RowCount = 1;
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            page.Controls.Add(stats, 0, 1);

            string[] statTitle = { "TOPLAM ÜRÜN / HİZMET", "FAVORİ KAYIT", "ORTALAMA SATIŞ FİYATI" };
            string[] statValue = {
                toplamUrun.ToString() + " Kayıt",
                favori.ToString(),
                ortalamaFiyat.ToString("N2") + " TL"
            };
            string[] statNote = {
                "Aktif ürün ve hizmet kartları",
                "Hızlı erişim için işaretlenenler",
                "Mevcut kayıtların ortalama birim fiyatı"
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
                cap.Text = statTitle[i];
                cap.AutoSize = true;
                cap.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                cap.ForeColor = _muted;
                cap.Location = new Point(18, 16);
                card.Controls.Add(cap);

                Label value = new Label();
                value.Text = statValue[i];
                value.AutoSize = true;
                value.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
                value.ForeColor = i == 2 ? _primary : _text;
                value.Location = new Point(17, 43);
                card.Controls.Add(value);

                Label note = new Label();
                note.Text = statNote[i];
                note.AutoSize = true;
                note.Font = new Font("Segoe UI", 8.5F);
                note.ForeColor = _muted;
                note.Location = new Point(19, 101);
                card.Controls.Add(note);

                stats.Controls.Add(card, i, 0);
            }

            RoundedPanel filterCard = new RoundedPanel();
            filterCard.Dock = DockStyle.Fill;
            filterCard.Margin = new Padding(0, 0, 0, 12);
            filterCard.Radius = 9;
            filterCard.FillColor = Color.White;
            filterCard.BorderColor = _border;
            filterCard.BorderThickness = 1;
            page.Controls.Add(filterCard, 0, 2);

            Label lblAra = new Label();
            lblAra.Text = "Ürün ara";
            lblAra.AutoSize = true;
            lblAra.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblAra.ForeColor = _muted;
            lblAra.Location = new Point(18, 12);
            filterCard.Controls.Add(lblAra);

            TextBox txtAra = new TextBox();
            txtAra.Location = new Point(18, 34);
            txtAra.Size = new Size(410, 30);
            txtAra.Font = new Font("Segoe UI", 9.5F);
            CueBanner(txtAra, "Stok kodu, ürün adı, kategori veya birim ara...");
            filterCard.Controls.Add(txtAra);

            Label recordCount = new Label();
            recordCount.Text = toplamUrun.ToString() + " kayıt listeleniyor";
            recordCount.AutoSize = true;
            recordCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            recordCount.ForeColor = _muted;
            recordCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterCard.Controls.Add(recordCount);
            filterCard.Resize += delegate
            {
                recordCount.Location = new Point(
                    Math.Max(500, filterCard.ClientSize.Width - recordCount.Width - 20), 38);
            };

            RoundedPanel gridCard = new RoundedPanel();
            gridCard.Dock = DockStyle.Fill;
            gridCard.Margin = new Padding(0);
            gridCard.Padding = new Padding(10);
            gridCard.Radius = 10;
            gridCard.FillColor = Color.White;
            gridCard.BorderColor = _border;
            gridCard.BorderThickness = 1;
            page.Controls.Add(gridCard, 0, 3);

            DataGridView grid = TemelGrid();
            grid.Dock = DockStyle.Fill;
            grid.RowTemplate.Height = 42;
            grid.ColumnHeadersHeight = 42;

            DataGridViewTextBoxColumn favoriColumn = new DataGridViewTextBoxColumn();
            favoriColumn.Name = "Favori";
            favoriColumn.HeaderText = "★";
            favoriColumn.Width = 55;
            favoriColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            favoriColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            favoriColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(favoriColumn);

            KolonEkle(grid, "StokKodu", "STOK KODU");
            KolonEkle(grid, "Urun", "ÜRÜN / HİZMET");
            KolonEkle(grid, "Kategori", "KATEGORİ");
            KolonEkle(grid, "Birim", "BİRİM");
            KolonEkle(grid, "Fiyat", "SATIŞ FİYATI");
            KolonEkle(grid, "Kdv", "KDV");
            KolonEkle(grid, "Stok", "STOK");

            grid.Columns["StokKodu"].FillWeight = 85;
            grid.Columns["Urun"].FillWeight = 155;
            grid.Columns["Kategori"].FillWeight = 95;
            grid.Columns["Birim"].FillWeight = 65;
            grid.Columns["Fiyat"].FillWeight = 90;
            grid.Columns["Kdv"].FillWeight = 55;
            grid.Columns["Stok"].FillWeight = 65;
            grid.Columns["Fiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["Kdv"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["Stok"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (UrunKaydi urun in AppData.Urunler)
            {
                int index = grid.Rows.Add(
                    urun.Favori ? "★" : "☆",
                    urun.StokKodu,
                    urun.UrunAdi,
                    urun.Kategori,
                    urun.Birim,
                    urun.BirimFiyat.ToString("N2") + " TL",
                    "%" + urun.KdvOrani.ToString("N0"),
                    urun.Stok);

                grid.Rows[index].Tag = urun;
            }

            grid.CellClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex != grid.Columns["Favori"].Index) return;

                UrunKaydi urun = grid.Rows[e.RowIndex].Tag as UrunKaydi;
                if (urun == null) return;

                urun.Favori = !urun.Favori;
                grid.Rows[e.RowIndex].Cells["Favori"].Value =
                    urun.Favori ? "★" : "☆";
            };

            txtAra.TextChanged += delegate
            {
                ListeFiltrele(grid, txtAra.Text);
                int visible = 0;
                foreach (DataGridViewRow row in grid.Rows)
                    if (row.Visible) visible++;
                recordCount.Text = visible.ToString() + " kayıt listeleniyor";
            };

            gridCard.Controls.Add(grid);
            pnlContent.ResumeLayout(true);
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


        // =========================================================
        // KULLANICILAR
        // =========================================================

        
        // =========================================================
        // AYARLAR
        // =========================================================

       
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
                new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);

            label.ForeColor =
                Color.FromArgb(
                    35,
                    48,
                    65);

            label.Location =
                new Point(
                    28,
                    22);

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
                new Size(150, 38);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.BackColor =
                Color.FromArgb(
                    35,
                    102,
                    215);

            button.ForeColor =
                Color.White;

            button.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        private DataGridView TemelGrid()
        {
            DataGridView grid =
                new DataGridView();

            grid.BackgroundColor =
                Color.White;

            grid.BorderStyle =
                BorderStyle.None;

            grid.AllowUserToAddRows =
                false;

            grid.AllowUserToDeleteRows =
                false;

            grid.RowHeadersVisible =
                false;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.MultiSelect =
                false;

            grid.ReadOnly =
                true;

            grid.ColumnHeadersHeight = 40;

            grid.RowTemplate.Height = 40;

            grid.EnableHeadersVisualStyles =
                false;

            grid.GridColor =
                Color.FromArgb(
                    224,
                    228,
                    234);

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 237, 243);

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(
                    48,
                    61,
                    78);

            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);

            grid.DefaultCellStyle.BackColor =
                Color.White;

            grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    48,
                    58,
                    70);

            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    225,
                    237,
                    255);

            grid.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(
                    249,
                    251,
                    253);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            grid.DefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            grid.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            grid.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            grid.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

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
            Panel panel =
                new Panel();

            panel.BackColor =
                Color.White;

            panel.BorderStyle =
                BorderStyle.FixedSingle;

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
                    11.5F,
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


        // =========================================================
        // REFERANS TASARIM YARDIMCILARI
        // =========================================================

        private Bitmap IconBitmap(
            string glyph,
            float fontSize,
            Color color,
            int canvasSize,
            Color background)
        {
            // IMPORTANT:
            // canvasSize/fontSize are logical WinForms sizes.
            // Do NOT multiply by DPI here; AutoScaleMode already handles DPI.
            Bitmap bmp =
                new Bitmap(
                    Math.Max(1, canvasSize),
                    Math.Max(1, canvasSize));

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint =
                    System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                if (background.A > 0)
                {
                    using (SolidBrush bg = new SolidBrush(background))
                    {
                        g.FillEllipse(
                            bg,
                            0,
                            0,
                            canvasSize - 1,
                            canvasSize - 1);
                    }
                }

                using (Font font =
                    new Font(
                        "Segoe MDL2 Assets",
                        fontSize,
                        FontStyle.Regular,
                        GraphicsUnit.Point))
                {
                    TextRenderer.DrawText(
                        g,
                        glyph,
                        font,
                        new Rectangle(0, 0, canvasSize, canvasSize),
                        color,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.NoPrefix);
                }
            }

            return bmp;
        }

        private Bitmap DotBitmap(
            Color color,
            int dotSize,
            int canvasSize)
        {
            Bitmap bmp =
                new Bitmap(
                    Math.Max(1, canvasSize),
                    Math.Max(1, canvasSize));

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int x = (canvasSize - dotSize) / 2;
                int y = (canvasSize - dotSize) / 2;

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(
                        brush,
                        x,
                        y,
                        Math.Max(1, dotSize),
                        Math.Max(1, dotSize));
                }
            }

            return bmp;
        }

        private void CueBanner(
            TextBox textBox,
            string text)
        {
            EventHandler apply =
                delegate
                {
                    if (!textBox.IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        SendMessage(
                            textBox.Handle,
                            EmSetCueBanner,
                            new IntPtr(1),
                            text);
                    }
                    catch (DllNotFoundException)
                    {
                        // Form Windows dışındaki bir test ortamında derlenirse
                        // yalnızca placeholder atlanır; uygulama çalışmaya devam eder.
                    }
                };

            textBox.HandleCreated +=
                apply;

            if (textBox.IsHandleCreated)
            {
                apply(
                    textBox,
                    EventArgs.Empty);
            }
        }

        private Label SmallLabel(
            string text,
            int x,
            int y,
            bool bold)
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
                    12.5F,
                    bold
                        ? FontStyle.Bold
                        : FontStyle.Regular);

            label.ForeColor =
                bold
                ? CurrentPrimaryText()
                : CurrentMutedText();

            label.Location =
                new Point(
                    x,
                    y);

            return label;
        }

        private Label Badge(
            string text,
            Color background,
            Color foreground)
        {
            Label label =
                new Label();

            label.Text =
                " " +
                text +
                " ";

            label.AutoSize =
                true;

            label.Font =
                new Font(
                    "Segoe UI",
                    12.5F,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

            label.BackColor =
                background;

            label.ForeColor =
                foreground;

            label.Padding =
                new Padding(
                    2,
                    2,
                    2,
                    2);

            return label;
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

        private Button SmallActionButton(
            string glyph,
            string text,
            bool primary)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.AutoSize = false;

            button.Size =
                primary
                ? new Size(155, 38)
                : new Size(105, 36);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                primary
                ? 0
                : 1;

            button.FlatAppearance.BorderColor =
                CurrentBorderColor();

            button.BackColor =
                primary
                ? _primary
                : CurrentCardColor();

            button.ForeColor =
                primary
                ? Color.White
                : CurrentPrimaryText();

            button.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            button.Padding =
                new Padding(12, 0, 12, 0);

            button.TextAlign =
                ContentAlignment.MiddleCenter;

            button.AutoEllipsis = false;
            button.UseCompatibleTextRendering = false;

            button.UseCompatibleTextRendering =
                false;

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        private Button PageButton(
            string text,
            bool selected)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Size =
                new Size(
                    31,
                    29);

            button.Margin =
                new Padding(
                    2,
                    0,
                    2,
                    0);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderColor =
                selected
                ? _primary
                : CurrentBorderColor();

            button.BackColor =
                selected
                ? _primary
                : CurrentCardColor();

            button.ForeColor =
                selected
                ? Color.White
                : CurrentPrimaryText();

            button.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            return button;
        }

        private void AddPreviewRow(
            Control parent,
            int y,
            string type,
            string title,
            string glyph)
        {
            Panel row = new Panel();
            row.Location = new Point(10, y);
            row.Size = new Size(parent.ClientSize.Width - 20, 48);
            row.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            row.BackColor =
                _isDarkMode
                ? Color.FromArgb(31, 43, 59)
                : Color.White;
            row.BorderStyle = BorderStyle.FixedSingle;

            Label typeLabel = new Label();
            typeLabel.Text = type;
            typeLabel.AutoSize = false;
            typeLabel.Size = new Size(66, 28);
            typeLabel.Location = new Point(8, 9);
            typeLabel.TextAlign = ContentAlignment.MiddleCenter;
            typeLabel.Font =
                new Font("Segoe UI", 7.5F, FontStyle.Bold, GraphicsUnit.Point);
            typeLabel.ForeColor = CurrentMutedText();
            typeLabel.BackColor =
                _isDarkMode
                ? Color.FromArgb(48, 61, 80)
                : Color.FromArgb(237, 241, 246);
            row.Controls.Add(typeLabel);

            PictureBox iconBox = new PictureBox();
            iconBox.Image =
                IconBitmap(glyph, 8.5F, _primary, 18, Color.Transparent);
            iconBox.Size = new Size(22, 22);
            iconBox.Location = new Point(82, 13);
            iconBox.SizeMode = PictureBoxSizeMode.CenterImage;
            row.Controls.Add(iconBox);

            Label value = new Label();
            value.Text = title;
            value.AutoSize = false;
            value.AutoEllipsis = true;
            value.Location = new Point(110, 8);
            value.Size = new Size(
                Math.Max(100, row.ClientSize.Width - 158),
                32);
            value.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;
            value.TextAlign = ContentAlignment.MiddleLeft;
            value.Font =
                new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            value.ForeColor = CurrentPrimaryText();
            row.Controls.Add(value);

            Label arrow = new Label();
            arrow.Text = "›";
            arrow.AutoSize = false;
            arrow.Size = new Size(30, 30);
            arrow.Location =
                new Point(row.ClientSize.Width - 36, 8);
            arrow.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;
            arrow.TextAlign = ContentAlignment.MiddleCenter;
            arrow.Font =
                new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
            arrow.ForeColor = CurrentMutedText();
            row.Controls.Add(arrow);

            parent.Controls.Add(row);
        }

        private void ApplyStatusStyle(
            DataGridViewCell cell,
            string status)
        {
            string value =
                status == null
                ? ""
                : status.ToLowerInvariant();

            if (value.Contains("onay"))
            {
                cell.Style.BackColor =
                    Color.FromArgb(
                        229,
                        239,
                        255);

                cell.Style.ForeColor =
                    _primary;
            }
            else if (value.Contains("gönder"))
            {
                cell.Style.BackColor =
                    Color.FromArgb(
                        225,
                        235,
                        247);

                cell.Style.ForeColor =
                    Color.FromArgb(
                        75,
                        95,
                        122);
            }
            else if (value.Contains("iptal"))
            {
                cell.Style.BackColor =
                    Color.FromArgb(
                        255,
                        220,
                        218);

                cell.Style.ForeColor =
                    Color.FromArgb(
                        190,
                        33,
                        28);
            }
            else
            {
                cell.Style.BackColor =
                    Color.FromArgb(
                        239,
                        242,
                        246);

                cell.Style.ForeColor =
                    CurrentMutedText();
            }

            cell.Style.Font =
                new Font(
                    "Segoe UI",
                    7.2F,
                    FontStyle.Bold);
        }

        private Color CurrentCardColor()
        {
            return _isDarkMode
                ? Color.FromArgb(
                    30,
                    41,
                    59)
                : _card;
        }

        private Color CurrentPrimaryText()
        {
            return _isDarkMode
                ? Color.FromArgb(
                    235,
                    240,
                    246)
                : _text;
        }

        private Color CurrentMutedText()
        {
            return _isDarkMode
                ? Color.FromArgb(
                    168,
                    180,
                    196)
                : _muted;
        }

        private Color CurrentBorderColor()
        {
            return _isDarkMode
                ? Color.FromArgb(
                    54,
                    67,
                    85)
                : _border;
        }

        private void TemaUygula()
        {
            if (btnTema != null)
            {
                btnTema.Text =
                    _isDarkMode
                    ? "Açık mod"
                    : "Koyu mod";

                btnTema.BackColor =
                    _isDarkMode
                    ? Color.FromArgb(
                        30,
                        41,
                        59)
                    : Color.White;

                btnTema.ForeColor =
                    CurrentPrimaryText();

                btnTema.Image =
                    IconBitmap(
                        _isDarkMode
                            ? "\uE706"
                            : "\uE708",
                        12,
                        CurrentPrimaryText(),
                        20,
                        Color.Transparent);
            }

            pnlTop.BackColor =
                _isDarkMode
                ? Color.FromArgb(
                    22,
                    31,
                    48)
                : Color.White;

            pnlContent.BackColor =
                _isDarkMode
                ? Color.FromArgb(
                    15,
                    23,
                    42)
                : _page;

            // Açık olan sayfa yerinde temalanır: filtre, seçim ve form verileri korunur.
            TemaSayfaKontrolleriniUygula(pnlContent);
        }

        // Sayfa içindeki standart WinForms kart, yazı ve grid renklerini birlikte
        // değiştirir. İlk renkler saklandığı için açık moda dönmek kayıpsızdır.
        private void TemaSayfaKontrolleriniUygula(Control root)
        {
            if (root == null || root.IsDisposed) return;
            Color zemin = Color.FromArgb(15, 23, 42);
            Color kart = Color.FromArgb(30, 41, 59);
            Color yazi = Color.FromArgb(235, 240, 246);
            Color soluk = Color.FromArgb(168, 180, 196);
            Color kenar = Color.FromArgb(54, 67, 85);
            foreach (Control c in TemaKontrolleri(root))
            {
                Color[] ilk;
                if (!_temaOrijinalRenkleri.TryGetValue(c, out ilk))
                {
                    ilk = new Color[] { c.BackColor, c.ForeColor };
                    _temaOrijinalRenkleri[c] = ilk;
                }
                if (!_isDarkMode)
                {
                    c.BackColor = ilk[0];
                    c.ForeColor = ilk[1];
                }
                else
                {
                    Color bg = ilk[0];
                    if (bg.A > 0 && bg.R >= 225 && bg.G >= 225 && bg.B >= 225)
                        c.BackColor = c == root || c is TableLayoutPanel ||
                            c is FlowLayoutPanel ? zemin : kart;
                    else if (bg.A > 0 && bg.R >= 175 && bg.G >= 175 && bg.B >= 175)
                        c.BackColor = kart;
                    if (ilk[1].R < 150 && ilk[1].G < 150 && ilk[1].B < 150)
                        c.ForeColor = yazi;
                    else if (ilk[1].R < 190 && ilk[1].G < 190 && ilk[1].B < 190)
                        c.ForeColor = soluk;
                }
                RoundedPanel rp = c as RoundedPanel;
                if (rp != null)
                {
                    Color[] orijinalKart;
                    if (!_temaKartRenkleri.TryGetValue(rp, out orijinalKart))
                    {
                        orijinalKart = new Color[] { rp.FillColor, rp.BorderColor };
                        _temaKartRenkleri[rp] = orijinalKart;
                    }
                    if (_isDarkMode)
                    {
                        Color fill = orijinalKart[0];
                        rp.FillColor = fill.R >= 225 && fill.G >= 225 &&
                            fill.B >= 225 ? kart : fill;
                        rp.BorderColor = kenar;
                    }
                    else
                    {
                        rp.FillColor = orijinalKart[0];
                        rp.BorderColor = orijinalKart[1];
                    }
                    rp.Invalidate();
                }
                DataGridView dg = c as DataGridView;
                if (dg != null)
                {
                    Color[] g;
                    if (!_temaGridRenkleri.TryGetValue(dg, out g))
                    {
                        g = new Color[] {
                            dg.BackgroundColor, dg.DefaultCellStyle.BackColor,
                            dg.DefaultCellStyle.ForeColor,
                            dg.ColumnHeadersDefaultCellStyle.BackColor,
                            dg.ColumnHeadersDefaultCellStyle.ForeColor,
                            dg.AlternatingRowsDefaultCellStyle.BackColor,
                            dg.GridColor,
                            dg.DefaultCellStyle.SelectionBackColor,
                            dg.DefaultCellStyle.SelectionForeColor };
                        _temaGridRenkleri[dg] = g;
                    }
                    dg.BackgroundColor = _isDarkMode ? zemin : g[0];
                    dg.DefaultCellStyle.BackColor = _isDarkMode ? kart : g[1];
                    dg.DefaultCellStyle.ForeColor = _isDarkMode ? yazi : g[2];
                    dg.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? zemin : g[3];
                    dg.ColumnHeadersDefaultCellStyle.ForeColor = _isDarkMode ? yazi : g[4];
                    dg.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ?
                        Color.FromArgb(36, 49, 68) : g[5];
                    dg.GridColor = _isDarkMode ? kenar : g[6];
                    dg.DefaultCellStyle.SelectionBackColor = _isDarkMode ?
                        Color.FromArgb(26, 86, 155) : g[7];
                    dg.DefaultCellStyle.SelectionForeColor = _isDarkMode ? yazi : g[8];
                    dg.EnableHeadersVisualStyles = false;
                    dg.Invalidate();
                }
            }
        }

        private IEnumerable<Control> TemaKontrolleri(Control root)
        {
            yield return root;
            foreach (Control child in root.Controls)
                foreach (Control nested in TemaKontrolleri(child))
                    yield return nested;
        }

        private class RoundedPanel :
            Panel
        {
            public int Radius { get; set; }

            public Color FillColor { get; set; }

            public Color BorderColor { get; set; }

            public int BorderThickness { get; set; }

            public RoundedPanel()
            {
                DoubleBuffered =
                    true;

                Radius =
                    5;

                FillColor =
                    Color.White;

                BorderColor =
                    Color.LightGray;

                BorderThickness =
                    1;

                BackColor =
                    Color.Transparent;
            }

            protected override void OnPaint(
                PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle rect =
                    new Rectangle(
                        0,
                        0,
                        Width - 1,
                        Height - 1);

                if (rect.Width <= 0 ||
                    rect.Height <= 0)
                {
                    return;
                }

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                using (GraphicsPath path =
                    RoundedPath(
                        rect,
                        Math.Max(
                            1,
                            (int)Math.Round(
                                Radius * e.Graphics.DpiX / 96F))))
                using (SolidBrush fill =
                    new SolidBrush(
                        FillColor))
                using (Pen border =
                    new Pen(
                        BorderColor,
                        Math.Max(
                            1F,
                            BorderThickness * e.Graphics.DpiX / 96F)))
                {
                    e.Graphics.FillPath(
                        fill,
                        path);

                    if (BorderThickness > 0)
                    {
                        e.Graphics.DrawPath(
                            border,
                            path);
                    }
                }
            }

            private GraphicsPath RoundedPath(
                Rectangle rect,
                int radius)
            {
                GraphicsPath path =
                    new GraphicsPath();

                int d =
                    Math.Max(
                        2,
                        radius * 2);

                path.AddArc(
                    rect.X,
                    rect.Y,
                    d,
                    d,
                    180,
                    90);

                path.AddArc(
                    rect.Right - d,
                    rect.Y,
                    d,
                    d,
                    270,
                    90);

                path.AddArc(
                    rect.Right - d,
                    rect.Bottom - d,
                    d,
                    d,
                    0,
                    90);

                path.AddArc(
                    rect.X,
                    rect.Bottom - d,
                    d,
                    d,
                    90,
                    90);

                path.CloseFigure();

                return path;
            }
        }

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


        // =========================================================
        // NEXORA AI - GELİŞMİŞ YEREL SÜRÜM
        // =========================================================

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

        private string AiSorgusunuCalistir(string kullaniciMesaji)
        {
            string q = TurkceKucult(kullaniciMesaji);


            DateTime simdi = DateTime.Now;
            DateTime buAyBaslangic = new DateTime(simdi.Year, simdi.Month, 1);
            DateTime sonrakiAyBaslangic = buAyBaslangic.AddMonths(1);
            DateTime gecenAyBaslangic = buAyBaslangic.AddMonths(-1);

            bool buAy = q.Contains("bu ay") || q.Contains("bu ayki") ||
                        q.Contains("bu ayın") || q.Contains("bu ayda");
            bool gecenAy = q.Contains("geçen ay") || q.Contains("gecen ay");
            bool bugun = q.Contains("bugün") || q.Contains("bugun");

            string cariAdi = AiCariAdiniBul(kullaniciMesaji);

            DateTime? baslangic = null;
            DateTime? bitis = null;

            if (buAy)
            {
                baslangic = buAyBaslangic;
                bitis = sonrakiAyBaslangic;
            }
            else if (gecenAy)
            {
                baslangic = gecenAyBaslangic;
                bitis = buAyBaslangic;
            }
            else if (bugun)
            {
                baslangic = simdi.Date;
                bitis = simdi.Date.AddDays(1);
            }

            var temel = AppData.Faturalar.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(cariAdi))
            {
                temel = temel.Where(f =>
                    f.CariAdi.IndexOf(
                        cariAdi,
                        StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            if (baslangic.HasValue)
                temel = temel.Where(f => f.Tarih >= baslangic.Value);

            if (bitis.HasValue)
                temel = temel.Where(f => f.Tarih < bitis.Value);

            decimal tutarEsigi = AiTutarEsiginiBul(kullaniciMesaji);
            bool uzeri =
                q.Contains("üzeri") || q.Contains("üstü") ||
                q.Contains("fazla") || q.Contains("büyük");

            if (tutarEsigi > 0 && uzeri)
                temel = temel.Where(f => f.GenelToplam > tutarEsigi);

            var sonuc = temel
                .OrderByDescending(f => f.Tarih)
                .ToList();

            // -----------------------------------------------------
            // "BU AY İŞLER NASIL?" / GENEL YÖNETİCİ ÖZETİ
            // -----------------------------------------------------
            if (q.Contains("işler nasıl") ||
                q.Contains("isler nasil") ||
                q.Contains("durum nasıl") ||
                q.Contains("durum nasil") ||
                q.Contains("özetle") ||
                q.Contains("özet"))
            {
                var buAyFaturalari = AppData.Faturalar
                    .Where(f => f.Tarih >= buAyBaslangic &&
                                f.Tarih < sonrakiAyBaslangic)
                    .ToList();

                var gecenAyFaturalari = AppData.Faturalar
                    .Where(f => f.Tarih >= gecenAyBaslangic &&
                                f.Tarih < buAyBaslangic)
                    .ToList();

                decimal buAyToplam =
                    buAyFaturalari.Sum(f => f.GenelToplam);
                decimal gecenAyToplam =
                    gecenAyFaturalari.Sum(f => f.GenelToplam);

                string degisim;
                if (gecenAyToplam > 0)
                {
                    decimal oran =
                        ((buAyToplam - gecenAyToplam) / gecenAyToplam) * 100M;
                    degisim =
                        oran >= 0
                        ? "%" + oran.ToString("N1") + " artış"
                        : "%" + Math.Abs(oran).ToString("N1") + " düşüş";
                }
                else
                {
                    degisim = "geçen ay karşılaştırma verisi yok";
                }

                string enIyiCari = "-";
                decimal enIyiCariToplam = 0;

                var grup = buAyFaturalari
                    .GroupBy(f => f.CariAdi)
                    .Select(g => new
                    {
                        Cari = g.Key,
                        Toplam = g.Sum(x => x.GenelToplam)
                    })
                    .OrderByDescending(x => x.Toplam)
                    .FirstOrDefault();

                if (grup != null)
                {
                    enIyiCari = grup.Cari;
                    enIyiCariToplam = grup.Toplam;
                }

                return
                    "Bu ayın kısa özeti:\r\n\r\n" +
                    "• Fatura sayısı: " + buAyFaturalari.Count + "\r\n" +
                    "• Toplam fatura tutarı: " +
                    buAyToplam.ToString("N2") + " TL\r\n" +
                    "• Geçen aya göre: " + degisim + "\r\n" +
                    "• En yüksek hacimli cari: " + enIyiCari +
                    (grup == null ? "" :
                        " (" + enIyiCariToplam.ToString("N2") + " TL)") +
                    "\r\n" +
                    "• Toplam cari: " + AppData.Cariler.Count + "\r\n" +
                    "• Toplam ürün/hizmet: " + AppData.Urunler.Count;
            }

            // -----------------------------------------------------
            // BU AY - GEÇEN AY KARŞILAŞTIR
            // -----------------------------------------------------
            if ((q.Contains("karşılaştır") || q.Contains("karsilastir")) &&
                q.Contains("ay"))
            {
                var b = AppData.Faturalar
                    .Where(f => f.Tarih >= buAyBaslangic &&
                                f.Tarih < sonrakiAyBaslangic)
                    .ToList();

                var g = AppData.Faturalar
                    .Where(f => f.Tarih >= gecenAyBaslangic &&
                                f.Tarih < buAyBaslangic)
                    .ToList();

                decimal bt = b.Sum(f => f.GenelToplam);
                decimal gt = g.Sum(f => f.GenelToplam);

                string yorum;
                if (gt == 0)
                    yorum = "Geçen ay karşılaştırılabilir tutar bulunmuyor.";
                else
                {
                    decimal oran = ((bt - gt) / gt) * 100M;
                    yorum = oran >= 0
                        ? "Tutar %" + oran.ToString("N1") + " arttı."
                        : "Tutar %" + Math.Abs(oran).ToString("N1") + " azaldı.";
                }

                return
                    "Aylık karşılaştırma:\r\n\r\n" +
                    "Bu ay: " + b.Count + " fatura • " +
                    bt.ToString("N2") + " TL\r\n" +
                    "Geçen ay: " + g.Count + " fatura • " +
                    gt.ToString("N2") + " TL\r\n\r\n" +
                    yorum;
            }

            // -----------------------------------------------------
            // EN ÇOK HANGİ MÜŞTERİ/CARİ?
            // -----------------------------------------------------
            if ((q.Contains("en çok") || q.Contains("en cok")) &&
                (q.Contains("müşteri") || q.Contains("musteri") ||
                 q.Contains("cari") || q.Contains("kime")))
            {
                var grup = sonuc
                    .GroupBy(f => f.CariAdi)
                    .Select(g => new
                    {
                        Cari = g.Key,
                        Toplam = g.Sum(x => x.GenelToplam),
                        Adet = g.Count()
                    })
                    .OrderByDescending(x => x.Toplam)
                    .FirstOrDefault();

                if (grup == null)
                    return "Bu dönem için fatura kaydı bulamadım.";

                return
                    "En yüksek fatura hacmine sahip cari:\r\n\r\n" +
                    grup.Cari + "\r\n" +
                    grup.Adet + " fatura\r\n" +
                    grup.Toplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // SON N FATURA
            // -----------------------------------------------------
            int sonAdet = AiSonAdediBul(q);
            if (sonAdet > 0 && q.Contains("fatura"))
            {
                var sonlar = sonuc.Take(sonAdet).ToList();

                if (sonlar.Count == 0)
                    return "Bu sorguya uygun fatura bulamadım.";

                string metin =
                    (string.IsNullOrWhiteSpace(cariAdi) ? "" : cariAdi + " • ") +
                    "Son " + sonlar.Count + " fatura:\r\n\r\n";

                foreach (FaturaKaydi f in sonlar)
                {
                    metin +=
                        "• " + f.FaturaNo + " | " +
                        f.Tarih.ToString("dd.MM.yyyy") + " | " +
                        f.GenelToplam.ToString("N2") + " TL\r\n";
                }

                FaturalarAc(cariAdi, baslangic, bitis);
                pnlAiAssistant.BringToFront();
                return metin.TrimEnd();
            }

            // -----------------------------------------------------
            // EN YÜKSEK FATURA
            // -----------------------------------------------------
            if ((q.Contains("en yüksek") || q.Contains("en yuksek") ||
                 q.Contains("en büyük") || q.Contains("en buyuk")) &&
                q.Contains("fatura"))
            {
                FaturaKaydi enYuksek = sonuc
                    .OrderByDescending(f => f.GenelToplam)
                    .FirstOrDefault();

                if (enYuksek == null)
                    return "Gösterilecek fatura bulunamadı.";

                FaturalarAc();
                FaturaSatiriniSec(enYuksek.FaturaNo);
                pnlAiAssistant.BringToFront();

                return
                    "En yüksek fatura:\r\n\r\n" +
                    enYuksek.FaturaNo + "\r\n" +
                    enYuksek.CariAdi + "\r\n" +
                    enYuksek.Tarih.ToString("dd.MM.yyyy") + "\r\n" +
                    enYuksek.GenelToplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // TOPLAM / NE KADAR?
            // -----------------------------------------------------
            if ((q.Contains("toplam") || q.Contains("ne kadar") ||
                 q.Contains("ciro")) &&
                (q.Contains("fatura") || q.Contains("kest") ||
                 q.Contains("ciro") || !string.IsNullOrWhiteSpace(cariAdi)))
            {
                decimal toplam = sonuc.Sum(f => f.GenelToplam);

                return
                    (string.IsNullOrWhiteSpace(cariAdi)
                        ? ""
                        : cariAdi + " için ") +
                    AiDonemAdi(buAy, gecenAy, bugun) +
                    sonuc.Count + " fatura var.\r\n" +
                    "Toplam tutar: " +
                    toplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // KAÇ FATURA?
            // -----------------------------------------------------
            if (q.Contains("kaç") && q.Contains("fatura"))
            {
                return
                    AiDonemAdi(buAy, gecenAy, bugun) +
                    sonuc.Count + " fatura bulunuyor.";
            }

            // -----------------------------------------------------
            // FATURALARI GETİR / GÖSTER / LİSTELE
            // -----------------------------------------------------
            if (q.Contains("fatura") &&
                (q.Contains("getir") || q.Contains("göster") ||
                 q.Contains("goster") || q.Contains("listele") ||
                 !string.IsNullOrWhiteSpace(cariAdi) ||
                 tutarEsigi > 0))
            {
                if (sonuc.Count == 0)
                {
                    return
                        (string.IsNullOrWhiteSpace(cariAdi)
                            ? ""
                            : cariAdi + " için ") +
                        AiDonemAdi(buAy, gecenAy, bugun) +
                        "uygun fatura bulamadım.";
                }

                decimal toplam = sonuc.Sum(f => f.GenelToplam);

                FaturalarAc(cariAdi, baslangic, bitis);
                pnlAiAssistant.BringToFront();

                return
                    (string.IsNullOrWhiteSpace(cariAdi)
                        ? ""
                        : cariAdi + " için ") +
                    sonuc.Count + " fatura buldum.\r\n" +
                    "Toplam: " + toplam.ToString("N2") + " TL\r\n\r\n" +
                    "Faturalar ekranını açtım.";
            }

            if (q.Contains("yardım") || q.Contains("yardim") ||
                q.Contains("ne yapabilirsin"))
            {
                return
                    "Şunları deneyebilirsiniz:\r\n\r\n" +
                    "• Bu ay işler nasıl?\r\n" +
                    "• ABC Mobilya'nın bu ayki faturalarını getir\r\n" +
                    "• ABC'nin son 3 faturasını göster\r\n" +
                    "• Bu ay en çok hangi müşteriye fatura kestim?\r\n" +
                    "• Bu ay ile geçen ayı karşılaştır\r\n" +
                    "• 10.000 TL üzerindeki faturaları göster\r\n" +
                    "• En yüksek faturam hangisi?";
            }

            return
                "Bu cümleyi henüz güvenli bir veri sorgusuna çeviremedim.\r\n\r\n" +
                "Fatura, müşteri, dönem, toplam veya karşılaştırma içeren bir soru deneyin. " +
                "Örneğin “Bu ay işler nasıl?” yazabilirsiniz.";
        }

        // =========================================================
        // NEXORA AI - FATURA HAZIRLAMA
        // =========================================================
        
        
        private string AiBosDegilse(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Kayıtta yok" : value;
        }

        private UrunKaydi AiUrunuBul(string mesaj)
        {
            if (AppData.Urunler == null)
                return null;

            string normalMesaj = TurkceKucult(mesaj);

            foreach (UrunKaydi urun in AppData.Urunler
                .Where(u => u != null && !string.IsNullOrWhiteSpace(u.UrunAdi))
                .OrderByDescending(u => u.UrunAdi.Length))
            {
                if (normalMesaj.Contains(TurkceKucult(urun.UrunAdi)))
                    return urun;
            }

            UrunKaydi enIyi = null;
            int enIyiPuan = 0;

            foreach (UrunKaydi urun in AppData.Urunler)
            {
                if (urun == null || string.IsNullOrWhiteSpace(urun.UrunAdi))
                    continue;

                string[] kelimeler = TurkceKucult(urun.UrunAdi).Split(
                    new char[] { ' ', '-', '/', '(', ')' },
                    StringSplitOptions.RemoveEmptyEntries);

                int puan = 0;
                foreach (string kelime in kelimeler)
                {
                    if (kelime.Length >= 3 && normalMesaj.Contains(kelime))
                        puan++;
                }

                if (puan > enIyiPuan)
                {
                    enIyiPuan = puan;
                    enIyi = urun;
                }
            }

            return enIyiPuan > 0 ? enIyi : null;
        }

        private decimal AiMiktariBul(string mesaj, bool devamKonusmasi)
        {
            Match m = Regex.Match(
                mesaj,
                @"(?<miktar>\d+(?:[\.,]\d+)?)\s*(?:adet|tane|saat|gün|gun|kg|paket|lisans|birim)",
                RegexOptions.IgnoreCase);

            // AI miktarı sorduysa kullanıcının yalnızca "2" yazması da yeterlidir.
            if (!m.Success && devamKonusmasi)
            {
                m = Regex.Match(
                    mesaj.Trim(),
                    @"^(?<miktar>\d+(?:[\.,]\d+)?)$");
            }

            if (!m.Success)
                return 0m;

            string deger = m.Groups["miktar"].Value.Replace(',', '.');
            decimal miktar;
            if (decimal.TryParse(
                deger,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out miktar))
            {
                return miktar;
            }

            return 0m;
        }

        
        
       
        private void AiFormMetodunuCalistirParametreli(object nesne, string metodAdi, int deger)
        {
            if (nesne == null) return;
            System.Reflection.MethodInfo method = nesne.GetType().GetMethod(
                metodAdi,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public,
                null,
                new Type[] { typeof(int) },
                null);
            if (method != null)
                method.Invoke(nesne, new object[] { deger });
        }

        private void AiTextBoxDoldur(object form, string[] alanAdlari, string deger)
        {
            if (form == null || alanAdlari == null)
                return;

            foreach (string alanAdi in alanAdlari)
            {
                TextBox tb = AiFormAlaniBul<TextBox>(form, alanAdi);
                if (tb != null)
                    tb.Text = deger ?? "";
            }
        }

        private T AiFormAlaniBul<T>(object nesne, string alanAdi) where T : class
        {
            if (nesne == null)
                return null;

            System.Reflection.FieldInfo field = nesne.GetType().GetField(
                alanAdi,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
                return null;

            return field.GetValue(nesne) as T;
        }

        private void AiFormMetodunuCalistir(object nesne, string metodAdi)
        {
            if (nesne == null)
                return;

            System.Reflection.MethodInfo method = nesne.GetType().GetMethod(
                metodAdi,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (method != null)
                method.Invoke(nesne, null);
        }

        private string AiDonemAdi(
            bool buAy,
            bool gecenAy,
            bool bugun)
        {
            if (buAy) return "Bu ay ";
            if (gecenAy) return "Geçen ay ";
            if (bugun) return "Bugün ";
            return "";
        }

        private int AiSonAdediBul(string q)
        {
            if (!(q.Contains("son") && q.Contains("fatura")))
                return 0;

            string[] parcalar = q.Split(
                new char[] { ' ', ',', '.', '?', '!', ':', ';' },
                StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parcalar.Length; i++)
            {
                int n;
                if (int.TryParse(parcalar[i], out n) &&
                    n > 0 && n <= 50)
                {
                    return n;
                }
            }

            return 1;
        }

        private decimal AiTutarEsiginiBul(string mesaj)
        {
            string temiz = mesaj
                .Replace(".", "")
                .Replace(",", ".")
                .Replace("TL", "")
                .Replace("tl", "");

            Match m = Regex.Match(
                temiz,
                @"\d+(?:\.\d+)?");

            if (!m.Success)
                return 0;

            decimal d;
            if (decimal.TryParse(
                m.Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out d))
            {
                return d;
            }

            return 0;
        }

        private string AiCariAdiniBul(string mesaj)
        {
            if (AppData.Cariler == null)
                return null;

            string normalMesaj = TurkceKucult(mesaj);

            foreach (CariKaydi cari in AppData.Cariler)
            {
                string tamAd = TurkceKucult(cari.CariAdi);
                if (normalMesaj.Contains(tamAd))
                    return cari.CariAdi;
            }

            foreach (CariKaydi cari in AppData.Cariler)
            {
                string sade = TurkceKucult(cari.CariAdi)
                    .Replace(" a.ş.", "")
                    .Replace(" a.ş", "")
                    .Replace(" a.s.", "")
                    .Replace(" ltd.", "")
                    .Replace(" ltd", "")
                    .Replace(" limited", "")
                    .Replace(" sanayi", "")
                    .Replace(" ticaret", "")
                    .Trim();

                if (sade.Length >= 3 &&
                    normalMesaj.Contains(sade))
                    return cari.CariAdi;

                string[] kelimeler = sade.Split(
                    new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                // Tek kelimeyle eşleşmede çok genel kelimeleri kullanma.
                foreach (string kelime in kelimeler)
                {
                    if (kelime.Length >= 4 &&
                        kelime != "mobilya" &&
                        kelime != "sanayi" &&
                        kelime != "ticaret" &&
                        normalMesaj.Contains(kelime))
                    {
                        return cari.CariAdi;
                    }
                }
            }

            return null;
        }

        private string TurkceKucult(string metin)
        {
            if (metin == null)
                return "";

            return metin.ToLower(
                new System.Globalization.CultureInfo("tr-TR"));
        }

        private void ListeGridleriniBoyutlandir()
        {
            if (pnlContent == null)
                return;

            foreach (Control c in pnlContent.Controls)
            {
                DataGridView grid = c as DataGridView;
                if (grid == null)
                    continue;

                grid.Width =
                    Math.Max(
                        700,
                        pnlContent.ClientSize.Width -
                        grid.Left - 28);

                grid.Height =
                    Math.Max(
                        350,
                        pnlContent.ClientSize.Height -
                        grid.Top - 28);
            }
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
