using Microsoft.EntityFrameworkCore;
using GameBoyRPG.API.Data;
using GameBoyRPG.API.Models;
using System.Text.Json;

namespace GameBoyRPG.API.Services;

public class GameService
{
    private readonly GameDbContext _db;
    private readonly Random _rng = new();

    public GameService(GameDbContext db) => _db = db;

    // ── PLAYER ──────────────────────────────────────────────────────────────

    public async Task<Player?> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Players.AnyAsync(p => p.Username == dto.Username))
            return null;

        var player = new Player
        {
            Username    = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            HeroName    = dto.HeroName
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        // Give starter item
        _db.PlayerItems.Add(new PlayerItem { PlayerId = player.Id, ItemId = 1, Quantity = 3 });
        await _db.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> LoginAsync(LoginDto dto)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == dto.Username);
        if (player == null || !BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
            return null;
        player.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return player;
    }

    public async Task<PlayerDto?> GetPlayerAsync(int id)
    {
        return await _db.Players
            .Where(p => p.Id == id)
            .Select(p => new PlayerDto(
                p.Id, p.Username, p.HeroName,
                p.Level, p.Experience, p.MaxHp, p.CurrentHp,
                p.Attack, p.Defense, p.Gold, p.MapX, p.MapY))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> MovePlayerAsync(int playerId, MoveDto dto)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return false;
        player.MapX = Math.Clamp(dto.X, 0, 15);
        player.MapY = Math.Clamp(dto.Y, 0, 15);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── BATTLE ──────────────────────────────────────────────────────────────

    public async Task<BattleResultDto?> FightAsync(int playerId, BattleDto dto)
    {
        var player  = await _db.Players.FindAsync(playerId);
        var monster = await _db.Monsters.FindAsync(dto.MonsterId);
        if (player == null || monster == null) return null;

        int monsterHp = monster.MaxHp;
        int totalDmgDealt  = 0;
        int totalDmgTaken  = 0;
        bool playerWon     = false;

        // Turn-based loop (max 20 rounds)
        for (int round = 0; round < 20; round++)
        {
            // Player attacks
            int playerDmg = Math.Max(1, player.Attack - monster.Defense + _rng.Next(-2, 3));
            monsterHp    -= playerDmg;
            totalDmgDealt += playerDmg;

            if (monsterHp <= 0) { playerWon = true; break; }

            // Monster attacks
            int monsterDmg = Math.Max(1, monster.Attack - player.Defense + _rng.Next(-2, 3));
            player.CurrentHp  -= monsterDmg;
            totalDmgTaken     += monsterDmg;

            if (player.CurrentHp <= 0)
            {
                player.CurrentHp = 1; // Never die — respawn at 1 hp
                break;
            }
        }

        int expGained  = playerWon ? monster.ExpReward : 0;
        int goldGained = playerWon ? monster.GoldReward : 0;
        bool leveledUp = false;
        int newLevel   = player.Level;

        if (playerWon)
        {
            player.Experience += expGained;
            player.Gold       += goldGained;

            // Level up: 100 * level exp needed
            while (player.Experience >= player.Level * 100)
            {
                player.Experience -= player.Level * 100;
                player.Level++;
                player.MaxHp  += 5;
                player.Attack += 2;
                player.Defense++;
                player.CurrentHp = player.MaxHp;
                leveledUp = true;
                newLevel  = player.Level;
            }
        }

        _db.BattleLogs.Add(new BattleLog
        {
            PlayerId    = playerId,
            MonsterId   = dto.MonsterId,
            PlayerWon   = playerWon,
            DamageDealt = totalDmgDealt,
            DamageTaken = totalDmgTaken,
            ExpGained   = expGained,
            GoldGained  = goldGained,
        });

        await _db.SaveChangesAsync();

        string msg = playerWon
            ? leveledUp ? $"Victoire ! Niveau {newLevel} atteint ! 🎉" : "Victoire ! ⚔️"
            : "Défaite... tu t'en sors de justesse. 💀";

        return new BattleResultDto(
            playerWon, totalDmgDealt, totalDmgTaken,
            expGained, goldGained, leveledUp,
            newLevel, player.Experience, player.CurrentHp, msg);
    }

    // ── INVENTORY ───────────────────────────────────────────────────────────

    public async Task<List<object>> GetInventoryAsync(int playerId)
    {
        return await _db.PlayerItems
            .Where(pi => pi.PlayerId == playerId)
            .Include(pi => pi.Item)
            .Select(pi => (object)new {
                pi.Id,
                pi.Quantity,
                pi.Item.Name,
                pi.Item.Description,
                pi.Item.Type,
                pi.Item.Effect,
                pi.Item.Icon,
                pi.Item.Price
            })
            .ToListAsync();
    }

    public async Task<string> UseItemAsync(int playerId, UseItemDto dto)
    {
        var pi = await _db.PlayerItems
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.ItemId == dto.ItemId);

        if (pi == null || pi.Quantity <= 0) return "Item introuvable.";

        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return "Joueur introuvable.";

        string result;
        if (pi.Item.Type == "Consumable")
        {
            int healed = Math.Min(pi.Item.Effect, player.MaxHp - player.CurrentHp);
            player.CurrentHp += healed;
            pi.Quantity--;
            if (pi.Quantity == 0) _db.PlayerItems.Remove(pi);
            result = $"{pi.Item.Icon} {pi.Item.Name} utilisé — +{healed} HP";
        }
        else if (pi.Item.Type == "Weapon")
        {
            player.Attack += pi.Item.Effect;
            pi.Quantity--;
            if (pi.Quantity == 0) _db.PlayerItems.Remove(pi);
            result = $"{pi.Item.Icon} {pi.Item.Name} équipé — ATK +{pi.Item.Effect}";
        }
        else
        {
            player.Defense += pi.Item.Effect;
            pi.Quantity--;
            if (pi.Quantity == 0) _db.PlayerItems.Remove(pi);
            result = $"{pi.Item.Icon} {pi.Item.Name} équipé — DEF +{pi.Item.Effect}";
        }

        await _db.SaveChangesAsync();
        return result;
    }

    public async Task<List<Item>> GetShopAsync() =>
        await _db.Items.OrderBy(i => i.Price).ToListAsync();

    public async Task<string> BuyItemAsync(int playerId, int itemId)
    {
        var player = await _db.Players.FindAsync(playerId);
        var item   = await _db.Items.FindAsync(itemId);
        if (player == null || item == null) return "Erreur.";
        if (player.Gold < item.Price) return "Pas assez d'or. 💰";

        player.Gold -= item.Price;
        var existing = await _db.PlayerItems
            .FirstOrDefaultAsync(pi => pi.PlayerId == playerId && pi.ItemId == itemId);
        if (existing != null) existing.Quantity++;
        else _db.PlayerItems.Add(new PlayerItem { PlayerId = playerId, ItemId = itemId, Quantity = 1 });

        await _db.SaveChangesAsync();
        return $"Acheté : {item.Icon} {item.Name}";
    }

    // ── SAVE / LOAD ─────────────────────────────────────────────────────────

    public async Task SaveGameAsync(int playerId, SaveGameDto dto)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return;

        var existing = await _db.SaveSlots
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.SlotNumber == dto.SlotNumber);

        var json = JsonSerializer.Serialize(new {
            player.Level, player.Experience, player.MaxHp,
            player.CurrentHp, player.Attack, player.Defense,
            player.Gold, player.MapX, player.MapY
        });

        if (existing != null)
        {
            existing.SaveData = json;
            existing.SavedAt  = DateTime.UtcNow;
        }
        else
        {
            _db.SaveSlots.Add(new SaveSlot {
                PlayerId   = playerId,
                SlotNumber = dto.SlotNumber,
                SaveData   = json
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<SaveSlot>> GetSaveSlotsAsync(int playerId) =>
        await _db.SaveSlots.Where(s => s.PlayerId == playerId)
            .OrderBy(s => s.SlotNumber).ToListAsync();

    // ── LEADERBOARD ─────────────────────────────────────────────────────────

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync() =>
        await _db.Players
            .OrderByDescending(p => p.Level)
            .ThenByDescending(p => p.Experience)
            .Take(20)
            .Select(p => new LeaderboardEntryDto(
                p.HeroName, p.Username, p.Level, p.Experience,
                p.BattleLogs.Count(),
                p.BattleLogs.Count(b => b.PlayerWon)))
            .ToListAsync();

    // ── MONSTERS ────────────────────────────────────────────────────────────

    public async Task<List<Monster>> GetMonstersAsync() =>
        await _db.Monsters.OrderBy(m => m.Level).ToListAsync();

    public async Task<List<object>> GetBattleLogsAsync(int playerId) =>
        await _db.BattleLogs
            .Where(b => b.PlayerId == playerId)
            .Include(b => b.Monster)
            .OrderByDescending(b => b.FoughtAt)
            .Take(20)
            .Select(b => (object)new {
                b.Id,
                MonsterName = b.Monster.Name,
                b.Monster.Sprite,
                b.PlayerWon,
                b.DamageDealt,
                b.DamageTaken,
                b.ExpGained,
                b.GoldGained,
                b.FoughtAt
            })
            .ToListAsync();
}
