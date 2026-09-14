using System;
using System.Drawing;
using System.IO;

namespace UrunFaturaYonetimi
{
    public static class ThemeManager
    {
        private static readonly string ThemeFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "UrunFaturaYonetimi");

        private static readonly string ThemeFile =
            Path.Combine(
                ThemeFolder,
                "theme.txt");

        public static AppTheme CurrentTheme { get; private set; } =
            AppTheme.Light;

        public static bool IsDark
        {
            get
            {
                return CurrentTheme == AppTheme.Dark;
            }
        }

        public static Color PageBackground
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(15, 23, 42)
                    : Color.FromArgb(246, 248, 252);
            }
        }

        public static Color Surface
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(30, 41, 59)
                    : Color.White;
            }
        }

        public static Color TextPrimary
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(241, 245, 249)
                    : Color.FromArgb(30, 41, 59);
            }
        }

        public static Color TextSecondary
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(148, 163, 184)
                    : Color.FromArgb(100, 116, 139);
            }
        }

        public static Color Border
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(71, 85, 105)
                    : Color.FromArgb(226, 232, 240);
            }
        }

        public static Color Accent
        {
            get
            {
                return Color.FromArgb(37, 99, 235);
            }
        }

        public static Color AccentHover
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(59, 130, 246)
                    : Color.FromArgb(29, 78, 216);
            }
        }

        public static Color Success
        {
            get
            {
                return Color.FromArgb(16, 185, 129);
            }
        }

        public static Color LeftPanel
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(9, 17, 31)
                    : Color.FromArgb(15, 35, 63);
            }
        }

        public static Color LeftPanelSecondary
        {
            get
            {
                return IsDark
                    ? Color.FromArgb(148, 163, 184)
                    : Color.FromArgb(191, 204, 224);
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ThemeFile))
                    return;

                string value =
                    File.ReadAllText(ThemeFile).Trim();

                AppTheme parsed;

                if (Enum.TryParse(
                    value,
                    true,
                    out parsed))
                {
                    CurrentTheme = parsed;
                }
            }
            catch
            {
                CurrentTheme = AppTheme.Light;
            }
        }

        public static void Toggle()
        {
            SetTheme(
                IsDark
                    ? AppTheme.Light
                    : AppTheme.Dark);
        }

        public static void SetTheme(
            AppTheme theme)
        {
            CurrentTheme = theme;

            try
            {
                if (!Directory.Exists(ThemeFolder))
                {
                    Directory.CreateDirectory(
                        ThemeFolder);
                }

                File.WriteAllText(
                    ThemeFile,
                    CurrentTheme.ToString());
            }
            catch
            {
            }
        }
    }
}