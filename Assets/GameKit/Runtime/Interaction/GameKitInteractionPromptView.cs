using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.GameKit.Interaction
{
    public sealed class GameKitInteractionPromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text keyText;
        [SerializeField] private Text labelText;
        [SerializeField] private Image progressFill;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            Hide();
        }

        public void Show(GameKitInteractionTarget target, float normalizedProgress)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (keyText != null)
            {
                keyText.text = target.PromptKey;
            }

            if (labelText != null)
            {
                labelText.text = target.PromptText;
            }

            if (progressFill != null)
            {
                progressFill.fillAmount = Mathf.Clamp01(normalizedProgress);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (progressFill != null)
            {
                progressFill.fillAmount = 0f;
            }
        }
    }
}
