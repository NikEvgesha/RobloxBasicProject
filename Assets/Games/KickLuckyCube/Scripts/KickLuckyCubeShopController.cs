using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeShopController : MonoBehaviour
    {
        public enum ItemType
        {
            SpeedUpgrade,
            StrengthTool,
            StrengthBoost
        }

        [Serializable]
        public sealed class ShopItemView
        {
            [SerializeField] private ItemType itemType;
            [SerializeField] private Button buyButton;
            [SerializeField] private Text nameText;
            [SerializeField] private Text priceText;
            [SerializeField] private Image frameImage;

            public ItemType ItemType => itemType;
            public Button BuyButton => buyButton;

            public void ApplyTheme()
            {
                if (frameImage != null)
                {
                    KickLuckyCubeUiTheme.StyleTree(frameImage.gameObject);
                }

                KickLuckyCubeUiTheme.StyleButton(buyButton, buyButton != null ? buyButton.gameObject.name : string.Empty);
                KickLuckyCubeUiTheme.StyleText(nameText, nameText != null ? nameText.gameObject.name : string.Empty);
                KickLuckyCubeUiTheme.StyleText(priceText, priceText != null ? priceText.gameObject.name : string.Empty);
            }

            public void Refresh(string name, string price, bool canBuy, bool maxed, Color availableColor, Color lockedColor, Color maxedColor)
            {
                if (nameText != null)
                {
                    nameText.text = name;
                }

                if (priceText != null)
                {
                    priceText.text = maxed ? "MAX" : price;
                }

                if (buyButton != null)
                {
                    buyButton.interactable = canBuy && !maxed;
                }

                if (frameImage != null)
                {
                    frameImage.color = maxed ? maxedColor : canBuy ? availableColor : lockedColor;
                }
            }
        }

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private ShopItemView[] items = Array.Empty<ShopItemView>();
        [SerializeField] private Text statusText;
        [SerializeField, Min(0)] private int baseSpeedCost = 60;
        [SerializeField, Min(0)] private int speedCostStep = 55;
        [SerializeField, Min(0.1f)] private float speedGain = 0.8f;
        [SerializeField, Min(0)] private int baseToolCost = 90;
        [SerializeField, Min(1f)] private float toolCostMultiplier = 2f;
        [SerializeField, Min(1)] private int maxStrengthToolTier = 5;
        [SerializeField, Min(0)] private int strengthBoostCost = 180;
        [SerializeField, Min(1f)] private float strengthBoostAmount = 75f;
        [SerializeField] private Color availableColor = KickLuckyCubeUiTheme.Secondary;
        [SerializeField] private Color lockedColor = KickLuckyCubeUiTheme.CardDark;
        [SerializeField] private Color maxedColor = KickLuckyCubeUiTheme.Primary;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            WireButtons();
            ApplyTheme();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            UnwireButtons();
        }

        public bool Buy(ItemType itemType)
        {
            var bought = itemType switch
            {
                ItemType.SpeedUpgrade => BuySpeedUpgrade(),
                ItemType.StrengthTool => BuyStrengthTool(),
                ItemType.StrengthBoost => BuyStrengthBoost(),
                _ => false
            };

            Refresh();
            return bought;
        }

        public void Refresh()
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                RefreshItem(item);
            }
        }

        private bool BuySpeedUpgrade()
        {
            if (wallet == null || stats == null)
            {
                SetStatus("Speed shop is not ready.");
                return false;
            }

            var cost = SpeedCost;
            if (!wallet.TrySpendSoft(cost))
            {
                SetStatus($"Need {cost} soft for speed.");
                return false;
            }

            stats.AddAnimalSpeed(speedGain);
            SetStatus($"Speed upgraded to Lv {stats.SpeedUpgradeLevel}.");
            return true;
        }

        private bool BuyStrengthTool()
        {
            if (wallet == null || stats == null || stats.StrengthToolTier >= maxStrengthToolTier)
            {
                SetStatus("Best tool already owned.");
                return false;
            }

            var cost = ToolCost;
            if (!wallet.TrySpendSoft(cost))
            {
                SetStatus($"Need {cost} soft for tool.");
                return false;
            }

            stats.UpgradeStrengthTool();
            SetStatus($"Tool upgraded to Lv {stats.StrengthToolTier}.");
            return true;
        }

        private bool BuyStrengthBoost()
        {
            if (wallet == null || stats == null)
            {
                return false;
            }

            if (!wallet.TrySpendSoft(strengthBoostCost))
            {
                SetStatus($"Need {strengthBoostCost} soft for boost.");
                return false;
            }

            stats.AddStrength(strengthBoostAmount);
            SetStatus($"+{strengthBoostAmount:0} strength.");
            return true;
        }

        private void RefreshItem(ShopItemView item)
        {
            var canBuy = false;
            var maxed = false;
            var name = string.Empty;
            var price = string.Empty;

            switch (item.ItemType)
            {
                case ItemType.SpeedUpgrade:
                    maxed = false;
                    canBuy = wallet != null && stats != null && wallet.SoftCurrency >= SpeedCost;
                    name = stats != null ? $"Speed Lv {stats.SpeedUpgradeLevel + 1}" : "Speed";
                    price = $"{SpeedCost} soft";
                    break;
                case ItemType.StrengthTool:
                    maxed = stats != null && stats.StrengthToolTier >= maxStrengthToolTier;
                    canBuy = wallet != null && stats != null && wallet.SoftCurrency >= ToolCost && !maxed;
                    name = stats != null ? $"Tool Lv {stats.StrengthToolTier + 1}" : "Tool";
                    price = $"{ToolCost} soft";
                    break;
                case ItemType.StrengthBoost:
                    canBuy = wallet != null && wallet.SoftCurrency >= strengthBoostCost;
                    name = $"+{strengthBoostAmount:0} Strength";
                    price = $"{strengthBoostCost} soft";
                    break;
            }

            item.Refresh(name, price, canBuy, maxed, availableColor, lockedColor, maxedColor);
        }

        private int SpeedCost => stats != null
            ? Mathf.RoundToInt(baseSpeedCost + stats.SpeedUpgradeLevel * speedCostStep)
            : baseSpeedCost;

        private int ToolCost => stats != null
            ? Mathf.RoundToInt(baseToolCost * Mathf.Pow(toolCostMultiplier, stats.StrengthToolTier - 1))
            : baseToolCost;

        private void WireButtons()
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item?.BuyButton == null)
                {
                    continue;
                }

                var itemType = item.ItemType;
                item.BuyButton.onClick.RemoveAllListeners();
                item.BuyButton.onClick.AddListener(() => Buy(itemType));
            }
        }

        private void ApplyTheme()
        {
            KickLuckyCubeUiTheme.StyleText(statusText, statusText != null ? statusText.gameObject.name : string.Empty);

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                item?.ApplyTheme();
            }
        }

        private void UnwireButtons()
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                item?.BuyButton?.onClick.RemoveAllListeners();
            }
        }

        private void Subscribe()
        {
            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            if (stats != null)
            {
                stats.Changed += Refresh;
            }
        }

        private void Unsubscribe()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (stats != null)
            {
                stats.Changed -= Refresh;
            }
        }

        private void OnWalletChanged(int soft, int hard)
        {
            Refresh();
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }
    }
}
