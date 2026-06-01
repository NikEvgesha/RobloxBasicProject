using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeRewardsController : MonoBehaviour
    {
        private const string ElapsedKey = "ElapsedSeconds";
        private const string ClaimedMaskKey = "ClaimedMask";
        private const string StartedAtKey = "StartedAt";

        [Serializable]
        public sealed class RewardSlot
        {
            [SerializeField] private string title = "Reward";
            [SerializeField, Min(1f)] private float unlockSeconds = 60f;
            [SerializeField, Min(0)] private int softAmount;
            [SerializeField, Min(0)] private int hardAmount = 1;
            [SerializeField] private Button claimButton;
            [SerializeField] private Text titleText;
            [SerializeField] private Text rewardText;
            [SerializeField] private Text statusText;
            [SerializeField] private Image frameImage;

            public float UnlockSeconds => Mathf.Max(1f, unlockSeconds);
            public Button ClaimButton => claimButton;

            public void Refresh(bool claimed, bool claimable, float remainingSeconds)
            {
                if (titleText != null)
                {
                    titleText.text = title;
                }

                if (rewardText != null)
                {
                    rewardText.text = FormatReward();
                }

                if (statusText != null)
                {
                    statusText.text = claimed
                        ? "Claimed"
                        : claimable
                            ? "Ready"
                            : FormatTime(remainingSeconds);
                }

                if (claimButton != null)
                {
                    claimButton.interactable = claimable;
                }

                if (frameImage != null)
                {
                    frameImage.color = claimed
                        ? new Color(0.4f, 0.42f, 0.46f, 0.85f)
                        : claimable
                            ? new Color(0.25f, 0.92f, 0.22f, 1f)
                            : new Color(0.12f, 0.66f, 0.95f, 1f);
                }
            }

            public void Grant(KickLuckyCubeWallet wallet)
            {
                if (wallet == null)
                {
                    return;
                }

                wallet.AddSoft(softAmount);
                wallet.AddHard(hardAmount);
            }

            private string FormatReward()
            {
                if (softAmount > 0 && hardAmount > 0)
                {
                    return $"+{softAmount} soft / +{hardAmount} hard";
                }

                if (softAmount > 0)
                {
                    return $"+{softAmount} soft";
                }

                return $"+{hardAmount} hard";
            }

            private static string FormatTime(float seconds)
            {
                var clampedSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
                var minutes = clampedSeconds / 60;
                var remaining = clampedSeconds % 60;
                return $"{minutes:00}:{remaining:00}";
            }
        }

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private RewardSlot[] rewards =
        {
            new(),
            new(),
            new(),
            new()
        };
        [SerializeField] private Text headerText;
        [SerializeField] private Text statusText;
        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Rewards.";
        [SerializeField, Min(1f)] private float resetAfterHours = 20f;
        [SerializeField, Min(1f)] private float saveIntervalSeconds = 5f;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private bool[] claimed = Array.Empty<bool>();
        private float elapsedSeconds;
        private float saveTimer;

        public float ElapsedSeconds => elapsedSeconds;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            EnsureClaimedArray();
            LoadState();
            WireButtons();
            RefreshView();
        }

        private void OnDestroy()
        {
            UnwireButtons();
            SaveState();
        }

        private void Update()
        {
            elapsedSeconds += Time.deltaTime;
            saveTimer += Time.deltaTime;

            if (saveTimer >= saveIntervalSeconds)
            {
                saveTimer = 0f;
                SaveState();
            }

            RefreshView();
        }

        public bool CanClaim(int index)
        {
            EnsureClaimedArray();

            return IsValidIndex(index)
                && !claimed[index]
                && elapsedSeconds >= rewards[index].UnlockSeconds;
        }

        public bool Claim(int index)
        {
            if (!CanClaim(index))
            {
                RefreshView();
                return false;
            }

            rewards[index].Grant(wallet);
            claimed[index] = true;
            SaveState();
            RefreshView();
            return true;
        }

        public void ResetRewardsForPrototype()
        {
            elapsedSeconds = 0f;
            claimed = new bool[rewards != null ? rewards.Length : 0];
            SaveState();
            RefreshView();
        }

        private void RefreshView()
        {
            EnsureClaimedArray();

            if (headerText != null)
            {
                headerText.text = "Playtime Rewards";
            }

            var readyCount = 0;
            if (rewards != null)
            {
                for (var i = 0; i < rewards.Length; i++)
                {
                    var reward = rewards[i];
                    if (reward == null)
                    {
                        continue;
                    }

                    var claimable = CanClaim(i);
                    if (claimable)
                    {
                        readyCount++;
                    }

                    reward.Refresh(claimed[i], claimable, reward.UnlockSeconds - elapsedSeconds);
                }
            }

            if (statusText != null)
            {
                statusText.text = readyCount > 0
                    ? $"{readyCount} reward ready"
                    : $"Playtime: {FormatElapsed(elapsedSeconds)}";
            }
        }

        private void WireButtons()
        {
            if (rewards == null)
            {
                return;
            }

            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward?.ClaimButton == null)
                {
                    continue;
                }

                var index = i;
                reward.ClaimButton.onClick.RemoveAllListeners();
                reward.ClaimButton.onClick.AddListener(() => Claim(index));
            }
        }

        private void UnwireButtons()
        {
            if (rewards == null)
            {
                return;
            }

            foreach (var reward in rewards)
            {
                reward?.ClaimButton?.onClick.RemoveAllListeners();
            }
        }

        private void LoadState()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var startedAt = PlayerPrefs.GetString(saveKeyPrefix + StartedAtKey, string.Empty);
            if (!long.TryParse(startedAt, out var startedAtUnix)
                || now - startedAtUnix > resetAfterHours * 3600f)
            {
                PlayerPrefs.SetString(saveKeyPrefix + StartedAtKey, now.ToString());
                elapsedSeconds = 0f;
                claimed = new bool[rewards != null ? rewards.Length : 0];
                SaveState();
                return;
            }

            elapsedSeconds = Mathf.Max(0f, PlayerPrefs.GetFloat(saveKeyPrefix + ElapsedKey, 0f));
            var claimedMask = PlayerPrefs.GetInt(saveKeyPrefix + ClaimedMaskKey, 0);
            for (var i = 0; i < claimed.Length; i++)
            {
                claimed[i] = (claimedMask & (1 << i)) != 0;
            }
        }

        private void SaveState()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            var claimedMask = 0;
            for (var i = 0; i < claimed.Length; i++)
            {
                if (claimed[i])
                {
                    claimedMask |= 1 << i;
                }
            }

            PlayerPrefs.SetFloat(saveKeyPrefix + ElapsedKey, elapsedSeconds);
            PlayerPrefs.SetInt(saveKeyPrefix + ClaimedMaskKey, claimedMask);
        }

        private void EnsureClaimedArray()
        {
            var count = rewards != null ? rewards.Length : 0;
            if (claimed != null && claimed.Length == count)
            {
                return;
            }

            claimed = new bool[count];
        }

        private bool IsValidIndex(int index)
        {
            return rewards != null && index >= 0 && index < rewards.Length && rewards[index] != null;
        }

        private static string FormatElapsed(float seconds)
        {
            var clampedSeconds = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            var minutes = clampedSeconds / 60;
            var remaining = clampedSeconds % 60;
            return $"{minutes:00}:{remaining:00}";
        }
    }
}
