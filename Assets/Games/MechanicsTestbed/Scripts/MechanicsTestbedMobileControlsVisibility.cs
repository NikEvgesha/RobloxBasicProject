using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MechanicsTestbedMobileControlsVisibility : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("forceVisible")] private bool simulateMobileControls;
        [SerializeField] private bool showOnNarrowScreens;
        [SerializeField] private int mobileScreenWidthThreshold = 980;
        [SerializeField] private bool hideWhenDesktopInputIsPresent = true;
        [SerializeField] private MechanicsTestbedMobileInput mobileInput;
        [SerializeField] private float refreshInterval = 0.5f;

        private CanvasGroup canvasGroup;
        private Graphic[] graphics;
        private bool isVisible;
        private float nextRefreshTime;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            graphics = GetComponentsInChildren<Graphic>(true);

            if (mobileInput == null)
            {
                mobileInput = GetComponentInChildren<MechanicsTestbedMobileInput>(true);
            }

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
            if (isVisible && !visible)
            {
                mobileInput?.Clear();
            }

            isVisible = visible;
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
            if (simulateMobileControls)
            {
                return true;
            }

            if (IsDesktopEditorOrPlayer())
            {
                return false;
            }

            if (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld)
            {
                return true;
            }

            if (hideWhenDesktopInputIsPresent && HasDesktopInputDevice())
            {
                return false;
            }

            return showOnNarrowScreens && Screen.width <= mobileScreenWidthThreshold;
        }

        private static bool IsDesktopEditorOrPlayer()
        {
            return Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.LinuxEditor
                || Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.OSXPlayer
                || Application.platform == RuntimePlatform.LinuxPlayer;
        }

        private static bool HasDesktopInputDevice()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null || Mouse.current != null;
#else
            return !Input.touchSupported;
#endif
        }
    }
}
