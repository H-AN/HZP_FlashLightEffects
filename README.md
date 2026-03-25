# HZP_Flashlight

`HZP_Flashlight` is a SwiftlyS2 plugin that lets players toggle a flashlight-like dynamic light.

## What it does

- Press `F` to toggle the flashlight on and off.
- The plugin also registers the commands `flashlight` and `swiftly_flashlight_toggle`.
- If your server environment does not report the raw `F` key correctly, you can bind the command fallback instead.

## Recommended bind fallback

Use this only if raw `F` does not work on your server:

```cfg
bind f swiftly_flashlight_toggle
```

The plugin has a debounce window, so if both the raw `F` event and the command fire close together, the second toggle is ignored.

## Config

Config file: `src/HZP_Flashlight.jsonc`

Available settings:

- `Enable`
- `Brightness`
- `Distance`
- `AttachmentDistance`
- `FovOrConeAngle`
- `Shadows`
- `Attachment`
- `ColorR`
- `ColorG`
- `ColorB`
- `ColorA`
- `AllowBots`
- `ToggleDebounceMs`
- `VisibleToOthers`

## Commands

- `flashlight`
  Toggle your own flashlight in-game.
- `flashlight <playerId>`
  Toggle another player by numeric player ID.
- `swiftly_flashlight_toggle`
  Alias intended for `bind f ...` fallback usage.

## Lifecycle behavior

- Disconnect: tracked state and entity are removed.
- Death: active light entity is removed immediately.
- Respawn: if the player had the flashlight enabled before death, the light is restored after spawn.
- Map unload / plugin unload: all runtime entities and tracked state are cleared.

## Light entity notes

The plugin now creates a dedicated `light_barn` entity, applies the same core barn-light parameters used by `CS2Fixes`, and dispatches spawn with the `lightcookie` value set to `materials/effects/lightcookies/flashlight.vtex`.

By default the light attaches to `axis_of_intent`. If your player model does not expose that attachment correctly, set `Attachment` to `clip_limit` in the config and retest.

## Suggested verification

1. Build the project.
2. Load the plugin.
3. Press `F` once and confirm the light appears.
4. Press `F` again and confirm the light disappears.
5. Hold `F` and confirm it does not spam-toggle.
6. Die while the light is enabled and confirm there is no orphaned light.
7. Respawn and confirm the light restores only if it was enabled before death.
8. If raw `F` does not work, use `bind f swiftly_flashlight_toggle` and retest.
