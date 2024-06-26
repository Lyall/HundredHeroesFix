﻿# Eiyuden Chronicle: Hundred Heroes Fix
[![Patreon-Button](https://github.com/Lyall/HundredHeroesFix/assets/695941/940fe808-0708-4284-bd4c-40c55573df20)](https://www.patreon.com/Wintermance) [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W01UAI9)<br />
[![Github All Releases](https://img.shields.io/github/downloads/Lyall/HundredHeroesFix/total.svg)](https://github.com/Lyall/HundredHeroesFix/releases)

This is a BepInEx plugin for Eiyuden Chronicle: Hundred Heroes that adds custom resolutions, ultrawide/narrower support and more.<br />

## Features
- Custom resolution support.
- Ultrawide and narrower aspect ratio support.
- Intro skip.
- Hide mouse cursor.
- Graphical tweaks like the ability to change render scale, shadow resolution/distance and vignette/CA toggles.
- Dialog tweaks (removing delay on voice lines, text speed etc).
- Battle tweaks (auto-battle and manual battle speed options).
- Force specific controller button icons (DS4/DS5/Xbox).

## Installation
- Grab the latest release of HundredHeroesFix from [here.](https://github.com/Lyall/HundredHeroesFix/releases)
- Extract the contents of the release zip in to the the game folder. (e.g. "**steamapps\common\Eiyuden Chronicle**" for Steam).
- 🚩First boot of the game may take a few minutes as BepInEx generates an assembly cache!

### Steam Deck/Linux Additional Instructions
🚩**You do not need to do this if you are using Windows!**
- Open up the game properties in Steam and add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the launch options.

## Configuration
- See **`GameFolder`\BepInEx\config\HundredHeroesFix.cfg** to adjust settings for the fix.

## Known Issues
Please report any issues you see.
This list will contain bugs which may or may not be fixed.

- When using an ultrawide display, the HUD is a mixture of spanned and 16:9.
- Some screens may be mis-aligned or zoomed in when using an ultrawide/narrower display.

## Screenshots

| ![ezgif-7-f21ac54832](https://github.com/Lyall/HundredHeroesFix/assets/695941/3ab8ef7b-fd8b-4ae4-858d-fce5b00a21f2) |
|:--:|
| Gameplay |

## Credits
[BepinEx](https://github.com/BepInEx/BepInEx) for plugin loading.
