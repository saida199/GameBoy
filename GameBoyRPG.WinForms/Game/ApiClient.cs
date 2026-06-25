using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameBoyRPG.WinForms.Game
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private const string BASE = "http://localhost:5000/";

        public ApiClient() =>
            _http = new HttpClient { BaseAddress = new Uri(BASE), Timeout = TimeSpan.FromSeconds(5) };

        private StringContent Json(object o) =>
            new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");

        public async Task<JObject?> LoginAsync(string user, string pass)
        {
            try
            {
                var r = await _http.PostAsync("api/players/login", Json(new { username = user, password = pass }));
                if (!r.IsSuccessStatusCode) return null;
                return JObject.Parse(await r.Content.ReadAsStringAsync());
            }
            catch { return null; }
        }

        public async Task<JObject?> RegisterAsync(string user, string pass, string hero)
        {
            try
            {
                var r = await _http.PostAsync("api/players/register", Json(new { username = user, password = pass, heroName = hero }));
                if (!r.IsSuccessStatusCode) return null;
                return JObject.Parse(await r.Content.ReadAsStringAsync());
            }
            catch { return null; }
        }

        public async Task SyncPlayerAsync(int id, PlayerState p)
        {
            try
            {
                await _http.PostAsync($"api/players/{id}/move",
                    Json(new { x = p.MapX, y = p.MapY }));
            }
            catch { /* offline mode ok */ }
        }

        public async Task SaveAsync(int playerId, int slot)
        {
            try { await _http.PostAsync($"api/players/{playerId}/save", Json(new { slotNumber = slot })); }
            catch { }
        }

        public async Task<JObject?> FightApiAsync(int playerId, int monsterId)
        {
            try
            {
                var r = await _http.PostAsync($"api/battle/{playerId}", Json(new { monsterId }));
                if (!r.IsSuccessStatusCode) return null;
                return JObject.Parse(await r.Content.ReadAsStringAsync());
            }
            catch { return null; }
        }

        public async System.Threading.Tasks.Task BuyItemAsync(int playerId, int itemId)
        {
            try { await _http.PostAsync($"api/shop/{playerId}/buy/{itemId}", Json(new { })); }
            catch { }
        }
    }
}
