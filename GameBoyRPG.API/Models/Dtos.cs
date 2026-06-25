namespace GameBoyRPG.API.Models;

public record RegisterDto(string Username, string Password, string HeroName);
public record LoginDto(string Username, string Password);
public record PlayerDto(
    int Id, string Username, string HeroName,
    int Level, int Experience, int MaxHp, int CurrentHp,
    int Attack, int Defense, int Gold, int MapX, int MapY);

public record BattleResultDto(
    bool PlayerWon, int DamageDealt, int DamageTaken,
    int ExpGained, int GoldGained, bool LeveledUp,
    int NewLevel, int NewExp, int RemainingHp, string Message);

public record LeaderboardEntryDto(
    string HeroName, string Username, int Level,
    int Experience, int TotalBattles, int TotalWins);

public record UseItemDto(int ItemId);
public record SaveGameDto(int SlotNumber);
public record BattleDto(int MonsterId);
public record MoveDto(int X, int Y);
