using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Managers;

namespace UrunFaturaYonetimi
{
    public class LoginForm : Form
    {
        private const string BrandName = "NEXORA";

        private readonly IUserService _userService;

        private LeftBrandPanel _left;
        private RightLoginPanel _right;

        private PremiumInput _userInput;
        private PremiumInput _passwordInput;
        private TextBox _txtUser;
        private TextBox _txtPassword;

        private PremiumButton _btnLogin;
        private PremiumButton _btnTheme;
        private EyeButton _btnEye;

        private LinkLabel _lnkForgot;
        private LinkLabel _lnkRegister;

        private bool _passwordVisible;
        private bool _loginBusy;

        public LoginForm()
            : this(new UserManager())
        {
        }

        public LoginForm(IUserService userService)
        {
            _userService = userService;

            ThemeManager.Load();

            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1180, 720);
            Text = BrandName + " - Güvenli Giriş";
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(8, 20, 38);
            DoubleBuffered = true;
            KeyPreview = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BuildUi();
            ApplyTheme();
        }

        private void BuildUi()
        {
            SuspendLayout();

            _left = new LeftBrandPanel();
            _left.BrandName = BrandName;
            _left.Dock = DockStyle.Left;
            Controls.Add(_left);

            _right = new RightLoginPanel();
            _right.Dock = DockStyle.Fill;
            Controls.Add(_right);
            _right.BringToFront();

            _btnTheme = new PremiumButton();
            _btnTheme.Text = "☾  Koyu mod";
            _btnTheme.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnTheme.Size = new Size(132, 42);
            _btnTheme.CornerRadius = 18;
            _btnTheme.Cursor = Cursors.Hand;
            _btnTheme.Click += BtnTheme_Click;
            _right.Controls.Add(_btnTheme);

            _userInput = new PremiumInput();
            _userInput.IconGlyph = "\uE77B";
            _userInput.Placeholder = "E-posta veya kullanıcı adı";
            _right.Controls.Add(_userInput);
            _txtUser = _userInput.InnerTextBox;

            _passwordInput = new PremiumInput();
            _passwordInput.IconGlyph = "\uE72E";
            _passwordInput.Placeholder = "Şifrenizi girin";
            _right.Controls.Add(_passwordInput);
            _txtPassword = _passwordInput.InnerTextBox;
            _txtPassword.UseSystemPasswordChar = true;

            _btnEye = new EyeButton();
            _btnEye.Glyph = "\uE890";
            _btnEye.Size = new Size(42, 36);
            _btnEye.Cursor = Cursors.Hand;
            _btnEye.Click += BtnEye_Click;
            _passwordInput.Controls.Add(_btnEye);
            _btnEye.BringToFront();

            _lnkForgot = new LinkLabel();
            _lnkForgot.Text = "Şifremi unuttum?";
            _lnkForgot.AutoSize = true;
            _lnkForgot.Font = new Font("Segoe UI", 9.4F, FontStyle.Regular);
            _lnkForgot.LinkBehavior = LinkBehavior.HoverUnderline;
            _lnkForgot.Cursor = Cursors.Hand;
            _lnkForgot.UseCompatibleTextRendering = false;
            _lnkForgot.Click += SifremiUnuttum_Click;
            _right.Controls.Add(_lnkForgot);

            _btnLogin = new PremiumButton();
            _btnLogin.Text = "Giriş yap  →";
            _btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnLogin.CornerRadius = 10;
            _btnLogin.Cursor = Cursors.Hand;
            _btnLogin.Click += BtnGiris_Click;
            _right.Controls.Add(_btnLogin);

            _lnkRegister = new LinkLabel();
            _lnkRegister.Text = "Kayıt olun";
            _lnkRegister.AutoSize = true;
            _lnkRegister.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            _lnkRegister.LinkBehavior = LinkBehavior.HoverUnderline;
            _lnkRegister.Cursor = Cursors.Hand;
            _lnkRegister.UseCompatibleTextRendering = false;
            _lnkRegister.Click += KayitOl_Click;
            _right.Controls.Add(_lnkRegister);

            AcceptButton = _btnLogin;

            Resize += delegate { LayoutAll(); };
            Shown += delegate
            {
                LayoutAll();
                _txtUser.Focus();
            };

            ResumeLayout(false);
            LayoutAll();
        }

        private void LayoutAll()
        {
            if (_left == null || _right == null)
                return;

            int leftWidth = (int)Math.Round(ClientSize.Width * 0.47);
            leftWidth = Math.Max(520, Math.Min(760, leftWidth));
            _left.Width = leftWidth;

            int rightWidth = ClientSize.Width - leftWidth;

            int cardWidth = Math.Min(690, Math.Max(560, rightWidth - 90));
            int cardHeight = Math.Min(690, Math.Max(610, ClientSize.Height - 86));

            Rectangle card = new Rectangle(
                Math.Max(32, (rightWidth - cardWidth) / 2),
                Math.Max(26, (ClientSize.Height - cardHeight) / 2),
                cardWidth,
                cardHeight);

            _right.CardBounds = card;

            int left = card.Left + 40;
            int width = card.Width - 80;

            _btnTheme.Location = new Point(
                card.Right - _btnTheme.Width - 25,
                card.Top + 21);

            _userInput.Location = new Point(left, card.Top + 178);
            _userInput.Size = new Size(width, 48);

            _passwordInput.Location = new Point(left, card.Top + 272);
            _passwordInput.Size = new Size(width, 48);

            _btnEye.Location = new Point(
                _passwordInput.Width - 47,
                6);

            _lnkForgot.Location = new Point(
                card.Right - _lnkForgot.PreferredWidth - 41,
                card.Top + 338);

            _btnLogin.Location = new Point(left, card.Top + 377);
            _btnLogin.Size = new Size(width, 55);

            _lnkRegister.Location = new Point(
                card.Left + (card.Width / 2) + 54,
                card.Top + 566);

            _right.Invalidate();
        }

        private void ApplyTheme()
        {
            bool dark = ThemeManager.IsDark;

            Color page = dark
                ? Color.FromArgb(8, 19, 37)
                : Color.FromArgb(238, 245, 255);

            Color card = dark
                ? Color.FromArgb(19, 33, 56)
                : Color.FromArgb(248, 251, 255);

            Color cardBorder = dark
                ? Color.FromArgb(44, 60, 84)
                : Color.FromArgb(211, 223, 239);

            Color primary = dark
                ? Color.FromArgb(244, 247, 252)
                : Color.FromArgb(16, 35, 65);

            Color secondary = dark
                ? Color.FromArgb(156, 174, 201)
                : Color.FromArgb(93, 112, 142);

            Color inputFill = dark
                ? Color.FromArgb(20, 33, 55)
                : Color.FromArgb(255, 255, 255);

            Color inputBorder = dark
                ? Color.FromArgb(67, 83, 110)
                : Color.FromArgb(198, 212, 230);

            Color accent = Color.FromArgb(55, 150, 255);
            Color accent2 = Color.FromArgb(37, 102, 246);
            Color success = Color.FromArgb(18, 196, 108);

            BackColor = page;

            _right.PageColor = page;
            _right.CardColor = card;
            _right.CardBorderColor = cardBorder;
            _right.PrimaryText = primary;
            _right.SecondaryText = secondary;
            _right.Accent = accent;
            _right.Success = success;

            _left.DarkMode = dark;
            _left.Invalidate();

            _userInput.FillColor = inputFill;
            _userInput.BorderColor = inputBorder;
            _userInput.FocusBorderColor = accent;
            _userInput.TextColor = primary;
            _userInput.PlaceholderColor = secondary;
            _userInput.IconColor = Color.FromArgb(183, 198, 220);

            _passwordInput.FillColor = inputFill;
            _passwordInput.BorderColor = inputBorder;
            _passwordInput.FocusBorderColor = accent;
            _passwordInput.TextColor = primary;
            _passwordInput.PlaceholderColor = secondary;
            _passwordInput.IconColor = Color.FromArgb(183, 198, 220);

            _btnEye.ForeColor = primary;

            _btnLogin.FillColor = accent2;
            _btnLogin.HoverColor = Color.FromArgb(55, 139, 255);
            _btnLogin.TextColor = Color.White;
            _btnLogin.BorderColor = Color.Transparent;

            _btnTheme.FillColor = dark
                ? Color.FromArgb(47, 61, 84)
                : Color.FromArgb(255, 255, 255);

            _btnTheme.HoverColor = dark
                ? Color.FromArgb(58, 73, 98)
                : Color.FromArgb(245, 249, 255);

            _btnTheme.TextColor = primary;
            _btnTheme.BorderColor = dark
                ? Color.FromArgb(66, 82, 108)
                : Color.FromArgb(211, 223, 239);

            _btnTheme.Text = dark
                ? "☀  Açık mod"
                : "☾  Koyu mod";

            _lnkForgot.LinkColor = accent;
            _lnkForgot.ActiveLinkColor = accent;
            _lnkForgot.VisitedLinkColor = accent;

            _lnkRegister.LinkColor = accent;
            _lnkRegister.ActiveLinkColor = accent;
            _lnkRegister.VisitedLinkColor = accent;

            _right.Invalidate();
            _userInput.Invalidate();
            _passwordInput.Invalidate();
            _btnLogin.Invalidate();
            _btnTheme.Invalidate();
        }

        private void BtnTheme_Click(object sender, EventArgs e)
        {
            ThemeManager.Toggle();
            ApplyTheme();
        }

        private void BtnEye_Click(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            _txtPassword.UseSystemPasswordChar = !_passwordVisible;
            _btnEye.Glyph = _passwordVisible ? "\uE8F4" : "\uE890";

            _txtPassword.Focus();
            _txtPassword.SelectionStart = _txtPassword.Text.Length;
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            if (_loginBusy)
                return;

            string login = _txtUser.Text.Trim();
            string password = _txtPassword.Text;

            if (login == "")
            {
                Uyari("Kullanıcı adı veya e-posta adresinizi giriniz.");
                _txtUser.Focus();
                return;
            }

            if (password == "")
            {
                Uyari("Şifrenizi giriniz.");
                _txtPassword.Focus();
                return;
            }

            try
            {
                SetLoginBusy(true);

                UserAccount user = _userService.Login(login, password);

                if (user == null)
                {
                    Uyari("Kullanıcı adı / e-posta veya şifre hatalı.");
                    _txtPassword.SelectAll();
                    _txtPassword.Focus();
                    return;
                }

                AnaEkraniAc(user);
            }
            finally
            {
                SetLoginBusy(false);
            }
        }

        private void SetLoginBusy(bool busy)
        {
            _loginBusy = busy;
            _btnLogin.Enabled = !busy;
            _btnLogin.Text = busy ? "Giriş yapılıyor..." : "Giriş yap  →";
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            Application.DoEvents();
        }

        private void KayitOl_Click(object sender, EventArgs e)
        {
            using (RegisterForm form = new RegisterForm(_userService))
            {
                DialogResult result = form.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    _txtPassword.Clear();
                    _txtUser.Focus();
                }
            }
        }

        private void SifremiUnuttum_Click(object sender, EventArgs e)
        {
            using (ForgotPasswordForm form = new ForgotPasswordForm(_userService))
            {
                DialogResult result = form.ShowDialog(this);

                if (result == DialogResult.OK &&
                    form.VerifiedUser != null)
                {
                    AnaEkraniAc(form.VerifiedUser);
                }
            }
        }

        private void AnaEkraniAc(UserAccount user)
        {
            Hide();

            MainForm mainForm = new MainForm(
                user.Username,
                _userService);

            mainForm.Shown += delegate
            {
                if (!user.MustChangePassword)
                    return;

                using (ChangePasswordForm changeForm =
                    new ChangePasswordForm(user, _userService))
                {
                    DialogResult result =
                        changeForm.ShowDialog(mainForm);

                    if (result != DialogResult.OK ||
                        !changeForm.PasswordChanged)
                    {
                        mainForm.Close();
                    }
                }
            };

            mainForm.FormClosed += delegate
            {
                if (!IsDisposed)
                {
                    _txtPassword.Clear();
                    Show();
                }
            };

            mainForm.Show();
        }

        private void Uyari(string message)
        {
            MessageBox.Show(
                message,
                BrandName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private sealed class LeftBrandPanel : Control
        {
            public string BrandName { get; set; }
            public bool DarkMode { get; set; }

            public LeftBrandPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BrandName = "NEXORA";
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                Rectangle rect = ClientRectangle;

                using (LinearGradientBrush bg = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(7, 31, 59),
                    Color.FromArgb(5, 18, 36),
                    90F))
                {
                    g.FillRectangle(bg, rect);
                }

                // Sağ üst koyu diyagonal geçiş
                Point[] wedge =
                {
                    new Point((int)(Width * 0.71), 0),
                    new Point(Width, 0),
                    new Point(Width, Height),
                    new Point((int)(Width * 0.58), Height)
                };

                using (LinearGradientBrush wedgeBrush = new LinearGradientBrush(
                    new Rectangle((int)(Width * 0.52), 0, Math.Max(1, Width / 2), Height),
                    Color.FromArgb(34, 58, 99),
                    Color.FromArgb(6, 21, 41),
                    0F))
                {
                    g.FillPolygon(wedgeBrush, wedge);
                }

                // Alt-sol parlak mavi kıvrımlar
                using (Pen p1 = new Pen(Color.FromArgb(125, 25, 82, 195), 42F))
                using (Pen p2 = new Pen(Color.FromArgb(105, 30, 103, 240), 12F))
                using (Pen p3 = new Pen(Color.FromArgb(58, 76, 144, 245), 4F))
                {
                    g.DrawArc(p1, -235, Height - 245, 560, 515, 12, 118);
                    g.DrawArc(p2, -205, Height - 215, 505, 470, 10, 118);
                    g.DrawArc(p3, -165, Height - 180, 450, 425, 8, 120);
                }

                float sx = Width / 705f;
                float sy = Height / 768f;

                Func<float, int> X = v => (int)Math.Round(v * sx);
                Func<float, int> Y = v => (int)Math.Round(v * sy);

                DrawLogo(g, new Rectangle(X(68), Y(64), X(64), Y(64)));

                DrawText(g, BrandName,
                    new Font("Segoe UI", 25F, FontStyle.Bold),
                    Color.White, X(148), Y(70));

                DrawText(g, "İşletme Yönetim Platformu",
                    new Font("Segoe UI", 10F),
                    Color.FromArgb(151, 169, 195), X(149), Y(112));

                DrawText(g, "İşlerin dağılmasın.",
                    new Font("Segoe UI", 20F, FontStyle.Bold),
                    Color.White, X(69), Y(180));

                DrawText(g, "İşletmen tek ekranda kalsın.",
                    new Font("Segoe UI", 20F, FontStyle.Bold),
                    Color.FromArgb(92, 160, 255), X(69), Y(218));

                DrawText(g,
                    "Cari hesaplarınızı, ürünlerinizi ve faturalarınızı\ntek merkezden düzenli ve hızlı şekilde yönetin.",
                    new Font("Segoe UI", 10.2F),
                    Color.FromArgb(186, 199, 220), X(69), Y(270));

                DrawFeatureCard(
                    g, X, Y, 331,
                    "\uE9D2",
                    "Tek merkezden yönetim",
                    "Cari, ürün ve fatura işlemleri bir arada.");

                DrawFeatureCard(
                    g, X, Y, 399,
                    "\uE72E",
                    "Güvenli hesap erişimi",
                    "Parolalar hash ve salt yapısıyla korunur.");

                DrawFeatureCard(
                    g, X, Y, 467,
                    "\uE945",
                    "Hızlı işlem akışı",
                    "Sık kullanılan işlemlere daha az adımla ulaşın.");

                using (Pen line = new Pen(Color.FromArgb(33, 63, 96), 1F))
                    g.DrawLine(line, X(70), Y(559), X(426), Y(559));

                DrawText(g,
                    "Güvenli   •   Hızlı   •   Kolay Yönetim",
                    new Font("Segoe UI", 9F),
                    Color.FromArgb(159, 177, 202),
                    X(150),
                    Y(579));
            }

            private static void DrawFeatureCard(
                Graphics g,
                Func<float, int> X,
                Func<float, int> Y,
                int y,
                string glyph,
                string title,
                string body)
            {
                Rectangle card = new Rectangle(
                    X(67), Y(y), X(360), Y(55));

                // Çok hafif mor/mavi dış ışıma
                using (GraphicsPath glowPath = RoundedRect(
                    new Rectangle(card.X - 2, card.Y - 2, card.Width + 4, card.Height + 4), 15))
                using (Pen glowPen = new Pen(
                    Color.FromArgb(105, 151, 89, 255), 2F))
                {
                    g.DrawPath(glowPen, glowPath);
                }

                using (GraphicsPath path = RoundedRect(card, 14))
                using (LinearGradientBrush fill = new LinearGradientBrush(
                    card,
                    Color.FromArgb(31, 63, 101),
                    Color.FromArgb(27, 37, 66),
                    0F))
                using (Pen border = new Pen(
                    Color.FromArgb(96, 109, 138, 177), 1F))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(border, path);
                }

                Rectangle iconBox = new Rectangle(
                    card.X + 8,
                    card.Y + 5,
                    Y(47),
                    Y(45));

                using (GraphicsPath ip = RoundedRect(iconBox, 10))
                using (LinearGradientBrush ib = new LinearGradientBrush(
                    iconBox,
                    Color.FromArgb(54, 95, 162),
                    Color.FromArgb(31, 63, 115),
                    90F))
                {
                    g.FillPath(ib, ip);
                }

                TextRenderer.DrawText(
                    g,
                    glyph,
                    new Font("Segoe MDL2 Assets", 14F),
                    iconBox,
                    Color.White,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);

                DrawText(g, title,
                    new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Color.White,
                    card.X + iconBox.Width + 22,
                    card.Y + 9);

                DrawText(g, body,
                    new Font("Segoe UI", 8.5F),
                    Color.FromArgb(182, 195, 215),
                    card.X + iconBox.Width + 22,
                    card.Y + 31);
            }

            private static void DrawLogo(Graphics g, Rectangle r)
            {
                using (GraphicsPath p = new GraphicsPath())
                {
                    p.AddBezier(
                        r.X + 4, r.Y + 19,
                        r.X + 16, r.Y - 4,
                        r.X + 28, r.Y + 46,
                        r.X + 42, r.Y + 17);

                    p.AddBezier(
                        r.X + 42, r.Y + 17,
                        r.X + 54, r.Y - 3,
                        r.Right + 3, r.Y + 7,
                        r.Right - 6, r.Bottom - 17);

                    p.AddBezier(
                        r.Right - 6, r.Bottom - 17,
                        r.Right - 14, r.Bottom + 2,
                        r.X + 44, r.Bottom,
                        r.X + 31, r.Bottom - 22);

                    p.AddBezier(
                        r.X + 31, r.Bottom - 22,
                        r.X + 20, r.Bottom - 43,
                        r.X + 12, r.Bottom + 1,
                        r.X + 4, r.Bottom - 13);

                    p.CloseFigure();

                    using (LinearGradientBrush b = new LinearGradientBrush(
                        r,
                        Color.FromArgb(75, 184, 255),
                        Color.FromArgb(34, 74, 255),
                        45F))
                    {
                        g.FillPath(b, p);
                    }
                }
            }

            private static void DrawText(
                Graphics g,
                string text,
                Font font,
                Color color,
                int x,
                int y)
            {
                TextRenderer.DrawText(
                    g,
                    text,
                    font,
                    new Point(x, y),
                    color,
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.NoPrefix);
            }
        }

        private sealed class RightLoginPanel : Control
        {
            public Rectangle CardBounds { get; set; }

            public Color PageColor { get; set; }
            public Color CardColor { get; set; }
            public Color CardBorderColor { get; set; }
            public Color PrimaryText { get; set; }
            public Color SecondaryText { get; set; }
            public Color Accent { get; set; }
            public Color Success { get; set; }

            public RightLoginPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.Clear(PageColor);

                Rectangle c = CardBounds;
                if (c.Width <= 0 || c.Height <= 0)
                    return;

                // Sağ panelde üstten alta çok hafif degrade
                using (LinearGradientBrush pageOverlay = new LinearGradientBrush(
                    ClientRectangle,
                    Color.FromArgb(18, 38, 67),
                    PageColor,
                    90F))
                {
                    g.FillRectangle(pageOverlay, ClientRectangle);
                }

                // Kart gölgesi
                Rectangle shadow = new Rectangle(
                    c.X + 6, c.Y + 8, c.Width, c.Height);

                using (GraphicsPath sp = RoundedRect(shadow, 24))
                using (SolidBrush sb = new SolidBrush(
                    Color.FromArgb(40, 0, 0, 0)))
                {
                    g.FillPath(sb, sp);
                }

                using (GraphicsPath p = RoundedRect(c, 24))
                using (LinearGradientBrush b = new LinearGradientBrush(
                    c,
                    CardColor,
                    Color.FromArgb(
                        CardColor.R,
                        Math.Max(0, CardColor.G - 2),
                        Math.Max(0, CardColor.B - 4)),
                    90F))
                using (Pen pen = new Pen(CardBorderColor, 1F))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }

                int x = c.Left + 40;

                DrawText(g,
                    "Tekrar hoş geldiniz",
                    new Font("Segoe UI", 22F, FontStyle.Bold),
                    PrimaryText,
                    x,
                    c.Top + 70);

                DrawText(g,
                    "Hesabınıza güvenli şekilde giriş yapın.",
                    new Font("Segoe UI", 10.6F),
                    SecondaryText,
                    x,
                    c.Top + 111);

                DrawText(g,
                    "E-posta / Kullanıcı adı",
                    new Font("Segoe UI", 9.3F, FontStyle.Bold),
                    PrimaryText,
                    x,
                    c.Top + 153);

                DrawText(g,
                    "Şifre",
                    new Font("Segoe UI", 9.3F, FontStyle.Bold),
                    PrimaryText,
                    x,
                    c.Top + 247);

                DrawText(g,
                    "●  Güvenli oturum",
                    new Font("Segoe UI", 8.8F, FontStyle.Bold),
                    Success,
                    x,
                    c.Top + 445);

                using (Pen sep = new Pen(CardBorderColor, 1F))
                {
                    g.DrawLine(
                        sep,
                        c.Left + 40,
                        c.Top + 514,
                        c.Right - 40,
                        c.Top + 514);
                }

                Size noAccountSize = TextRenderer.MeasureText(
                    "Henüz hesabınız yok mu?",
                    new Font("Segoe UI", 9.2F));

                DrawText(g,
                    "Henüz hesabınız yok mu?",
                    new Font("Segoe UI", 9.2F),
                    SecondaryText,
                    c.Left + (c.Width / 2) - noAccountSize.Width - 14,
                    c.Top + 566);
            }

            private static void DrawText(
                Graphics g,
                string text,
                Font font,
                Color color,
                int x,
                int y)
            {
                TextRenderer.DrawText(
                    g,
                    text,
                    font,
                    new Point(x, y),
                    color,
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.NoPrefix);
            }
        }

        private sealed class PremiumInput : Panel
        {
            private readonly Label _icon;
            private readonly Label _placeholder;

            public TextBox InnerTextBox { get; private set; }

            public string IconGlyph
            {
                get { return _icon.Text; }
                set { _icon.Text = value; }
            }

            public string Placeholder
            {
                get { return _placeholder.Text; }
                set { _placeholder.Text = value; }
            }

            public Color FillColor { get; set; }
            public Color BorderColor { get; set; }
            public Color FocusBorderColor { get; set; }
            public Color TextColor { get; set; }
            public Color PlaceholderColor { get; set; }
            public Color IconColor { get; set; }

            private bool _focused;

            public PremiumInput()
            {
                DoubleBuffered = true;
                BackColor = Color.Transparent;

                FillColor = Color.White;
                BorderColor = Color.LightGray;
                FocusBorderColor = Color.DodgerBlue;
                TextColor = Color.Black;
                PlaceholderColor = Color.Gray;
                IconColor = Color.Gray;

                _icon = new Label();
                _icon.AutoSize = false;
                _icon.Size = new Size(34, 34);
                _icon.Location = new Point(12, 7);
                _icon.TextAlign = ContentAlignment.MiddleCenter;
                _icon.Font = new Font("Segoe MDL2 Assets", 12.5F);
                _icon.BackColor = Color.Transparent;
                _icon.UseCompatibleTextRendering = false;
                Controls.Add(_icon);

                InnerTextBox = new TextBox();
                InnerTextBox.BorderStyle = BorderStyle.None;
                InnerTextBox.Font = new Font("Segoe UI", 9.8F);
                InnerTextBox.Location = new Point(50, 16);
                InnerTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                Controls.Add(InnerTextBox);

                _placeholder = new Label();
                _placeholder.AutoSize = true;
                _placeholder.Font = new Font("Segoe UI", 9.8F);
                _placeholder.Location = new Point(50, 15);
                _placeholder.BackColor = Color.Transparent;
                _placeholder.Cursor = Cursors.IBeam;
                _placeholder.UseCompatibleTextRendering = false;
                _placeholder.Click += delegate { InnerTextBox.Focus(); };
                Controls.Add(_placeholder);
                _placeholder.BringToFront();

                InnerTextBox.TextChanged += delegate
                {
                    _placeholder.Visible = string.IsNullOrEmpty(InnerTextBox.Text);
                };

                InnerTextBox.Enter += delegate
                {
                    _focused = true;
                    Invalidate();
                };

                InnerTextBox.Leave += delegate
                {
                    _focused = false;
                    Invalidate();
                };

                Resize += delegate
                {
                    InnerTextBox.Width = Math.Max(100, Width - 120);
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Rectangle r = new Rectangle(
                    1, 1, Width - 3, Height - 3);

                using (GraphicsPath p = RoundedRect(r, 10))
                using (SolidBrush b = new SolidBrush(FillColor))
                using (Pen pen = new Pen(
                    _focused ? FocusBorderColor : BorderColor,
                    _focused ? 2F : 1F))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }

                _icon.ForeColor = IconColor;
                _icon.BackColor = FillColor;

                InnerTextBox.ForeColor = TextColor;
                InnerTextBox.BackColor = FillColor;

                _placeholder.ForeColor = PlaceholderColor;
                _placeholder.BackColor = FillColor;

                base.OnPaint(e);
            }
        }

        private sealed class PremiumButton : Button
        {
            public int CornerRadius { get; set; }
            public Color FillColor { get; set; }
            public Color HoverColor { get; set; }
            public Color TextColor { get; set; }
            public Color BorderColor { get; set; }

            private bool _hover;

            public PremiumButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                CornerRadius = 10;
                FillColor = Color.DodgerBlue;
                HoverColor = Color.RoyalBlue;
                TextColor = Color.White;
                BorderColor = Color.Transparent;
                BackColor = Color.Transparent;
                UseCompatibleTextRendering = false;

                MouseEnter += delegate
                {
                    _hover = true;
                    Invalidate();
                };

                MouseLeave += delegate
                {
                    _hover = false;
                    Invalidate();
                };
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                Graphics g = pevent.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(
                    0, 0, Width - 1, Height - 1);

                using (GraphicsPath p = RoundedRect(r, CornerRadius))
                using (LinearGradientBrush b = new LinearGradientBrush(
                    r,
                    _hover ? HoverColor : Color.FromArgb(
                        Math.Min(255, FillColor.R + 18),
                        Math.Min(255, FillColor.G + 18),
                        Math.Min(255, FillColor.B + 4)),
                    _hover ? HoverColor : FillColor,
                    0F))
                {
                    g.FillPath(b, p);

                    if (BorderColor.A > 0)
                    {
                        using (Pen pen = new Pen(BorderColor, 1F))
                            g.DrawPath(pen, p);
                    }
                }

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    r,
                    Enabled ? TextColor : Color.FromArgb(175, TextColor),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }
        }

        private sealed class EyeButton : Button
        {
            public string Glyph { get; set; }

            public EyeButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Font = new Font("Segoe MDL2 Assets", 12F);
                Glyph = "\uE890";
                BackColor = Color.Transparent;
                UseCompatibleTextRendering = false;
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                Graphics g = pevent.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                TextRenderer.DrawText(
                    g,
                    Glyph,
                    Font,
                    ClientRectangle,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            if (d <= 0)
            {
                path.AddRectangle(bounds);
                path.CloseFigure();
                return path;
            }

            Rectangle arc = new Rectangle(bounds.Left, bounds.Top, d, d);

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
