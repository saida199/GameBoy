using System;
using System.Collections.Generic;

namespace GameBoyRPG.WinForms.Game
{
    public enum TileType { Grass, Tree, Mountain, Water, House, Path }
    public enum Direction { Up, Down, Left, Right }
    public enum GameState { Login, Explore, Battle, Shop, Menu, GameOver }

    public class Tile
    {
        public TileType Type { get; set; }
        public bool Walkable { get; set; }
        public string Symbol { get; set; }
        public System.Drawing.Color Color { get; set; }

        public Tile(TileType type)
        {
            Type = type;
            switch (type)
            {
                case TileType.Grass:    Walkable = true;  Symbol = "·"; Color = System.Drawing.Color.FromArgb(139, 172, 15);  break;
                case TileType.Tree:     Walkable = false; Symbol = "T"; Color = System.Drawing.Color.FromArgb(48, 98, 48);    break;
                case TileType.Mountain: Walkable = false; Symbol = "^"; Color = System.Drawing.Color.FromArgb(100, 100, 80);  break;
                case TileType.Water:    Walkable = false; Symbol = "~"; Color = System.Drawing.Color.FromArgb(30, 100, 200);  break;
                case TileType.House:    Walkable = false; Symbol = "H"; Color = System.Drawing.Color.FromArgb(180, 120, 60);  break;
                case TileType.Path:     Walkable = true;  Symbol = "="; Color = System.Drawing.Color.FromArgb(180, 160, 100); break;
                default:                Walkable = true;  Symbol = " "; Color = System.Drawing.Color.Gray;                    break;
            }
        }
    }

    public class GameMap
    {
        public const int WIDTH  = 20;
        public const int HEIGHT = 16;
        private Tile[,] _tiles;

        private static readonly int[,] _blueprint = {
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
            {2,0,0,0,0,1,1,0,0,0,0,0,1,0,0,0,3,3,3,2},
            {2,0,4,4,0,1,0,0,0,0,5,0,0,0,0,0,3,0,0,2},
            {2,0,4,4,0,0,0,1,0,0,5,0,0,1,0,0,0,0,0,2},
            {2,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,3,3,0,2},
            {2,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,2},
            {2,0,0,0,0,5,5,5,5,5,5,5,5,0,0,0,0,0,0,2},
            {2,0,0,1,0,5,0,0,0,0,0,0,5,0,0,1,1,0,0,2},
            {2,0,0,1,0,5,0,0,0,0,0,0,5,0,0,1,0,0,0,2},
            {2,0,0,0,0,5,5,5,5,5,5,5,5,0,0,0,0,0,0,2},
            {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,2},
            {2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            {2,1,0,0,1,1,0,0,0,0,0,0,0,1,0,0,0,0,0,2},
            {2,0,0,0,1,0,0,0,0,0,0,0,0,1,0,1,1,1,0,2},
            {2,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,2},
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2}
        };

        public GameMap()
        {
            _tiles = new Tile[WIDTH, HEIGHT];
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                {
                    int v = _blueprint[y, x];
                    TileType t = v switch {
                        0 => TileType.Grass,
                        1 => TileType.Tree,
                        2 => TileType.Mountain,
                        3 => TileType.Water,
                        4 => TileType.House,
                        5 => TileType.Path,
                        _ => TileType.Grass
                    };
                    _tiles[x, y] = new Tile(t);
                }
        }

        public Tile GetTile(int x, int y) =>
            (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) ? new Tile(TileType.Mountain) : _tiles[x, y];
    }

    public class PlayerState
    {
        public int Id         { get; set; }
        public string Name    { get; set; } = "Héros";
        public int Level      { get; set; } = 1;
        public int Exp        { get; set; } = 0;
        public int MaxHp      { get; set; } = 30;
        public int Hp         { get; set; } = 30;
        public int Attack     { get; set; } = 8;
        public int Defense    { get; set; } = 5;
        public int Gold       { get; set; } = 50;
        public int MapX       { get; set; } = 5;
        public int MapY       { get; set; } = 5;
        public int ExpToLevel => Level * 100;

        public bool Move(Direction dir, GameMap map)
        {
            int nx = MapX, ny = MapY;
            switch (dir) {
                case Direction.Up:    ny--; break;
                case Direction.Down:  ny++; break;
                case Direction.Left:  nx--; break;
                case Direction.Right: nx++; break;
            }
            if (map.GetTile(nx, ny).Walkable) { MapX = nx; MapY = ny; return true; }
            return false;
        }
    }

    public class Monster
    {
        public int    Id         { get; set; }
        public string Name       { get; set; } = "Monstre";
        public string Sprite     { get; set; } = "?";
        public int    Level      { get; set; } = 1;
        public int    MaxHp      { get; set; } = 10;
        public int    Hp         { get; set; } = 10;
        public int    Attack     { get; set; } = 4;
        public int    Defense    { get; set; } = 1;
        public int    ExpReward  { get; set; } = 8;
        public int    GoldReward { get; set; } = 4;
    }

    public class BattleEngine
    {
        private readonly Random _rng = new Random();

        public List<string> Log { get; } = new List<string>();
        public bool PlayerWon   { get; private set; }
        public int  ExpGained   { get; private set; }
        public int  GoldGained  { get; private set; }
        public bool LeveledUp   { get; private set; }

        public void RunBattle(PlayerState p, Monster m)
        {
            Log.Clear();
            Log.Add($"⚔️  Combat contre {m.Sprite} {m.Name} (Niv.{m.Level}) !");
            int mHp = m.MaxHp;

            for (int round = 1; round <= 30; round++)
            {
                // Player turn
                int pd = Math.Max(1, p.Attack - m.Defense + _rng.Next(-2, 3));
                mHp -= pd;
                Log.Add($"  Tour {round} › Tu infliges {pd} dégâts. ({mHp}/{m.MaxHp} HP restants)");

                if (mHp <= 0)
                {
                    PlayerWon  = true;
                    ExpGained  = m.ExpReward;
                    GoldGained = m.GoldReward;
                    p.Exp  += ExpGained;
                    p.Gold += GoldGained;

                    while (p.Exp >= p.ExpToLevel)
                    {
                        p.Exp   -= p.ExpToLevel;
                        p.Level++;
                        p.MaxHp  += 5;
                        p.Attack += 2;
                        p.Defense++;
                        p.Hp      = p.MaxHp;
                        LeveledUp = true;
                        Log.Add($"  🎉 NIVEAU {p.Level} !");
                    }
                    Log.Add($"  ✅ Victoire ! +{ExpGained} EXP  +{GoldGained} Or");
                    return;
                }

                // Monster turn
                int md = Math.Max(1, m.Attack - p.Defense + _rng.Next(-2, 3));
                p.Hp -= md;
                Log.Add($"  {m.Name} t'inflige {md} dégâts. ({Math.Max(0, p.Hp)}/{p.MaxHp} HP)");

                if (p.Hp <= 0)
                {
                    p.Hp = 1;
                    PlayerWon = false;
                    Log.Add("  💀 Défaite... tu t'en sors de justesse !");
                    return;
                }
            }
            PlayerWon = false;
            Log.Add("  ⏱ Le combat a duré trop longtemps — fuite !");
        }
    }

    public static class MonsterPool
    {
        public static List<Monster> All { get; } = new List<Monster>
        {
            new Monster { Id=1, Name="Slime",       Sprite="●", Level=1, MaxHp=10, Hp=10, Attack=3,  Defense=1, ExpReward=8,  GoldReward=3  },
            new Monster { Id=2, Name="Chauve-souris",Sprite="ψ", Level=2, MaxHp=14, Hp=14, Attack=5,  Defense=2, ExpReward=12, GoldReward=5  },
            new Monster { Id=3, Name="Loup",         Sprite="W", Level=3, MaxHp=20, Hp=20, Attack=7,  Defense=3, ExpReward=18, GoldReward=8  },
            new Monster { Id=4, Name="Gobelin",      Sprite="G", Level=4, MaxHp=24, Hp=24, Attack=9,  Defense=4, ExpReward=24, GoldReward=12 },
            new Monster { Id=5, Name="Squelette",    Sprite="S", Level=5, MaxHp=30, Hp=30, Attack=11, Defense=6, ExpReward=32, GoldReward=15 },
            new Monster { Id=6, Name="Dragon Noir",  Sprite="D", Level=8, MaxHp=60, Hp=60, Attack=20, Defense=10,ExpReward=90, GoldReward=50 },
        };

        private static readonly Random _rng = new Random();
        public static Monster GetRandom(int playerLevel)
        {
            var suitable = All.FindAll(m => Math.Abs(m.Level - playerLevel) <= 2);
            if (suitable.Count == 0) suitable = All;
            var template = suitable[_rng.Next(suitable.Count)];
            // Return a clone
            return new Monster {
                Id=template.Id, Name=template.Name, Sprite=template.Sprite,
                Level=template.Level, MaxHp=template.MaxHp, Hp=template.MaxHp,
                Attack=template.Attack, Defense=template.Defense,
                ExpReward=template.ExpReward, GoldReward=template.GoldReward
            };
        }
    }
}
