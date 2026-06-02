# Kick Lucky Cube

## Game

Name: Kick Lucky Cube

Game id: kick-lucky-cube

Status: concept / overview blockout

## Concept

Roblox-style WebGL game where the player trains strength, kicks a lucky cube from the current player position down a long rarity corridor, then controls the spawned animal while escaping a wave back to the kick start point.

The animal can be sold immediately or placed in a stable to generate soft currency over time.

## Scenes

```text
Assets/Games/KickLuckyCube/Scenes/KickLuckyCubeOverview.unity
```

## Current Prototype

Implemented in the overview scene:

- prototype player placeholder near the kick area;
- shared `GameKit` hold interaction prompt for `E`;
- lucky cube hidden at Play Mode start, visible in Edit Mode for placement, and shown at the player when the kick starts;
- lucky cube flight from the player's current kick position;
- distance calculation from current strength;
- landing marker;
- rarity zone detection by corridor depth;
- animal spawn from the landed rarity pool;
- runner phase for the spawned animal;
- chasing wave visual behind the animal;
- return success state where the player carries the animal;
- fail/reset state when the wave catches the animal;
- sell pad for carried animals;
- stable placement slots for carried animals;
- stable passive soft income;
- green stable collect button;
- local wallet with soft/hard balances and PlayerPrefs-backed saves in Play Mode;
- PlayerPrefs-backed progression saves for strength, animal speed, speed upgrade level, owned tool tier, and selected tool tier;
- PlayerPrefs-backed stable slot saves for placed animals, pending soft, and capped offline income;
- HUD/status text with current strength, tool level, animal speed, predicted distance, landing result, run phase state, and economy state;
- progression stations for training strength, buying animal speed, and buying the next strength tool;
- bottom tool belt selection for owned strength tools;
- `x2` training bonus prompt that can be claimed by pressing `X` or clicking the prompt;
- working Shop card purchases for speed upgrades, strength tools, and one-shot strength boosts;
- working playtime Rewards window with soft/hard claims;
- working Lucky Wheel window with cooldown and soft/hard/strength rewards;
- Settings-driven audio mute backend and first UI localization binding for EN/RU labels;
- mobile control layer with virtual joystick and hold-interact button, hidden on desktop by default;
- third-person follow camera that starts behind the player and follows the spawned animal during the run phase;
- rebirth window and multiplier economy;
- fake online ambient plots with animated bots and pre-upgraded stable mobs.

## ProBuilder Visual Layer

The first visual pass lives in the scene under:

```text
KLC_ProBuilderVisuals
```

This group is a game-specific ProBuilder layer placed on top of the functional blockout. It is intentionally visual-only:

- no gameplay colliders are added by this layer;
- interaction triggers and gameplay components stay on the original functional objects;
- the group can be deleted and regenerated without changing the mechanic wiring.

Current ProBuilder coverage:

- launch platform trims, gate, kick arrows, and lucky cube dress-up;
- rarity gates and colored zone accents;
- river banks, bridge planks, rails, and posts;
- stable building shell, roof, fences, hay blocks, and coin markers;
- sell kiosk with counter, awning, and coin stacks;
- training rack/tool placeholders;
- stylized wave wall and foam crests;
- corridor fence details.

Current verification:

```text
KLC_ProBuilderVisuals -> 270 ProBuilderMesh objects
Visual layer colliders -> 0
```

Next visual priorities:

1. Replace rough floor labels with diegetic 3D signs.
2. Convert the main lucky cube visual into a single authored ProBuilder prefab.
3. Add zone-specific props and animal silhouettes per rarity.
4. Improve stable/sell/training areas with final scale and interaction-readable silhouettes.
5. Reduce or hide old blockout primitives only after every gameplay reference is moved to dedicated anchors/triggers.

## UI Visual Layer

The first UI visual pass lives in the overview scene under:

```text
KLC_UIBackplates
KLC_UIVisualPass
KLC_UIWindows
```

This pass uses the imported casual Roblox UI toolkit from `Assets/SharedArt/UI` and keeps the existing gameplay HUD objects alive:

- `KLC_KickHud` still receives status text from gameplay scripts;
- `KLC_EconomyHud` still receives economy text from gameplay scripts;
- `KLC_InteractionPrompt` still uses the shared `GameKitInteractionPromptView`.

Current UI coverage:

- top-left kick/status backplate;
- top-center objective strip;
- top-right soft/hard currency panel with toolkit icons;
- left-side square icon buttons for Shop, Settings, Rewards, and Rebirth;
- bottom tool belt with four visual slots;
- restyled hold-`E` interaction prompt;
- visual Shop and Settings windows under `KLC_UIWindows`;
- reusable local window open/close controller for game-specific UI panels.

The Shop and Settings side buttons are wired to visual windows:

- the windows open from the left-side `Shop` and `Settings` buttons;
- each window closes through the red close button, backdrop click, or `Escape`;
- `KLC_EventSystem` uses `InputSystemUIInputModule`;
- there is no `StandaloneInputModule` in the overview scene;
- Shop cards buy speed upgrades, strength tools, and strength boosts with soft currency;
- Settings contents are UI-backed, but the final audio mixer and localization routing are still pending.
- the current audio backend applies Music/SFX toggles to configured sources, and falls back to `AudioListener.volume` while there are no final scene audio sources;
- the first localization backend applies EN/RU text to selected UI labels from the Settings language toggle.

The Rewards side button is wired to a working playtime window:

- the window opens from the left-side `Rewards` button;
- the window closes through the red close button, backdrop click, or `Escape`;
- rewards unlock by playtime and can grant hard and soft currency;
- reward elapsed time and claimed mask are saved in PlayerPrefs during Play Mode;
- current prototype rewards reset after the configured reset window.

The Wheel side button is wired to a working Lucky Wheel window:

- the window opens from the left-side `Wheel` button;
- the window closes through the red close button, backdrop click, or `Escape`;
- a spin grants one weighted reward: soft currency, hard currency, or strength;
- the next spin time is saved in PlayerPrefs;
- the current prototype cooldown is short for testing and should be retuned for production pacing.

Settings prototype behavior:

- Music and SFX rows toggle between `ON` and `OFF`;
- Language cycles between `EN` and `RU`;
- settings state is saved to PlayerPrefs in Play Mode;
- the current scene has no final audio mixer or localization table yet, so these toggles currently define the UI contract for the later audio/localization pass.

Latest UI wiring probe:

```text
Shop window: closed -> open -> closed
Settings window: closed -> open -> closed
Settings controls: 3 clickable rows
Settings toggle check: Music OFF, SFX OFF, Language RU
Settings reset check: Music ON, SFX ON, Language EN
EventSystem: InputSystemUIInputModule, no StandaloneInputModule
```

The Rebirth side button is wired to a working window:

- the window opens from the left-side `Rebirth` button;
- the window closes through the red close button or backdrop click;
- first rebirth requires 1000 strength;
- each next rebirth requirement is 10x higher;
- rebirth resets strength, speed, and strength-tool progression;
- soft gain multiplier becomes `x2`, then `x3`, then `x4`, and so on.

## Fake Online Ambient Layer

The fake online pass lives in the overview scene under:

```text
KLC_FakeOnline
```

This layer is visual/ambient only:

- no colliders are added under `KLC_FakeOnline`;
- fake bots do not spend player currency;
- fake mobs do not generate player income;
- the layer can be deleted and regenerated without changing the real core loop.

Current fake online coverage:

- 4 neighboring player plots: `Astrozto`, `MiraKit`, `NoobPro77`, `LuckyMax`;
- 12 pre-upgraded stable mobs across Common, Uncommon, Rare, Epic, and Legendary tiers;
- animated bots driven by `KickLuckyCubeFakeOnlineBot`;
- bot activity cycle: patrol, train with a tool, kick a decorative lucky cube.

Latest Play Mode check:

```text
plots=4
bots=4
fake stable mobs=12
colliders=0
```

Default tuning:

```text
distance = 18 + strength * 0.23
minimum distance = 10
maximum distance = 132
starter strength = 120
```

Expected test values:

```text
0 strength -> Common
120 strength -> Uncommon -> Boar with current deterministic prototype picker
500 strength -> Legendary
Boar sell value -> 82 soft
Boar stable income -> 3 soft/s
```

To test manually, open `KickLuckyCubeOverview`, enter Play Mode, stand near the kick interaction area, and hold `E`.
After the cube lands, control switches to the spawned animal. Run back toward the kick start point before the wave reaches it.
After a successful return, walk to `SELL ANIMAL` to sell the carried animal, or walk to an empty stable slot to place it.
Placed animals generate pending soft every second. Walk to the green stable collect button and hold `E` to claim it.

Prototype controls:

```text
WASD / arrows: move the prototype player, and later the animal runner during the chase phase
Space: jump while controlling the prototype player
Shift: sprint while controlling the prototype player
S / down arrow: run the animal back toward the kick start point in the current blockout
E: hold interaction for kick, sell pad, stable slot, collect button, and progression stations
1-4: select owned strength tool
Right mouse drag: rotate third-person camera
Mouse wheel: zoom third-person camera
```

Camera:

- `KLC_OverviewCamera` uses `KickLuckyCubeThirdPersonCamera`;
- on scene start it snaps behind `KLC_PrototypePlayer`;
- `KLC_PrototypePlayer` uses `KickLuckyCubePlayerController` and a blocky child visual named `KLC_PlayerVisual`;
- `KLC_PlayerVisual` has `KickLuckyCubePlayer.controller` with idle, move, walk, and run clips copied from the MechanicsTestbed prototype and stored under `Assets/Games/KickLuckyCube/Animations`;
- during animal run phase it follows the current spawned animal;
- after run/fail/sell/stable flow it falls back to the prototype player target.

Location layout blockout:

- clean placement workspace is `KLC_LayoutBlockout_Workspace`;
- move only objects with names starting with `MOVE_`;
- blockout mesh objects are editable `ProBuilderMesh` objects with scale baked into geometry;
- plot allocation is grouped under `KLC_PlotAllocationSystem`;
- reusable plot source is `KLC_PlotTemplate_EditSource`;
- `Template_PlotGround` previews the plot footprint and should match the scaled `MOVE_PlotSlot_*` footprint;
- current plot template follows the reference base layout: tan room floor, center aisle, two five-slot stable rows, front boards, and green interaction pads;
- obsolete plot decorations are intentionally removed from the template: owner tint strip, player spawn pad, sell pad, and boundary walls;
- reusable root modules are grouped as `Template_FrontBoards`, `Template_CenterAisleGroup`, `Template_BackWallGroup`, and `Template_FloorExpansion`;
- `Template_BaseUpgradeBoard` is the placeholder for upward base expansion / extra floors;
- `Template_CollectAllAdRewardBoard` is the placeholder for the ad-gated collect-all reward board;
- each of the ten `Template_StableSlot_*` objects is a grouped stable module with a paired child `MobAnchor`, `CollectSpot` placeholder, and `UpgradeBoard` booster placeholder;
- `Template_FloorExpansion` contains visual-only posts, ladder, and outline beams for future upper-floor expansion;
- `MOVE_PlotSlot_01..04` define placed plot locations;
- `KLC_PlotSlot_StaticInstances` contains edit-mode plot copies placed from the reusable template on every current `MOVE_PlotSlot`;
- `KLC_PlotTemplate_EditSource` is kept inactive as the reusable edit source and should be enabled only when editing the template shape;
- runtime plot auto-allocation is currently disabled to avoid duplicate plots while static scene placement is being tuned;
- `KLC_HubKiosks_Blockout/KLC_ImmediateKiosks_LeftToRight` holds the current left-to-right hub kiosks: animal sell, style shop, speed upgrade, weights training, and leaderboard;
- sell, speed, and weights kiosks have visible stand pads; style shop uses a hold-interaction anchor without a visible stand pad;
- `KLC_HubKiosks_Blockout/KLC_FutureFeatureSpots` reserves visual-only spots for weather machine, animal exchange, epic mob shop, and rating gift stand;
- plot template, placed plot copies, and hub kiosk blockout meshes are converted to `ProBuilderMesh`; `TextMesh` labels remain regular text objects;
- `00_BaseEnvelope_DoNotMoveAsAGroup` holds the flat green grass floor and tan boundary walls;
- `02_ZoneAndRiverGuides` holds non-final zone, corridor, and river guides;
- older noisy prototype visual groups are disabled while this layout pass is being placed.

Mobile controls:

```text
Virtual joystick: move animal runner
Jump button: queue a prototype player jump
Green E button: hold current interaction through GameKitInteractionDriver.SetExternalHold
Visibility: hidden on desktop/editor by default, shown on mobile/handheld platforms or when simulation is enabled
```

## Core Loop

1. Player stands where they want the kick to start.
2. Player trains strength with a selected tool.
3. Player buys speed upgrades for the future animal runner with soft currency.
4. Player kicks the lucky cube.
5. Cube flies forward based on strength.
6. Cube passes through corridor zones and changes rarity as it reaches deeper zones.
7. Cube lands in one zone and spawns an animal from that zone's rarity pool.
8. Camera shows a wave appearing behind the animal.
9. Player controls the animal and runs back to the kick start point.
10. If the wave catches the animal, the run fails.
11. If the animal returns in time, the original player picks it up and the wave disappears.
12. Player sells the animal or places it in a stable.
13. Stable animals generate soft currency every second.
14. Player stands on the green collect button to claim stable income.
15. Player spends soft currency on animal speed and strength tools.
16. Rebirth resets progression for a permanent money multiplier.

## Rarity Corridor

Initial zone direction:

- Zone 1: Common;
- Zone 2: Uncommon;
- Zone 3: Rare;
- Zone 4: Epic;
- Zone 5: Legendary.

Each zone is separated by a river gap. The deeper the cube lands, the better the animal pool.

## Progression

Strength:

- first prototype is trained through the green `Train Strength` station;
- each tool level increases strength gained per hold;
- after each training hold, a short-lived `x2` UI prompt can grant an extra `toolStrength * 2`;
- tools can have different training animations;
- future pass should replace the station-only flow with selectable bottom-slot tools and per-tool animations.

Speed:

- first prototype is bought through the blue `Buy Speed` station;
- each bought level increases animal runner speed by the station's configured gain;
- affects the animal while escaping the wave.

Stable income:

- stable animals generate soft currency per second;
- income waits on the stable collect button until claimed.
- placed stable animals, pending income, and last save time are stored in PlayerPrefs in Play Mode;
- offline stable income is capped to avoid unbounded prototype rewards.

Tool belt:

- bottom slots select the active training tool;
- only owned tool tiers are selectable;
- keyboard numbers `1`-`4` also select tools on desktop;
- training gain uses the selected tool tier, while purchases unlock higher owned tiers.

Shop:

- Speed cards buy animal speed levels with soft currency;
- Tool cards buy the next strength tool tier with soft currency;
- Strength boost cards buy a one-shot strength increase with soft currency;
- duplicated shop cards currently point to the same first-pass purchase actions and should become distinct final catalog items later.

Lucky Wheel:

- spin is free when cooldown is ready;
- weighted rewards currently include soft, hard, and strength;
- the wheel visual rotates briefly after a successful spin;
- cooldown persists through PlayerPrefs.

Settings:

- Music and SFX toggles update `KickLuckyCubeAudioSettingsApplier`;
- Language toggle updates `KickLuckyCubeLocalizationController`;
- final work should replace the fallback listener mute with real music/SFX source groups and expand localization coverage to every user-facing text.

Rebirth:

- first rebirth requires 1000 strength;
- each next rebirth requirement is 10x higher;
- multiplier increases from x2 to x3 to x4 and so on;
- rebirth restarts character progression while increasing money gain.

## First Implementation Order

1. Overview blockout scene with readable landmarks. Done.
2. Kick line interaction and power-to-distance prototype. Done.
3. Cube flight and rarity zone detection. Done.
4. Animal spawn from rarity pools. Done.
5. Animal control and wave chase. Done.
6. Return success/fail flow. First prototype done.
7. Sell/stable placement. Done.
8. Stable income and collect button. Done.
9. Strength tools and training prompts. First station + `x2` prompt prototype done.
10. Speed upgrades. First station level prototype done.
11. Rebirth window and multiplier economy. Done.
12. Tool belt selection. First owned-tier selection prototype done.
13. Rewards window and playtime hard/soft claims. Done.
14. Shop card purchase wiring. First speed/tool/boost purchase prototype done.
15. Player progression and stable slot PlayerPrefs saves. First prototype done.
16. Settings audio/localization backend. First prototype done.
17. Lucky Wheel window and cooldown rewards. Done.
18. Mobile joystick and hold-interact layer. First prototype done.

## Core Loop Probe

Latest logic probe result:

```text
120 strength -> 45.6 m -> Uncommon -> Boar
Boar placed in stable -> collected 150 soft
Buy Speed -> animal speed 7.8
Buy Tool -> tool level 2
3x Train Strength -> 179 strength
179 strength -> 59.3 m -> Rare
```

This confirms the playable progression loop is now connected: kick, spawn, escape, place animal, collect soft, buy speed/tool, train strength, and kick into a deeper zone.

Latest training bonus probe:

```text
120 strength
Train Strength hold -> 132 strength
x2 prompt pending -> +24 strength
Claim x2 -> 156 strength
Buy Speed level 1 -> animal speed 7.8
```

Latest rebirth probe:

```text
999 strength -> rebirth blocked
1000 strength -> rebirth complete
rebirth count -> 1
soft gain -> x2
strength reset -> 120
animal speed reset -> 7.0
tool level reset -> 1
100 soft reward after rebirth -> 200 soft
next requirement -> 10000 strength
```

Latest UI/economy probe:

```text
Tool belt selected tier -> 2
Tool selection clamp after selecting tier 99 with 3 owned tiers -> 3
Rewards window -> closed/open/closed
First reward claim -> true, second claim attempt -> false
Shop soft before -> 1000
Buy Speed -> animal speed 7.8, speed level 1
Buy Tool -> tool level 2, selected tool 2
Buy Strength Boost -> strength 195
Shop soft after -> 670
EventSystem -> InputSystemUIInputModule, no StandaloneInputModule
```

Latest Play Mode smoke:

```text
selected tool -> 2
Buy Speed -> true
Buy Tool -> true
Buy Strength Boost -> true
Claim first playtime reward -> true
Place animal in stable -> true
Stable pending before collect -> 42
Stable collected -> 42
wallet after smoke before cleanup -> 400 soft / 5 hard
stats after smoke before cleanup -> 195 strength, 7.8 speed, tool tier 4
Play Mode cleanup reset wallet, stats, rewards, and stable slot prototype state
```

Latest final systems smoke:

```text
Settings mute -> true
Localization RU label -> Колесо
Localization EN label -> Wheel
Wheel spin -> first true, second false due cooldown
Wheel reward changed wallet/stats -> true
Mobile joystick input -> 0.4, -0.9
Mobile hold bridge -> true, then false after clear
```

## Latest Agent Test Pass

Date: 2026-05-31

Unity Test Framework:

```text
EditMode: passed, 1 total, 1 passed, 0 failed
PlayMode: no PlayMode tests found
```

Editor scene probes:

```text
Active scene -> KickLuckyCubeOverview
Scene dirty -> false
Required roots -> OK
Core systems -> OK
UI/economy/settings/mobile systems -> OK
Side buttons -> Shop, Settings, Rewards, Rebirth, Wheel present
Window controllers -> Shop, Settings, Rewards, Wheel
Window open/close -> closed/open/closed for all four windows
EventSystem -> InputSystemUIInputModule
StandaloneInputModule -> 0
KLC_ProBuilderVisuals -> 270 ProBuilderMesh objects, 0 colliders
KLC_FakeOnline -> 0 colliders
```

Runtime smoke:

```text
Kick distance at starter strength -> 45.6
Landing rarity -> Uncommon
Animal run starts -> true
Animal return/carry -> true
Stable placement -> true
Stable pending/collect -> 42 / 42
Stable save sample -> Boar, 0 pending after collect
Second carried animal sell -> true, +82 soft
Tool belt selected tier -> 2
Shop speed/tool/strength boost buys -> true / true / true
PlayerPrefs progression sample -> strength 195, speed level 1
Rewards claim -> first true, second false
Wheel spin -> first true, second false due cooldown
Rebirth -> true, count 1, multiplier x2
Settings mute -> true
Localization RU label -> Колесо
Mobile simulation visibility -> true then false
Mobile joystick sample -> 0.4, -0.9
Mobile hold bridge -> true then false
```

Latest camera probe:

```text
Start target -> KLC_PrototypePlayer
Start camera position -> 0.0, 4.6, -8.9
Start camera rotation -> 18.0, 0.0, 0.0
After kick active run -> true
Run target -> KLC_Runner_Boar
```

Manual test focus for the next human pass:

1. Feel of the full kick/run/carry loop with real camera movement.
2. UI readability at desktop and narrow/mobile-like aspect ratios.
3. Whether wheel/rewards/shop pacing feels too generous or too slow.
4. Whether mobile joystick and hold button placement is comfortable on a phone viewport.
5. Whether fake online bots visually distract from the real interaction areas.

## Visual Implementation Order

1. First ProBuilder visual pass over the overview blockout. Done.
2. ProBuilder-authored lucky cube prefab.
3. ProBuilder-authored zone gates/signs per rarity.
4. ProBuilder-authored stable/sell/training props.
5. Final cleanup pass: remove or hide old visual blockout primitives after gameplay anchors are separated.

## Game-Specific Rules

All unique mechanics stay in:

```text
Assets/Games/KickLuckyCube/
```

Move behavior to `Assets/GameKit` only after it becomes reusable across multiple games.
