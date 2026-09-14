using System;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business.Abstract;
using UrunFaturaYonetimi.Business.Managers;

namespace UrunFaturaYonetimi
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IUserService userService =
                new UserManager();

            userService.Initialize();

            Application.Run(
                new LoginForm(userService));
        }
    }
}