using System;
using System.Globalization;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeWallet : MonoBehaviour
    {
        private const string SoftKey = "Soft";
        private const string HardKey = "Hard";
        private const string SoftLongKey = "Soft64";
        private const string HardLongKey = "Hard64";

        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Wallet.";
        [SerializeField, Min(0)] private int initialSoftCurrency;
        [SerializeField, Min(0)] private int initialHardCurrency;
        [SerializeField, Min(1f)] private float initialSoftGainMultiplier = 1f;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private long softCurrency;
        private long hardCurrency;
        private float baseSoftGainMultiplier;
        private float bonusSoftGainMultiplier = 1f;
        private bool initialized;

        public event Action<long, long> Changed;
        public event Action<long, long> CurrencyGained;
        public event Action<long, long> CurrencySpent;

        public long SoftCurrency => initialized ? softCurrency : initialSoftCurrency;
        public long HardCurrency => initialized ? hardCurrency : initialHardCurrency;
        public float SoftGainMultiplier => Mathf.Max(1f, BaseSoftGainMultiplier * BonusSoftGainMultiplier);
        public float BaseSoftGainMultiplier => initialized ? baseSoftGainMultiplier : Mathf.Max(1f, initialSoftGainMultiplier);
        public float BonusSoftGainMultiplier => initialized ? bonusSoftGainMultiplier : 1f;

        private void Awake()
        {
            Load();
        }

        public void AddSoft(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureInitialized();
            var gainedAmount = PreviewSoftGain(amount);
            softCurrency = SaturatingAdd(softCurrency, gainedAmount);
            CurrencyGained?.Invoke(gainedAmount, 0);
            Save();
        }

        public void AddHard(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureInitialized();
            hardCurrency = SaturatingAdd(hardCurrency, amount);
            CurrencyGained?.Invoke(0, amount);
            Save();
        }

        public bool TrySpendSoft(long amount)
        {
            EnsureInitialized();
            if (amount <= 0 || softCurrency < amount)
            {
                return false;
            }

            softCurrency -= amount;
            CurrencySpent?.Invoke(amount, 0);
            Save();
            return true;
        }

        public bool TrySpendHard(long amount)
        {
            EnsureInitialized();
            if (amount <= 0 || hardCurrency < amount)
            {
                return false;
            }

            hardCurrency -= amount;
            CurrencySpent?.Invoke(0, amount);
            Save();
            return true;
        }

        public void ResetWallet()
        {
            softCurrency = initialSoftCurrency;
            hardCurrency = initialHardCurrency;
            baseSoftGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);
            bonusSoftGainMultiplier = 1f;
            initialized = true;
            Save();
        }

        public long PreviewSoftGain(long amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            var multiplied = amount * (double)SoftGainMultiplier;
            if (double.IsNaN(multiplied) || multiplied <= 0d)
            {
                return 0L;
            }

            return multiplied >= long.MaxValue
                ? long.MaxValue
                : Math.Max(1L, (long)Math.Round(multiplied, MidpointRounding.AwayFromZero));
        }

        public void SetSoftGainMultiplier(float value)
        {
            EnsureInitialized();
            var nextMultiplier = Mathf.Max(1f, value);
            if (Mathf.Approximately(baseSoftGainMultiplier, nextMultiplier))
            {
                return;
            }

            baseSoftGainMultiplier = nextMultiplier;
            Changed?.Invoke(softCurrency, hardCurrency);
        }

        public void SetSoftGainBonusMultiplier(float value)
        {
            EnsureInitialized();
            var nextMultiplier = Mathf.Max(1f, value);
            if (Mathf.Approximately(bonusSoftGainMultiplier, nextMultiplier))
            {
                return;
            }

            bonusSoftGainMultiplier = nextMultiplier;
            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void Load()
        {
            baseSoftGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);
            bonusSoftGainMultiplier = 1f;

            if (Application.isPlaying && saveInPlayerPrefs)
            {
                softCurrency = LoadCurrency(SoftLongKey, SoftKey, initialSoftCurrency, "soft");
                hardCurrency = LoadCurrency(HardLongKey, HardKey, initialHardCurrency, "hard");
            }
            else
            {
                softCurrency = initialSoftCurrency;
                hardCurrency = initialHardCurrency;
            }

            initialized = true;
            if (Application.isPlaying && saveInPlayerPrefs)
            {
                PersistCurrency();
            }

            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void Save()
        {
            if (Application.isPlaying && saveInPlayerPrefs)
            {
                PersistCurrency();
            }

            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void PersistCurrency()
        {
            PlayerPrefs.SetString(saveKeyPrefix + SoftLongKey, softCurrency.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(saveKeyPrefix + HardLongKey, hardCurrency.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.DeleteKey(saveKeyPrefix + SoftKey);
            PlayerPrefs.DeleteKey(saveKeyPrefix + HardKey);
            PlayerPrefs.Save();
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            softCurrency = initialSoftCurrency;
            hardCurrency = initialHardCurrency;
            baseSoftGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);
            bonusSoftGainMultiplier = 1f;
            initialized = true;
        }

        private long LoadCurrency(string longKey, string legacyKey, int fallback, string label)
        {
            var fullLongKey = saveKeyPrefix + longKey;
            var value = (long)Mathf.Max(0, fallback);
            if (PlayerPrefs.HasKey(fullLongKey))
            {
                var stored = PlayerPrefs.GetString(fullLongKey, string.Empty);
                if (long.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    value = parsed;
                }
            }
            else
            {
                value = PlayerPrefs.GetInt(saveKeyPrefix + legacyKey, fallback);
            }

            if (value >= 0L)
            {
                return value;
            }

            Debug.LogWarning($"Kick Lucky Cube repaired negative {label} currency ({value}) to 0 during save load.");
            return 0L;
        }

        private static long SaturatingAdd(long current, long amount)
        {
            if (amount <= 0L)
            {
                return Math.Max(0L, current);
            }

            return current > long.MaxValue - amount ? long.MaxValue : current + amount;
        }
    }
}
