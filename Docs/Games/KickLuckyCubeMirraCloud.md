# Kick Lucky Cube: Mirra Cloud Integration

## Current State

The project uses two different Mirra packages with different responsibilities:

- `com.romanlee17.mirrasdk5` `5.1.20` is the existing cross-platform provider layer for ads, platform payments, device services, language, time, and legacy data adapters under `Assets/Igrodelnya`.
- `com.mirrahub.cloud-sdk` `0.2.2` is the new Mirra Hub backend SDK for accounts, cloud data, server economy, LiveOps, social features, competitive features, analytics, Cloud Code, asset delivery, and WebGL hosting.

Do not remove the old package until every active platform-provider use has been audited. The Cloud SDK does not currently replace the project's advertising providers.

`KickLuckyCubeRuntimeBootstrap` creates one persistent `KickLuckyCubeMirraCloudService`. The service does nothing until a valid Mirra Cloud configuration exists. Once configured, it initializes one SDK instance, restores a previous player session, and falls back to guest login.

## Local Configuration

1. In Mirra Hub, create a development scope and a development branch. Do not point the first integration pass at production player data.
2. Create a service account with access to the project, then create its one-time key.
3. In Unity, open `Tools > Mirra Cloud > Manager`, connect with the service-account key, and select the project, development branch, and project API token.
4. Restart Play Mode and confirm `Kick Lucky Cube: Mirra Cloud player session is ready.` in the Console.

The generated `Assets/MirraCloud/Resources/Configuration.asset` contains the project token and is intentionally ignored by Git. Never commit service-account keys or API tokens.

## Recommended Rollout

### Phase 1: Foundation And Observability

- Keep local PlayerPrefs as the fallback while a versioned cloud snapshot is introduced.
- Register analytics events before emitting them; Mirra Cloud discards unregistered event names.
- Start with `session_start`, `tutorial_step`, `kick_started`, `kick_completed`, `animal_obtained`, `animal_sold`, `shop_opened`, `upgrade_purchased`, `rebirth`, `reward_claimed`, and `wave_failed`.
- Build onboarding, first-kick, first-animal, first-sale, and first-upgrade funnels.

### Phase 2: Progress And Economy

- Store a versioned player snapshot containing wallet, stats, inventory, stable slots, owned cosmetics, discovered animals, rewards, wheel cooldown, rebirths, and settings.
- Migrate once from PlayerPrefs after guest authentication; keep the local snapshot as an offline cache, not an independent authority.
- Move soft/hard currency, owned tools/styles/animals, VIP state, and reward grants to Mirra Economy and Cloud Code so client-side edits cannot mint value.
- Use Cloud Code for kick rewards, sales, upgrades, rebirth, wheel results, daily rewards, and promotional grants.

### Phase 3: LiveOps

- Put balance multipliers, prices, cooldowns, biome unlock thresholds, rarity weights, wave tuning, and offer visibility into Remote Config.
- Use segments for new, returning, payer, high-progression, churn-risk, and tester cohorts.
- Run A/B tests on tutorial length, first upgrade price, reward cadence, and power-meter timing.
- Replace the prototype playtime rewards with server daily rewards and use promo codes for tester compensation.

### Phase 4: Competitive And Social

- Back the existing leaderboard UI with Mirra leaderboards: furthest biome, strongest kick, animal collection, wealth, and seasonal score.
- Add time-limited challenges and tournaments after score submission is server-validated.
- Add friends, player profiles, groups, group chat, and profanity filtering only after moderation UX and privacy rules are ready.

### Phase 5: Content And Delivery

- Use Assets Storage for remotely updated event art, icons, audio, and lightweight configuration-linked content.
- Use Mirra game hosting for staging and production WebGL builds. It is static build hosting, not a multiplayer game-server service.
- The embedded WebView requires the SDK's custom WebGL template; platform-native purchases remain the fallback where embedded payment flow is unsupported.

## Safety Rules

- Use a separate development scope for test players and test transactions.
- Treat branch drafts as live for every build routed to that branch; a draft is not a private sandbox.
- Pin the SDK tag and review the changelog before updating because the public API is still `0.x`.
- Never trust WebView messages or client-side calculations for rewards, currency, scores, or purchases.
- Do not install separate `net.gree.unity-webview` or `com.gilzoide.sqlite-net` packages; both are vendored in the Cloud SDK.
- Do not upload raw PlayerPrefs blindly. Define a versioned schema and conflict policy first.

## Development Hub Configuration

The Mirra Hub project `BlockKick` uses the isolated branch `codex-dev` and runtime scope `codex-development`. The development branch currently contains:

- guest authentication;
- the `Unity Editor Development` analytics platform;
- Remote Config fields `cloud_sync_enabled`, `cloud_save_interval_seconds`, `leaderboard_submit_interval_seconds`, `analytics_enabled`, and `soft_gain_multiplier`;
- Cloud Save player data for wallet and progression;
- Economy currencies `soft` and `hard`;
- leaderboard `klc_score` (`Top Kickers`, highest/best score).

`KickLuckyCubeMirraCloudGameplaySync` keeps PlayerPrefs as the offline fallback, restores Cloud Save after authentication, mirrors the wallet into Economy, applies Remote Config values, and submits the calculated player score. The authored leaderboard board displays Mirra results when available and retains its local preview data otherwise.

As of Cloud SDK `0.2.2`, leaderboard join succeeds but score submission can return the beta backend error `PlayerId was not present in the dictionary`. The integration disables further leaderboard submissions for that session and keeps the local leaderboard fallback active. Re-test this route after a Mirra backend or SDK update.

## Remaining External Setup

- Register production analytics events and their parameter schemas.
- Add Cloud Code scripts for authoritative rewards, purchases, upgrades, and rebirths.
- Add inventory item definitions, daily rewards, promo codes, and LiveOps events.
- Configure purchase-provider credentials and catalog mappings.
- Create hosting instances and staging/production routing.
