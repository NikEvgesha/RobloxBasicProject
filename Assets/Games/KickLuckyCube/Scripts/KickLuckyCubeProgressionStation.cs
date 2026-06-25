using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeProgressionStation : MonoBehaviour, GameKitInteractionCondition
    {
        public enum StationMode
        {
            TrainStrength,
            BuySpeedUpgrade,
            BuyStrengthTool
        }

        [SerializeField] private StationMode mode;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeSpeedShopController speedShop;
        [SerializeField] private KickLuckyCubeTrainingBonusPrompt trainingBonusPrompt;
        [SerializeField] private KickLuckyCubeBalanceConfig balanceConfig;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField, Min(0)] private int baseSpeedCost = 60;
        [SerializeField, Min(0)] private int speedCostStep = 55;
        [SerializeField, Min(0.1f)] private float speedGain = 0.8f;
        [SerializeField, Min(0)] private int baseToolCost = 90;
        [SerializeField, Min(1f)] private float toolCostMultiplier = 2f;
        [SerializeField, Min(1)] private int maxStrengthToolTier = 5;
        [SerializeField, Min(0.1f)] private float baseStrengthGain = 12f;
        [SerializeField, Min(1f)] private float strengthGainMultiplier = 1.65f;

        private GameKitInteractionTarget interactionTarget;

        public StationMode Mode => mode;

        private int SpeedCost
        {
            get
            {
                if (stats == null)
                {
                    return baseSpeedCost;
                }

                var balance = ResolveBalanceConfig();
                return balance != null
                    ? ClampCostToInt(balance.GetSpeedLevelCost(stats.SpeedUpgradeLevel + 1))
                    : Mathf.RoundToInt(baseSpeedCost + stats.SpeedUpgradeLevel * speedCostStep);
            }
        }

        private int ToolCost
        {
            get
            {
                if (stats == null)
                {
                    return baseToolCost;
                }

                var balance = ResolveBalanceConfig();
                return balance != null
                    ? balance.GetToolCost(stats.StrengthToolTier + 1)
                    : Mathf.RoundToInt(baseToolCost * Mathf.Pow(toolCostMultiplier, stats.StrengthToolTier - 1));
            }
        }

        private float TrainStrengthGain
        {
            get
            {
                if (stats == null)
                {
                    return baseStrengthGain;
                }

                var balance = ResolveBalanceConfig();
                return balance != null
                    ? balance.GetToolStrengthPerSecond(stats.SelectedStrengthToolTier)
                    : baseStrengthGain * Mathf.Pow(strengthGainMultiplier, stats.SelectedStrengthToolTier - 1);
            }
        }
        private float SpeedGainPerLevel => ResolveBalanceConfig() != null
            ? ResolveBalanceConfig().SpeedGainPerLevel
            : speedGain;
        private int ResolvedMaxStrengthToolTier => ResolveBalanceConfig() != null
            ? ResolveBalanceConfig().MaxToolTier
            : Mathf.Max(1, maxStrengthToolTier);

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();
            speedShop ??= FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
            trainingBonusPrompt ??= FindFirstObjectByType<KickLuckyCubeTrainingBonusPrompt>(FindObjectsInactive.Include);
            balanceConfig ??= KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            interactionTarget.ActorInteracted.AddListener(Interact);
            RefreshLabel();
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(Interact);
            }
        }

        private void Update()
        {
            RefreshLabel();
        }

        public bool CanInteract(GameObject actor)
        {
            if (stats == null || !IsProgressionPhase())
            {
                return false;
            }

            return mode switch
            {
                StationMode.TrainStrength => true,
                StationMode.BuySpeedUpgrade => speedShop != null
                    || (wallet != null
                        && wallet.SoftCurrency >= SpeedCost),
                StationMode.BuyStrengthTool => wallet != null
                    && stats.StrengthToolTier < ResolvedMaxStrengthToolTier
                    && wallet.SoftCurrency >= ToolCost,
                _ => false
            };
        }

        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor))
            {
                RefreshLabel();
                return;
            }

            switch (mode)
            {
                case StationMode.TrainStrength:
                    var strengthGain = TrainStrengthGain;
                    stats.AddStrength(strengthGain);
                    trainingBonusPrompt?.ShowBonus(strengthGain * 2f, stats.StrengthToolTier);
                    break;
                case StationMode.BuySpeedUpgrade:
                    speedShop ??= FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
                    if (speedShop != null)
                    {
                        speedShop.OpenWindow();
                        break;
                    }

                    if (wallet.TrySpendSoft(SpeedCost))
                    {
                        stats.AddAnimalSpeed(SpeedGainPerLevel);
                    }
                    break;
                case StationMode.BuyStrengthTool:
                    if (wallet.TrySpendSoft(ToolCost))
                    {
                        stats.UpgradeStrengthTool();
                    }
                    break;
            }

            RefreshLabel();
        }

        private bool IsProgressionPhase()
        {
            return runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal);
        }

        private void RefreshLabel()
        {
            if (statusLabel == null || stats == null)
            {
                return;
            }

            statusLabel.text = mode switch
            {
                StationMode.TrainStrength => $"Train strength\n+{TrainStrengthGain:0} per hold\nTool {stats.SelectedStrengthToolTier}/{stats.StrengthToolTier}\nX for x2",
                StationMode.BuySpeedUpgrade => $"Open speed shop\nNext {SpeedCost} soft\nLv {stats.SpeedUpgradeLevel}",
                StationMode.BuyStrengthTool => stats.StrengthToolTier >= ResolvedMaxStrengthToolTier
                    ? $"Tool maxed\nLv {stats.StrengthToolTier}/{ResolvedMaxStrengthToolTier}\n+{TrainStrengthGain:0}/hold"
                    : $"Buy tool\nCost {ToolCost} soft\nNext Lv {stats.StrengthToolTier + 1}",
                _ => statusLabel.text
            };
        }

        private KickLuckyCubeBalanceConfig ResolveBalanceConfig()
        {
            balanceConfig ??= KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            return balanceConfig;
        }

        private static int ClampCostToInt(long cost)
        {
            return cost > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)cost);
        }
    }
}
