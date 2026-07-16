# WorldCup Project Functionalities

## Overview

This project is a Unity-based educational football game for the Horizon Education platform. It combines:

- a **2D soccer gameplay loop**
- a **quiz-based educational mechanic**
- a **country progression system**
- a reusable **platform/game shell**
- **multilingual support** including Arabic RTL UI

The project appears to be structured as one main Unity game with supporting framework modules and third-party packages.

---

## Main Functional Areas

## 1. Core Soccer Gameplay

Location: `Assets/Scripts/`

The game includes a 2D football match where the player competes against an AI-controlled bot.

### Functionalities

- Player vs bot soccer match
- Physics-based ball interaction
- Goal detection
- Score tracking
- Match win condition at **1 goal** (score once, advance to next country)
- Round reset after scoring events
- Match end popup with replay/navigation actions

### Key scripts

- `Assets/Scripts/SoccerGameManager.cs`
- `Assets/Scripts/BallController.cs`
- `Assets/Scripts/GoalDetector.cs`
- `Assets/Scripts/MatchEndPopup.cs`
- `Assets/Scripts/PlayerUIController.cs`

### Verified behavior from code

From `SoccerGameManager`:

- `WIN_SCORE` is set to `1` (1 goal wins the match, advancing to next country)
- gameplay pauses when a goal or quiz-triggering event happens
- the game resumes after quiz resolution or round reset
- player and bot scores are tracked centrally
- super-shoot state is tracked across rounds within the same match

---

## 2. Educational Quiz System

Location: `Assets/Scripts/QuizManager.cs`

The game uses quiz questions as part of the scoring mechanic instead of awarding points automatically.

### Functionalities

- Display quiz panel when a goal happens
- Display quiz panel when a power-up bubble is collected by the player
- Random question selection
- Non-repeating question pool until exhausted
- Answer selection and submission flow
- Correct/wrong answer feedback through button colors
- Delayed close animation and callback to gameplay manager

### Verified quiz topics in code

The current question set includes:

- 2026 World Cup host countries
- number of teams in the 2026 World Cup
- confederation with most qualified teams
- 2022 World Cup winner
- number of players per team on the pitch

### Scoring logic

Verified from `SoccerGameManager.cs`:

- If the **player scores** and answers correctly -> player gets `+1`
- If the **player scores** and answers incorrectly -> no point
- If the **bot scores** and the answer is incorrect -> bot gets `+1`
- If the **bot scores** and the answer is correct -> bot score is blocked

This makes the game both reflex-based and knowledge-based.

---

## 3. Power-Up Bubble System

Location: `Assets/Scripts/PowerUpSpawner.cs`, `Assets/Scripts/PowerUpBubble.cs`

The game includes collectible floating power-up bubbles that can affect gameplay.

### Functionalities

- Bubble spawning during matches
- Bubble movement across the field
- Player and bot bubble collection handling
- Quiz-gated player reward
- Direct bot reward behavior
- Super shoot activation support

### Verified behavior

- Bubble collection by the player can trigger a quiz
- Correct player answer grants **Super Shoot**
- Bot can also gain super-shoot capability
- `SoccerGameManager.OnSuperShootFired()` activates special ball behavior through `BallController`

---

## 4. Bot AI System

Location: `Assets/Scripts/BotAIController.cs`

The enemy bot uses a fairly advanced AI controller with configurable stats.

### Functionalities

- Ball chasing and interception
- Goal defense behavior
- Shooting behavior
- Goalie-style blocking behavior
- Return-to-home positioning
- Bubble collection behavior
- Repositioning after kicks
- Jump logic for aerial situations
- Difficulty-based reaction and pressure tuning
- Country-specific AI stat application

### Verified AI states from code

`BotAIController` defines these states:

- `Idle`
- `Chase`
- `Shoot`
- `Defend`
- `GoalieBlock`
- `ReturnGoal`
- `CollectBubble`
- `Reposition`

### Configurable bot attributes

The bot supports per-country tuning such as:

- move speed
- jump force
- kick force
- kick range
- pressure boost
- difficulty
- jersey color
- **aggressive** (0-1): how aggressively the bot chases the ball
- **defensive** (0-1): how much the bot prioritizes defense
- **possession** (0-1): tendency to dribble vs shoot

These are applied through `BotStatsData` and `ApplyStats()`.

### Advanced AI Features (v2.0+)

| Feature | Description |
|---------|-------------|
| **Super shoot awareness** | Bot detects when the player has Super Shoot ready (`PlayerSuperShootEarned`) and plays more defensively — stays in goalie mode, avoids reckless chasing |
| **Player pattern adaptation** | Bot records the Y-position of the player's last 5 shots (tracked via `RecordPlayerShot()` on player goals). Goalie biases away from the player's preferred shooting zone using `patternAvoidBias` |
| **Dribble vs shoot decision** | When `playStylePossession > 0.4` and there's open space ahead (`>25%` of field width to goal), the bot dribbles forward instead of shooting — controlled by `ShouldKickInsteadOfDribble()` |
| **Country-specific play styles** | Each country's `BotStatsData` defines `aggressive`, `defensive`, `possession` values. Aggressive countries chase sooner; defensive countries prioritize goalie positioning |
| **Goalie vs super shoot** | When `BallController.SuperModeActive` is true, the goalie tracks the ball's current position (not prediction) and jumps at lower height thresholds |
| **Tactical possession fallback** | After losing possession, bot waits `0.6s` in a defensive posture before re-engaging — prevents instant chasing after turnovers |

---

## 5. Country Selection and Match Setup

Location: `Assets/Scripts/CountryFlagButtonHandler.cs`, `Assets/Scripts/CountryDataHolder.cs`, `Assets/Scripts/CountryBotLinker.cs`, `Assets/Scripts/BotStatsLinker.cs`

The player selects an opponent country from a map-like screen.

### Functionalities

- Flag button selection for countries
- Locked/unlocked country handling
- Country preview panel
- Store selected opponent in `PlayerPrefs`
- Store selected opponent in shared runtime holder
- Load match scene after pressing Play
- Link selected country to bot prefab and stats
- Update map lock overlays visually

### Verified behavior from code

`CountryFlagButtonHandler`:

- prevents selecting locked countries
- stores selected country index in `OpponentIndex`
- stores selected country name in `OpponentCountry`
- loads the match scene `Dlc_2_Level[AR] 0`

---

## 6. Progression and Unlock System

Location: `Assets/Scripts/ProgressionManager.cs`

The game includes a progression system that unlocks countries based on wins.

### Functionalities

- First country unlocked by default
- Country completion tracking
- Region-based unlock logic
- Sequential opponent unlocking inside regions
- Unlock first country of next region when a region is fully completed
- Save progress with `PlayerPrefs`
- Reset progression support

### Verified region structure from code

- **Africa**: Algeria, Germany
- **America**: Argentina, Brazil, USA
- **Oceania**: Australia
- **Europe**: France, Russia
- **Asia**: Japan, Niger

### Persistence keys

- `unlocked_<index>`
- `completed_<index>`

### Verified behavior

- Country `0` is unlocked initially
- Winning a match calls `OnMatchWin(int countryIndex)`
- Completing a country unlocks the next one in that region
- Completing a region unlocks the first country in the next region

---

## 7. Match End and Navigation Flow

Location: `Assets/Scripts/MatchEndPopup.cs`

The project includes a match completion popup for end-of-game decisions.

### Verified functionalities

- Show win/lose result
- Restart current match
- **Move to next match** — advances to the next unlocked country in the progression before loading the scene
- **"Next Match" button hidden** when no more countries to play (all completed)
- Return to map

---

## 8. Visual and Background Management

Location: `Assets/Scripts/BackgroundManager.cs`, `Assets/Scripts/CountryBackgroundData.cs`

The project supports visual customization of matches depending on the selected country.

### Functionalities

- Load country-specific background or pitch art
- Use selected opponent data to decide visuals
- Keep match presentation themed by country/opponent

---

## 9. Data Holders and Runtime Context

Location: `Assets/Scripts/CountryDataHolder.cs`, `Assets/Scripts/GameDataHolder.cs`

The project uses singleton-style holders to keep runtime state available across scenes.

### Functionalities

- persist selected country information during runtime
- provide bot prefab lookup context
- provide shared game/session data between map and match scenes

---

## 10. Horizon Platform Framework Integration

Location: `Assets/Game_Over_Manager_2.0/`

A major part of the repository is a reusable Horizon Education framework used around the core mini-game.

### Functionalities

- startup/game shell management
- scene loading helpers
- XP and level calculation
- user profile persistence
- audio management
- UI panels and menus
- leaderboard/backend model support
- map/level node systems
- game over and result flows
- mascot/avatar support through Ferid-related scripts

### Important scripts mentioned in project structure

- `Game_Over_2_Manager`
- `Game_Over_2_UIManager`
- `Game_Over_2_LevelManager`
- `Game_Over_2_GameInteraction`
- `Game_Over_2_AudioManager`
- `Game_Over_2_SaveSystem`
- `Game_Over_2_LoadScene`
- `Game_Over_2_DataBase_Manager`
- `Game_Over_2_Ferid`

### Notes

Some of this framework appears generic and reusable across Horizon games, not unique to this specific football gameplay.

---

## 11. Localization and Arabic RTL Support

Locations:

- `Assets/NoSuchStudio/`
- `Assets/_FeridAroundTheWorld/`
- `Assets/RTLTMPro/`
- `Assets/ArabicSupport/`

### Functionalities

- multilingual UI support
- Arabic right-to-left text rendering
- localization helpers and runtime language management
- CSV-based localization workflows
- localized audio support in framework scripts
- editor tooling for localization workflows

### Supported languages

Based on project documentation and code organization:

- Arabic
- English
- French

### Related packages/components

- `RTLTMPro`
- `ArabicSupport`
- `TextMesh Pro`
- NoSuchStudio localization system

---

## 12. Audio System

Location: `Assets/Game_Over_Manager_2.0/Over_Manger_2_Scripts/Audio_Manager_Script/`

### Functionalities

- sound effect playback
- music playback
- option panels for audio settings
- localized or language-aware audio clips
- mascot voice lines in the Horizon framework

---

## 13. UI/Animation Systems

Locations:

- `Assets/LeanTween/`
- `Assets/TextMesh Pro/`
- `Assets/Game_Over_Manager_2.0/GUI PRO Kit - Simple Casual/`

### Functionalities

- animated UI transitions
- quiz modal open/close animation
- tweening support
- styled menu and panel components
- score, text, and UI rendering through TextMesh Pro

---

## 14. Project Scenes and User Flow

From the repository overview and naming structure, the active flow is:

1. Startup scene
2. Map/country selection scene
3. Level/match scene
4. End-of-match navigation back to map or replay

### Main scenes referenced

- `Dlc_0_StartUpScene[AR]`
- `Dlc_1_Map[AR]`
- `Dlc_2_Level[AR] 0`

There are also additional framework scenes under `Assets/Game_Over_Manager_2.0/Over_Manger_2_Scenes/`.

---

## 15. Persistence and Save Data

The project uses `PlayerPrefs` in several places.

### Verified usages

- selected opponent index
- selected opponent name
- unlocked countries
- completed countries
- framework-related save data such as settings, stars, XP, or profile data

This means the game preserves both gameplay progression and platform-level profile/settings data locally.

---

## 16. Dependencies and Unity Packages

From `Packages/manifest.json`, the project depends on:

- `com.unity.feature.2d`
- `com.unity.textmeshpro`
- `com.unity.ugui`
- `com.unity.nuget.newtonsoft-json`
- `com.unity.test-framework`
- `com.unity.timeline`
- `com.unity.visualscripting`
- standard Unity modules for audio, physics, physics2d, animation, UI, networking, and more

### What this indicates functionally

- 2D gameplay
- modern UI support
- JSON serialization support
- test framework availability
- timeline/animation capability

---

## 17. Summary of Key Game Features

This project provides the following end-user functionality:

- Play a 2D soccer match against AI bots
- Select different country opponents
- Progress through a country unlock system
- Answer educational football/world-cup quiz questions
- Use quiz-gated rewards like super shoot
- Experience localized UI in Arabic, English, and French
- Use a broader Horizon Education platform shell with progression, UI, audio, and profile systems

---

## 18. World Cup Configuration File

Location: `Assets/Resources/WorldCupConfig.json`, `Assets/Scripts/WorldCupConfig.cs`

The game reads a JSON config file to control per-country gameplay parameters instead of using hardcoded values.

### Configurable fields per country

| Field | Type | Description |
|-------|------|-------------|
| `index` | int | Country index (0-9) |
| `displayName` | string | Display name |
| `goalsToWin` | int | How many goals the player needs to win this match |
| `startUnlocked` | bool | Whether this country is unlocked when starting a new game |
| `region` | string | Which region this country belongs to |

### Configurable regions

The `regions` array defines the progression order: completing all countries in one region unlocks the first country of the next region.

### How to modify

Edit `Assets/Resources/WorldCupConfig.json`:
- Change `goalsToWin` to make a country harder/easier (e.g., `3` means the player must score 3 goals)
- Change `startUnlocked` to `true` for countries that should be available from the start
- Reorder or add regions to change the unlock flow

### Key classes

- `WorldCupConfig` (static) — loads JSON from Resources and provides query methods
- `SoccerGameManager` — reads `WorldCupConfig.GetGoalsToWin()` per country for win condition
- `ProgressionManager` — reads `startUnlocked` for initial unlocks and `regions` for unlock ordering

---

## 19. Notable Technical Observations

- The project is a **Unity educational mini-game integrated into a larger reusable platform framework**.
- The quiz system is tightly tied to scoring, which is the game's main educational hook.
- The bot AI is more advanced than a simple chase script and includes multiple tactical states.
- Progression is lightweight and implemented with `PlayerPrefs` rather than an external save database.
- A pre-existing `PROJECT_OVERVIEW.md` already documents much of the architecture, and this file focuses specifically on functional behavior.

---

## 19. Generated Deliverable

This analysis was generated from:

- project structure inspection
- direct review of core gameplay scripts
- Unity package manifest inspection
- existing project documentation in `PROJECT_OVERVIEW.md`

If needed, this file can be expanded further with:

- scene-by-scene breakdown
- script-by-script responsibilities
- gameplay flow diagrams
- setup/build instructions
