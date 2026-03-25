<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" alt="SwiftlyS2" width="780" />
  </a>
</div>

<div align="center">
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>
</div>

<hr>

# HZP_Flashlight

A **SwiftlyS2** plugin for CS2 flashlight effects.  
It supports separate human/zombie lighting profiles, special-zombie overrides, key + command dual entry, and production-ready lifecycle cleanup.

---

## Credit : 

<div style="display:flex; align-items:center; gap:6px; flex-wrap:wrap;">
  <span>Powered by yumiai :</span>
  <a href="https://yumi.chat:3000/">
    <img src="https://yumi.chat:3000/logo.png" width="50" alt="yumiai logo">
  </a>
  <span>(AI model provider)</span>
</div>

<div style="display:flex; align-items:center; gap:6px; flex-wrap:wrap;">
  <span>SwiftlyS2-Toolkit & agents By laoshi :</span>
  <a href="https://github.com/2oaJ">
    <img src="https://github.com/user-attachments/assets/2da5deb4-2be9-4269-8f8e-df0029bb7c91" width="50" alt="yumiai logo">
  </a>
  <span>(swiftlys2 Skills & agents)</span>
</div>

<div style="display:flex; align-items:center; gap:6px; flex-wrap:wrap;">
  <span>SwiftlyS2-mdwiki By LynchMus :</span>
  <a href="https://github.com/himenekocn/sw2-mdwiki">
    <img src="https://github.com/user-attachments/assets/c7f3b4ca-629a-4df9-a405-3f1a7507ecf2" width="50" alt="yumiai logo">
  </a>
  <span>(swiftlys2 mdwiki)</span>
</div>

---

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)

---

## Feature Overview

- Press `F` to toggle flashlight.
- Also registers `flashlight` and `swiftly_flashlight_toggle` for command/bind fallback.
- Supports target resolution via `@me` or `playerId`.
- Applies separate profiles for `Human` and `Zombie`.
- Supports `SpecialZombies` profile override by zombie class name (HanZombiePlague API).
- Has debounce window via `ToggleDebounceMs` to prevent duplicate toggles.
- Supports visibility policy via `VisibleToTeammates`.
- Supports bot control via `AllowBots`.

## Visual/Light Behavior

- Human and zombie profiles can independently configure brightness, distance, color, shadows, cone/FOV, and attachment.
- Uses `light_barn` with `lightcookie = materials/effects/lightcookies/flashlight.vtex`.
- Default attachment is `axis_of_intent`, configurable per profile.

## Commands and Key Binding

### Commands

- `flashlight`
Toggle flashlight for the command sender (in-game player).
- `flashlight @me`
Toggle flashlight for self.
- `flashlight <playerId>`
Toggle flashlight for a specific player.
- `swiftly_flashlight_toggle`
Alias command, mainly for bind fallback.

### Recommended bind fallback

If raw `F` key events are unreliable on your server, use:

```cfg
bind f swiftly_flashlight_toggle
```

## Lifecycle Behavior (Current Code Behavior)

- Disconnect: removes session state and destroys light entity.
- Death: immediately clears light entity and disables state.
- Spawn: runs cleanup logic to avoid stale lights.
- Round start/end: clears all active flashlight states.
- Map unload / plugin unload: clears all runtime state and entities.
- Infection event (HanZombiePlague): instantly disables the infected player's flashlight.

## Dependencies

- Required: SwiftlyS2 runtime.
- Optional: `HanZombiePlague` shared interface (auto-attached when available).
- Fallback without HanZombiePlague: faction is resolved by team number (T/CT).

## Configuration

- File: `configs/plugins/HZP_Flashlight/HZP_Flashlight.jsonc`
- Root section: `HZP_FlashlightCFG`

### Global fields

| Field | Type | Description |
|------|------|------|
| `Enable` | bool | Global plugin switch |
| `AllowBots` | bool | Whether bots are allowed |
| `ToggleDebounceMs` | int | Toggle debounce in milliseconds |
| `Human` | object | Human profile |
| `Zombie` | object | Zombie profile |
| `SpecialZombies` | array | Special zombie override list |

### Profile fields (Human/Zombie)

| Field | Type | Description |
|------|------|------|
| `Enable` | bool | Enable flashlight for this profile |
| `Brightness` | float | Brightness |
| `Distance` | float | Light range |
| `AttachmentDistance` | float | Forward offset from eye direction |
| `FovOrConeAngle` | float | Cone/FOV value |
| `Shadows` | bool | Shadow switch |
| `Attachment` | string | Attachment name |
| `ColorR/G/B/A` | int | Light color |
| `VisibleToTeammates` | bool | Visible to same-faction teammates |

### `SpecialZombies` item

- `Name`: zombie class name to match (case-insensitive).
- `Enable`: switch for this override item.
- Other fields are optional overrides (brightness, distance, color, visibility, and so on).

## Installation

1. Deploy plugin files to your server plugin directory.
2. Configure `configs/plugins/HZP_Flashlight/HZP_Flashlight.jsonc`.
3. Start/restart server and load plugin.
4. Validate behavior via `F` key or `flashlight` command.

## Pre-release Validation Checklist

1. Press `F` rapidly to verify debounce behavior.
2. Confirm no stale light remains after player death.
3. Confirm round start/end cleanup works.
4. Confirm no orphan entities after player disconnect.
5. With HanZombiePlague enabled, confirm infected players lose flashlight instantly.
6. Verify profile outputs for `Human`, `Zombie`, and `SpecialZombies`.
