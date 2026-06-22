using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeUiWindowController : MonoBehaviour
    {
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private GameObject backdropRoot;
        [SerializeField] private Button[] openButtons;
        [SerializeField] private Button[] closeButtons;
        [SerializeField] private Button backdropButton;
        [SerializeField] private bool closeOnEscape = true;
        [SerializeField] private bool closeOnAwake = true;

        public bool IsOpen => windowRoot != null && windowRoot.activeSelf;

        private void Awake()
        {
            ApplyTheme();
            WireButtons();

            if (closeOnAwake)
            {
                CloseWindow();
            }
        }

        private void OnDestroy()
        {
            UnwireButtons();
        }

        private void Update()
        {
            if (!closeOnEscape || !IsOpen || !ReadEscapePressed())
            {
                return;
            }

            CloseWindow();
        }

        public void OpenWindow()
        {
            if (backdropRoot != null)
            {
                backdropRoot.SetActive(true);
                backdropRoot.transform.SetAsLastSibling();
            }

            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
                windowRoot.transform.SetAsLastSibling();
            }
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }

            if (backdropRoot != null)
            {
                backdropRoot.SetActive(false);
            }
        }

        private void ApplyTheme()
        {
            KickLuckyCubeUiTheme.StyleTree(windowRoot);

            if (openButtons != null)
            {
                foreach (var button in openButtons)
                {
                    if (button != null)
                    {
                        KickLuckyCubeUiTheme.StyleButton(button, button.gameObject.name);
                    }
                }
            }

            if (closeButtons != null)
            {
                foreach (var button in closeButtons)
                {
                    if (button != null)
                    {
                        KickLuckyCubeUiTheme.StyleButton(button, button.gameObject.name);
                    }
                }
            }
        }

        private void WireButtons()
        {
            if (openButtons != null)
            {
                foreach (var button in openButtons)
                {
                    if (button == null)
                    {
                        continue;
                    }

                    button.onClick.RemoveListener(OpenWindow);
                    button.onClick.AddListener(OpenWindow);
                }
            }

            if (closeButtons != null)
            {
                foreach (var button in closeButtons)
                {
                    if (button == null)
                    {
                        continue;
                    }

                    button.onClick.RemoveListener(CloseWindow);
                    button.onClick.AddListener(CloseWindow);
                }
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(CloseWindow);
                backdropButton.onClick.AddListener(CloseWindow);
            }
        }

        private void UnwireButtons()
        {
            if (openButtons != null)
            {
                foreach (var button in openButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveListener(OpenWindow);
                    }
                }
            }

            if (closeButtons != null)
            {
                foreach (var button in closeButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveListener(CloseWindow);
                    }
                }
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(CloseWindow);
            }
        }

        private static bool ReadEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
