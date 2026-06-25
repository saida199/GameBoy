using Microsoft.AspNetCore.Mvc;
using GameBoyRPG.API.Models;
using GameBoyRPG.API.Services;

namespace GameBoyRPG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly GameService _svc;
    public PlayersController(GameService svc) => _svc = svc;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var player = await _svc.RegisterAsync(dto);
        if (player == null) return Conflict("Nom d'utilisateur déjà pris.");
        return Ok(new { player.Id, player.Username, player.HeroName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var player = await _svc.LoginAsync(dto);
        if (player == null) return Unauthorized("Identifiants incorrects.");
        return Ok(new { player.Id, player.Username, player.HeroName, player.Level });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _svc.GetPlayerAsync(id);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpPost("{id}/move")]
    public async Task<IActionResult> Move(int id, MoveDto dto)
    {
        var ok = await _svc.MovePlayerAsync(id, dto);
        return ok ? Ok("Position mise à jour.") : NotFound();
    }

    [HttpPost("{id}/save")]
    public async Task<IActionResult> Save(int id, SaveGameDto dto)
    {
        await _svc.SaveGameAsync(id, dto);
        return Ok("Sauvegardé.");
    }

    [HttpGet("{id}/saves")]
    public async Task<IActionResult> GetSaves(int id) =>
        Ok(await _svc.GetSaveSlotsAsync(id));

    [HttpGet("{id}/inventory")]
    public async Task<IActionResult> Inventory(int id) =>
        Ok(await _svc.GetInventoryAsync(id));

    [HttpPost("{id}/inventory/use")]
    public async Task<IActionResult> UseItem(int id, UseItemDto dto) =>
        Ok(await _svc.UseItemAsync(id, dto));

    [HttpGet("{id}/battlelogs")]
    public async Task<IActionResult> BattleLogs(int id) =>
        Ok(await _svc.GetBattleLogsAsync(id));
}

[ApiController]
[Route("api/[controller]")]
public class BattleController : ControllerBase
{
    private readonly GameService _svc;
    public BattleController(GameService svc) => _svc = svc;

    [HttpPost("{playerId}")]
    public async Task<IActionResult> Fight(int playerId, BattleDto dto)
    {
        var result = await _svc.FightAsync(playerId, dto);
        return result == null ? NotFound() : Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly GameService _svc;
    public ShopController(GameService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetItems() => Ok(await _svc.GetShopAsync());

    [HttpPost("{playerId}/buy/{itemId}")]
    public async Task<IActionResult> Buy(int playerId, int itemId) =>
        Ok(await _svc.BuyItemAsync(playerId, itemId));
}

[ApiController]
[Route("api/[controller]")]
public class MonstersController : ControllerBase
{
    private readonly GameService _svc;
    public MonstersController(GameService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _svc.GetMonstersAsync());
}

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly GameService _svc;
    public LeaderboardController(GameService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _svc.GetLeaderboardAsync());
}
