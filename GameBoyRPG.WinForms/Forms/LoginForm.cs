using System;
using System.Drawing;
using System.Windows.Forms;
using GameBoyRPG.WinForms.Game;

namespace GameBoyRPG.WinForms.Forms
{
    public class LoginForm : Form
    {
        // GameBoy palette
        private static readonly Color BG     = Color.FromArgb(15, 56, 15);
        private static readonly Color DARK   = Color.FromArgb(48, 98, 48);
        private static readonly Color MED    = Color.FromArgb(139, 172, 15);
        private static readonly Color LIGHT  = Color.FromArgb(155, 188, 15);

        private TextBox _user, _pass, _hero;
        private Button  _btnLogin, _btnRegister;
        private Label   _lblStatus;
        private TabControl _tabs;

        public string? LoggedUsername { get; private set; }
        public int     LoggedPlayerId { get; private set; }
        public string? LoggedHeroName { get; private set; }

        private readonly ApiClient _api = new ApiClient();

        public LoginForm()
        {
            Text           = "GAMEBOY RPG — Connexion";
            Size           = new Size(380, 440);
            BackColor      = BG;
            ForeColor      = LIGHT;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox    = false;
            StartPosition  = FormStartPosition.CenterScreen;
            Font           = new Font("Courier New", 10, FontStyle.Regular);

            BuildUI();
        }

        void BuildUI()
        {
            // Title
            var title = new Label
            {
                Text      = "▶ GAMEBOY RPG ◀",
                Font      = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = LIGHT,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 16, 380, 36)
            };

            _tabs = new TabControl
            {
                Bounds    = new Rectangle(20, 60, 340, 260),
                Font      = new Font("Courier New", 9),
                BackColor = DARK,
                ForeColor = LIGHT,
                Appearance = TabAppearance.Normal
            };

            // Login tab
            var tabLogin = new TabPage("CONNEXION") { BackColor = DARK, ForeColor = LIGHT };
            _user = MakeInput("Utilisateur", 20, 50, tabLogin);
            _pass = MakeInput("Mot de passe", 20, 110, tabLogin, isPassword: true);
            _btnLogin = MakeButton("SE CONNECTER", 20, 170, tabLogin, MED);
            _btnLogin.Click += async (s, e) => await DoLogin();

            // Register tab
            var tabReg = new TabPage("INSCRIPTION") { BackColor = DARK, ForeColor = LIGHT };
            MakeLabel("Utilisateur",  20, 10, tabReg);
            _user = MakeInput("", 20, 30, tabReg);
            MakeLabel("Mot de passe", 20, 75, tabReg);
            _pass = MakeInput("", 20, 95, tabReg, isPassword: true);
            MakeLabel("Nom du héros", 20, 140, tabReg);
            _hero = MakeInput("", 20, 160, tabReg);
            _btnRegister = MakeButton("CRÉER PERSONNAGE", 20, 205, tabReg, DARK);
            _btnRegister.Click += async (s, e) => await DoRegister();

            // Re-create login fields pointing to tab 0
            tabLogin.Controls.Clear();
            var lUser = MakeLabel("Utilisateur",  20, 10, tabLogin);
            _user = MakeInput("", 20, 32, tabLogin);
            var lPass = MakeLabel("Mot de passe", 20, 78, tabLogin);
            _pass = MakeInput("", 20, 100, tabLogin, isPassword: true);
            _btnLogin = MakeButton("SE CONNECTER", 20, 155, tabLogin, MED);
            _btnLogin.Click += async (s2, e2) => await DoLogin();

            _tabs.TabPages.Add(tabLogin);
            _tabs.TabPages.Add(tabReg);

            _lblStatus = new Label
            {
                Bounds    = new Rectangle(20, 330, 340, 50),
                ForeColor = Color.Red,
                Font      = new Font("Courier New", 9),
                TextAlign = ContentAlignment.TopCenter
            };

            Controls.AddRange(new Control[] { title, _tabs, _lblStatus });
        }

        Label MakeLabel(string text, int x, int y, Control parent)
        {
            var l = new Label { Text = text, Bounds = new Rectangle(x, y, 280, 20), ForeColor = MED };
            parent.Controls.Add(l);
            return l;
        }

        TextBox MakeInput(string placeholder, int x, int y, Control parent, bool isPassword = false)
        {
            var tb = new TextBox
            {
                Bounds         = new Rectangle(x, y, 295, 26),
                BackColor      = Color.FromArgb(10, 40, 10),
                ForeColor      = LIGHT,
                BorderStyle    = BorderStyle.FixedSingle,
                UseSystemPasswordChar = isPassword
            };
            if (!string.IsNullOrEmpty(placeholder)) tb.Text = placeholder;
            parent.Controls.Add(tb);
            return tb;
        }

        Button MakeButton(string text, int x, int y, Control parent, Color bg)
        {
            var btn = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, 295, 34),
                BackColor = bg,
                ForeColor = LIGHT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Courier New", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = LIGHT;
            parent.Controls.Add(btn);
            return btn;
        }

        async System.Threading.Tasks.Task DoLogin()
        {
            _lblStatus.Text = "Connexion...";
            var res = await _api.LoginAsync(_user.Text.Trim(), _pass.Text);
            if (res != null)
            {
                LoggedPlayerId = (int)res["id"]!;
                LoggedUsername = (string)res["username"]!;
                LoggedHeroName = (string)res["heroName"]!;
                _lblStatus.ForeColor = MED;
                _lblStatus.Text      = $"Bienvenue, {LoggedHeroName} !";
                System.Threading.Thread.Sleep(600);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblStatus.ForeColor = Color.Red;
                _lblStatus.Text = "Identifiants incorrects.";
            }
        }

        async System.Threading.Tasks.Task DoRegister()
        {
            _lblStatus.Text = "Création du compte...";
            var res = await _api.RegisterAsync(_user.Text.Trim(), _pass.Text, _hero.Text.Trim());
            if (res != null)
            {
                _lblStatus.ForeColor = MED;
                _lblStatus.Text = "Compte créé ! Va te connecter.";
                _tabs.SelectedIndex = 0;
            }
            else
            {
                _lblStatus.ForeColor = Color.Red;
                _lblStatus.Text = "Erreur — nom déjà pris ?";
            }
        }
    }
}
