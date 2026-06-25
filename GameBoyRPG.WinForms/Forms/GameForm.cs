using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GameBoyRPG.WinForms.Game;

namespace GameBoyRPG.WinForms.Forms
{
    public class GameForm : Form
    {
        // ── Palette (authentic Game Boy green) ─────────────────────────────
        private static readonly Color C0 = Color.FromArgb(15,  56,  15);   // darkest  (bg)
        private static readonly Color C1 = Color.FromArgb(48,  98,  48);   // dark
        private static readonly Color C2 = Color.FromArgb(139, 172, 15);   // medium
        private static readonly Color C3 = Color.FromArgb(155, 188, 15);   // lightest
        private static readonly Font  GBFont  = new Font("Courier New", 9, FontStyle.Regular);
        private static readonly Font  GBBold  = new Font("Courier New", 9, FontStyle.Bold);
        private static readonly Font  GBTitle = new Font("Courier New", 11, FontStyle.Bold);

        // ── Layout ─────────────────────────────────────────────────────────
        private const int TILE       = 24;
        private const int MAP_COLS   = 16;
        private const int MAP_ROWS   = 12;
        private const int MAP_W      = TILE * MAP_COLS;   // 384
        private const int MAP_H      = TILE * MAP_ROWS;   // 288
        private const int SIDE_W     = 200;
        private const int LOG_H      = 120;
        private const int FORM_W     = MAP_W + SIDE_W + 40;
        private const int FORM_H     = MAP_H + LOG_H + 80;

        // ── State ──────────────────────────────────────────────────────────
        private Game.GameState _state = Game.GameState.Explore;
        private PlayerState    _player;
        private GameMap        _map;
        private ApiClient      _api;
        private Monster?       _currentEnemy;
        private BattleEngine   _battle = new BattleEngine();
        private List<string>   _log    = new List<string>();
        private Random         _rng    = new Random();
        private int            _apiPlayerId;
        private Timer          _encounterTimer;
        private int            _stepCount;

        // ── Controls ───────────────────────────────────────────────────────
        private Panel      _mapPanel;
        private Panel      _sidePanel;
        private Panel      _logPanel;
        private Label      _lblHero, _lblStats, _lblLog;
        private Button     _btnSave, _btnFight, _btnFlee, _btnShop;
        private ProgressBar _hpBar, _expBar;

        public GameForm(int apiPlayerId, string heroName, ApiClient api)
        {
            _apiPlayerId = apiPlayerId;
            _api         = api;
            _player      = new PlayerState { Id = apiPlayerId, Name = heroName };
            _map         = new GameMap();

            Text            = $"GAMEBOY RPG — {heroName}";
            ClientSize      = new Size(FORM_W, FORM_H);
            BackColor       = C0;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            DoubleBuffered  = true;
            KeyPreview      = true;

            BuildUI();
            SetupEncounterTimer();
            KeyDown += OnKeyDown;
            Refresh();
        }

        // ── UI Builder ─────────────────────────────────────────────────────
        void BuildUI()
        {
            // MAP PANEL (double-buffered custom draw)
            _mapPanel = new Panel
            {
                Location    = new Point(10, 50),
                Size        = new Size(MAP_W, MAP_H),
                BackColor   = C0,
            };
            _mapPanel.Paint += DrawMap;

            // SIDE PANEL
            _sidePanel = new Panel
            {
                Location  = new Point(MAP_W + 20, 50),
                Size      = new Size(SIDE_W, MAP_H),
                BackColor = C1,
            };
            BuildSidePanel();

            // LOG PANEL
            _logPanel = new Panel
            {
                Location  = new Point(10, MAP_H + 58),
                Size      = new Size(MAP_W + SIDE_W + 10, LOG_H),
                BackColor = Color.FromArgb(10, 40, 10),
            };
            _lblLog = new Label
            {
                Bounds    = new Rectangle(6, 4, MAP_W + SIDE_W - 4, LOG_H - 8),
                ForeColor = C2,
                Font      = GBFont,
                AutoSize  = false,
            };
            _logPanel.Controls.Add(_lblLog);

            // Top bar
            var topBar = new Label
            {
                Text      = "▶  GAMEBOY RPG  ◀   [Z/Q/S/D] Déplacer   [F] Combattre   [Echap] Menu",
                Bounds    = new Rectangle(10, 12, FORM_W - 20, 26),
                ForeColor = C3,
                Font      = new Font("Courier New", 9, FontStyle.Bold),
            };

            Controls.AddRange(new Control[] { topBar, _mapPanel, _sidePanel, _logPanel });
            PushLog("Bienvenue dans GAMEBOY RPG !");
            PushLog("Utilise Z Q S D pour te déplacer, F pour combattre.");
        }

        void BuildSidePanel()
        {
            int y = 10;

            _lblHero = new Label { Bounds = new Rectangle(8, y, SIDE_W - 16, 20),
                ForeColor = C3, Font = GBBold };
            y += 24;

            // HP
            var lHp = new Label { Text = "HP", Bounds = new Rectangle(8, y, 30, 16), ForeColor = C2, Font = GBFont };
            _hpBar = new ProgressBar { Bounds = new Rectangle(42, y, SIDE_W - 50, 14),
                BackColor = C1, ForeColor = Color.Red, Style = ProgressBarStyle.Continuous, Maximum = 100 };
            y += 20;

            // EXP
            var lExp = new Label { Text = "EXP", Bounds = new Rectangle(8, y, 34, 16), ForeColor = C2, Font = GBFont };
            _expBar = new ProgressBar { Bounds = new Rectangle(46, y, SIDE_W - 54, 14),
                BackColor = C1, ForeColor = C2, Style = ProgressBarStyle.Continuous, Maximum = 100 };
            y += 24;

            _lblStats = new Label { Bounds = new Rectangle(8, y, SIDE_W - 16, 120),
                ForeColor = C2, Font = GBFont, AutoSize = false };
            y += 126;

            var div = new Label { Text = new string('─', 22),
                Bounds = new Rectangle(8, y, SIDE_W - 16, 16), ForeColor = C1, Font = GBFont };
            y += 20;

            _btnFight = MakeSideBtn("⚔  COMBATTRE [F]", y); y += 38;
            _btnFlee  = MakeSideBtn("🏃 FUIR [X]", y);       y += 38;
            _btnShop  = MakeSideBtn("🏪 BOUTIQUE [B]", y);   y += 38;
            _btnSave  = MakeSideBtn("💾 SAUVEGARDER [P]", y);

            _btnFight.Click += (s,e) => StartBattle();
            _btnFlee.Click  += (s,e) => Flee();
            _btnShop.Click  += (s,e) => OpenShop();
            _btnSave.Click  += async (s,e) => { await _api.SaveAsync(_apiPlayerId, 1); PushLog("💾 Sauvegardé !"); };

            _sidePanel.Controls.AddRange(new Control[]
            { _lblHero, lHp, _hpBar, lExp, _expBar, _lblStats, div,
              _btnFight, _btnFlee, _btnShop, _btnSave });

            UpdateSidePanel();
        }

        Button MakeSideBtn(string text, int y)
        {
            var b = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(8, y, SIDE_W - 16, 30),
                BackColor = C0,
                ForeColor = C2,
                FlatStyle = FlatStyle.Flat,
                Font      = GBFont,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            b.FlatAppearance.BorderColor = C1;
            return b;
        }

        // ── Map Drawing ────────────────────────────────────────────────────
        void DrawMap(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(C0);

            int camX = Math.Max(0, Math.Min(_player.MapX - MAP_COLS/2, GameMap.WIDTH  - MAP_COLS));
            int camY = Math.Max(0, Math.Min(_player.MapY - MAP_ROWS/2, GameMap.HEIGHT - MAP_ROWS));

            for (int ty = 0; ty < MAP_ROWS; ty++)
                for (int tx = 0; tx < MAP_COLS; tx++)
                {
                    int wx = camX + tx, wy = camY + ty;
                    var tile = _map.GetTile(wx, wy);
                    int px = tx * TILE, py = ty * TILE;

                    using (var br = new SolidBrush(C0))
                        g.FillRectangle(br, px, py, TILE, TILE);

                    using (var br = new SolidBrush(tile.Color))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center,
                                                       LineAlignment = StringAlignment.Center })
                        g.DrawString(tile.Symbol, GBBold, br,
                            new RectangleF(px, py, TILE, TILE), sf);
                }

            // Draw hero
            int hx = (_player.MapX - camX) * TILE;
            int hy = (_player.MapY - camY) * TILE;
            using (var br = new SolidBrush(C3))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center,
                                               LineAlignment = StringAlignment.Center })
                g.DrawString("@", GBBold, br, new RectangleF(hx, hy, TILE, TILE), sf);

            // Draw enemy during battle
            if (_state == Game.GameState.Battle && _currentEnemy != null)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)),
                    0, 0, MAP_W, MAP_H);

                using (var sf = new StringFormat { Alignment = StringAlignment.Center,
                                                   LineAlignment = StringAlignment.Center })
                using (var enemyFont = new Font("Courier New", 48, FontStyle.Bold))
                    g.DrawString(_currentEnemy.Sprite, enemyFont, new SolidBrush(C3),
                        new RectangleF(0, 0, MAP_W, MAP_H / 2), sf);

                string enemyInfo = $"{_currentEnemy.Name}  Niv.{_currentEnemy.Level}  HP:{_currentEnemy.Hp}/{_currentEnemy.MaxHp}";
                g.DrawString(enemyInfo, GBTitle, new SolidBrush(C3),
                    new RectangleF(10, MAP_H / 2 + 10, MAP_W - 20, 30));
            }
        }

        // ── Side Panel Update ──────────────────────────────────────────────
        void UpdateSidePanel()
        {
            _lblHero.Text = _player.Name;
            _hpBar.Value  = (int)Math.Round((double)_player.Hp / _player.MaxHp * 100);
            _expBar.Value = (int)Math.Round((double)_player.Exp / _player.ExpToLevel * 100);
            _lblStats.Text =
                $"Niv. {_player.Level}\r\n" +
                $"HP   {_player.Hp}/{_player.MaxHp}\r\n" +
                $"EXP  {_player.Exp}/{_player.ExpToLevel}\r\n" +
                $"ATK  {_player.Attack}\r\n" +
                $"DEF  {_player.Defense}\r\n" +
                $"Or   {_player.Gold} 💰\r\n" +
                $"Pos  ({_player.MapX},{_player.MapY})";

            bool inBattle = _state == Game.GameState.Battle;
            _btnFight.Enabled = !inBattle;
            _btnFlee.Enabled  = inBattle;
        }

        // ── Log ────────────────────────────────────────────────────────────
        void PushLog(string msg)
        {
            _log.Insert(0, msg);
            if (_log.Count > 6) _log.RemoveAt(_log.Count - 1);
            _lblLog.Text = string.Join("\r\n", _log);
        }

        // ── Input ──────────────────────────────────────────────────────────
        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_state == Game.GameState.Battle) { HandleBattleKey(e.KeyCode); return; }

            Direction? dir = e.KeyCode switch {
                Keys.Z or Keys.Up    => Direction.Up,
                Keys.S or Keys.Down  => Direction.Down,
                Keys.Q or Keys.Left  => Direction.Left,
                Keys.D or Keys.Right => Direction.Right,
                _ => (Direction?)null
            };

            if (dir.HasValue)
            {
                _player.Move(dir.Value, _map);
                _stepCount++;
                _ = _api.SyncPlayerAsync(_apiPlayerId, _player);
                CheckRandomEncounter();
                UpdateSidePanel();
                _mapPanel.Invalidate();
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F: StartBattle(); break;
                case Keys.X: Flee();        break;
                case Keys.B: OpenShop();    break;
                case Keys.P: _ = _api.SaveAsync(_apiPlayerId, 1); PushLog("💾 Sauvegardé !"); break;
            }
        }

        void HandleBattleKey(Keys k)
        {
            if (k == Keys.F) { ContinueBattle(); return; }
            if (k == Keys.X) { Flee(); return; }
        }

        // ── Random Encounter ───────────────────────────────────────────────
        void CheckRandomEncounter()
        {
            if (_stepCount % 8 != 0) return;
            if (_rng.Next(100) < 30) StartBattle();
        }

        void SetupEncounterTimer() { /* using step-based encounters */ }

        // ── Battle ─────────────────────────────────────────────────────────
        void StartBattle()
        {
            if (_state == Game.GameState.Battle) { ContinueBattle(); return; }
            _currentEnemy = MonsterPool.GetRandom(_player.Level);
            _state        = Game.GameState.Battle;
            PushLog($"⚔️  Un {_currentEnemy.Name} apparaît !");
            UpdateSidePanel();
            _mapPanel.Invalidate();
        }

        void ContinueBattle()
        {
            if (_currentEnemy == null || _state != Game.GameState.Battle) return;

            _battle.RunBattle(_player, _currentEnemy);
            foreach (var line in _battle.Log)
                PushLog(line);

            _ = _api.FightApiAsync(_apiPlayerId, _currentEnemy.Id);

            _state        = Game.GameState.Explore;
            _currentEnemy = null;

            if (_battle.LeveledUp)
                MessageBox.Show($"🎉 NIVEAU {_player.Level} !\nHP Max: {_player.MaxHp}  ATK: {_player.Attack}",
                    "LEVEL UP !", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateSidePanel();
            _mapPanel.Invalidate();
        }

        void Flee()
        {
            if (_state != Game.GameState.Battle) return;
            _state        = Game.GameState.Explore;
            _currentEnemy = null;
            PushLog("🏃 Tu t'es enfui !");
            UpdateSidePanel();
            _mapPanel.Invalidate();
        }

        // ── Shop (simple dialog) ───────────────────────────────────────────
        void OpenShop()
        {
            var items = new[]
            {
                ("Potion (20 HP)",     1,  15),
                ("Hi-Potion (50 HP)",  2,  40),
                ("Elixir (HP max)",    3, 100),
                ("Épée de Bois +ATK3", 4,  30),
                ("Épée de Fer  +ATK7", 5,  80),
                ("Bouclier    +DEF4",  6,  60),
            };

            var form = new Form
            {
                Text = "🏪 BOUTIQUE", Size = new Size(320, 400),
                BackColor = C0, ForeColor = C3,
                Font = GBFont, StartPosition = FormStartPosition.CenterParent
            };

            int y = 10;
            form.Controls.Add(new Label { Text = $"Or disponible : {_player.Gold} 💰",
                Bounds = new Rectangle(10, y, 290, 20), ForeColor = C2 });
            y += 28;

            foreach (var (name, id, price) in items)
            {
                var btn = new Button
                {
                    Text = $"{name}  — {price} Or",
                    Bounds = new Rectangle(10, y, 280, 28),
                    BackColor = C1, ForeColor = C3,
                    FlatStyle = FlatStyle.Flat, Tag = (id, price)
                };
                btn.FlatAppearance.BorderColor = C2;
                btn.Click += (s, e) =>
                {
                    var (iid, iprice) = ((int, int))((Button)s!).Tag!;
                    if (_player.Gold >= iprice)
                    {
                        _player.Gold -= iprice;
                        _ = _api.BuyItemAsync(_apiPlayerId, iid);
                        PushLog($"Acheté : {((Button)s).Text.Split('—')[0].Trim()}");
                        UpdateSidePanel();
                        form.Close();
                    }
                    else MessageBox.Show("Pas assez d'or ! 💰");
                };
                form.Controls.Add(btn);
                y += 34;
            }

            form.ShowDialog(this);
        }
    }
}
