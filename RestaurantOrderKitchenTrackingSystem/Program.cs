using System;
using System.Windows.Forms;

namespace RestaurantOrderKitchenTrackingSystem
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    using (var mainForm = new MainForm(loginForm.LoggedInWaiter))
                    {
                        Application.Run(mainForm);
                        if (!mainForm.IsLoggingOut)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}
