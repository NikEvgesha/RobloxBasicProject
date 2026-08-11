using RobloxBasicProject.GameKit.Interaction;
using System;
using System.Globalization;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStablePlacementPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeStableSlot stableSlot;
        [SerializeField] private TextMesh collectLabel;
        [SerializeField] private string collectLabelResourcePath = "KickLuckyCube/World/KLC_StableCollectSpot_PendingLabel";
        [SerializeField] private Vector3 collectLabelLocalPosition = new(0f, 0.08f, 0f);
        [SerializeField] private Vector3 collectLabelLocalEuler = new(90f, 0f, 0f);
        [SerializeField, Min(1f)] private float collectLabelVisibleDistance = 22f;
        [SerializeField] private string placePromptText = "Поставить";
        [SerializeField] private string takePromptText = "Забрать";

        private GameKitInteractionTarget interactionTarget;
        private GameKitInteractionDriver interactionDriver;
        private bool ownsCollectLabel;

        private void Awake()
        {
            ResolveReferences();
            collectLabel = ResolveCollectLabel();
            if (collectLabel == null)
            {
                collectLabel = CreateCollectLabel();
                ownsCollectLabel = collectLabel != null;
            }
            ApplyCollectLabelStyle(collectLabel);
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.AddListener(Interact);
            }
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(Interact);
            }

            if (Application.isPlaying && ownsCollectLabel && collectLabel != null)
            {
                Destroy(collectLabel.gameObject);
            }
        }

        private void Update()
        {
            UpdateCollectLabelView();
        }

        public bool CanInteract(GameObject actor)
        {
            ResolveReferences();
            if (stableSlot == null)
            {
                return false;
            }

            if (stableSlot.IsOccupied)
            {
                interactionTarget?.SetPromptText(takePromptText);
                return inventory != null;
            }

            var canPlace = (runPhase != null && runPhase.HasCarriedAnimal)
                || (inventory != null && inventory.HasSelectedAnimal);
            if (canPlace)
            {
                interactionTarget?.SetPromptText(placePromptText);
            }

            return canPlace;
        }

        public void Place(GameObject actor)
        {
            Interact(actor);
        }

        private void Interact(GameObject actor)
        {
            ResolveReferences();
            if (stableSlot == null)
            {
                return;
            }

            if (stableSlot.IsOccupied)
            {
                if (inventory != null && stableSlot.TryTakeToInventory(inventory, out var collectedSoft))
                {
                    if (collectedSoft > 0)
                    {
                        wallet?.AddSoft(collectedSoft);
                    }

                    KickLuckyCubeSfxController.Play(KickLuckyCubeSfxType.Take);
                }

                return;
            }

            if (runPhase != null && runPhase.TryPlaceCarriedAnimal(stableSlot))
            {
                KickLuckyCubeSfxController.Play(KickLuckyCubeSfxType.Place);
                return;
            }

            if (inventory == null || !inventory.TryRemoveSelectedAnimal(out var selectedAnimal))
            {
                return;
            }

            if (stableSlot.TryPlace(selectedAnimal))
            {
                KickLuckyCubeSfxController.Play(KickLuckyCubeSfxType.Place);
                return;
            }

            inventory.TryAddAnimal(selectedAnimal, true);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryAutoCollect(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAutoCollect(other);
        }

        private void TryAutoCollect(Collider other)
        {
            ResolveReferences();
            if (stableSlot == null
                || wallet == null
                || !stableSlot.IsOccupied
                || stableSlot.PendingSoft <= 0
                || interactionDriver == null
                || !interactionDriver.IsActorCollider(other))
            {
                return;
            }

            var amount = stableSlot.Collect();
            if (amount > 0)
            {
                wallet.AddSoft(amount);
                UpdateCollectLabelView();
            }
        }

        private void UpdateCollectLabelView()
        {
            if (collectLabel == null)
            {
                collectLabel = ResolveCollectLabel() ?? CreateCollectLabel();
                ApplyCollectLabelStyle(collectLabel);
            }

            ResolveReferences();
            if (collectLabel == null || stableSlot == null || !stableSlot.IsOccupied)
            {
                if (collectLabel != null)
                {
                    collectLabel.gameObject.SetActive(false);
                }

                return;
            }

            var viewer = ResolveViewer();
            if (viewer != null && (viewer.position - transform.position).sqrMagnitude > collectLabelVisibleDistance * collectLabelVisibleDistance)
            {
                collectLabel.gameObject.SetActive(false);
                return;
            }

            collectLabel.gameObject.SetActive(true);
            collectLabel.text = stableSlot.PendingSoft > 0
                ? "+" + FormatAmount(stableSlot.PendingSoft)
                : "0";
            ApplyCollectLabelStyle(collectLabel);
        }

        private void ResolveReferences()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stableSlot ??= GetComponentInParent<KickLuckyCubeStableSlot>();
            interactionDriver ??= FindFirstObjectByType<GameKitInteractionDriver>(FindObjectsInactive.Include);
        }

        private TextMesh ResolveCollectLabel()
        {
            var labels = GetComponentsInChildren<TextMesh>(true);
            for (var index = 0; index < labels.Length; index++)
            {
                if (labels[index] != null && string.Equals(labels[index].name, "KLC_StableCollectSpot_PendingLabel", StringComparison.Ordinal))
                {
                    return labels[index];
                }
            }

            return null;
        }

        private TextMesh CreateCollectLabel()
        {
            var labelPrefab = Resources.Load<TextMesh>(collectLabelResourcePath);
            if (labelPrefab == null)
            {
                Debug.LogError($"[KLC-STABLE] Missing authored collect label at Resources/{collectLabelResourcePath}.", this);
                return null;
            }

            var label = Instantiate(labelPrefab, KickLuckyCubeWorldTextOutline.ResolveRuntimeLabelRoot(), false);
            label.name = "KLC_StableCollectSpot_PendingLabel";
            ownsCollectLabel = true;
            ApplyCollectLabelStyle(label);
            label.gameObject.SetActive(false);
            return label;
        }

        private static Transform ResolveViewer()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                return camera.transform;
            }

            var player = FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            return player != null ? player.transform : null;
        }

        private static string FormatAmount(int amount)
        {
            if (amount >= 1_000_000)
            {
                return (amount / 1_000_000f).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }

            return amount >= 1_000
                ? (amount / 1_000f).ToString("0.#", CultureInfo.InvariantCulture) + "K"
                : amount.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplyCollectLabelStyle(TextMesh label)
        {
            if (label == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                label.transform.SetParent(transform, false);
                label.transform.localPosition = collectLabelLocalPosition;
                label.transform.localRotation = Quaternion.Euler(collectLabelLocalEuler);
            }
            else
            {
                label.transform.position = transform.TransformPoint(collectLabelLocalPosition);
                label.transform.rotation = transform.rotation * Quaternion.Euler(collectLabelLocalEuler);
            }

            label.transform.localScale = Vector3.one;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 72;
            label.characterSize = 0.045f;
            label.lineSpacing = 0.85f;
            KickLuckyCubeUiTheme.StyleWorldText(label, Color.white, 0.009f);
        }
    }
}
