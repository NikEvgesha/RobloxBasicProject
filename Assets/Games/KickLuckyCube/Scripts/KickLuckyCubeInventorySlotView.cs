using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text badgeText;
        [SerializeField, HideInInspector] private Text legacyTitleText;
        [SerializeField, HideInInspector] private Text legacyDetailText;
        [SerializeField, HideInInspector] private Text legacyBadgeText;

        public SlotKind Kind => kind;
        public int Index => index;
        public bool CanDrag => inventory != null && inventory.CanDragFrom(this);

        public void Configure(
            KickLuckyCubeInventoryController controller,
            SlotKind slotKind,
            int slotIndex,
            Image frame,
            Image icon,
            TMP_Text title,
            TMP_Text detail,
            TMP_Text badge)
        {
            inventory = controller;
            kind = slotKind;
            index = slotIndex;
            frameImage = frame;
            iconImage = icon;
            titleText = title;
            detailText = detail;
            badgeText = badge;
            ResolveTextReferences();
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
            SetText(titleText, legacyTitleText, title);
            SetText(detailText, legacyDetailText, detail);
            SetText(badgeText, legacyBadgeText, badge);
        }

        public void SetAnimal(KickLuckyCubeInventoryAnimal animal, bool selected, Color frameColor, Color emptyFrameColor)
        {
            SetFrameColor(animal.IsValid ? ResolveAnimalFrameColor(animal, frameColor) : emptyFrameColor);

            if (!animal.IsValid)
            {
                SetVisible(false);
                SetIconSprite(null);
                SetText(titleText, legacyTitleText, string.Empty);
                SetText(detailText, legacyDetailText, string.Empty);
                SetText(badgeText, legacyBadgeText, string.Empty);
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

            SetText(titleText, legacyTitleText, animal.DisplayName);
            SetText(detailText, legacyDetailText, $"+{animal.IncomePerSecond}/s\n{KickLuckyCubeAnimalGradeUtility.GetDisplayName(animal.Grade)}");
            SetText(badgeText, legacyBadgeText, selected ? "Selected" : GetBadgeText(animal));
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

        private static void SetText(TMP_Text text, Text legacyText, string value)
        {
            if (text != null)
            {
                text.text = value;
            }

            if (legacyText != null)
            {
                legacyText.text = value;
            }
        }

        private void ApplyTheme()
        {
            ResolveTextReferences();

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
            KickLuckyCubeUiTheme.StyleText(legacyTitleText, legacyTitleText != null ? legacyTitleText.gameObject.name : string.Empty);
            KickLuckyCubeUiTheme.StyleText(legacyDetailText, legacyDetailText != null ? legacyDetailText.gameObject.name : string.Empty);
            KickLuckyCubeUiTheme.StyleText(legacyBadgeText, legacyBadgeText != null ? legacyBadgeText.gameObject.name : string.Empty);
        }

        private void ResolveTextReferences()
        {
            ResolveLabelReference("Title", ref titleText, ref legacyTitleText);
            ResolveLabelReference("Detail", ref detailText, ref legacyDetailText);
            ResolveLabelReference("Badge", ref badgeText, ref legacyBadgeText);
        }

        private void ResolveLabelReference(string childName, ref TMP_Text tmpText, ref Text legacyText)
        {
            if (tmpText != null && legacyText != null)
            {
                return;
            }

            var child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            tmpText ??= child.GetComponent<TMP_Text>();
            legacyText ??= child.GetComponent<Text>();
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
