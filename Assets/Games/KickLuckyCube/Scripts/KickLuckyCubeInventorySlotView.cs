using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeInventorySlotView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public enum SlotKind
        {
            Tool,
            HotbarAnimal,
            InventoryAnimal
        }

        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private SlotKind kind;
        [SerializeField] private int index;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text badgeText;

        public SlotKind Kind => kind;
        public int Index => index;
        public bool CanDrag => inventory != null && inventory.CanDragFrom(this);

        public void Configure(
            KickLuckyCubeInventoryController controller,
            SlotKind slotKind,
            int slotIndex,
            Image frame,
            Image icon,
            Text title,
            Text detail,
            Text badge)
        {
            inventory = controller;
            kind = slotKind;
            index = slotIndex;
            frameImage = frame;
            iconImage = icon;
            titleText = title;
            detailText = detail;
            badgeText = badge;
            ApplyTheme();
        }

        public void SetIndex(int slotIndex)
        {
            index = slotIndex;
        }

        public void SetTool(string title, string detail, string badge, Color frameColor, Color iconColor)
        {
            SetVisible(true);
            SetFrameColor(frameColor);
            SetIconColor(iconColor);
            SetText(titleText, title);
            SetText(detailText, detail);
            SetText(badgeText, badge);
        }

        public void SetAnimal(KickLuckyCubeInventoryAnimal animal, bool selected, Color frameColor, Color emptyFrameColor)
        {
            SetFrameColor(animal.IsValid ? ResolveAnimalFrameColor(animal, frameColor) : emptyFrameColor);

            if (!animal.IsValid)
            {
                SetVisible(false);
                SetIconSprite(null);
                SetText(titleText, string.Empty);
                SetText(detailText, string.Empty);
                SetText(badgeText, string.Empty);
                return;
            }

            SetVisible(true);
            var icon = KickLuckyCubeAnimalCatalog.LoadIcon(animal);
            if (icon != null)
            {
                SetIconSprite(icon);
                SetIconColor(Color.white);
            }
            else
            {
                SetIconSprite(null);
                SetIconColor(animal.BodyColor);
            }

            SetText(titleText, animal.DisplayName);
            SetText(detailText, $"+{animal.IncomePerSecond}/s\n{KickLuckyCubeAnimalGradeUtility.GetDisplayName(animal.Grade)}");
            SetText(badgeText, selected ? "Selected" : GetBadgeText(animal));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inventory?.HandleSlotClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            inventory?.BeginDrag(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventory?.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventory?.EndDrag(this, eventData);
        }

        private void SetVisible(bool visible)
        {
            if (iconImage != null)
            {
                iconImage.enabled = visible;
            }
        }

        private void SetFrameColor(Color color)
        {
            if (frameImage != null)
            {
                frameImage.color = color;
            }
        }

        private void SetIconColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }

        private void SetIconSprite(Sprite sprite)
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.sprite = sprite;
            iconImage.preserveAspect = sprite != null;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void ApplyTheme()
        {
            if (frameImage != null)
            {
                KickLuckyCubeUiTheme.AddImage(frameImage.gameObject, frameImage.color);
            }

            if (iconImage != null)
            {
                KickLuckyCubeUiTheme.AddImage(iconImage.gameObject, iconImage.color);
            }

            KickLuckyCubeUiTheme.StyleText(titleText, titleText != null ? titleText.gameObject.name : string.Empty);
            KickLuckyCubeUiTheme.StyleText(detailText, detailText != null ? detailText.gameObject.name : string.Empty);
            KickLuckyCubeUiTheme.StyleText(badgeText, badgeText != null ? badgeText.gameObject.name : string.Empty);
        }

        private static Color ResolveAnimalFrameColor(KickLuckyCubeInventoryAnimal animal, Color fallback)
        {
            if (animal.Grade == KickLuckyCubeAnimalGrade.Normal)
            {
                return fallback;
            }

            return Color.Lerp(fallback, KickLuckyCubeAnimalGradeUtility.GetTintColor(animal.Grade), 0.58f);
        }

        private static string GetBadgeText(KickLuckyCubeInventoryAnimal animal)
        {
            return animal.Grade == KickLuckyCubeAnimalGrade.Normal
                ? animal.Rarity.ToString()
                : KickLuckyCubeAnimalGradeUtility.GetShortName(animal.Grade);
        }
    }
}
