using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MirraCloud.Core.CloudSave.Requests;
using MirraCloud.Core.Leaderboard.Dto;
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

        [SerializeField, Min(5f)] private float defaultCloudSaveIntervalSeconds = 20f;
        [SerializeField, Min(10f)] private float defaultLeaderboardIntervalSeconds = 30f;
        [SerializeField] private bool logCloudOperations = true;

        private KickLuckyCubeMirraCloudService cloud;
        private KickLuckyCubeWallet wallet;
        private KickLuckyCubePlayerStats stats;
        private KickLuckyCubeInventoryController inventory;
        private float cloudSaveIntervalSeconds;
        private float leaderboardIntervalSeconds;
        private bool cloudSyncEnabled = true;
        private bool analyticsEnabled = true;
        private bool localStateDirty = true;
        private bool applyingCloudState;
        private bool leaderboardDisabledForSession;

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
            leaderboardIntervalSeconds = defaultLeaderboardIntervalSeconds;
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
                yield return SynchronizeEconomy();
            }

            yield return RefreshLeaderboard(true);
            IsOperational = true;
            Log("gameplay services are operational");

            var leaderboardTimer = 0f;
            while (enabled)
            {
                yield return new WaitForSecondsRealtime(cloudSaveIntervalSeconds);
                leaderboardTimer += cloudSaveIntervalSeconds;

                if (cloudSyncEnabled && localStateDirty)
                {
                    yield return SaveCloudProgress();
                    yield return SynchronizeEconomy();
                }

                if (leaderboardTimer >= leaderboardIntervalSeconds)
                {
                    leaderboardTimer = 0f;
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
            cloudSaveIntervalSeconds = Mathf.Max(5f, GetFloat(fields, "cloud_save_interval_seconds", defaultCloudSaveIntervalSeconds));
            leaderboardIntervalSeconds = Mathf.Max(10f, GetFloat(fields, "leaderboard_submit_interval_seconds", defaultLeaderboardIntervalSeconds));
            wallet.SetSoftGainBonusMultiplier(Mathf.Max(1f, GetFloat(fields, "soft_gain_multiplier", 1f)));
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
                yield return SaveCloudProgress();
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
            Log("cloud progression restored");
        }

        private IEnumerator SaveCloudProgress()
        {
            var request = new CloudSaveDataRequest()
                .AddString(SoftKey, wallet.SoftCurrency.ToString(CultureInfo.InvariantCulture))
                .AddString(HardKey, wallet.HardCurrency.ToString(CultureInfo.InvariantCulture))
                .AddFloat(StrengthKey, stats.Strength)
                .AddFloat(AnimalSpeedKey, stats.AnimalSpeed)
                .AddInt(ToolTierKey, stats.StrengthToolTier)
                .AddInt(SelectedToolTierKey, stats.SelectedStrengthToolTier)
                .AddInt(SpeedLevelKey, stats.SpeedUpgradeLevel)
                .AddInt(SaveVersionKey, 1);

            var operation = cloud.Sdk.CloudSave.UpsertPlayerDataAsync(request);
            yield return operation;
            if (!operation.Result.IsSuccess)
            {
                LogWarning("Cloud Save write", operation.Result.Error?.Message);
                yield break;
            }

            localStateDirty = false;
            if (analyticsEnabled)
            {
                cloud.Sdk.Analytics.EnqueueEvent("klc_cloud_save", new Dictionary<string, string>
                {
                    ["save_version"] = "1",
                });
            }
        }

        private IEnumerator SynchronizeEconomy()
        {
            var configs = cloud.Sdk.Economy.LoadConfigsAsync();
            yield return configs;
            if (!configs.Result.IsSuccess)
            {
                LogWarning("Economy configs", configs.Result.Error?.Message);
                yield break;
            }

            if (cloud.Sdk.Economy.Currencies.ContainsKey("soft"))
            {
                var soft = cloud.Sdk.Economy.SetCurrencyAsync("soft", wallet.SoftCurrency);
                yield return soft;
                if (!soft.Result.IsSuccess)
                {
                    LogWarning("Economy soft sync", soft.Result.Error?.Message);
                }
            }

            if (cloud.Sdk.Economy.Currencies.ContainsKey("hard"))
            {
                var hard = cloud.Sdk.Economy.SetCurrencyAsync("hard", wallet.HardCurrency);
                yield return hard;
                if (!hard.Result.IsSuccess)
                {
                    LogWarning("Economy hard sync", hard.Result.Error?.Message);
                }
            }
        }

        private IEnumerator RefreshLeaderboard(bool submitScore)
        {
            if (leaderboardDisabledForSession)
            {
                yield break;
            }

            var initialize = cloud.Sdk.Leaderboard.InitializeAsync();
            yield return initialize;
            if (!initialize.Result.IsSuccess)
            {
                LogWarning("Leaderboard config", initialize.Result.Error?.Message);
                yield break;
            }

            if (!cloud.Sdk.Leaderboard.LeaderboardConfigs.Any(config => config.Key == LeaderboardKey))
            {
                LogWarning("Leaderboard config", $"'{LeaderboardKey}' was not found.");
                yield break;
            }

            if (submitScore)
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

                var join = cloud.Sdk.Leaderboard.JoinAsync(LeaderboardKey);
                yield return join;
                if (!join.Result.IsSuccess)
                {
                    LogWarning("Leaderboard join", join.Result.Error?.Message);
                    yield break;
                }

                var submit = cloud.Sdk.Leaderboard.SubmitScoreAsync(CalculateScore(), LeaderboardKey);
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

        private void HandleLocalStateChanged(long soft, long hard) => HandleLocalStateChanged();

        private void HandleLocalStateChanged()
        {
            if (!applyingCloudState)
            {
                localStateDirty = true;
            }
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
