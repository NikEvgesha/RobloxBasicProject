using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    /// <summary>
    /// Sends only meaningful gameplay milestones. Currency deltas are aggregated for ten seconds so
    /// rapid income ticks do not turn into one request/event per frame.
    /// </summary>
    public sealed class KickLuckyCubeMirraAnalyticsController : MonoBehaviour
    {
        private const float CurrencyFlushIntervalSeconds = 10f;

        private KickLuckyCubeMirraCloudService cloud;
        private KickLuckyCubeKickController kick;
        private KickLuckyCubeInventoryController inventory;
        private KickLuckyCubeWallet wallet;
        private KickLuckyCubeRebirthController rebirth;
        private long gainedSoft;
        private long gainedHard;
        private long spentSoft;
        private long spentHard;
        private float nextCurrencyFlushAt;

        public static KickLuckyCubeMirraAnalyticsController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            cloud = KickLuckyCubeMirraCloudService.Instance;
            nextCurrencyFlushAt = Time.realtimeSinceStartup + CurrencyFlushIntervalSeconds;
        }

        private void Start()
        {
            kick = FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);
            inventory = FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            wallet = FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            rebirth = FindFirstObjectByType<KickLuckyCubeRebirthController>(FindObjectsInactive.Include);

            if (kick != null) kick.Landed += OnKickLanded;
            if (inventory != null) inventory.AnimalSold += OnAnimalSold;
            if (wallet != null)
            {
                wallet.CurrencyGained += OnCurrencyGained;
                wallet.CurrencySpent += OnCurrencySpent;
            }
            if (rebirth != null) rebirth.Rebirthed += OnRebirthed;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup >= nextCurrencyFlushAt)
            {
                nextCurrencyFlushAt = Time.realtimeSinceStartup + CurrencyFlushIntervalSeconds;
                FlushCurrencyEvents();
            }
        }

        public static void Track(string eventName, string parameterName, string value)
        {
            Instance?.Enqueue(eventName, new Dictionary<string, string>
            {
                [parameterName] = value ?? string.Empty,
            });
        }

        public static void Track(string eventName, Dictionary<string, string> parameters)
        {
            Instance?.Enqueue(eventName, parameters);
        }

        private void OnKickLanded(KickLuckyCubeKickResult result)
        {
            Enqueue("klc_kick_completed", new Dictionary<string, string>
            {
                ["distance"] = result.Distance.ToString("0.##", CultureInfo.InvariantCulture),
                ["value"] = result.Rarity.ToString(),
            });
        }

        private void OnAnimalSold(KickLuckyCubeInventoryAnimal animal)
        {
            Enqueue("klc_animal_sold", new Dictionary<string, string>
            {
                ["animal_id"] = animal.CatalogId,
                ["value"] = animal.SellValue.ToString(CultureInfo.InvariantCulture),
            });
        }

        private void OnCurrencyGained(long soft, long hard)
        {
            gainedSoft += Math.Max(0, soft);
            gainedHard += Math.Max(0, hard);
        }

        private void OnCurrencySpent(long soft, long hard)
        {
            spentSoft += Math.Max(0, soft);
            spentHard += Math.Max(0, hard);
        }

        private void OnRebirthed(int count)
        {
            Enqueue("klc_rebirth", new Dictionary<string, string>
            {
                ["rebirth_count"] = count.ToString(CultureInfo.InvariantCulture),
                ["value"] = count.ToString(CultureInfo.InvariantCulture),
            });
        }

        private void FlushCurrencyEvents()
        {
            if (gainedSoft > 0 || gainedHard > 0)
            {
                Enqueue("klc_currency_gained", CurrencyParameters(gainedSoft, gainedHard));
                gainedSoft = 0;
                gainedHard = 0;
            }

            if (spentSoft > 0 || spentHard > 0)
            {
                Enqueue("klc_currency_spent", CurrencyParameters(spentSoft, spentHard));
                spentSoft = 0;
                spentHard = 0;
            }
        }

        private Dictionary<string, string> CurrencyParameters(long soft, long hard)
        {
            return new Dictionary<string, string>
            {
                ["event_source"] = "gameplay_10s_aggregate",
                ["soft_balance"] = soft.ToString(CultureInfo.InvariantCulture),
                ["hard_balance"] = hard.ToString(CultureInfo.InvariantCulture),
            };
        }

        private void Enqueue(string eventName, Dictionary<string, string> parameters)
        {
            cloud ??= KickLuckyCubeMirraCloudService.Instance;
            var sync = KickLuckyCubeMirraCloudGameplaySync.Instance;
            if (cloud == null || !cloud.IsAuthenticated || (sync != null && !sync.AnalyticsEnabled))
            {
                return;
            }

            cloud.Sdk.Analytics.EnqueueEvent(eventName, parameters);
        }

        private void OnDestroy()
        {
            if (kick != null) kick.Landed -= OnKickLanded;
            if (inventory != null) inventory.AnimalSold -= OnAnimalSold;
            if (wallet != null)
            {
                wallet.CurrencyGained -= OnCurrencyGained;
                wallet.CurrencySpent -= OnCurrencySpent;
            }
            if (rebirth != null) rebirth.Rebirthed -= OnRebirthed;
            if (Instance == this) Instance = null;
        }
    }
}
