using System;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedWallet : MonoBehaviour
    {
        private const string SoftKey = "soft";
        private const string HardKey = "hard";

        [SerializeField] private string saveKeyPrefix = "MechanicsTestbed.Wallet.";
        [SerializeField] private int initialSoftCurrency = 500;
        [SerializeField] private int initialHardCurrency = 10;

        private int softCurrency;
        private int hardCurrency;

        public event Action<int, int> Changed;

        public int SoftCurrency => softCurrency;
        public int HardCurrency => hardCurrency;

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

            softCurrency += amount;
            SaveAndNotify();
        }

        public void AddHard(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            hardCurrency += amount;
            SaveAndNotify();
        }

        public bool SpendSoft(int amount)
        {
            if (amount <= 0 || softCurrency < amount)
            {
                return false;
            }

            softCurrency -= amount;
            SaveAndNotify();
            return true;
        }

        public bool SpendHard(int amount)
        {
            if (amount <= 0 || hardCurrency < amount)
            {
                return false;
            }

            hardCurrency -= amount;
            SaveAndNotify();
            return true;
        }

        public void ResetWallet()
        {
            softCurrency = initialSoftCurrency;
            hardCurrency = initialHardCurrency;
            SaveAndNotify();
        }

        private void Load()
        {
            softCurrency = PlayerPrefs.GetInt(saveKeyPrefix + SoftKey, initialSoftCurrency);
            hardCurrency = PlayerPrefs.GetInt(saveKeyPrefix + HardKey, initialHardCurrency);
            Changed?.Invoke(softCurrency, hardCurrency);
        }

        private void SaveAndNotify()
        {
            PlayerPrefs.SetInt(saveKeyPrefix + SoftKey, softCurrency);
            PlayerPrefs.SetInt(saveKeyPrefix + HardKey, hardCurrency);
            PlayerPrefs.Save();
            Changed?.Invoke(softCurrency, hardCurrency);
        }
    }
}
