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
- chat and profanity-filter services plus filter group `klc-chat` in the development branch;
- Remote Config fields `chat_enabled=true` and `chat_channel_id=00d3778c-7526-42ff-888a-f7357981faf3`;
- Cloud Save player data for wallet and progression;
- Economy currencies `soft` and `hard`;
- leaderboard `klc_score` (`Top Kickers`, highest/best score).
- Economy items `starter_crate`, `speed_token`, and `style_ticket`, plus regenerating `kick_energy` (100 maximum, one point per five minutes).
- seven-day sequential calendar `klc_welcome_week`, with manual claims and a repeatable weekly cycle;
- active tester campaign `klc_welcome_test` with promo code `KLC-CODEX-TEST` (one redemption per profile/account, 100 total redemptions);
- segment `klc_all_testers`, experiment `klc_social_ui` (20% audience, equal A/B split), repeating event `klc_weekend_boost_test`, and tournament `klc_weekly_score`;
- English/Russian localization collection `Kick Lucky Cube LiveOps UI`;
- draft WebGL hosting instance `Kick Lucky Cube Dev WebGL`;
- inactive test purchase `klc_starter_pack_test`, intentionally left without a payment provider or price.

The development analytics schema includes cloud-save, kick, sale, currency, rebirth, chat, friend, daily-reward, promo, and Cloud Code test events. Currency changes are aggregated for ten seconds before sending, and funnel `KLC First Kick` measures `SessionsStarted -> klc_kick_completed`.

WebGL build `codex-dev-2026-08-29` is uploaded to the staging channel at `https://hosting.cloud.godreams.io/hosting/games/6a9322d19052ffd94edc5070/?env=staging`. It is a Development build (195.2 MB, including a 163 MB uncompressed wasm), so it is suitable for integration diagnostics but must be replaced by an optimized release build before public rollout. The CDN serves the wasm with `application/wasm`; the embedded browser can take a long time to instantiate this diagnostic build.

`KickLuckyCubeMirraCloudGameplaySync` keeps PlayerPrefs as the offline fallback, restores Cloud Save after authentication, mirrors the wallet into Economy, applies Remote Config values, and submits the calculated player score. The authored leaderboard board displays Mirra results when available and retains its local preview data otherwise.

## Network Budget

Mirra's public agreement says API-request quotas can depend on the tariff, but does not publish a single numeric request limit. The client therefore uses conservative defaults that can be tuned through Remote Config:

- Remote Config, Cloud Save, and Economy inventory are loaded once after authentication, not polled every frame.
- Progress is written as one combined Cloud Save snapshot after a 10-second quiet period, no more often than once per 60 seconds, with a maximum dirty-data delay of 180 seconds.
- Economy configuration is cached for the session. Only currencies whose balances changed are written; an unchanged snapshot causes no request.
- Leaderboard refresh/submission is limited to once per 180 seconds, and an unchanged score is not submitted again.
- Failed synchronization retries after 5 seconds and doubles the delay up to 300 seconds. This also protects the service when it responds with `common.rate_limited` or HTTP 429.
- Analytics uses the SDK queue and its built-in batching rather than issuing an HTTP request for each gameplay event.
- Social presence is a login snapshot, not live tracking: nickname, XYZ position, and UTC timestamp share the combined Cloud Save write. The client loads friends once and reads at most four presence snapshots; when there are no friends, one random-profile request supplies at most four candidates. Nothing is polled afterward.
- Chat keeps its WebSocket disconnected by default. Opening it performs join, one 25-message history load, connect, and subscribe. Closing it stops the connection and SDK heartbeat; outgoing messages are capped at 180 characters and one send every three seconds.

The `LIVE` panel is also lazy. Daily Rewards loads only when its tab is opened, has a 60-second automatic refresh cooldown, and otherwise refreshes only on explicit user action. Promo redemption and friend-list operations are likewise user-triggered. Successful Daily Rewards and Promo grants reload the authoritative Economy inventory before updating the local wallet, preventing a later Cloud Save write from replacing a server grant with an older balance.

Optional Remote Config overrides are `cloud_save_debounce_seconds`, `cloud_save_max_delay_seconds`, and `network_retry_max_delay_seconds`. Client-side lower bounds remain in force so an accidental configuration cannot create request spam.

## Social Presence And Chat

Presence keys `presence_nickname`, `presence_position_x`, `presence_position_y`, `presence_position_z`, and `presence_seen_at_utc` are owner-writable and readable by other players. A snapshot older than 14 days is ignored. `KickLuckyCubeMirraGhostIdentity` keeps the Mirra profile id on every loaded ghost. A nearby non-friend can receive one friend request per session through `F` or a click. The current implementation prefers friends and uses real project profiles only as a fallback.

The production `main` branch contains the moderated template `klc-global`, deployed separately without promoting unrelated development services. Active room channel `Kick Lucky Cube Global` uses that template and has id `00d3778c-7526-42ff-888a-f7357981faf3`. The `codex-dev` Remote Config enables it. The runtime creates a visible `CHAT` window, but still makes zero chat requests and opens no WebSocket until the player opens it; closing the window disconnects it again.

As of Cloud SDK `0.2.2`, leaderboard join succeeds but score submission can return the beta backend error `PlayerId was not present in the dictionary`. The integration disables further leaderboard submissions for that session and keeps the local leaderboard fallback active. Re-test this route after a Mirra backend or SDK update.

The Friends list endpoint also returned HTTP 500 during the first `codex-dev` Play Mode test. Presence loading falls back to the SDK's capped random-profile endpoint for that session and does not retry Friends in a loop. Re-test Friends after a backend update.

## Test Matrix And Remaining Setup

Completed with one development account:

- SDK initialization, guest session restore/login, Remote Config, Cloud Save, Economy, Analytics, and leaderboard fallback;
- visible lazy Chat and LiveOps UI, presence ghosts, and rate-limited network behavior;
- Unity compilation, Play Mode smoke test, and WebGL build validation;
- development Hub configuration and deployment through the `codex-dev` branch.

Still requires either external configuration or a second account:

- validate two-player chat delivery, presence/nickname visibility, friend request/accept/remove, and friend-first ghost selection with two concurrent accounts;
- configure a real purchase provider, prices, and catalog mappings before activating `klc_starter_pack_test`;
- create authoritative Cloud Code flows. The current beta graph editor rejects its default graph until a valid path reaches a Return node;
- attach an Economy override to `klc_weekend_boost_test`; the current Hub editor exposes the target selector but no editable override value;
- upload versioned remote content to Asset Storage when its file uploader is available;
- replace the diagnostic hosting artifact with an optimized release build and verify startup in target desktop/mobile browsers before promoting the staging channel;
- promote selected schemas/configuration to production only after development verification. No development test setup should be copied to `main` implicitly.

Known beta backend issues remain unchanged: Friends GET can return HTTP 500, and leaderboard score submission can fail because the backend omits `PlayerId`. Both paths fail closed and avoid retry loops.
