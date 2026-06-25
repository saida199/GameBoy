using System;
using System.Windows.Forms;
using GameBoyRPG.WinForms.Forms;
using GameBoyRPG.WinForms.Game;

namespace GameBoyRPG.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var api   = new ApiClient();
            var login = new LoginForm();

            if (login.ShowDialog() == DialogResult.OK)
            {
                var game = new GameForm(
                    login.LoggedPlayerId,
                    login.LoggedHeroName ?? "Héros",
                    api);
                Application.Run(game);
            }
        }
    }
}
