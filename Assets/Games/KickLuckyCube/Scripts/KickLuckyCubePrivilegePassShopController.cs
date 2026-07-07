using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePrivilegePassShopController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeCommerceController commerce;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string shopWindowName = "KLC_ShopWindowVisual";

        private RectTransform cardRoot;
        private Text titleText;
        private Text detailText;
        private Text statusText;
        private Button buyButton;
        private Text buyButtonText;
        private Font uiFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubePrivilegePassShopController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_PrivilegePassShopController_Runtime").AddComponent<KickLuckyCubePrivilegePassShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            BuildShopCard();
            Subscribe();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (cardRoot != null && cardRoot.gameObject.activeInHierarchy)
            {
                Refresh();
            }
        }

        private void BuyPass()
        {
            ResolveReferences();
            if (commerce == null)
            {
                SetStatus("VIP pass is not ready.");
                return;
            }

            commerce.TryPurchasePrivilege(out var status);
            SetStatus(status);
            Refresh();
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            commerce ??= FindFirstObjectByType<KickLuckyCubeCommerceController>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            uiFont ??= KickLuckyCubeUiTheme.Font;
        }

        private void Subscribe()
        {
            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            if (commerce != null)
            {
                commerce.Changed += Refresh;
            }
        }

        private void Unsubscribe()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (commerce != null)
            {
                commerce.Changed -= Refresh;
            }
        }

        private void BuildShopCard()
        {
            if (cardRoot != null)
            {
                return;
            }

            var parent = ResolveShopWindowParent();
            if (parent == null)
            {
                return;
            }

            cardRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_PrivilegePassShopCard_Runtime", parent);
            cardRoot.anchorMin = new Vector2(0.5f, 0.5f);
            cardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            cardRoot.pivot = new Vector2(0.5f, 0.5f);
            if (cardRoot.sizeDelta.sqrMagnitude <= 1f)
            {
                cardRoot.sizeDelta = new Vector2(700f, 116f);
            }

            cardRoot.anchoredPosition = new Vector2(0f, -228f);
            KickLuckyCubeUiTheme.AddImage(cardRoot.gameObject, new Color(0.10f, 0.44f, 0.92f, 0.94f));
            KickLuckyCubeUiTheme.StyleTree(cardRoot.gameObject);

            titleText = CreateLabel(cardRoot, "Title", "VIP 30 Days", 24, TextAnchor.MiddleLeft, new Vector2(210f, 34f), new Vector2(-220f, 28f));
            detailText = CreateLabel(cardRoot, "Detail", string.Empty, 16, TextAnchor.MiddleLeft, new Vector2(410f, 42f), new Vector2(-110f, -10f));
            statusText = CreateLabel(cardRoot, "Status", string.Empty, 14, TextAnchor.MiddleLeft, new Vector2(310f, 24f), new Vector2(0f, 33f));
            buyButton = CreateButton(cardRoot, "BuyButton", "Buy", new Vector2(170f, 46f), new Vector2(245f, -20f));
            buyButtonText = buyButton.GetComponentInChildren<Text>();
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyPass);
        }

        private RectTransform ResolveShopWindowParent()
        {
            var target = FindTransformByName(shopWindowName);
            if (target is RectTransform rect)
            {
                return rect;
            }

            return canvas != null ? canvas.transform as RectTransform : null;
        }

        private static Transform FindTransformByName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < transforms.Length; index++)
            {
                var current = transforms[index];
                if (current != null && string.Equals(current.name, objectName, StringComparison.Ordinal))
                {
                    return current;
                }
            }

            return null;
        }

        private void Refresh()
        {
            ResolveReferences();
            if (commerce == null)
            {
                return;
            }

            BuildShopCard();

            if (titleText != null)
            {
                titleText.text = "VIP 30 Days";
            }

            if (detailText != null)
            {
                detailText.text = $"x{commerce.PrivilegeSoftMultiplier:0.#} income, +{commerce.PrivilegeHardReward} hard, no ads.";
            }

            if (statusText != null)
            {
                statusText.text = commerce.IsPrivilegeActive
                    ? $"Active: {commerce.FormatPrivilegeRemaining()}"
                    : commerce.SupportsInAppPurchases
                        ? "Buy as 30-day IAP pass."
                        : "IAP unavailable here. Uses hard currency.";
            }

            if (buyButtonText != null)
            {
                buyButtonText.text = commerce.IsPrivilegeActive
                    ? "Extend"
                    : $"Buy {commerce.PrivilegePurchasePriceText}";
            }

            if (buyButton != null)
            {
                buyButton.interactable = commerce.SupportsInAppPurchases
                    || (wallet != null && wallet.HardCurrency >= commerce.PrivilegeFallbackHardCost);
            }
        }

        private Text CreateLabel(RectTransform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, uiFont, value, fontSize, anchor, size, position);
        }

        private Button CreateButton(RectTransform parent, string name, string value, Vector2 size, Vector2 position)
        {
            var button = KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, value, uiFont, size, KickLuckyCubeUiTheme.Warning, 16);
            var rect = button.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
            }

            return button;
        }

        private void OnWalletChanged(int soft, int hard)
        {
            Refresh();
        }

        private void SetStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }
    }
}
