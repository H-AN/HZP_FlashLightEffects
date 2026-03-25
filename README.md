<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://opengraph.githubassets.com/1/swiftly-solution/swiftlys2" alt="SwiftlyS2" width="780" />
  </a>
</div>

<div align="center">
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>
</div>

<hr>

# HZP_Flashlight

基于 **SwiftlyS2** 的 CS2 手电筒插件。  
支持人类/僵尸分组光效、自定义特感僵尸光效、按键与命令双入口、以及完整生命周期清理，适合直接用于上线服。

## 作者推广

<div style="display:flex; align-items:center; gap:6px; flex-wrap:wrap;">
  <span>技术支持 / Powered by yumiai :</span>
  <a href="https://yumi.chat:3000/">
    <img src="https://yumi.chat:3000/logo.png" width="50" alt="yumiai logo">
  </a>
  <span>(AI 模型服务 / AI model provider)</span>
</div>

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)

---

## 功能概览

- 支持按 `F` 键直接切换手电筒。
- 同时注册命令 `flashlight` 与 `swiftly_flashlight_toggle`（便于 `bind` 回退方案）。
- 支持目标玩家切换：`@me` 或 `playerId`。
- 基于阵营应用不同光照配置：`Human` / `Zombie`。
- 支持 `SpecialZombies` 按僵尸职业名覆盖配置（对接 HanZombiePlague API）。
- 支持防抖时间窗口 `ToggleDebounceMs`，避免同一动作重复触发。
- 支持可见性策略：仅自己可见、或队友可见（`VisibleToTeammates`）。
- 支持机器人开关（`AllowBots`）。

## 效果说明

- 人类与僵尸可分别配置亮度、距离、颜色、阴影、FOV/锥角、挂点等参数。
- 实体使用 `light_barn`，`lightcookie` 为 `materials/effects/lightcookies/flashlight.vtex`。
- 默认挂点为 `axis_of_intent`，可在配置中改为其他骨骼挂点。

## 命令与按键

### 命令

- `flashlight`
当前发送者切换手电筒（游戏内玩家）。
- `flashlight @me`
当前发送者切换手电筒。
- `flashlight <playerId>`
指定玩家切换手电筒。
- `swiftly_flashlight_toggle`
与 `flashlight` 相同，推荐用于按键绑定回退。

### 推荐按键回退

如果你的服环境下 `F` 原始按键事件不稳定，可使用：

```cfg
bind f swiftly_flashlight_toggle
```

## 生命周期行为（当前代码行为）

- 玩家断开：移除会话状态并销毁手电筒实体。
- 玩家死亡：立刻清除手电筒实体并关闭状态。
- 玩家重生：执行清理逻辑，确保不会遗留旧灯。
- 回合开始/结束：统一清除当前激活手电状态。
- 地图卸载 / 插件卸载：清理全部状态与实体。
- 僵尸感染事件（HanZombiePlague）：被感染玩家会立即关闭手电。

## 依赖关系

- 必需：SwiftlyS2 运行环境。
- 可选：`HanZombiePlague` 共享接口（存在时自动接入）。
- 未接入 HanZombiePlague 时：阵营识别将回退到队伍号判断（T/CT）。

## 配置文件

- 路径：`configs/plugins/HZP_Flashlight/HZP_Flashlight.jsonc`
- 根节点：`HZP_FlashlightCFG`

### 全局配置

| 字段 | 类型 | 说明 |
|------|------|------|
| `Enable` | bool | 插件总开关 |
| `AllowBots` | bool | 是否允许机器人使用 |
| `ToggleDebounceMs` | int | 切换防抖毫秒 |
| `Human` | object | 人类光照配置 |
| `Zombie` | object | 僵尸光照配置 |
| `SpecialZombies` | array | 特感僵尸覆盖配置 |

### Profile 配置（Human/Zombie）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Enable` | bool | 该阵营是否启用手电 |
| `Brightness` | float | 亮度 |
| `Distance` | float | 光照距离 |
| `AttachmentDistance` | float | 视角前推距离 |
| `FovOrConeAngle` | float | 光束角/视角参数 |
| `Shadows` | bool | 是否开启阴影 |
| `Attachment` | string | 挂点名称 |
| `ColorR/G/B/A` | int | 光照颜色 |
| `VisibleToTeammates` | bool | 是否给同阵营队友可见 |

### SpecialZombies 项

- `Name`：需与僵尸职业名匹配（不区分大小写）。
- `Enable`：该覆盖项开关。
- 其余字段为可选覆盖值（如亮度、距离、颜色、可见性等）。

## 安装与使用

1. 将插件部署到服务器插件目录。
2. 确认 `configs/plugins/HZP_Flashlight/HZP_Flashlight.jsonc` 已按需配置。
3. 启动服务器并加载插件。
4. 进服按 `F` 或执行 `flashlight` 验证效果。

## 回归检查建议（上线前）

1. 连续快速按 `F`，确认防抖生效。
2. 玩家死亡后确认灯光实体被清除。
3. 回合开始/结束后确认无残留灯光。
4. 玩家断开后确认无孤儿实体。
5. 接入 HanZombiePlague 时，感染后确认手电立即关闭。
6. 分别验证 `Human`、`Zombie`、`SpecialZombies` 三类配置效果。
