using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeWallet : MonoBehaviour
    {
        private const string SoftKey = "Soft";
        private const string HardKey = "Hard";

        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Wallet.";
        [SerializeField, Min(0)] private int initialSoftCurrency;
        [SerializeField, Min(0)] private int initialHardCurrency;
        [SerializeField, Min(1f)] private float initialSoftGainMultiplier = 1f;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private int softCurrency;
        private int hardCurrency;
        private float baseSoftGainMultiplier;
        private float bonusSoftGainMultiplier = 1f;
        private bool initialized;

        public event Action<int, int> Changed;
        public event Action<int, int> CurrencyGained;
        public event Action<int, int> CurrencySpent;

        public int SoftCurrency => initialized ? softCurrency : initialSoftCurrency;
        public int HardCurrency => initialized ? hardCurrency : initialHardCurrency;
        public float SoftGainMultiplier => Mathf.Max(1f, BaseSoftGainMultiplier * BonusSoftGainMultiplier);
        public float BaseSoftGainMultiplier => initialized ? baseSoftGainMultiplier : Mathf.Max(1f, initialSoftGainMultiplier);
        public float BonusSoftGainMultiplier => initialized ? bonusSoftGainMultiplier : 1f;

        private void Awake()
        {
            Load();
        }

        public void AddSoft(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureInitialized();
            var gainedAmount = PreviewSoftGain(amount);
            softCurrency += gainedAmount;
            CurrencyGained?.Invoke(gainedAmount, 0);
            Save();
        }

        public void AddHard(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureInitialized();
            hardCurrency += amount;
            CurrencyGained?.Invoke(0, amount);
            Save();
        }

        public bool TrySpendSoft(int amount)
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

        public bool TrySpendHard(int amount)
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

        public int PreviewSoftGain(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(amount * SoftGainMultiplier));
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
                softCurrency = PlayerPrefs.GetInt(saveKeyPrefix + SoftKey, initialSoftCurrency);
                hardCurrency = PlayerPrefs.GetInt(saveKeyPrefix + HardKey, initialHardCurrency);
            }
            else
            {
                softCurrency = initialSoftCurrency;
                hardCurrency = initialHardCurrency;
            }

            initialized = true;
            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void Save()
        {
            if (Application.isPlaying && saveInPlayerPrefs)
            {
                PlayerPrefs.SetInt(saveKeyPrefix + SoftKey, softCurrency);
                PlayerPrefs.SetInt(saveKeyPrefix + HardKey, hardCurrency);
                PlayerPrefs.Save();
            }

            Changed?.Invoke(softCurrency, hardCurrency);
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
    }
}
