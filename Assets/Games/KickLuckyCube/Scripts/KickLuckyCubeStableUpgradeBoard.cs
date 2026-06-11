using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStableUpgradeBoard : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeStableSlot stableSlot;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField, Min(1f)] private float clickRayDistance = 200f;
        [SerializeField, Min(0.01f)] private float labelCharacterSizeToBoardHeight = 0.075f;

        private bool ownsStatusLabel;

        private void Awake()
        {
            ResolveReferences();
            RefreshLabel();
        }

        private void Update()
        {
            if (ReadClickScreenPosition(out var screenPosition))
            {
                TryUpgradeFromClick(screenPosition);
            }

            RefreshLabel();
            ApplyBoardLabelStyle(statusLabel);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && ownsStatusLabel && statusLabel != null)
            {
                Destroy(statusLabel.gameObject);
            }
        }

        public void Configure(KickLuckyCubeStableSlot targetSlot)
        {
            stableSlot = targetSlot;
            ResolveReferences();
            RefreshLabel();
        }

        private bool CanUpgradeNow()
        {
            ResolveReferences();
            return stableSlot != null
                && wallet != null
                && stableSlot.CanUpgrade
                && wallet.SoftCurrency >= stableSlot.NextUpgradeCost
                && IsProgressionPhase();
        }

        private void Upgrade()
        {
            ResolveReferences();
            if (stableSlot != null && stableSlot.TryUpgrade(wallet))
            {
                KickLuckyCubeSfxController.Play(KickLuckyCubeSfxType.Upgrade);
            }

            RefreshLabel();
        }

        private void TryUpgradeFromClick(Vector2 screenPosition)
        {
            if (IsPointerOverUi())
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var ray = camera.ScreenPointToRay(screenPosition);
            var hits = Physics.RaycastAll(ray, clickRayDistance, ~0, QueryTriggerInteraction.Collide);
            for (var index = 0; index < hits.Length; index++)
            {
                var hitTransform = hits[index].collider != null ? hits[index].collider.transform : null;
                if (hitTransform == null || (hitTransform != transform && !hitTransform.IsChildOf(transform)))
                {
                    continue;
                }

                if (CanUpgradeNow())
                {
                    Upgrade();
                }
                else
                {
                    KickLuckyCubeSfxController.Play(KickLuckyCubeSfxType.Deny);
                }

                return;
            }
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
                var existingLabel = ResolveStatusLabel();
                statusLabel = existingLabel != null ? existingLabel : CreateStatusLabel();
                ownsStatusLabel = statusLabel != null;
            }

            ApplyBoardLabelStyle(statusLabel);
        }

        private void RefreshLabel()
        {
            if (statusLabel == null)
            {
                return;
            }

            if (stableSlot == null)
            {
                statusLabel.text = "No slot";
                return;
            }

            statusLabel.text = $"Lv {stableSlot.UpgradeLevel}\n{FormatCost(stableSlot.NextUpgradeCost)}";
        }

        private TextMesh CreateStatusLabel()
        {
            var labelObject = new GameObject("KLC_StableUpgradeBoard_StatusLabel");
            labelObject.transform.SetParent(KickLuckyCubeWorldTextOutline.ResolveRuntimeLabelRoot(), false);

            var label = labelObject.AddComponent<TextMesh>();
            ownsStatusLabel = true;
            ApplyBoardLabelStyle(label);
            return label;
        }

        private TextMesh ResolveStatusLabel()
        {
            var labels = GetComponentsInChildren<TextMesh>(true);
            for (var index = 0; index < labels.Length; index++)
            {
                var label = labels[index];
                if (label != null && string.Equals(label.name, "KLC_StableUpgradeBoard_StatusLabel", StringComparison.Ordinal))
                {
                    return label;
                }
            }

            for (var index = 0; index < labels.Length; index++)
            {
                var label = labels[index];
                if (label != null && !label.name.StartsWith("Outline_", StringComparison.Ordinal))
                {
                    return label;
                }
            }

            return null;
        }

        private static string FormatCost(int cost)
        {
            if (cost >= 1_000_000)
            {
                return (cost / 1_000_000f).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }

            return cost >= 1_000
                ? (cost / 1_000f).ToString("0.#", CultureInfo.InvariantCulture) + "K"
                : cost.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplyBoardLabelStyle(TextMesh label)
        {
            if (label == null)
            {
                return;
            }

            var localPosition = new Vector3(0f, 0.68f, -0.075f);
            var localRotation = Quaternion.Euler(60f, 0f, 0f);
            if (Application.isPlaying)
            {
                label.transform.SetParent(KickLuckyCubeWorldTextOutline.ResolveRuntimeLabelRoot(), false);
                label.transform.position = transform.TransformPoint(localPosition);
                label.transform.rotation = transform.rotation * localRotation;
            }
            else
            {
                label.transform.SetParent(transform, false);
                label.transform.localPosition = localPosition;
                label.transform.localRotation = localRotation;
            }

            label.transform.localScale = Vector3.one;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 96;
            label.characterSize = ResolveBoardLabelCharacterSize();
            label.lineSpacing = 0.62f;
            KickLuckyCubeWorldTextOutline.ApplyTextColor(label, Color.white);

            var outline = label.GetComponent<KickLuckyCubeWorldTextOutline>()
                ?? label.gameObject.AddComponent<KickLuckyCubeWorldTextOutline>();
            outline.Configure(Color.black, Mathf.Max(0.004f, label.characterSize * 0.09f));
        }

        private float ResolveBoardLabelCharacterSize()
        {
            if (!TryGetBoardMeshBounds(out var bounds))
            {
                return 0.065f;
            }

            return Mathf.Clamp(bounds.size.y * labelCharacterSizeToBoardHeight, 0.04f, 0.065f);
        }

        private bool TryGetBoardMeshBounds(out Bounds bounds)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            var renderers = GetComponentsInChildren<MeshRenderer>(true);
            var hasBounds = false;
            foreach (var current in renderers)
            {
                if (current == null || current.GetComponent<TextMesh>() != null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = current.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(current.bounds);
                }
            }

            return hasBounds;
        }

        private static bool ReadClickScreenPosition(out Vector2 screenPosition)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }
#endif

            screenPosition = Vector2.zero;
            return false;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
