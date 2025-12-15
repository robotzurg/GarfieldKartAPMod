# Garfield Kart - Assembly-CSharp.dll Class Reference

## Overview
This document provides a comprehensive reference of key classes in Garfield Kart for modding purposes, particularly for Archipelago integration.

## Key Namespaces
- **Global Namespace**: 1203 types (most game logic)
- **Aube**: 172 types (core engine/framework)
- **Rewired**: Input system
- **AssetBundles**: Asset management
- **Polybrush**: Terrain/mesh tools

---

## RACE MANAGEMENT CLASSES

### `RcRace`
**Purpose**: Main race controller
**Usage**: Tracks race state, lap counts, positions

### `RaceGameState` (extends `GameState`)
**Purpose**: Active racing state
**Related Classes**:
- `StartGameState`: Pre-race countdown
- `EndRaceGameState`: Post-race results
- `PodiumGameState`: Podium ceremony
- `RaceTutorialGameState`: Tutorial-specific race state

### `InGameGameMode` (abstract)
**Purpose**: Base class for all game modes during races
**Subclasses**:
- `SingleRaceGameMode`: Standard single race
- `ChampionShipGameMode`: Championship cup races
- `TimeTrialGameMode`: Time trial mode
- `TutorialGameMode`: Tutorial races
- `CommonRaceGameMode`: Shared logic for common race types
- `DebugGameMode`: Debug/development mode

### `RaceScoreData`
**Purpose**: Stores race results for a player
**Key Fields**: Position, time, score data

### `RaceMedalTimes`
**Purpose**: Gold/silver/bronze medal time requirements

### `E_GameState` (enum)
**Values**: Different game states (start, race, end, etc.)

---

## KART & PLAYER CLASSES

### `Kart`
**Purpose**: Main kart vehicle class
**Key Components**:
- Physics, animation, sound, FX
- Controls acceleration, braking, steering

### `KartHumanController`
**Purpose**: Human player input handler for karts
**Usage**: Hook here to intercept/modify player controls

### `KartArcadeGearBox`
**Purpose**: Arcade-style transmission system

### `KartKinematicPhysic`
**Purpose**: Kart physics simulation

### `KartAnim`
**Purpose**: Kart animation controller

### `KartSound`
**Purpose**: Kart sound effects management

### `KartCarac`
**Purpose**: Kart characteristics/stats

### `KartCustom`
**Purpose**: Kart customization data

### `KartBonus`
**Purpose**: Power-up/item handling for karts

### `KartBonusMgr`
**Purpose**: Manages bonus items for karts

---

## PLAYER MANAGEMENT

### `Player`
**Purpose**: Represents a player in the game

### `PlayerData`
**Purpose**: Persistent player data/stats

### `PlayerCustom`
**Purpose**: Player customization options

### `PlayerBuilder`
**Purpose**: Constructs player entities

### `PlayerCarac`
**Purpose**: Player characteristics

### `PlayerRank`
**Purpose**: Player's current race position

### `PlayerRaceProgress`
**Purpose**: Tracks player progress through race (laps, checkpoints)

### `PlayerConfig`
**Purpose**: Player configuration settings

### `PlayerSelection`
**Purpose**: Handles player selection in menus

### `PlayerId`
**Purpose**: Unique player identifier

---

## POSITION & TIME TRACKING

### `Position`
**Purpose**: Represents a race position (1st, 2nd, etc.)

### `RaceTime`
**Purpose**: Tracks race timing

### `Timer`
**Purpose**: Generic timer functionality

### `TimerDisplay`
**Purpose**: UI display for timers

---

## ITEMS & POWER-UPS

### `RaceItem` (abstract)
**Purpose**: Base class for collectible race items
**Subclasses**:
- `RaceCoin`: Collectible coins
- `RacePuzzlePiece`: Puzzle pieces to collect

### `ITEM` (enum)
**Purpose**: Enum of all available items/power-ups

### `BonusEntity`
**Purpose**: Individual bonus item entity

### `BonusMgr`
**Purpose**: Manages all bonus items in race

### `ItemChance`
**Purpose**: Probability distribution for item drops

### `IBonusItem` (interface)
**Purpose**: Interface for bonus item behaviors

---

## GAME MANAGERS

### `GameManager`
**Purpose**: Main game state manager

### `Aube.GameManager`
**Purpose**: Core framework game manager

### `GameEntryPoint`
**Purpose**: Game initialization entry point

### `GameSaveManager` (extends `BaseSaveManager`)
**Purpose**: Handles saving/loading game data

### `GameSave`
**Purpose**: Saved game data structure

### `GameSettings`
**Purpose**: Game settings/options

### `GameOptionManager`
**Purpose**: Manages game options

### `GamePreferences`
**Purpose**: Player preferences

### `MenuManager`
**Purpose**: Menu system controller

### `LoadingManager`
**Purpose**: Handles loading screens

### `SoundManager`
**Purpose**: Audio management

### `TimeManager`
**Purpose**: Game time management

### `Aube.TimeManager`
**Purpose**: Framework time manager

### `InputManager`
**Purpose**: Input handling

### `VibrationManager`
**Purpose**: Controller vibration

---

## REWARD & PROGRESSION SYSTEM

### `RewardManager`
**Purpose**: **KEY CLASS FOR ARCHIPELAGO** - Manages unlocks and rewards

### `RewardBase` (abstract)
**Purpose**: Base class for all rewards
**Subclasses**:
- `RewardKart`: Unlock a kart
- `RewardCharacter`: Unlock a character
- `RewardCustom`: Unlock customization
- `RewardHat`: Unlock a hat
- `RewardCoins`: Award coins
- `RewardChampionShip`: Championship rewards

### `RewardConditionBase` (abstract)
**Purpose**: Conditions for earning rewards
**Subclasses**:
- `RewardConditionChallenge`
- `RewardConditionChampionShip`
- `RewardConditionCoins`
- `RewardConditionTimeTrial`

### `E_RewardType` (enum)
**Values**: Types of rewards available

### `UnlockableItemSate`
**Purpose**: Tracks which items are unlocked

### `ChallengeManager`
**Purpose**: Manages challenge system

### `RankingManager`
**Purpose**: Leaderboard management

---

## TRACK & ENVIRONMENT

### `TrackId`
**Purpose**: Track identifier

### `TrackList`
**Purpose**: List of available tracks

### `TrackSelectionButton`
**Purpose**: UI for track selection

### `TrackPresentationGameState`
**Purpose**: Track intro/presentation

### `BezierCurveManager`
**Purpose**: Manages racing line curves

### `PathPosition`
**Purpose**: Position along track path

### `MultiPathPosition`
**Purpose**: Multiple path tracking

---

## UI & MENUS

### HUD Classes
- `HUDInGame`: Main in-game HUD
- `HUDPlayerInGame`: Per-player HUD elements
- `HUDPosition`: Position display
- `HUDFinish`: Finish line UI
- `HUDEndRace`: End of race UI
- `HUDInGameHD`: HD version of in-game HUD

### Menu Classes (too many to list all)
Key ones for Archipelago:
- `MenuHDMain`: Main menu
- `MenuHDKartSelection`: Kart selection
- `MenuHDTrackSelection`: Track selection
- `MenuRewards`: Rewards screen **IMPORTANT**
- `MenuHDPause`: Pause menu
- `MenuHDOptions`: Options menu

---

## MULTIPLAYER & NETWORKING

### `PhotonPun2NetworkMgr`
**Purpose**: Photon networking integration

### `NetworkType`
**Purpose**: Network configuration types

### `MultiplayerRoomPlayer`
**Purpose**: Player in multiplayer lobby

### `MultiplayerGameListItem`
**Purpose**: Game list entry

---

## ARCHIPELAGO INTEGRATION TARGETS

### Key Classes to Patch/Hook:

1. **`RewardManager`** - Intercept unlock/reward logic
   - Hook reward granting methods
   - Redirect to Archipelago checks

2. **`GameSaveManager` / `GameSave`** - Track progression
   - Monitor what's been collected
   - Sync with Archipelago server

3. **`RaceGameState` / `InGameGameMode`** - Race completion detection
   - Detect race finish
   - Send location checks to Archipelago

4. **`RaceItem` / `RaceCoin` / `RacePuzzlePiece`** - Collectible detection
   - Hook collection events
   - Send checks for collected items

5. **`Position` / `PlayerRank`** - Track placement
   - Monitor race positions for goal completion

6. **`MenuRewards`** - Display received items
   - Show items received from Archipelago

7. **`PlayerData`** - Persistent player state
   - Store Archipelago connection info
   - Track which checks have been sent

8. **`ChallengeManager`** - Challenge completion
   - Detect challenge completion
   - Send as location checks

9. **`ChampionShipGameMode`** - Championship tracking
   - Monitor cup completion
   - Track positions in championships

10. **`TimeTrialGameMode`** - Time trial goals
    - Detect medal achievements
    - Send time-based goals

---

## COMMON PATTERNS FOR MODDING

### Using Harmony to Patch Methods

```csharp
[HarmonyPatch(typeof(RewardManager), "GiveReward")]
class RewardManager_GiveReward_Patch
{
    static bool Prefix(RewardBase reward)
    {
        // Intercept reward - check if from Archipelago
        if (ArchipelagoManager.ShouldGrantReward(reward))
        {
            return true; // Allow original method
        }
        return false; // Block original method
    }
}
```

### Detecting Race Completion

```csharp
[HarmonyPatch(typeof(RaceGameState), "OnRaceFinished")]
class RaceGameState_OnRaceFinished_Patch
{
    static void Postfix(Player player, Position position)
    {
        // Send location check to Archipelago
        ArchipelagoManager.SendLocationCheck(
            GetRaceId(), 
            player, 
            position
        );
    }
}
```

### Tracking Collectibles

```csharp
[HarmonyPatch(typeof(RaceCoin), "OnCollected")]
class RaceCoin_OnCollected_Patch
{
    static void Postfix(RaceCoin __instance, Player player)
    {
        ArchipelagoManager.SendCoinCheck(__instance.GetId());
    }
}
```

---

## Additional Notes

- Most game logic is in the global namespace
- Aube namespace contains framework/engine code
- Use Harmony patches to intercept methods without modifying original DLL
- BepInEx logger available for debugging: `Logger.LogInfo()`
- Unity's MonoBehaviour lifecycle methods (Awake, Start, Update) are commonly used

---

## Enums Reference

### Important Enums:
- `E_GameState`: Game state machine states
- `E_GameModeType`: Type of game mode
- `E_GameTypeEntity`: Entity types
- `E_RewardType`: Types of rewards
- `E_TimeTrialMedal`: Gold/Silver/Bronze medals
- `ERaceSounds`: Race sound effects
- `ITEM`: Available power-up items
- `PLAYER_TYPE`: Human/AI player types
- `BONUS_TYPE`: Bonus item types
- `BONUS_STATE`: Bonus state machine
