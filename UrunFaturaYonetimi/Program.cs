using System;
using System.Windows;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Managers;

namespace UrunFaturaYonetimi
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            IUserService userService = new UserManager();
            userService.Initialize();

            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            LoginWindow loginWindow =
                new LoginWindow(userService);

            app.MainWindow = loginWindow;
            app.Run(loginWindow);
        }
    }
}