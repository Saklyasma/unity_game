# Changelog — WorldCup Game Session (July 2026)

## Bug Fixes

### Country click always loaded Algeria
- **File:** `Assets/Scripts/CountryDataHolder.cs`
- `GetSelectedBotPrefab()` now reads `PlayerPrefs.GetString("OpponentCountry")` directly instead of depending on the `Countries` list
- `GetSelected()` creates a fallback `CountryFlag` from PlayerPrefs when `Countries` is null

### Bot prefab instantiation
- **File:** `Assets/Scripts/BotStatsLinker.cs`
- Changed `GetComponent<BotAIController>()` → `GetComponentInChildren<BotAIController>(true)` to find the component in child objects
- Changed `Instantiate(prefabBot)` → `Instantiate(selectedPrefab)` so the entire prefab root is instantiated

### IsUnlocked() always returned true
- **File:** `Assets/Scripts/ProgressionManager.cs`
- `IsUnlocked()` now reads `PlayerPrefs.GetInt()` instead of always returning `true`

### Scene name mismatch in MatchEndPopup
- **File:** `Assets/Scenes/Dlc_2_Level[AR] 0.unity` (lines 5200-5202)
- Inspector values for `matchSceneName` and `nextSceneName` were set to `"MatchScene"` (non-existent) instead of `"Dlc_2_Level[AR] 0"`

---

## New Features

### Advanced Bot AI (6 features)
- **File:** `Assets/Scripts/BotAIController.cs`

| Feature | Description |
|---------|-------------|
| **Super shoot awareness** | Bot detects `PlayerSuperShootEarned` and plays more defensively |
| **Player pattern adaptation** | Bot records last 5 shot Y-positions; goalie biases away from preferred zone |
| **Dribble vs shoot decision** | High-possession bots dribble forward when open space >25% to goal |
| **Country play styles** | `BotStatsData` gains `aggressive`, `defensive`, `possession` fields (0-1) |
| **Goalie vs super shoot** | Goalie tracks ball position directly and jumps more during `SuperModeActive` |
| **Tactical possession fallback** | 0.6s defensive delay after losing possession before re-engaging |

### 1-Goal Win → Next Level
- **File:** `Assets/Scripts/SoccerGameManager.cs`
- `WIN_SCORE` reduced from 5 to 1 (now per-country via config)
- **File:** `Assets/Scripts/MatchEndPopup.cs`
- **Restart** button hidden on win, shown only on loss
- **Next Match** button advances to the next unlocked country before loading the scene
- **Next Match** button hidden when all countries are completed
- **File:** `Assets/Scripts/ProgressionManager.cs`
- Added `GetNextPlayableCountry()` — returns first unlocked + not-completed country

### World Cup Configuration File
- **New file:** `Assets/Resources/WorldCupConfig.json`
- **New file:** `Assets/Scripts/WorldCupConfig.cs`
- Per-country `goalsToWin` — replaceable per country (e.g., Algeria=1, Brazil=3)
- Per-country `startUnlocked` — controls initial unlock state
- Region data loaded from config instead of hardcoded
- Both `SoccerGameManager` and `ProgressionManager` read from config

---

## Tweaks

- Bot jersey color reverted to always white (removed per-country `jerseyColor`)
- Debug logging added across `CountryDataHolder.cs`, `CountryFlagButtonHandler.cs`, `BotStatsLinker.cs`
- `ProgressionManager.InitFirstUnlock()` → `InitUnlocks()` (reads all `startUnlocked` countries from config)
- `ProgressionManager.ResetProgress()` uses config's country count instead of iterating hardcoded regions

---

## Current Files

| File | Purpose |
|------|---------|
| `Assets/Scripts/BotAIController.cs` | Main AI controller (1265 lines, state machine + 6 new features) |
| `Assets/Scripts/BotStatsData.cs` | Per-country stat ScriptableObject |
| `Assets/Scripts/BotStatsLinker.cs` | Applies selected country's prefab sprite + stats |
| `Assets/Scripts/CountryDataHolder.cs` | DontDestroyOnLoad singleton for country list, index, bot prefab map |
| `Assets/Scripts/CountryFlagButtonHandler.cs` | Map scene UI handler |
| `Assets/Scripts/CountryBotLinker.cs` | Registers bot prefabs into CountryDataHolder |
| `Assets/Scripts/ProgressionManager.cs` | Region-based unlock/completion (now config-driven) |
| `Assets/Scripts/SoccerGameManager.cs` | Match manager, win condition (now per-country) |
| `Assets/Scripts/MatchEndPopup.cs` | Win/loss popup with context-sensitive buttons |
| `Assets/Scripts/QuizManager.cs` | Educational quiz, reads OpponentCountry from PlayerPrefs |
| `Assets/Scripts/WorldCupConfig.cs` | Config loader (new) |
| `Assets/Resources/WorldCupConfig.json` | Per-country config data (new) |
| `Assets/Prefabs/Bots/` | 10 visual-only bot prefabs |
| `FUNCTIONALITIES.md` | Full feature documentation |
