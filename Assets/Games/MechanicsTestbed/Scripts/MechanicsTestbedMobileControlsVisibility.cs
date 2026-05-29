using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MechanicsTestbedMobileControlsVisibility : MonoBehaviour
    {
        [SerializeField] private bool forceVisible;
        [SerializeField] private bool showOnNarrowScreens;
        [SerializeField] private int mobileScreenWidthThreshold = 980;
        [SerializeField] private float refreshInterval = 0.5f;

        private CanvasGroup canvasGroup;
        private Graphic[] graphics;
        private float nextRefreshTime;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            graphics = GetComponentsInChildren<Graphic>(true);
            RefreshVisibility();
        }

        private void OnEnable()
        {
            RefreshVisibility();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + refreshInterval;
            if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            {
                return;
            }

            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            var visible = ShouldShowMobileControls();
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            graphics ??= GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                if (graphic != null)
                {
                    graphic.enabled = visible;
                }
            }
        }

        private bool ShouldShowMobileControls()
        {
            return forceVisible
                || Application.isMobilePlatform
                || SystemInfo.deviceType == DeviceType.Handheld
                || (showOnNarrowScreens && Screen.width <= mobileScreenWidthThreshold);
        }
    }
}
