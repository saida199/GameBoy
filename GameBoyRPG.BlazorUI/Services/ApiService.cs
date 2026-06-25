using System.Net.Http.Json;

namespace GameBoyRPG.BlazorUI.Services;

public class ApiService
{
    private readonly HttpClient _http;
    public ApiService(HttpClient http) => _http = http;

    public async Task<T?> GetAsync<T>(string url)
    {
        try { return await _http.GetFromJsonAsync<T>(url); }
        catch { return default; }
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        try
        {
            var res = await _http.PostAsJsonAsync(url, body);
            return await res.Content.ReadFromJsonAsync<T>();
        }
        catch { return default; }
    }
}

// Shared DTOs for Blazor side
public record PlayerInfo(
    int Id, string Username, string HeroName,
    int Level, int Experience, int MaxHp, int CurrentHp,
    int Attack, int Defense, int Gold, int MapX, int MapY);

public record LeaderboardEntry(
    string HeroName, string Username, int Level,
    int Experience, int TotalBattles, int TotalWins);

public record BattleResult(
    bool PlayerWon, int DamageDealt, int DamageTaken,
    int ExpGained, int GoldGained, bool LeveledUp,
    int NewLevel, int NewExp, int RemainingHp, string Message);

public record MonsterInfo(
    int Id, string Name, string Sprite,
    int Level, int MaxHp, int Attack, int Defense,
    int ExpReward, int GoldReward, string Zone);
