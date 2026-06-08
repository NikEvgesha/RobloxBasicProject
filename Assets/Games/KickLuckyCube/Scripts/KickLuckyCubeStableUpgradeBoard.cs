using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeStableUpgradeBoard : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeStableSlot stableSlot;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private TextMesh statusLabel;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            ResolveReferences();
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            interactionTarget.ActorInteracted.AddListener(Upgrade);
            RefreshLabel();
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(Upgrade);
            }
        }

        private void Update()
        {
            RefreshLabel();
        }

        public void Configure(KickLuckyCubeStableSlot targetSlot)
        {
            stableSlot = targetSlot;
            ResolveReferences();
            RefreshLabel();
        }

        public bool CanInteract(GameObject actor)
        {
            ResolveReferences();
            return stableSlot != null
                && wallet != null
                && stableSlot.CanUpgrade
                && wallet.SoftCurrency >= stableSlot.NextUpgradeCost
                && IsProgressionPhase();
        }

        private void Upgrade(GameObject actor)
        {
            ResolveReferences();
            stableSlot?.TryUpgrade(wallet);
            RefreshLabel();
        }

        private bool IsProgressionPhase()
        {
            return runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal);
        }

        private void ResolveReferences()
        {
            if (stableSlot == null)
            {
                stableSlot = GetComponentInParent<KickLuckyCubeStableSlot>();
            }

            if (wallet == null)
            {
                wallet = FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            }

            if (runPhase == null)
            {
                runPhase = FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            }

            if (statusLabel == null)
            {
                var existingLabel = GetComponentInChildren<TextMesh>(true);
                statusLabel = existingLabel != null ? existingLabel : CreateStatusLabel();
            }
        }

        private void RefreshLabel()
        {
            if (statusLabel == null)
            {
                return;
            }

            if (stableSlot == null)
            {
                statusLabel.text = "Stable upgrade\nNo slot";
                return;
            }

            statusLabel.text = stableSlot.CanUpgrade
                ? $"Upgrade slot\nLv {stableSlot.UpgradeLevel}->{stableSlot.UpgradeLevel + 1}\n{stableSlot.NextUpgradeCost} soft\nx{stableSlot.IncomeMultiplier:0.0}->x{stableSlot.NextIncomeMultiplier:0.0}"
                : $"Stable slot\nLv {stableSlot.UpgradeLevel}/{stableSlot.MaxUpgradeLevel}\nx{stableSlot.IncomeMultiplier:0.0}\nMAX";
        }

        private TextMesh CreateStatusLabel()
        {
            var labelObject = new GameObject("KLC_StableUpgradeBoard_StatusLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.55f, -0.04f);
            labelObject.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            labelObject.transform.localScale = Vector3.one;

            var label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 32;
            label.characterSize = 0.045f;
            label.color = Color.white;
            return label;
        }
    }
}
