using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.IO;
using System.Text;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;

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
        private EventHandler _dashboardResizeHandler;

        // =========================================================
        // NEXORA AI - UYGULAMA İÇİ ASİSTAN
        // =========================================================
        private Panel pnlAiAssistant;
        private FlowLayoutPanel flpAiMessages;
        private TextBox txtAiMessage;
        private Button btnAiFloating;
        private bool _aiPanelOpen;

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
            pnlMenu =
                new Panel();

            pnlMenu.Dock =
                DockStyle.Fill;

            pnlMenu.Margin =
                new Padding(0);

            pnlMenu.BackColor =
                _sidebar;

            TableLayoutPanel side =
                new TableLayoutPanel();

            side.Dock =
                DockStyle.Fill;

            side.Margin =
                new Padding(0);

            side.Padding =
                new Padding(0);

            side.ColumnCount =
                1;

            side.RowCount =
                3;

            side.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            side.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    96F));

            side.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            side.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    86F));

            // -----------------------------------------------------
            // LOGO
            // -----------------------------------------------------

            Panel logoPanel =
                new Panel();

            logoPanel.Dock =
                DockStyle.Fill;

            logoPanel.Margin =
                new Padding(0);

            logoPanel.BackColor =
                _sidebar;

            Label logo =
                new Label();

            logo.Text =
                "NEXORA";

            logo.AutoSize =
                true;

            logo.Font =
                new Font(
                    "Segoe UI",
                    19F,
                    FontStyle.Bold);

            logo.ForeColor =
                Color.White;

            logo.Location =
                new Point(
                    16,
                    15);

            logoPanel.Controls.Add(
                logo);

            Label erp =
                new Label();

            erp.Text =
                "ERP";

            erp.AutoSize =
                false;

            erp.Size =
                new Size(
                    35,
                    18);

            erp.TextAlign =
                ContentAlignment.MiddleCenter;

            erp.BackColor =
                _primary;

            erp.ForeColor =
                Color.White;

            erp.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            erp.Location =
                new Point(
                    118,
                    21);

            logoPanel.Controls.Add(
                erp);

            logoPanel.Resize +=
                delegate
                {
                    erp.Left =
                        logo.Right + 8;
                };

            Label subtitle =
                new Label();

            subtitle.Text =
                "İşletme Yönetim Platformu";

            subtitle.AutoSize =
                true;

            subtitle.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            subtitle.ForeColor =
                Color.FromArgb(
                    190,
                    196,
                    202);

            subtitle.Location =
                new Point(
                    17,
                    49);

            logoPanel.Controls.Add(
                subtitle);

            Panel logoLine =
                new Panel();

            logoLine.Dock =
                DockStyle.Bottom;

            logoLine.Height =
                1;

            logoLine.BackColor =
                Color.FromArgb(
                    66,
                    72,
                    77);

            logoPanel.Controls.Add(
                logoLine);

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

            menu.Margin =
                new Padding(0);

            menu.Padding =
                new Padding(
                    14,
                    20,
                    14,
                    14);

            menu.BackColor =
                _sidebar;

            menu.Controls.Add(
                MenuButton(
                    "\uE80F",
                    "Genel Bakış",
                    Dashboard_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE716",
                    "Müşteriler",
                    Musteriler_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE821",
                    "Kurumsal Müşteriler",
                    Kurumsal_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE8C7",
                    "Cari Hesaplar",
                    Cari_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE7C3",
                    "Ürün / Hizmetler",
                    Urunler_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE70F",
                    "Yeni Fatura",
                    YeniFatura_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE8A5",
                    "Faturalar",
                    Faturalar_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE77B",
                    "Kullanıcılar",
                    Kullanicilar_Click));

            menu.Controls.Add(
                MenuButton(
                    "\uE713",
                    "Ayarlar",
                    Ayarlar_Click));

            // -----------------------------------------------------
            // ALT HESAP KARTI
            // -----------------------------------------------------

            Panel accountHost =
                new Panel();

            accountHost.Dock =
                DockStyle.Fill;

            accountHost.Margin =
                new Padding(0);

            accountHost.Padding =
                new Padding(
                    7,
                    9,
                    7,
                    9);

            accountHost.BackColor =
                _sidebar;

            RoundedPanel account =
                new RoundedPanel();

            account.Dock =
                DockStyle.Fill;

            account.Radius =
                6;

            account.FillColor =
                Color.FromArgb(
                    46,
                    52,
                    57);

            account.BorderColor =
                Color.FromArgb(
                    72,
                    78,
                    83);

            account.BorderThickness =
                1;

            PictureBox avatar =
                new PictureBox();

            avatar.Size =
                new Size(
                    34,
                    34);

            avatar.Location =
                new Point(
                    8,
                    10);

            avatar.SizeMode =
                PictureBoxSizeMode.CenterImage;

            avatar.Image =
                IconBitmap(
                    "\uE77B",
                    17,
                    Color.White,
                    34,
                    Color.FromArgb(
                        0,
                        88,
                        189));

            account.Controls.Add(
                avatar);

            Label accountName =
                new Label();

            accountName.Text =
                string.IsNullOrWhiteSpace(_kullaniciAdi) ? "Kullanıcı" : _kullaniciAdi;

            accountName.AutoSize =
                false;

            accountName.Size =
                new Size(
                    137,
                    18);

            accountName.Location =
                new Point(
                    49,
                    10);

            accountName.Font =
                new Font(
                    "Segoe UI",
                    11.5F,
                    FontStyle.Bold);

            accountName.ForeColor =
                Color.White;

            account.Controls.Add(
                accountName);

            Label accountCaption =
                new Label();

            accountCaption.Text =
                "Hesabım";

            accountCaption.AutoSize =
                true;

            accountCaption.Location =
                new Point(
                    49,
                    31);

            accountCaption.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            accountCaption.ForeColor =
                Color.FromArgb(
                    196,
                    201,
                    207);

            account.Controls.Add(
                accountCaption);

            Button logout =
                new Button();

            logout.Size =
                new Size(
                    32,
                    32);

            logout.FlatStyle =
                FlatStyle.Flat;

            logout.FlatAppearance.BorderSize =
                0;

            logout.BackColor =
                Color.FromArgb(
                    46,
                    52,
                    57);

            logout.Image =
                IconBitmap(
                    "\uE8AC",
                    14,
                    Color.FromArgb(
                        211,
                        217,
                        223),
                    22,
                    Color.Transparent);

            logout.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            logout.Location =
                new Point(
                    198,
                    11);

            logout.Cursor =
                Cursors.Hand;

            logout.Click +=
                delegate
                {
                    Close();
                };

            account.Resize +=
                delegate
                {
                    logout.Left =
                        account.ClientSize.Width -
                        logout.Width -
                        7;
                };

            account.Controls.Add(
                logout);

            accountHost.Controls.Add(
                account);

            side.Controls.Add(
                logoPanel,
                0,
                0);

            side.Controls.Add(
                menu,
                0,
                1);

            side.Controls.Add(
                accountHost,
                0,
                2);

            pnlMenu.Controls.Add(
                side);

            rootLayout.Controls.Add(
                pnlMenu,
                0,
                0);
        }

        private Button MenuButton(
            string glyph,
            string text,
            EventHandler click)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Size =
                new Size(360, 54);

            button.Margin = new Padding(0, 0, 0, 6);

            button.Padding = new Padding(12, 0, 0, 0);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.FlatAppearance.MouseOverBackColor =
                _sidebarHover;

            button.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(
                    35,
                    40,
                    44);

            button.BackColor =
                _sidebar;

            button.ForeColor =
                Color.FromArgb(
                    226,
                    230,
                    234);

            button.Font =
                new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);

            button.Image =
                IconBitmap(
                    glyph,
                    12.5F,
                    Color.FromArgb(
                        211,
                        217,
                        223),
                    24,
                    Color.Transparent);

            button.ImageAlign =
                ContentAlignment.MiddleLeft;

            button.TextImageRelation =
                TextImageRelation.ImageBeforeText;

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.Cursor =
                Cursors.Hand;

            button.Click +=
                click;

            return button;
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

            txtAkilliArama.Location = new Point(24, 20);

            txtAkilliArama.Size = new Size(600, 38);

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
                            370,
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
                            190,
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
            refresh.Size = new Size(132, 32);

            Button quickInvoice =
                SmallActionButton("\uE710", "Hızlı e-Fatura Düzenle", true);
            quickInvoice.Size = new Size(178, 32);

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
            helloText.Location = new Point(35, 145);
            welcome.Controls.Add(helloText);

            RoundedPanel searchPreview = new RoundedPanel();
            searchPreview.Size = new Size(420, 220);
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
            miniSearchIcon.Location = new Point(10, 9);
            miniSearchIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            searchPreview.Controls.Add(miniSearchIcon);

            Label previewTitle = new Label();
            previewTitle.Text = "ARAMA SONUÇLARI ÖNİZLEME";
            previewTitle.AutoSize = true;
            previewTitle.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            previewTitle.ForeColor = CurrentPrimaryText();
            previewTitle.Location = new Point(30, 9);
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
                searchPreview, 45, "CARİ",
                AppData.Cariler.Count > 0
                    ? AppData.Cariler[0].CariAdi
                    : "Anadolu Lojistik",
                "\uE77B");

            AddPreviewRow(
                searchPreview, 98, "ÜRÜN",
                AppData.Urunler.Count > 0
                    ? AppData.Urunler[0].UrunAdi
                    : "ERP Yazılım Lisansı V3",
                "\uE7C3");

            AddPreviewRow(
                searchPreview, 151, "FATURA",
                AppData.Faturalar.Count > 0
                    ? AppData.Faturalar[0].FaturaNo
                    : "FTR-2025-001",
                "\uE8A5");

            Action layoutWelcome = delegate
            {
                adminBadge.Left = hello.Right + 10;
                adminBadge.Top = hello.Top + 9;

                searchPreview.Left =
                    welcome.ClientSize.Width - searchPreview.Width - 20;
                searchPreview.Top = 34;

                matches.Left =
                    searchPreview.ClientSize.Width - matches.Width - 10;
                matches.Top = 9;
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
            arrange.Size = new Size(138, 32);
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
                    quick.ClientSize.Width - arrange.Width - 16;
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
            filter.Size = new Size(190, 32);
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
            excel.Size = new Size(82, 32);
            excel.Margin = new Padding(0, 0, 8, 0);

            Button print =
                SmallActionButton("\uE749", "Yazdır", false);
            print.Size = new Size(82, 32);
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
            viewCol.Width = 66;
            viewCol.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;
            viewCol.FlatStyle = FlatStyle.Flat;
            viewCol.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(viewCol);

            DataGridViewButtonColumn downloadCol =
                new DataGridViewButtonColumn();
            downloadCol.Name = "Indir";
            downloadCol.HeaderText = "";
            downloadCol.Text = "İndir";
            downloadCol.UseColumnTextForButtonValue = true;
            downloadCol.Width = 66;
            downloadCol.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;
            downloadCol.FlatStyle = FlatStyle.Flat;
            downloadCol.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
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
                    invoiceActions.Width - 15;
                invoiceActions.Top = 22;

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
            SayfayiTemizle(
                "Müşteriler");

            Label baslik =
                SayfaListeBasligi(
                    "Bireysel Müşteriler");

            pnlContent.Controls.Add(
                baslik);

            DataGridView grid =
                TemelGrid();

            grid.Location = new Point(24, 92);

            grid.Size =
                new Size(
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

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

            grid.Location = new Point(24, 92);

            grid.Size =
                new Size(
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

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
                    11.5F,
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
                    940,
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
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

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
                    11.5F,
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
                    940,
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
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

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
            FaturalarAc(null, null, null);
        }

        private void FaturalarAc(
            string cariAdi,
            DateTime? baslangic,
            DateTime? bitis)
        {
            SayfayiTemizle(
                "Faturalar");

            Label title =
                SayfaListeBasligi(
                    "Fatura Listesi");

            pnlContent.Controls.Add(
                title);

            Label filtreBilgisi = null;

            if (!string.IsNullOrWhiteSpace(cariAdi) ||
                baslangic.HasValue ||
                bitis.HasValue)
            {
                filtreBilgisi = new Label();
                filtreBilgisi.AutoSize = true;
                filtreBilgisi.ForeColor = _primary;
                filtreBilgisi.Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold);

                string metin = "AI filtresi: ";

                if (!string.IsNullOrWhiteSpace(cariAdi))
                {
                    metin += cariAdi;
                }

                if (baslangic.HasValue)
                {
                    metin +=
                        (metin.EndsWith(": ") ? "" : " • ") +
                        baslangic.Value.ToString("dd.MM.yyyy");
                }

                if (bitis.HasValue)
                {
                    metin +=
                        " - " +
                        bitis.Value.AddDays(-1).ToString("dd.MM.yyyy");
                }

                filtreBilgisi.Text = metin;
                filtreBilgisi.Location = new Point(28, 62);
                pnlContent.Controls.Add(filtreBilgisi);
            }

            DataGridView grid =
                TemelGrid();

            grid.Location =
                new Point(
                    28,
                    95);

            grid.Size =
                new Size(
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            KolonEkle(grid, "FaturaNo", "Fatura No");
            KolonEkle(grid, "Tarih", "Tarih");
            KolonEkle(grid, "Cari", "Cari");
            KolonEkle(grid, "Tip", "Cari Tipi");
            KolonEkle(grid, "BelgeTipi", "Belge Tipi");
            KolonEkle(grid, "Tutar", "Genel Toplam");
            KolonEkle(grid, "Durum", "Durum");

            foreach (FaturaKaydi fatura in AppData.Faturalar)
            {
                bool uygun = true;

                if (!string.IsNullOrWhiteSpace(cariAdi))
                {
                    uygun =
                        fatura.CariAdi.IndexOf(
                            cariAdi,
                            StringComparison.CurrentCultureIgnoreCase) >= 0;
                }

                if (uygun &&
                    baslangic.HasValue)
                {
                    uygun =
                        fatura.Tarih >=
                        baslangic.Value;
                }

                if (uygun &&
                    bitis.HasValue)
                {
                    uygun =
                        fatura.Tarih <
                        bitis.Value;
                }

                if (!uygun)
                {
                    continue;
                }

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
                    940,
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
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
                    Math.Max(420, pnlContent.ClientSize.Height - grid.Top - 24));

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
                    Math.Max(900, pnlContent.ClientSize.Width - 48),
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
                    16F,
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

            button.AutoSize =
                false;

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
            RoundedPanel row =
                new RoundedPanel();

            row.Location =
                new Point(
                    8,
                    y);

            row.Size =
                new Size(
                    315,
                    30);

            row.Radius =
                3;

            row.FillColor =
                CurrentCardColor();

            row.BorderColor =
                CurrentBorderColor();

            row.BorderThickness =
                1;

            Label typeBadge =
                Badge(
                    type,
                    _isDarkMode
                        ? Color.FromArgb(
                            51,
                            64,
                            82)
                        : Color.FromArgb(
                            235,
                            239,
                            244),
                    CurrentMutedText());

            typeBadge.Location =
                new Point(
                    5,
                    5);

            row.Controls.Add(
                typeBadge);

            PictureBox icon =
                new PictureBox();

            icon.Image =
                IconBitmap(
                    glyph,
                    9,
                    _primary,
                    16,
                    Color.Transparent);

            icon.Size =
                new Size(
                    16,
                    16);

            icon.Location =
                new Point(
                    54,
                    7);

            icon.SizeMode =
                PictureBoxSizeMode.CenterImage;

            row.Controls.Add(
                icon);

            Label label =
                new Label();

            label.Text =
                title;

            label.AutoEllipsis =
                true;

            label.Size =
                new Size(
                    205,
                    18);

            label.Location =
                new Point(
                    73,
                    6);

            label.Font =
                new Font(
                    "Segoe UI",
                    10.5F);

            label.ForeColor =
                CurrentPrimaryText();

            row.Controls.Add(
                label);

            PictureBox arrow =
                new PictureBox();

            arrow.Image =
                IconBitmap(
                    "\uE72A",
                    9,
                    CurrentMutedText(),
                    16,
                    Color.Transparent);

            arrow.Size =
                new Size(
                    16,
                    16);

            arrow.Location =
                new Point(
                    291,
                    7);

            arrow.SizeMode =
                PictureBoxSizeMode.CenterImage;

            row.Controls.Add(
                arrow);

            row.Resize +=
                delegate
                {
                    icon.Left =
                        typeBadge.Right + 7;

                    label.Left =
                        icon.Right + 5;

                    arrow.Left =
                        row.ClientSize.Width -
                        arrow.Width -
                        8;

                    label.Width =
                        Math.Max(
                            30,
                            arrow.Left -
                            label.Left -
                            5);
                };

            icon.Left =
                typeBadge.Right + 7;

            label.Left =
                icon.Right + 5;

            arrow.Left =
                row.ClientSize.Width -
                arrow.Width -
                8;

            label.Width =
                Math.Max(
                    30,
                    arrow.Left -
                    label.Left -
                    5);

            parent.Controls.Add(
                row);
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

            // Dashboard yeniden kurularak tüm kartlar aynı anda temalanır.
            if (lblSayfaBaslik != null &&
                lblSayfaBaslik.Text ==
                    "Genel Bakış")
            {
                DashboardAc();
            }
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
