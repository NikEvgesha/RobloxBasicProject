# Kick Lucky Cube Verification

Last targeted Play Mode regression: 2026-08-02, Unity 6000.3.9f1, `KickLuckyCubeOverview`.

> This report records isolated technical probes from 2026-08-01. It is not a production acceptance certificate. The 2026-08-02 manual product review found issues that the probes did not reproduce, including grounding, reveal/chase placement, camera direction, negative currency, and feature regressions. Current task status and acceptance criteria live in `Docs/Games/KickLuckyCubeProductionBacklog.md` and take precedence over this historical result.

## 2026-08-02 Agent-Owned Production Fixes

Implemented and checked in Play Mode:

- player root now starts at the resolved floor and follows the actual surface instead of retaining an arbitrary startup Y;
- cube landing, roulette/reveal animal, controlled runner, and returned player share one filtered ground resolver;
- runner is snapped before the wave introduction and again before control starts;
- wallet soft/hard values use invariant string-backed `long` keys, migrate old integer keys, repair negative data immediately, and use saturating additions;
- the observed `-1611036414` legacy soft balance was repaired to `0` on load and persisted in the new format;
- insufficient, exact-price, repeated-spend, and `long.MaxValue` saturation probes passed;
- Sell Shop and Style Shop resolve the gameplay canvas and open/close correctly; sale and style persistence continue through their existing save paths;
- kick-strength settings use an editable prefab, persist selected/max mode, clamp to current strength, and feed both prediction and actual kick distance;
- leaderboard binds authored scene labels (or its prefab), sorts local/fake scores, and no longer constructs fallback text objects;
- all 22 catalog entries resolve a model, icon, and non-null Animator controller; Normal/Golden/Diamond/Fire inventory round-trips preserve canonical identity;
- exchange results now retain the selected catalog entry's canonical name, rarity, visual/icon path, sell value, and income instead of inventing a `Trade` identity under an existing catalog id;
- allocated bot plots instantiate three active `KLC_FakeOnlineBotActor` prefab actors in the current four-plot allocation (`3/3` active), independent of disabled legacy preview roots;
- grade VFX and roulette hierarchy are prefab-backed; particle velocity X/Y/Z modes match and the previous curve-mode exception was not reproduced.

Targeted result:

```text
Ground start -> player root Y 0.000, floor Y 0.000, grounded true
Mechanic probe -> economy true, sell true, style true, leaderboard true, strength true
Catalog validator -> 22 entries, 0 errors
Bot allocator -> 4 total plots, 3 runtime bot actors, 3 active, pass true
Animal/VFX prefab probe -> Fire VFX present, particle curves compatible, roulette prefab contract valid
Unity Console compilation errors -> 0
```

The runtime-visual source audit reports `25` classified construction sites (`21` manager-only, `4` prefab-backed data instances), `0` visual migration sites, and `0` unclassified sites. `KLC-PROD-008` is complete; linked-prefab conversion of authored corridor, kiosk, and plot scene content remains tracked separately.

The final prefab-contract probe opened and closed Album, Sell, Style, Speed, Strength Tool, Kick Strength, Rebirth, Inventory, Weather, Exchange, and Epic Mob windows. It found `23` inventory slot views, `4` allocated plots, `3` active fake-online actors, non-negative wallet values, and no Console errors or exceptions.

Unity Test Framework result: EditMode `1/1` passed. No PlayMode tests are currently discovered; Play Mode coverage in this pass comes from the explicit runtime probes above and does not replace a human camera/input route.

## Verified Flow

- desktop/mobile-vector player movement, third-person camera target, zoom-ready camera distance, and player jump;
- instant and regular kick result path, rarity-zone resolution, animal roulette, wave start, failure/catch path, and successful return path;
- camera transfer player -> animal runner -> player and camera-relative runner movement;
- inventory selection, stable placement, visible stable animal, pending-income collection, infinite stable upgrade cost growth, take-to-inventory, and sale;
- stable and inventory persistence through a real Stop Play / Start Play cycle, including real-time pending income;
- speed purchases for `+1`, `+5`, and `+10` levels, strength training ticks, x2 training prompt, and rebirth while retaining speed/tools;
- settings, inventory, album, speed shop, tool shop, sell shop, style shop, rewards, and wheel open/close paths;
- playtime reward claim and duplicate-claim rejection;
- weather activation and rarity boost, exchange, elite mob purchase, rating gift one-time claim, VIP fallback purchase, normal offline income, and x2 offline income;
- 22/22 animal catalog entries resolve imported visuals, icons, renderers, and animator/procedural motion;
- mobile controls are hidden on desktop, visible in simulation, and hidden again after simulation is disabled;
- leaderboard runtime text exists and refreshes; all six SFX event types can be dispatched without exceptions.

The test temporarily replaced gameplay `PlayerPrefs`, then restored all 85 original values and deleted every key created by the verification and regression passes.

## Fixed By This Pass

- animal runner now consumes the mobile jump queue;
- Lucky Wheel cooldown uses UTC time instead of `Time.unscaledTime`, including cleanup of the obsolete key;
- runtime TMP creation removes a legacy `Text` before adding `TextMeshProUGUI`;
- Sell Shop reuses an existing `RectMask2D` instead of adding a duplicate.

## Remaining Finding

The old disabled `KLC_FakePlot_*` preview roots are still present for a human scene-cleanup pass, but active fake-online actors no longer depend on them. The strict collider matrix also reports visual-only animated limbs and several biome decorations without colliders; gameplay roots remained grounded. Remaining product work is tracked in `KickLuckyCubeProductionBacklog.md`, including wave camera choreography and authored scene-prefab conversion.

## Regression Checklist

1. Enter Play Mode with Console cleared and verify no UI component-add warnings.
2. Kick once at a short distance and verify the animal runner can jump from both `Space` and the mobile jump button.
3. Spin the wheel, stop Play Mode, start again, and verify remaining cooldown is at most the configured cooldown.
4. Open Album and Sell Shop and verify labels/masks render without duplicate component messages.
5. Place an animal, restart Play Mode, and verify animal identity, size, animation, pending income, and upgrade level.
