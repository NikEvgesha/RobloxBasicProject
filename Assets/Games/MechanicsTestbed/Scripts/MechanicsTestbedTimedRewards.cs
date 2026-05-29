using System;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedTimedRewards : MonoBehaviour
    {
        [SerializeField] private MechanicsTestbedWallet wallet;
        [SerializeField] private string saveKeyPrefix = "MechanicsTestbed.TimedRewards.";
        [SerializeField] private float intervalSeconds = 60f;
        [SerializeField] private int hardCurrencyReward = 1;

        private float elapsedSeconds;
        private bool rewardReady;

        public event Action Changed;

        public bool RewardReady => rewardReady;
        public float RemainingSeconds => rewardReady ? 0f : Mathf.Max(0f, intervalSeconds - elapsedSeconds);
        public int HardCurrencyReward => hardCurrencyReward;

        private void Awake()
        {
            if (wallet == null)
            {
                wallet = FindFirstObjectByType<MechanicsTestbedWallet>();
            }

            elapsedSeconds = PlayerPrefs.GetFloat(saveKeyPrefix + "elapsed", 0f);
            rewardReady = PlayerPrefs.GetInt(saveKeyPrefix + "ready", 0) == 1;
        }

        private void Update()
        {
            if (rewardReady)
            {
                return;
            }

            elapsedSeconds += Time.unscaledDeltaTime;
            if (elapsedSeconds >= intervalSeconds)
            {
                elapsedSeconds = intervalSeconds;
                rewardReady = true;
                Save();
                Changed?.Invoke();
            }
        }

        public bool Claim()
        {
            if (!rewardReady || wallet == null)
            {
                return false;
            }

            wallet.AddHard(hardCurrencyReward);
            elapsedSeconds = 0f;
            rewardReady = false;
            Save();
            Changed?.Invoke();
            return true;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetFloat(saveKeyPrefix + "elapsed", elapsedSeconds);
            PlayerPrefs.SetInt(saveKeyPrefix + "ready", rewardReady ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
