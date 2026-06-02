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
            SetFrameColor(animal.IsValid ? frameColor : emptyFrameColor);

            if (!animal.IsValid)
            {
                SetVisible(false);
                SetText(titleText, string.Empty);
                SetText(detailText, string.Empty);
                SetText(badgeText, string.Empty);
                return;
            }

            SetVisible(true);
            SetIconColor(animal.BodyColor);
            SetText(titleText, animal.AnimalName);
            SetText(detailText, $"+{animal.IncomePerSecond}/s\n${animal.SellValue}");
            SetText(badgeText, selected ? "Selected" : animal.Rarity.ToString());
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

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
