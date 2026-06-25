using Microsoft.EntityFrameworkCore;
using GameBoyRPG.API.Models;

namespace GameBoyRPG.API.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Monster> Monsters => Set<Monster>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PlayerItem> PlayerItems => Set<PlayerItem>();
    public DbSet<SaveSlot> SaveSlots => Set<SaveSlot>();
    public DbSet<BattleLog> BattleLogs => Set<BattleLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Username).IsUnique();
            e.Property(p => p.Username).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<PlayerItem>(e =>
        {
            e.HasKey(pi => pi.Id);
            e.HasOne(pi => pi.Player).WithMany(p => p.Inventory).HasForeignKey(pi => pi.PlayerId);
            e.HasOne(pi => pi.Item).WithMany(i => i.PlayerItems).HasForeignKey(pi => pi.ItemId);
        });

        modelBuilder.Entity<SaveSlot>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Player).WithMany(p => p.SaveSlots).HasForeignKey(s => s.PlayerId);
        });

        modelBuilder.Entity<BattleLog>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Player).WithMany(p => p.BattleLogs).HasForeignKey(b => b.PlayerId);
            e.HasOne(b => b.Monster).WithMany(m => m.BattleLogs).HasForeignKey(b => b.MonsterId);
        });

        // Seed Monsters
        modelBuilder.Entity<Monster>().HasData(
            new Monster { Id = 1, Name = "Slime",      Sprite = "🟢", Level = 1, MaxHp = 10, Attack = 3,  Defense = 1, ExpReward = 8,  GoldReward = 3,  Zone = "Forest" },
            new Monster { Id = 2, Name = "Bat",        Sprite = "🦇", Level = 2, MaxHp = 14, Attack = 5,  Defense = 2, ExpReward = 12, GoldReward = 5,  Zone = "Forest" },
            new Monster { Id = 3, Name = "Wolf",       Sprite = "🐺", Level = 3, MaxHp = 20, Attack = 7,  Defense = 3, ExpReward = 18, GoldReward = 8,  Zone = "Plains" },
            new Monster { Id = 4, Name = "Goblin",     Sprite = "👺", Level = 4, MaxHp = 24, Attack = 9,  Defense = 4, ExpReward = 24, GoldReward = 12, Zone = "Plains" },
            new Monster { Id = 5, Name = "Skeleton",   Sprite = "💀", Level = 5, MaxHp = 30, Attack = 11, Defense = 6, ExpReward = 32, GoldReward = 15, Zone = "Cave"   },
            new Monster { Id = 6, Name = "Dark Knight", Sprite = "⚔️", Level = 8, MaxHp = 50, Attack = 18, Defense = 12,ExpReward = 80, GoldReward = 40, Zone = "Castle" }
        );

        // Seed Items
        modelBuilder.Entity<Item>().HasData(
            new Item { Id = 1, Name = "Potion",       Description = "Restaure 20 HP",       Type = "Consumable", Effect = 20,  Price = 15,  Icon = "🧪" },
            new Item { Id = 2, Name = "Hi-Potion",    Description = "Restaure 50 HP",       Type = "Consumable", Effect = 50,  Price = 40,  Icon = "💊" },
            new Item { Id = 3, Name = "Elixir",       Description = "Restaure tous les HP", Type = "Consumable", Effect = 999, Price = 100, Icon = "✨" },
            new Item { Id = 4, Name = "Épée de Bois", Description = "ATK +3",               Type = "Weapon",     Effect = 3,   Price = 30,  Icon = "🗡️" },
            new Item { Id = 5, Name = "Épée de Fer",  Description = "ATK +7",               Type = "Weapon",     Effect = 7,   Price = 80,  Icon = "⚔️" },
            new Item { Id = 6, Name = "Bouclier",     Description = "DEF +4",               Type = "Armor",      Effect = 4,   Price = 60,  Icon = "🛡️" }
        );
    }
}
