using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeWheelController : MonoBehaviour
    {
        private const string NextSpinAtKey = "KickLuckyCube.Wheel.NextSpinAt";

        public enum RewardType
        {
            Soft,
            Hard,
            Strength
        }

        [Serializable]
        public sealed class WheelReward
        {
            [SerializeField] private string title = "Reward";
            [SerializeField] private RewardType rewardType;
            [SerializeField, Min(1)] private int amount = 25;
            [SerializeField, Min(1)] private int weight = 1;

            public string Title => string.IsNullOrWhiteSpace(title) ? rewardType.ToString() : title;
            public RewardType RewardType => rewardType;
            public int Amount => Mathf.Max(1, amount);
            public int Weight => Mathf.Max(1, weight);
        }

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private Button spinButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text resultText;
        [SerializeField] private Text cooldownText;
        [SerializeField] private Image pointerImage;
        [SerializeField] private RectTransform wheelVisual;
        [SerializeField, Min(1f)] private float cooldownSeconds = 120f;
        [SerializeField] private WheelReward[] rewards =
        {
            new(),
            new(),
            new()
        };

        private float visualSpinVelocity;

        public float RemainingCooldownSeconds => Mathf.Max(0f, GetNextSpinAt() - Time.unscaledTime);
        public bool CanSpin => RemainingCooldownSeconds <= 0f;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            WireButtons();
            Refresh();
        }

        private void OnDestroy()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(SpinFromButton);
            }
        }

        private void Update()
        {
            if (wheelVisual != null && Mathf.Abs(visualSpinVelocity) > 0.01f)
            {
                wheelVisual.Rotate(0f, 0f, visualSpinVelocity * Time.unscaledDeltaTime);
                visualSpinVelocity = Mathf.MoveTowards(visualSpinVelocity, 0f, 480f * Time.unscaledDeltaTime);
            }

            Refresh();
        }

        public bool Spin()
        {
            if (!CanSpin)
            {
                Refresh();
                return false;
            }

            var reward = PickReward();
            GrantReward(reward);
            PlayerPrefs.SetFloat(NextSpinAtKey, Time.unscaledTime + cooldownSeconds);
            visualSpinVelocity = 920f;

            if (resultText != null)
            {
                resultText.text = $"You won {FormatReward(reward)}";
            }

            Refresh();
            return true;
        }

        public void ResetCooldownForPrototype()
        {
            PlayerPrefs.DeleteKey(NextSpinAtKey);
            Refresh();
        }

        public void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "Lucky Wheel";
            }

            if (spinButton != null)
            {
                spinButton.interactable = CanSpin;
            }

            if (cooldownText != null)
            {
                cooldownText.text = CanSpin
                    ? "Ready"
                    : $"Next spin: {FormatTime(RemainingCooldownSeconds)}";
            }

            if (pointerImage != null)
            {
                pointerImage.color = CanSpin
                    ? new Color(1f, 0.82f, 0.12f, 1f)
                    : new Color(0.65f, 0.70f, 0.78f, 1f);
            }
        }

        private void WireButtons()
        {
            if (spinButton == null)
            {
                return;
            }

            spinButton.onClick.RemoveListener(SpinFromButton);
            spinButton.onClick.AddListener(SpinFromButton);
        }

        private void SpinFromButton()
        {
            Spin();
        }

        private WheelReward PickReward()
        {
            if (rewards == null || rewards.Length == 0)
            {
                return new WheelReward();
            }

            var totalWeight = 0;
            foreach (var reward in rewards)
            {
                if (reward != null)
                {
                    totalWeight += reward.Weight;
                }
            }

            var roll = UnityEngine.Random.Range(0, Mathf.Max(1, totalWeight));
            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                roll -= reward.Weight;
                if (roll < 0)
                {
                    return reward;
                }
            }

            return rewards[0];
        }

        private void GrantReward(WheelReward reward)
        {
            if (reward == null)
            {
                return;
            }

            switch (reward.RewardType)
            {
                case RewardType.Soft:
                    wallet?.AddSoft(reward.Amount);
                    break;
                case RewardType.Hard:
                    wallet?.AddHard(reward.Amount);
                    break;
                case RewardType.Strength:
                    stats?.AddStrength(reward.Amount);
                    break;
            }
        }

        private float GetNextSpinAt()
        {
            return PlayerPrefs.GetFloat(NextSpinAtKey, 0f);
        }

        private static string FormatReward(WheelReward reward)
        {
            if (reward == null)
            {
                return "nothing";
            }

            return reward.RewardType switch
            {
                RewardType.Soft => $"+{reward.Amount} soft",
                RewardType.Hard => $"+{reward.Amount} hard",
                RewardType.Strength => $"+{reward.Amount} strength",
                _ => reward.Title
            };
        }

        private static string FormatTime(float seconds)
        {
            var totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            var minutes = totalSeconds / 60;
            var remaining = totalSeconds % 60;
            return $"{minutes:00}:{remaining:00}";
        }
    }
}
