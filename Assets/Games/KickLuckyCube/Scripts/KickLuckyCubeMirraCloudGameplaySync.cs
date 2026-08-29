using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudSave.Requests;
using MirraCloud.Core.Friends.Dto;
using MirraCloud.Core.Leaderboard.Dto;
using Plugins.MirraCloud.Core.Services.PlayerAccount.Dto;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-9800)]
    public sealed class KickLuckyCubeMirraCloudGameplaySync : MonoBehaviour
    {
        private const string SoftKey = "wallet_soft";
        private const string HardKey = "wallet_hard";
        private const string StrengthKey = "strength";
        private const string AnimalSpeedKey = "animal_speed";
        private const string ToolTierKey = "strength_tool_tier";
        private const string SelectedToolTierKey = "selected_strength_tool_tier";
        private const string SpeedLevelKey = "speed_upgrade_level";
        private const string SaveVersionKey = "save_version";
        private const string LeaderboardKey = "klc_score";
        private const string PresenceNicknameKey = "presence_nickname";
        private const string PresencePositionXKey = "presence_position_x";
        private const string PresencePositionYKey = "presence_position_y";
        private const string PresencePositionZKey = "presence_position_z";
        private const string PresenceSeenAtKey = "presence_seen_at_utc";

        [SerializeField, Min(30f)] private float defaultCloudSaveIntervalSeconds = 60f;
        [SerializeField, Min(1f)] private float defaultCloudSaveDebounceSeconds = 10f;
        [SerializeField, Min(60f)] private float defaultCloudSaveMaxDelaySeconds = 180f;
        [SerializeField, Min(60f)] private float defaultLeaderboardIntervalSeconds = 180f;
        [SerializeField, Min(30f)] private float defaultMaximumRetryDelaySeconds = 300f;
        [SerializeField, Range(0, 8)] private int maximumLoginGhosts = 4;
        [SerializeField, Min(1)] private int maximumPresenceAgeDays = 14;
        [SerializeField] private string ghostActorResourcePath = "KickLuckyCube/World/KLC_FakeOnlineBotActor";
        [SerializeField] private bool logCloudOperations = true;

        private KickLuckyCubeMirraCloudService cloud;
        private KickLuckyCubeWallet wallet;
        private KickLuckyCubePlayerStats stats;
        private KickLuckyCubeInventoryController inventory;
        private float cloudSaveIntervalSeconds;
        private float cloudSaveDebounceSeconds;
        private float cloudSaveMaxDelaySeconds;
        private float leaderboardIntervalSeconds;
        private float maximumRetryDelaySeconds;
        private float lastLocalChangeAt;
        private float lastSuccessfulSyncAt;
        private float nextSyncAttemptAt;
        private float retryDelaySeconds = 5f;
        private bool cloudSyncEnabled = true;
        private bool analyticsEnabled = true;
        private bool localStateDirty = true;
        private bool applyingCloudState;
        private bool leaderboardDisabledForSession;
        private bool economyReady;
        private long lastEconomySoft = long.MinValue;
        private long lastEconomyHard = long.MinValue;
        private bool lastSaveSucceeded;
        private bool lastEconomySyncSucceeded;
        private bool leaderboardInitialized;
        private bool leaderboardJoined;
        private double lastSubmittedScore = double.NaN;
        private string lastCloudSnapshotSignature;
        private Vector3 loginPresencePosition;
        private string loginPresenceNickname;
        private string loginPresenceSeenAtUtc;
        private bool loginPresenceCaptured;

        public static KickLuckyCubeMirraCloudGameplaySync Instance { get; private set; }
        public bool IsOperational { get; private set; }
        public IReadOnlyList<LeaderboardEntryDto> TopEntries { get; private set; } = Array.Empty<LeaderboardEntryDto>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            cloudSaveIntervalSeconds = defaultCloudSaveIntervalSeconds;
            cloudSaveDebounceSeconds = defaultCloudSaveDebounceSeconds;
            cloudSaveMaxDelaySeconds = defaultCloudSaveMaxDelaySeconds;
            leaderboardIntervalSeconds = defaultLeaderboardIntervalSeconds;
            maximumRetryDelaySeconds = defaultMaximumRetryDelaySeconds;
            lastLocalChangeAt = Time.realtimeSinceStartup;
            ResolveReferences();
        }

        private IEnumerator Start()
        {
            var timeoutAt = Time.realtimeSinceStartup + 30f;
            while ((cloud == null || !cloud.IsAuthenticated) && Time.realtimeSinceStartup < timeoutAt)
            {
                ResolveReferences();
                yield return null;
            }

            if (cloud == null || !cloud.IsAuthenticated || wallet == null || stats == null)
            {
                yield break;
            }

            wallet.Changed += HandleLocalStateChanged;
            stats.Changed += HandleLocalStateChanged;

            yield return LoadRemoteConfig();
            if (cloudSyncEnabled)
            {
                yield return RestoreOrCreateCloudSave();
                CaptureLoginPresence();
                yield return InitializeEconomy();
                yield return SynchronizePendingState();
                yield return LoadLoginGhosts();
            }

            yield return RefreshLeaderboard(true);
            IsOperational = true;
            Log("gameplay services are operational");

            lastSuccessfulSyncAt = Time.realtimeSinceStartup;
            nextSyncAttemptAt = lastSuccessfulSyncAt;
            var nextLeaderboardAt = Time.realtimeSinceStartup + leaderboardIntervalSeconds;
            while (enabled)
            {
                yield return new WaitForSecondsRealtime(1f);
                var now = Time.realtimeSinceStartup;

                var debounceElapsed = now - lastLocalChangeAt >= cloudSaveDebounceSeconds;
                var maximumDelayElapsed = now - lastSuccessfulSyncAt >= cloudSaveMaxDelaySeconds;
                if (cloudSyncEnabled && localStateDirty && now >= nextSyncAttemptAt && (debounceElapsed || maximumDelayElapsed))
                {
                    yield return SynchronizePendingState();
                }

                if (now >= nextLeaderboardAt)
                {
                    nextLeaderboardAt = now + leaderboardIntervalSeconds;
                    yield return RefreshLeaderboard(true);
                }
            }
        }

        private void OnDestroy()
        {
            if (wallet != null)
            {
                wallet.Changed -= HandleLocalStateChanged;
            }

            if (stats != null)
            {
                stats.Changed -= HandleLocalStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveReferences()
        {
            cloud ??= KickLuckyCubeMirraCloudService.Instance;
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
        }

        private IEnumerator LoadRemoteConfig()
        {
            var operation = cloud.Sdk.RemoteConfig.LoadConfigAsync();
            yield return operation;
            if (!operation.Result.IsSuccess || cloud.Sdk.RemoteConfig.Config == null)
            {
                LogWarning("Remote Config", operation.Result.Error?.Message);
                yield break;
            }

            var fields = cloud.Sdk.RemoteConfig.Config.Fields;
            cloudSyncEnabled = GetBool(fields, "cloud_sync_enabled", true);
            analyticsEnabled = GetBool(fields, "analytics_enabled", true);
            cloudSaveIntervalSeconds = Mathf.Max(30f, GetFloat(fields, "cloud_save_interval_seconds", defaultCloudSaveIntervalSeconds));
            cloudSaveDebounceSeconds = Mathf.Max(1f, GetFloat(fields, "cloud_save_debounce_seconds", defaultCloudSaveDebounceSeconds));
            cloudSaveMaxDelaySeconds = Mathf.Max(60f, GetFloat(fields, "cloud_save_max_delay_seconds", defaultCloudSaveMaxDelaySeconds));
            leaderboardIntervalSeconds = Mathf.Max(60f, GetFloat(fields, "leaderboard_submit_interval_seconds", defaultLeaderboardIntervalSeconds));
            maximumRetryDelaySeconds = Mathf.Max(30f, GetFloat(fields, "network_retry_max_delay_seconds", defaultMaximumRetryDelaySeconds));
            wallet.SetSoftGainBonusMultiplier(Mathf.Max(1f, GetFloat(fields, "soft_gain_multiplier", 1f)));
            var chat = GetComponent<KickLuckyCubeMirraChatController>();
            if (chat != null)
            {
                chat.Configure(
                    GetBool(fields, "chat_enabled", false),
                    GetString(fields, "chat_channel_id", string.Empty));
            }
        }

        private IEnumerator RestoreOrCreateCloudSave()
        {
            var operation = cloud.Sdk.CloudSave.GetPlayerDataAsync(new[]
            {
                SoftKey, HardKey, StrengthKey, AnimalSpeedKey, ToolTierKey,
                SelectedToolTierKey, SpeedLevelKey, SaveVersionKey,
            });
            yield return operation;
            if (!operation.Result.IsSuccess)
            {
                LogWarning("Cloud Save load", operation.Result.Error?.Message);
                yield break;
            }

            var playerData = cloud.Sdk.CloudSave.PlayerData;
            if (playerData == null || playerData.Fields.Count == 0)
            {
                MarkLocalStateDirty();
                yield break;
            }

            applyingCloudState = true;
            wallet.RestoreCloudBalances(
                ParseLong(playerData.GetString(SoftKey), wallet.SoftCurrency),
                ParseLong(playerData.GetString(HardKey), wallet.HardCurrency));
            stats.RestoreCloudProgression(
                playerData.GetFloat(StrengthKey, stats.Strength),
                playerData.GetFloat(AnimalSpeedKey, stats.AnimalSpeed),
                playerData.GetInt(ToolTierKey, stats.StrengthToolTier),
                playerData.GetInt(SelectedToolTierKey, stats.SelectedStrengthToolTier),
                playerData.GetInt(SpeedLevelKey, stats.SpeedUpgradeLevel));
            applyingCloudState = false;
            localStateDirty = false;
            lastCloudSnapshotSignature = CreateSnapshotSignature();
            Log("cloud progression restored");
        }

        private IEnumerator SynchronizePendingState()
        {
            var snapshot = CreateSnapshotSignature();
            yield return SaveCloudProgress();
            if (!lastSaveSucceeded)
            {
                ScheduleRetry();
                yield break;
            }

            yield return SynchronizeEconomy();
            if (!lastEconomySyncSucceeded)
            {
                ScheduleRetry();
                yield break;
            }

            var now = Time.realtimeSinceStartup;
            lastSuccessfulSyncAt = now;
            retryDelaySeconds = 5f;
            nextSyncAttemptAt = now + cloudSaveIntervalSeconds;
            localStateDirty = !string.Equals(snapshot, CreateSnapshotSignature(), StringComparison.Ordinal);
        }

        private IEnumerator SaveCloudProgress()
        {
            lastSaveSucceeded = false;
            var snapshot = CreateSnapshotSignature();
            if (string.Equals(snapshot, lastCloudSnapshotSignature, StringComparison.Ordinal))
            {
                lastSaveSucceeded = true;
                yield break;
            }

            var request = new CloudSaveDataRequest()
                .AddString(SoftKey, wallet.SoftCurrency.ToString(CultureInfo.InvariantCulture))
                .AddString(HardKey, wallet.HardCurrency.ToString(CultureInfo.InvariantCulture))
                .AddFloat(StrengthKey, stats.Strength)
                .AddFloat(AnimalSpeedKey, stats.AnimalSpeed)
                .AddInt(ToolTierKey, stats.StrengthToolTier)
                .AddInt(SelectedToolTierKey, stats.SelectedStrengthToolTier)
                .AddInt(SpeedLevelKey, stats.SpeedUpgradeLevel)
                .AddInt(SaveVersionKey, 1);

            if (loginPresenceCaptured)
            {
                var publicRead = AccessMask.Owner | AccessMask.Other;
                request
                    .AddString(PresenceNicknameKey, loginPresenceNickname, publicRead)
                    .AddFloat(PresencePositionXKey, loginPresencePosition.x, publicRead)
                    .AddFloat(PresencePositionYKey, loginPresencePosition.y, publicRead)
                    .AddFloat(PresencePositionZKey, loginPresencePosition.z, publicRead)
                    .AddString(PresenceSeenAtKey, loginPresenceSeenAtUtc, publicRead);
            }

            var operation = cloud.Sdk.CloudSave.UpsertPlayerDataAsync(request);
            yield return operation;
            if (!operation.Result.IsSuccess)
            {
                LogWarning("Cloud Save write", operation.Result.Error?.Message);
                yield break;
            }

            lastSaveSucceeded = true;
            lastCloudSnapshotSignature = snapshot;
            if (analyticsEnabled)
            {
                cloud.Sdk.Analytics.EnqueueEvent("klc_cloud_save", new Dictionary<string, string>
                {
                    ["save_version"] = "1",
                });
            }
        }

        private IEnumerator InitializeEconomy()
        {
            economyReady = false;
            var configs = cloud.Sdk.Economy.LoadConfigsAsync();
            yield return configs;
            if (!configs.Result.IsSuccess)
            {
                LogWarning("Economy configs", configs.Result.Error?.Message);
                yield break;
            }

            var inventoryOperation = cloud.Sdk.Economy.LoadInventoryAsync();
            yield return inventoryOperation;
            if (!inventoryOperation.Result.IsSuccess)
            {
                LogWarning("Economy inventory", inventoryOperation.Result.Error?.Message);
                yield break;
            }

            var remoteWallet = inventoryOperation.Result.Data?.Wallet;
            if (remoteWallet != null)
            {
                var soft = remoteWallet.FirstOrDefault(entry => entry.CurrencyId == "soft");
                var hard = remoteWallet.FirstOrDefault(entry => entry.CurrencyId == "hard");
                if (soft != null)
                {
                    lastEconomySoft = (long)soft.Balance;
                }

                if (hard != null)
                {
                    lastEconomyHard = (long)hard.Balance;
                }
            }

            economyReady = true;
        }

        private IEnumerator SynchronizeEconomy()
        {
            lastEconomySyncSucceeded = false;
            if (!economyReady)
            {
                yield return InitializeEconomy();
            }

            if (!economyReady)
            {
                yield break;
            }

            if (cloud.Sdk.Economy.Currencies.ContainsKey("soft") && lastEconomySoft != wallet.SoftCurrency)
            {
                var soft = cloud.Sdk.Economy.SetCurrencyAsync("soft", wallet.SoftCurrency);
                yield return soft;
                if (!soft.Result.IsSuccess)
                {
                    LogWarning("Economy soft sync", soft.Result.Error?.Message);
                    yield break;
                }

                lastEconomySoft = wallet.SoftCurrency;
            }

            if (cloud.Sdk.Economy.Currencies.ContainsKey("hard") && lastEconomyHard != wallet.HardCurrency)
            {
                var hard = cloud.Sdk.Economy.SetCurrencyAsync("hard", wallet.HardCurrency);
                yield return hard;
                if (!hard.Result.IsSuccess)
                {
                    LogWarning("Economy hard sync", hard.Result.Error?.Message);
                    yield break;
                }

                lastEconomyHard = wallet.HardCurrency;
            }

            lastEconomySyncSucceeded = true;
        }

        private IEnumerator RefreshLeaderboard(bool submitScore)
        {
            if (leaderboardDisabledForSession)
            {
                yield break;
            }

            if (!leaderboardInitialized)
            {
                var initialize = cloud.Sdk.Leaderboard.InitializeAsync();
                yield return initialize;
                if (!initialize.Result.IsSuccess)
                {
                    LogWarning("Leaderboard config", initialize.Result.Error?.Message);
                    yield break;
                }

                leaderboardInitialized = true;
            }

            if (!cloud.Sdk.Leaderboard.LeaderboardConfigs.Any(config => config.Key == LeaderboardKey))
            {
                LogWarning("Leaderboard config", $"'{LeaderboardKey}' was not found.");
                yield break;
            }

            var score = CalculateScore();
            if (submitScore && (double.IsNaN(lastSubmittedScore) || Math.Abs(score - lastSubmittedScore) >= 1d))
            {
                var accountInfo = cloud.Sdk.PlayerAccount.PlayerAccountInfo;
                if (accountInfo == null)
                {
                    var account = cloud.Sdk.PlayerAccount.GetAccountAsync();
                    yield return account;
                    if (!account.Result.IsSuccess)
                    {
                        LogWarning("Player account", account.Result.Error?.Message);
                        yield break;
                    }

                    accountInfo = cloud.Sdk.PlayerAccount.PlayerAccountInfo;
                }

                if (accountInfo == null || string.IsNullOrWhiteSpace(accountInfo.Nickname))
                {
                    var nickname = cloud.Sdk.PlayerAccount.UpdateNicknameAsync("Kicker");
                    yield return nickname;
                    if (!nickname.Result.IsSuccess)
                    {
                        LogWarning("Player nickname", nickname.Result.Error?.Message);
                        yield break;
                    }

                    var refreshedAccount = cloud.Sdk.PlayerAccount.GetAccountAsync();
                    yield return refreshedAccount;
                    if (!refreshedAccount.Result.IsSuccess)
                    {
                        LogWarning("Player account refresh", refreshedAccount.Result.Error?.Message);
                        yield break;
                    }
                }

                if (!leaderboardJoined)
                {
                    var join = cloud.Sdk.Leaderboard.JoinAsync(LeaderboardKey);
                    yield return join;
                    if (!join.Result.IsSuccess)
                    {
                        LogWarning("Leaderboard join", join.Result.Error?.Message);
                        yield break;
                    }

                    leaderboardJoined = true;
                }

                var submit = cloud.Sdk.Leaderboard.SubmitScoreAsync(score, LeaderboardKey);
                yield return submit;
                if (!submit.Result.IsSuccess)
                {
                    leaderboardDisabledForSession = true;
                    LogWarning(
                        "Leaderboard submit",
                        string.IsNullOrWhiteSpace(submit.Result.ResponseBody)
                            ? submit.Result.Error?.Message
                            : submit.Result.ResponseBody);
                    yield break;
                }

                lastSubmittedScore = score;
            }

            var top = cloud.Sdk.Leaderboard.GetLeaderboardTopEntries(LeaderboardKey, 10);
            yield return top;
            if (!top.Result.IsSuccess)
            {
                LogWarning("Leaderboard top", top.Result.Error?.Message);
                yield break;
            }

            TopEntries = top.Result.Data?.entries ?? Array.Empty<LeaderboardEntryDto>();
        }

        private double CalculateScore()
        {
            var mobs = inventory != null ? inventory.HotbarAnimalCount + inventory.StoredAnimalCount : 0;
            return Math.Max(stats.Strength, 0f) + wallet.SoftCurrency / 10d + mobs * 250d;
        }

        private void CaptureLoginPresence()
        {
            var player = FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            if (player == null)
            {
                return;
            }

            loginPresencePosition = player.transform.position;
            loginPresenceNickname = cloud.Sdk.PlayerAccount.PlayerAccountInfo?.Nickname;
            if (string.IsNullOrWhiteSpace(loginPresenceNickname))
            {
                loginPresenceNickname = "Kicker";
            }

            loginPresenceSeenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            loginPresenceCaptured = true;
            MarkLocalStateDirty();
        }

        private IEnumerator LoadLoginGhosts()
        {
            if (maximumLoginGhosts <= 0)
            {
                yield break;
            }

            var candidates = new List<SocialCandidate>();
            var friendsOperation = cloud.Sdk.Friends.GetFriendsAsync(true);
            yield return friendsOperation;
            if (friendsOperation.Result.IsSuccess && friendsOperation.Result.Data != null)
            {
                foreach (var friend in friendsOperation.Result.Data.Take(maximumLoginGhosts))
                {
                    candidates.Add(new SocialCandidate(
                        friend.PlayerId,
                        friend.PlayerInfo?.Nickname,
                        true));
                }
            }
            else if (!friendsOperation.Result.IsSuccess)
            {
                LogWarning("Friends load", friendsOperation.Result.Error?.Message);
            }

            if (candidates.Count == 0)
            {
                var randomProfiles = cloud.Sdk.PlayerAccount.GetRandomProfilesAsync(maximumLoginGhosts);
                yield return randomProfiles;
                if (!randomProfiles.Result.IsSuccess)
                {
                    LogWarning("Random profiles", randomProfiles.Result.Error?.Message);
                    yield break;
                }

                foreach (var profile in randomProfiles.Result.Data ?? Array.Empty<ProfileInfo>())
                {
                    candidates.Add(new SocialCandidate(profile.Id, profile.Nickname, false));
                }
            }

            var ghostsRoot = new GameObject("KLC_MirraLoginGhosts_Runtime").transform;
            foreach (var candidate in candidates.Take(maximumLoginGhosts))
            {
                if (string.IsNullOrWhiteSpace(candidate.ProfileId))
                {
                    continue;
                }

                if (string.Equals(candidate.ProfileId, cloud.Sdk.PlayerAccount.PlayerAccountInfo?.Id, StringComparison.Ordinal))
                {
                    continue;
                }

                var dataOperation = cloud.Sdk.CloudSave.GetOtherPlayerDataAsync(candidate.ProfileId, new[]
                {
                    PresenceNicknameKey,
                    PresencePositionXKey,
                    PresencePositionYKey,
                    PresencePositionZKey,
                    PresenceSeenAtKey,
                });
                yield return dataOperation;
                if (!dataOperation.Result.IsSuccess || dataOperation.Result.Data == null)
                {
                    continue;
                }

                var presence = new PlayerData(dataOperation.Result.Data);
                if (!IsRecentPresence(presence.GetString(PresenceSeenAtKey)))
                {
                    continue;
                }

                var position = new Vector3(
                    presence.GetFloat(PresencePositionXKey),
                    presence.GetFloat(PresencePositionYKey),
                    presence.GetFloat(PresencePositionZKey));
                var nickname = presence.GetString(PresenceNicknameKey, candidate.Nickname);
                SpawnLoginGhost(ghostsRoot, candidate.ProfileId, nickname, position, candidate.IsFriend);
            }
        }

        private void SpawnLoginGhost(Transform root, string profileId, string nickname, Vector3 position, bool isFriend)
        {
            var prefab = Resources.Load<KickLuckyCubeFakeOnlineBot>(ghostActorResourcePath);
            if (prefab == null)
            {
                LogWarning("Presence ghost", $"prefab Resources/{ghostActorResourcePath}.prefab was not found");
                return;
            }

            var actor = Instantiate(prefab, position, Quaternion.identity, root);
            actor.name = "KLC_MirraGhost_" + profileId;
            actor.Configure(nickname, null, Mathf.Abs(profileId.GetHashCode() % 1000) / 10f);
            var identity = actor.gameObject.AddComponent<KickLuckyCubeMirraGhostIdentity>();
            identity.Configure(profileId, nickname, isFriend);
        }

        private bool IsRecentPresence(string timestamp)
        {
            return DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var seenAt)
                && DateTime.UtcNow - seenAt.ToUniversalTime() <= TimeSpan.FromDays(maximumPresenceAgeDays);
        }

        private void HandleLocalStateChanged(long soft, long hard) => HandleLocalStateChanged();

        private void HandleLocalStateChanged()
        {
            if (!applyingCloudState)
            {
                MarkLocalStateDirty();
            }
        }

        private void MarkLocalStateDirty()
        {
            localStateDirty = true;
            lastLocalChangeAt = Time.realtimeSinceStartup;
        }

        private void ScheduleRetry()
        {
            nextSyncAttemptAt = Time.realtimeSinceStartup + retryDelaySeconds;
            retryDelaySeconds = Mathf.Min(maximumRetryDelaySeconds, retryDelaySeconds * 2f);
        }

        private string CreateSnapshotSignature()
        {
            return string.Join("|",
                wallet.SoftCurrency.ToString(CultureInfo.InvariantCulture),
                wallet.HardCurrency.ToString(CultureInfo.InvariantCulture),
                stats.Strength.ToString("R", CultureInfo.InvariantCulture),
                stats.AnimalSpeed.ToString("R", CultureInfo.InvariantCulture),
                stats.StrengthToolTier.ToString(CultureInfo.InvariantCulture),
                stats.SelectedStrengthToolTier.ToString(CultureInfo.InvariantCulture),
                stats.SpeedUpgradeLevel.ToString(CultureInfo.InvariantCulture),
                loginPresenceCaptured ? loginPresenceNickname : string.Empty,
                loginPresenceCaptured ? loginPresencePosition.x.ToString("R", CultureInfo.InvariantCulture) : string.Empty,
                loginPresenceCaptured ? loginPresencePosition.y.ToString("R", CultureInfo.InvariantCulture) : string.Empty,
                loginPresenceCaptured ? loginPresencePosition.z.ToString("R", CultureInfo.InvariantCulture) : string.Empty,
                loginPresenceCaptured ? loginPresenceSeenAtUtc : string.Empty);
        }

        private readonly struct SocialCandidate
        {
            public SocialCandidate(string profileId, string nickname, bool isFriend)
            {
                ProfileId = profileId;
                Nickname = nickname;
                IsFriend = isFriend;
            }

            public string ProfileId { get; }
            public string Nickname { get; }
            public bool IsFriend { get; }
        }

        private static bool GetBool(IReadOnlyList<MirraCloud.Core.RemoteConfig.RemoteConfigField> fields, string key, bool fallback)
        {
            var value = fields.FirstOrDefault(field => field.Key == key).Value;
            return bool.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static float GetFloat(IReadOnlyList<MirraCloud.Core.RemoteConfig.RemoteConfigField> fields, string key, float fallback)
        {
            var value = fields.FirstOrDefault(field => field.Key == key).Value;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;
        }

        private static string GetString(IReadOnlyList<MirraCloud.Core.RemoteConfig.RemoteConfigField> fields, string key, string fallback)
        {
            var value = fields.FirstOrDefault(field => field.Key == key).Value;
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static long ParseLong(string value, long fallback)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? Math.Max(0L, parsed)
                : fallback;
        }

        private void Log(string message)
        {
            if (logCloudOperations)
            {
                Debug.Log($"Kick Lucky Cube: Mirra Cloud {message}.");
            }
        }

        private static void LogWarning(string operation, string message)
        {
            Debug.LogWarning($"Kick Lucky Cube: Mirra Cloud {operation} failed: {message ?? "unknown error"}");
        }
    }
}
