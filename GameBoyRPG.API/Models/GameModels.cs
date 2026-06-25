namespace GameBoyRPG.API.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string HeroName { get; set; } = "Hero";
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int MaxHp { get; set; } = 30;
    public int CurrentHp { get; set; } = 30;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 5;
    public int Gold { get; set; } = 50;
    public int MapX { get; set; } = 5;
    public int MapY { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;

    public ICollection<SaveSlot> SaveSlots { get; set; } = new List<SaveSlot>();
    public ICollection<PlayerItem> Inventory { get; set; } = new List<PlayerItem>();
    public ICollection<BattleLog> BattleLogs { get; set; } = new List<BattleLog>();
}

public class Monster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sprite { get; set; } = "👾";
    public int Level { get; set; } = 1;
    public int MaxHp { get; set; } = 15;
    public int Attack { get; set; } = 4;
    public int Defense { get; set; } = 2;
    public int ExpReward { get; set; } = 10;
    public int GoldReward { get; set; } = 5;
    public string Zone { get; set; } = "Forest";
    public ICollection<BattleLog> BattleLogs { get; set; } = new List<BattleLog>();
}

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Consumable"; // Consumable, Weapon, Armor
    public int Effect { get; set; } = 0;
    public int Price { get; set; } = 10;
    public string Icon { get; set; } = "🎒";
    public ICollection<PlayerItem> PlayerItems { get; set; } = new List<PlayerItem>();
}

public class PlayerItem
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public Player Player { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

public class SaveSlot
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int SlotNumber { get; set; }
    public string SaveData { get; set; } = "{}"; // JSON snapshot
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public Player Player { get; set; } = null!;
}

public class BattleLog
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int MonsterId { get; set; }
    public bool PlayerWon { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public int ExpGained { get; set; }
    public int GoldGained { get; set; }
    public DateTime FoughtAt { get; set; } = DateTime.UtcNow;
    public Player Player { get; set; } = null!;
    public Monster Monster { get; set; } = null!;
}
