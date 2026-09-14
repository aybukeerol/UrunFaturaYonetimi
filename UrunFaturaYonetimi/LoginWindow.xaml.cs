using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Managers;

namespace UrunFaturaYonetimi
{
    public partial class LoginWindow : Window
    {
        private readonly IUserService _userService;

        private bool _passwordVisible;
        private bool _loginBusy;
        private bool _syncingPassword;
        private bool _isDarkMode = true;

        public LoginWindow()
            : this(new UserManager())
        {
        }

        public LoginWindow(IUserService userService)
        {
            InitializeComponent();

            _userService = userService;

            UserPlaceholder.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Visible;

            ApplyTheme();
        }

        // =========================================================
        // TEMA
        // =========================================================

        private void ThemeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;

            ApplyTheme();
        }

        private SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }

        private void ApplyTheme()
        {
            if (_isDarkMode)
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }
        }

        private void ApplyDarkTheme()
        {
            SolidColorBrush root =
                Brush("#0B1B31");

            SolidColorBrush left =
                Brush("#071B32");

            SolidColorBrush right =
                Brush("#132844");

            SolidColorBrush card =
                Brush("#1A3153");

            SolidColorBrush cardBorder =
                Brush("#314B6C");

            SolidColorBrush input =
                Brush("#172B49");

            SolidColorBrush inputBorder =
                Brush("#4A6588");

            SolidColorBrush primary =
                Brush("#F7F9FC");

            SolidColorBrush secondary =
                Brush("#A8B8CF");

            SolidColorBrush accent =
                Brush("#3798FF");

            SolidColorBrush feature =
                Brush("#1D3659");

            SolidColorBrush featureBorder =
                Brush("#35618D");

            SolidColorBrush iconBackground =
                Brush("#132F50");

            SolidColorBrush themeButton =
                Brush("#30415E");

            SolidColorBrush separator =
                Brush("#334E70");

            SolidColorBrush success =
                Brush("#19C873");

            RootGrid.Background = root;
            LeftPanel.Background = left;
            RightPanel.Background = right;

            LoginCard.Background = card;
            LoginCard.BorderBrush = cardBorder;

            LogoBox.Background = accent;

            BrandText.Foreground = primary;
            BrandSubText.Foreground = secondary;

            Hero1.Foreground = primary;
            Hero2.Foreground = accent;

            DescriptionText.Foreground = secondary;

            SetFeatureTheme(
                Feature1,
                FeatureIcon1,
                FeatureIconText1,
                FeatureTitle1,
                FeatureDesc1,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            SetFeatureTheme(
                Feature2,
                FeatureIcon2,
                FeatureIconText2,
                FeatureTitle2,
                FeatureDesc2,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            SetFeatureTheme(
                Feature3,
                FeatureIcon3,
                FeatureIconText3,
                FeatureTitle3,
                FeatureDesc3,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            DecorEllipse1.Stroke = accent;
            DecorEllipse2.Stroke = accent;

            FooterSeparator.Background = separator;
            FooterText.Foreground = secondary;

            ThemeButton.Background = themeButton;
            ThemeButton.Foreground = primary;
            ThemeButton.Content = "☀  Açık mod";

            WelcomeTitle.Foreground = primary;
            WelcomeSubTitle.Foreground = secondary;

            UserLabel.Foreground = primary;
            PasswordLabel.Foreground = primary;

            UserBorder.Background = input;
            UserBorder.BorderBrush = inputBorder;

            PasswordBorder.Background = input;
            PasswordBorder.BorderBrush = inputBorder;

            UserTextBox.Foreground = primary;
            UserTextBox.CaretBrush = primary;

            PasswordBox.Foreground = primary;
            PasswordBox.CaretBrush = primary;

            VisiblePasswordBox.Foreground = primary;
            VisiblePasswordBox.CaretBrush = primary;

            UserPlaceholder.Foreground = secondary;
            PasswordPlaceholder.Foreground = secondary;

            UserIcon.Foreground = secondary;
            PasswordIcon.Foreground = secondary;
            EyeButton.Foreground = secondary;

            ForgotButton.Foreground = accent;
            RegisterButton.Foreground = accent;

            SecureText.Foreground = success;

            RegisterSeparator.Background = separator;
            RegisterQuestion.Foreground = secondary;
        }

        private void ApplyLightTheme()
        {
            SolidColorBrush root =
                Brush("#F4F7FB");

            SolidColorBrush left =
                Brush("#F7FAFE");

            SolidColorBrush right =
                Brush("#EEF3F9");

            SolidColorBrush card =
                Brush("#FFFFFF");

            SolidColorBrush cardBorder =
                Brush("#D7E1EC");

            SolidColorBrush input =
                Brush("#F8FAFD");

            SolidColorBrush inputBorder =
                Brush("#BCCBDD");

            SolidColorBrush primary =
                Brush("#142033");

            SolidColorBrush secondary =
                Brush("#617085");

            SolidColorBrush accent =
                Brush("#287EF0");

            SolidColorBrush feature =
                Brush("#FFFFFF");

            SolidColorBrush featureBorder =
                Brush("#D8E3EF");

            SolidColorBrush iconBackground =
                Brush("#E8F1FC");

            SolidColorBrush themeButton =
                Brush("#FFFFFF");

            SolidColorBrush separator =
                Brush("#DCE5EF");

            SolidColorBrush success =
                Brush("#16A765");

            RootGrid.Background = root;
            LeftPanel.Background = left;
            RightPanel.Background = right;

            LoginCard.Background = card;
            LoginCard.BorderBrush = cardBorder;

            LogoBox.Background = accent;

            BrandText.Foreground = primary;
            BrandSubText.Foreground = secondary;

            Hero1.Foreground = primary;
            Hero2.Foreground = accent;

            DescriptionText.Foreground = secondary;

            SetFeatureTheme(
                Feature1,
                FeatureIcon1,
                FeatureIconText1,
                FeatureTitle1,
                FeatureDesc1,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            SetFeatureTheme(
                Feature2,
                FeatureIcon2,
                FeatureIconText2,
                FeatureTitle2,
                FeatureDesc2,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            SetFeatureTheme(
                Feature3,
                FeatureIcon3,
                FeatureIconText3,
                FeatureTitle3,
                FeatureDesc3,
                feature,
                featureBorder,
                iconBackground,
                accent,
                primary,
                secondary);

            DecorEllipse1.Stroke = accent;
            DecorEllipse2.Stroke = accent;

            FooterSeparator.Background = separator;
            FooterText.Foreground = secondary;

            ThemeButton.Background = themeButton;
            ThemeButton.Foreground = primary;
            ThemeButton.Content = "☾  Koyu mod";

            WelcomeTitle.Foreground = primary;
            WelcomeSubTitle.Foreground = secondary;

            UserLabel.Foreground = primary;
            PasswordLabel.Foreground = primary;

            UserBorder.Background = input;
            UserBorder.BorderBrush = inputBorder;

            PasswordBorder.Background = input;
            PasswordBorder.BorderBrush = inputBorder;

            UserTextBox.Foreground = primary;
            UserTextBox.CaretBrush = primary;

            PasswordBox.Foreground = primary;
            PasswordBox.CaretBrush = primary;

            VisiblePasswordBox.Foreground = primary;
            VisiblePasswordBox.CaretBrush = primary;

            UserPlaceholder.Foreground = secondary;
            PasswordPlaceholder.Foreground = secondary;

            UserIcon.Foreground = secondary;
            PasswordIcon.Foreground = secondary;
            EyeButton.Foreground = secondary;

            ForgotButton.Foreground = accent;
            RegisterButton.Foreground = accent;

            SecureText.Foreground = success;

            RegisterSeparator.Background = separator;
            RegisterQuestion.Foreground = secondary;
        }

        private void SetFeatureTheme(
            Border featureBorderControl,
            Border iconBorder,
            TextBlock iconText,
            TextBlock title,
            TextBlock description,
            Brush featureBackground,
            Brush featureBorder,
            Brush iconBackground,
            Brush accent,
            Brush primary,
            Brush secondary)
        {
            featureBorderControl.Background =
                featureBackground;

            featureBorderControl.BorderBrush =
                featureBorder;

            iconBorder.Background =
                iconBackground;

            iconText.Foreground =
                accent;

            title.Foreground =
                primary;

            description.Foreground =
                secondary;
        }

        // =========================================================
        // GİRİŞ
        // =========================================================

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_loginBusy)
            {
                return;
            }

            string login =
                UserTextBox.Text.Trim();

            string password =
                GetPassword();

            if (string.IsNullOrWhiteSpace(login))
            {
                ShowWarning(
                    "Kullanıcı adı veya e-posta adresinizi giriniz.");

                UserTextBox.Focus();

                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowWarning(
                    "Şifrenizi giriniz.");

                FocusPassword();

                return;
            }

            try
            {
                SetLoginBusy(true);

                UserAccount user =
                    _userService.Login(
                        login,
                        password);

                if (user == null)
                {
                    ShowWarning(
                        "Kullanıcı adı / e-posta veya şifre hatalı.");

                    ClearPassword();

                    FocusPassword();

                    return;
                }

                OpenMainForm(user);
            }
            finally
            {
                SetLoginBusy(false);
            }
        }

        // =========================================================
        // KAYIT
        // =========================================================

        private void RegisterButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            using (
                RegisterForm form =
                    new RegisterForm(_userService))
            {
                System.Windows.Forms.DialogResult result =
                    form.ShowDialog();

                if (
                    result ==
                    System.Windows.Forms.DialogResult.OK)
                {
                    ClearPassword();

                    UserTextBox.Focus();
                }
            }
        }

        // =========================================================
        // ŞİFREMİ UNUTTUM
        // =========================================================

        private void ForgotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            using (
                ForgotPasswordForm form =
                    new ForgotPasswordForm(_userService))
            {
                System.Windows.Forms.DialogResult result =
                    form.ShowDialog();

                if (
                    result ==
                    System.Windows.Forms.DialogResult.OK
                    &&
                    form.VerifiedUser != null)
                {
                    OpenMainForm(
                        form.VerifiedUser);
                }
            }
        }

        // =========================================================
        // ANA FORM
        // =========================================================

        private void OpenMainForm(
            UserAccount user)
        {
            Hide();

            MainForm mainForm =
                new MainForm(
                    user.Username,
                    _userService);

            mainForm.Shown += delegate
            {
                if (!user.MustChangePassword)
                {
                    return;
                }

                using (
                    ChangePasswordForm changeForm =
                        new ChangePasswordForm(
                            user,
                            _userService))
                {
                    System.Windows.Forms.DialogResult result =
                        changeForm.ShowDialog(
                            mainForm);

                    if (
                        result !=
                        System.Windows.Forms.DialogResult.OK
                        ||
                        !changeForm.PasswordChanged)
                    {
                        mainForm.Close();
                    }
                }
            };

            mainForm.FormClosed += delegate
            {
                Dispatcher.Invoke(
                    delegate
                    {
                        ClearPassword();

                        Show();

                        Activate();

                        UserTextBox.Focus();
                    });
            };

            mainForm.Show();
        }

        // =========================================================
        // LOGIN BUTON DURUMU
        // =========================================================

        private void SetLoginBusy(
            bool busy)
        {
            _loginBusy = busy;

            LoginButton.IsEnabled =
                !busy;

            if (busy)
            {
                LoginButton.Content =
                    "Giriş yapılıyor...";
            }
            else
            {
                LoginButton.Content =
                    "Giriş yap  →";
            }
        }

        // =========================================================
        // ŞİFRE GÖSTER / GİZLE
        // =========================================================

        private void EyeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _passwordVisible =
                !_passwordVisible;

            _syncingPassword = true;

            if (_passwordVisible)
            {
                VisiblePasswordBox.Text =
                    PasswordBox.Password;

                PasswordBox.Visibility =
                    Visibility.Collapsed;

                VisiblePasswordBox.Visibility =
                    Visibility.Visible;

                EyeButton.Content =
                    "◎";

                VisiblePasswordBox.Focus();

                VisiblePasswordBox.CaretIndex =
                    VisiblePasswordBox.Text.Length;
            }
            else
            {
                PasswordBox.Password =
                    VisiblePasswordBox.Text;

                VisiblePasswordBox.Visibility =
                    Visibility.Collapsed;

                PasswordBox.Visibility =
                    Visibility.Visible;

                EyeButton.Content =
                    "◉";

                PasswordBox.Focus();
            }

            _syncingPassword = false;

            UpdatePasswordPlaceholder();
        }

        private string GetPassword()
        {
            if (_passwordVisible)
            {
                return VisiblePasswordBox.Text;
            }

            return PasswordBox.Password;
        }

        private void ClearPassword()
        {
            _syncingPassword = true;

            PasswordBox.Password =
                string.Empty;

            VisiblePasswordBox.Text =
                string.Empty;

            _syncingPassword = false;

            UpdatePasswordPlaceholder();
        }

        private void FocusPassword()
        {
            if (_passwordVisible)
            {
                VisiblePasswordBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
        }

        // =========================================================
        // PLACEHOLDER
        // =========================================================

        private void UserTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (
                string.IsNullOrEmpty(
                    UserTextBox.Text))
            {
                UserPlaceholder.Visibility =
                    Visibility.Visible;
            }
            else
            {
                UserPlaceholder.Visibility =
                    Visibility.Collapsed;
            }
        }

        private void PasswordBox_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (!_syncingPassword)
            {
                _syncingPassword = true;

                VisiblePasswordBox.Text =
                    PasswordBox.Password;

                _syncingPassword = false;
            }

            UpdatePasswordPlaceholder();
        }

        private void VisiblePasswordBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!_syncingPassword)
            {
                _syncingPassword = true;

                PasswordBox.Password =
                    VisiblePasswordBox.Text;

                _syncingPassword = false;
            }

            UpdatePasswordPlaceholder();
        }

        private void UpdatePasswordPlaceholder()
        {
            if (
                string.IsNullOrEmpty(
                    GetPassword()))
            {
                PasswordPlaceholder.Visibility =
                    Visibility.Visible;
            }
            else
            {
                PasswordPlaceholder.Visibility =
                    Visibility.Collapsed;
            }
        }

        // =========================================================
        // INPUT FOCUS
        // =========================================================

        private void Input_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            UserBorder.BorderBrush =
                Brush("#3A97FF");

            UserBorder.BorderThickness =
                new Thickness(1.5);
        }

        private void Input_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (_isDarkMode)
            {
                UserBorder.BorderBrush =
                    Brush("#4A6588");
            }
            else
            {
                UserBorder.BorderBrush =
                    Brush("#BCCBDD");
            }

            UserBorder.BorderThickness =
                new Thickness(1);
        }

        private void PasswordBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            PasswordBorder.BorderBrush =
                Brush("#3A97FF");

            PasswordBorder.BorderThickness =
                new Thickness(1.5);
        }

        private void PasswordBox_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (_isDarkMode)
            {
                PasswordBorder.BorderBrush =
                    Brush("#4A6588");
            }
            else
            {
                PasswordBorder.BorderBrush =
                    Brush("#BCCBDD");
            }

            PasswordBorder.BorderThickness =
                new Thickness(1);
        }

        // =========================================================
        // UYARI
        // =========================================================

        private void ShowWarning(
            string message)
        {
            System.Windows.MessageBox.Show(
                this,
                message,
                "NEXORA",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
