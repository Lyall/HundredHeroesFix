# Eiyuden Chronicle: Hundred Heroes Fix
[](https://www.patreon.com/Wintermance) [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W01UAI9)<br />
[![Github All Releases](https://img.shields.io/github/downloads/Lyall/HundredHeroesFix/total.svg)](https://github.com/Lyall/HundredHeroesFix/releases)

This is a BepInEx plugin for Eiyuden Chronicle: Hundred Heroes that adds custom resolutions, ultrawide support and more.<br />

## Features
- Custom resolution support.
- Ultrawide support.
- Intro skip.
- Graphical tweaks.
  Render scale, shadow resolution and distance.

## Installation
- Grab the latest release of HundredHeroesFix from [here.](https://github.com/Lyall/HundredHeroesFix/releases)
- Extract the contents of the release zip in to the the game folder.<br />(e.g. "**steamapps\common\Eiyuden Chronicle**" for Steam).

### Steam Deck/Linux Additional Instructions
🚩**You do not need to do this if you are using Windows!**
- Open up the game properties in Steam and add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the launch options.

## Configuration
- See **HundredHeroesFix.ini** to adjust settings for the fix.

## Known Issues
Please report any issues you see.
This list will contain bugs which may or may not be fixed.

- When using an ultrawide display, the HUD is a mixture of spanned and 16:9.

## Screenshots

| |
|:--:|
| Gameplay |

## Credits
[Ultimate ASI Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader) for ASI loading. <br />
[inipp](https://github.com/mcmtroffaes/inipp) for ini reading. <br />
[spdlog](https://github.com/gabime/spdlog) for logging. <br />
[safetyhook](https://github.com/cursey/safetyhook) for hooking.
