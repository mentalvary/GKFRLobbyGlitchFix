# Garfield Kart Furious Racing Lobby Glitch Fix

This mod fixes the _lobby glitch_ bug in Garfield Kart Furious Racing multiplayer games.

## Installation

**Only the multiplayer host needs to install the mod.**

1. Install BepInEx into the Garfield Kart game root folder
    * Grab latest 5.x release from https://github.com/BepInEx/BepInEx/releases
    * More info: https://docs.bepinex.dev/articles/user_guide/installation/index.html
2. Download the latest `GKFRLobbyGlitchFix.dll` from the [releases](https://github.com/mentalvary/GKFRLobbyGlitchFix/releases) page.
3. Copy it into `<game root folder>/BepInEx/plugins/`

Example file structure:

```
C:\Program Files (x86)\Steam\steamapps\common\Garfield Kart - Furious Racing
├───BepInEx
│   ├───cache
│   ├───config
│   ├───core
│   ├───patchers
│   └───plugins
|       └───GKFRLobbyGlitchFix.dll
├───Garfield Kart Furious Racing_Data
└───MonoBleedingEdge
```

## Background

### What's the lobby glitch?

What some circles call the "lobby glitch" happens semi-rarely in multiplayer games. It causes you to be stuck on the race results screen, instead of going to the next track after the countdown. You can't exit the game normally, but have to force quit and restart the game.

### Why does it happen?

Garfield Kart has special timers that take care of countdowns, such as:

* `END_OF_RACE`: 20s countdown after the first player finishes, after which the race ends for everyone and the results screen shows.
* `RESULTS`: 10s countdown on the results screen, after which the game goes to the next track (or the podium scene).

Side note: the countdown **display** only activates when there are 15s left, so the 20s `END_OF_RACE` countdown doesn't show for the first 5s.

**The game can only have one such timer active**. The game ignores new timers if one is already active.

The lobby glitch happens due to a race condition (pun intended):

* Whenever a player finishes, the `END_OF_RACE` timer is triggered again. Not just for 1st place. Usually, it doesn't actually start, because the `END_OF_RACE` timer from 1st place is already active.
* After the `END_OF_RACE` timer finishes, it moves to the results screen and triggers the `RESULTS` timer.
* If a player finishes exactly when the `END_OF_RACE` timer finishes, but before the `RESULTS` timer was started, it will trigger another `END_OF_RACE` timer (which now starts since no other timer is active).
* This second `END_OF_RACE` timer now blocks the `RESULTS` timer from starting, so the results screen never ends.

You can also observe this behavior in the game: the countdown on the results screen starts at 15s instead of 10s (because it's actually the 20s `END_OF_RACE` countdown again, the display only kicking in at 15s).

### How does this mod fix it?

To fix this, we prevent the game from starting more than one `END_OF_RACE` timer per race. It only triggers for 1st place.
