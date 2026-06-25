# 🎮 GAMEBOY RPG — Guide d'installation complet

## Architecture
```
GameBoyRPG.sln
├── GameBoyRPG.API        → ASP.NET Core 8 · MVC · EF Core · LINQ · PostgreSQL
├── GameBoyRPG.BlazorUI   → Blazor WASM · MudBlazor (dashboard web)
└── GameBoyRPG.WinForms   → WinForms .NET 4.8 (émulateur Game Boy jouable)
```

---

## Prérequis

| Outil | Version |
|-------|---------|
| .NET SDK | 8.0+ |
| .NET Framework | 4.8 |
| PostgreSQL | 14+ |
| VS Code | Dernière version |
| Extension C# (VS Code) | ms-dotnettools.csharp |
| EF Core Tools | `dotnet tool install --global dotnet-ef` |

---

## 1️⃣  Base de données PostgreSQL

```sql
-- Dans psql ou pgAdmin :
CREATE DATABASE gameboy_rpg;
```

Modifier le mot de passe si besoin dans :
`GameBoyRPG.API/appsettings.json`
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=gameboy_rpg;Username=postgres;Password=TON_MOT_DE_PASSE"
```

---

## 2️⃣  Migration EF Core (1 seule fois)

```bash
cd GameBoyRPG.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> ⚡ Ou laisse l'API faire la migration automatiquement au premier lancement (`db.Database.Migrate()` dans `Program.cs`).

---

## 3️⃣  Lancer l'API (backend)

```bash
cd GameBoyRPG.API
dotnet run
# → http://localhost:5000
# → Swagger : http://localhost:5000/swagger
```

---

## 4️⃣  Lancer le front Blazor (web dashboard)

```bash
cd GameBoyRPG.BlazorUI
dotnet run
# → http://localhost:5001  (ou port assigné)
```

---

## 5️⃣  Lancer le jeu WinForms (.NET 4.8)

```bash
cd GameBoyRPG.WinForms
dotnet run
```

Ou ouvre dans **Visual Studio 2022** et lance en F5.

---

## 🎮 Contrôles WinForms

| Touche | Action |
|--------|--------|
| Z / ↑  | Monter |
| S / ↓  | Descendre |
| Q / ←  | Gauche |
| D / →  | Droite |
| F      | Combattre / continuer le combat |
| X      | Fuir |
| B      | Boutique |
| P      | Sauvegarder |

---

## 📡 API Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | /api/players/register | Créer un compte |
| POST | /api/players/login | Connexion |
| GET  | /api/players/{id} | Infos joueur |
| POST | /api/players/{id}/move | Déplacer |
| POST | /api/battle/{playerId} | Combattre un monstre |
| GET  | /api/monsters | Liste monstres |
| GET  | /api/shop | Articles boutique |
| POST | /api/shop/{playerId}/buy/{itemId} | Acheter |
| GET  | /api/players/{id}/inventory | Inventaire |
| POST | /api/players/{id}/inventory/use | Utiliser item |
| POST | /api/players/{id}/save | Sauvegarder |
| GET  | /api/leaderboard | Classement top 20 |

---

## 🗃️ Structure des fichiers

```
GameBoyRPG/
├── GameBoyRPG.sln
│
├── GameBoyRPG.API/
│   ├── Controllers/GameControllers.cs    ← 5 controllers MVC
│   ├── Data/GameDbContext.cs             ← EF Core + seed data
│   ├── Models/GameModels.cs              ← Entités (Player, Monster, Item…)
│   ├── Models/Dtos.cs                    ← DTOs (record types)
│   ├── Services/GameService.cs           ← Logique + LINQ
│   ├── Program.cs
│   └── appsettings.json
│
├── GameBoyRPG.BlazorUI/
│   ├── Pages/Index.razor                 ← Login / Register
│   ├── Pages/Game.razor                  ← Dashboard jeu complet
│   ├── Pages/Leaderboard.razor           ← Classement MudBlazor
│   ├── Services/ApiService.cs            ← Client HTTP
│   ├── Shared/MainLayout.razor
│   ├── App.razor                         ← Thème Game Boy vert
│   ├── _Imports.razor
│   └── wwwroot/index.html
│
└── GameBoyRPG.WinForms/
    ├── Game/GameEngine.cs                ← Map, Player, Monster, BattleEngine
    ├── Game/ApiClient.cs                 ← HTTP vers l'API
    ├── Forms/LoginForm.cs                ← Écran connexion Game Boy style
    ├── Forms/GameForm.cs                 ← Jeu complet (carte, combats, HUD)
    └── Program.cs
```
