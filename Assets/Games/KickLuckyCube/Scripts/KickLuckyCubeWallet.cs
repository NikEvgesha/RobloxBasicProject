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
        private float softGainMultiplier;
        private bool initialized;

        public event Action<int, int> Changed;
        public event Action<int, int> CurrencyGained;
        public event Action<int, int> CurrencySpent;

        public int SoftCurrency => initialized ? softCurrency : initialSoftCurrency;
        public int HardCurrency => initialized ? hardCurrency : initialHardCurrency;
        public float SoftGainMultiplier => initialized ? softGainMultiplier : Mathf.Max(1f, initialSoftGainMultiplier);

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

        public void ResetWallet()
        {
            softCurrency = initialSoftCurrency;
            hardCurrency = initialHardCurrency;
            softGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);
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
            softGainMultiplier = Mathf.Max(1f, value);
            initialized = true;
            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void Load()
        {
            softGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);

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
            softGainMultiplier = Mathf.Max(1f, initialSoftGainMultiplier);
            initialized = true;
        }
    }
}
