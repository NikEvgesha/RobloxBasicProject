using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InvalidOperationException = System.InvalidOperationException;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal enum KickLuckyCubeUiTemplateKind
    {
        Empty,
        Backdrop,
        Window,
        Viewport,
        Card,
        ButtonFrame,
        IconFrame
    }

    internal static class KickLuckyCubeUiPrefabFactory
    {
        private const string ElementRoot = "KickLuckyCube/UI/Elements/";
        private const string TemplateRoot = "KickLuckyCube/UI/Templates/";

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var existingChild = FindDirectChild(parent, name);
            if (existingChild != null)
            {
                return existingChild;
            }

            var kind = GuessKind(name);
            var elementTemplate = LoadElementTemplate(name);
            var template = elementTemplate != null ? elementTemplate : LoadTemplate(kind);
            if (template == null)
            {
                throw MissingPrefab(name);
            }

            var rect = Object.Instantiate(template, parent, false);
            rect.name = name;

            if (elementTemplate == null)
            {
                ResetRect(rect);
            }

            return rect;
        }

        public static RectTransform CreateRectInstance(string templateName, string instanceName, Transform parent)
        {
            var kind = GuessKind(templateName);
            var elementTemplate = LoadElementTemplate(templateName);
            var template = elementTemplate != null ? elementTemplate : LoadTemplate(kind);
            if (template == null)
            {
                throw MissingPrefab(templateName);
            }

            var rect = Object.Instantiate(template, parent, false);
            rect.name = string.IsNullOrWhiteSpace(instanceName) ? templateName : instanceName;

            if (elementTemplate == null)
            {
                ResetRect(rect);
            }

            return rect;
        }

        public static bool HasAuthoredChildren(RectTransform rect)
        {
            return rect != null && rect.childCount > 0;
        }

        public static RectTransform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (parent.name == childName && parent is RectTransform parentRect)
            {
                return parentRect;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var found = FindChildRecursive(parent.GetChild(index), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static Text FindText(Transform parent, string childName)
        {
            var rect = FindChildRecursive(parent, childName);
            return rect != null ? rect.GetComponent<Text>() : null;
        }

        public static Button FindButton(Transform parent, string childName)
        {
            var rect = FindChildRecursive(parent, childName);
            return rect != null ? rect.GetComponent<Button>() : null;
        }

        public static Image FindImage(Transform parent, string childName)
        {
            var rect = FindChildRecursive(parent, childName);
            return rect != null ? rect.GetComponent<Image>() : null;
        }

        public static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target != null ? target.GetComponent<T>() : null;
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Authored UI object '{target?.name ?? "<null>"}' is missing required component {typeof(T).Name}. Fix its prefab instead of adding the component at runtime.");
            }

            return component;
        }

        public static T GetRequiredComponent<T>(GameObject target)
            where T : Component
        {
            return GetOrAddComponent<T>(target);
        }

        public static Canvas ResolveMainCanvas(Canvas current = null)
        {
            if (current != null && !IsRuntimeFxCanvas(current))
            {
                return current;
            }

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < canvases.Length; index++)
            {
                var canvas = canvases[index];
                if (canvas != null && canvas.name == "KLC_PrototypeCanvas")
                {
                    return canvas;
                }
            }

            for (var index = 0; index < canvases.Length; index++)
            {
                var canvas = canvases[index];
                if (canvas != null && !IsRuntimeFxCanvas(canvas))
                {
                    return canvas;
                }
            }

            return current;
        }

        public static Text GetOrCreateLabel(RectTransform parent, string name, Font font, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            var existing = FindDirectChild(parent, name);
            var rect = existing ?? CreateRect(name, parent);
            if (existing == null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
            }

            var text = GetOrAddText(rect.gameObject);
            text.text = value;
            text.font = font != null ? font : KickLuckyCubeUiTheme.Font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            KickLuckyCubeUiTheme.StyleText(text, name);
            return text;
        }

        public static TMP_Text GetOrCreateTmpLabel(RectTransform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            var existing = FindDirectChild(parent, name);
            var rect = existing ?? CreateRect(name, parent);
            if (existing == null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
            }

            var text = GetOrAddTmpText(rect.gameObject);

            text.text = value;
            text.font = KickLuckyCubeUiTheme.TmpFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = ToTmpAlignment(anchor);
            text.color = Color.white;
            text.raycastTarget = false;
            KickLuckyCubeUiTheme.StyleText(text, name);
            return text;
        }

        public static Button GetOrCreateButton(RectTransform parent, string name, string value, Font font, Vector2 size, Color fallbackColor, int labelFontSize)
        {
            var existing = FindDirectChild(parent, name);
            var rect = existing ?? CreateRect(name, parent);
            if (existing == null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
            }

            var image = KickLuckyCubeUiTheme.AddImage(rect.gameObject, fallbackColor);
            var button = GetOrAddComponent<Button>(rect.gameObject);
            button.targetGraphic = image;
            KickLuckyCubeUiTheme.StyleButton(button, name);
            var labelRect = FindDirectChild(rect, "Label");
            if (labelRect != null && labelRect.GetComponent<TMP_Text>() != null)
            {
                GetOrCreateTmpLabel(rect, "Label", value, labelFontSize, TextAnchor.MiddleCenter, size, Vector2.zero);
            }
            else if (labelRect != null && labelRect.GetComponent<Text>() != null)
            {
                GetOrCreateLabel(rect, "Label", font, value, labelFontSize, TextAnchor.MiddleCenter, size, Vector2.zero);
            }
            else
            {
                // Icon-only buttons intentionally own no text child.
            }
            return button;
        }

        public static RectTransform FindDirectChild(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var child = parent.GetChild(index);
                if (child != null && child.name == childName && child is RectTransform rect)
                {
                    return rect;
                }
            }

            return null;
        }

        private static RectTransform LoadElementTemplate(string elementName)
        {
            var sanitized = SanitizeResourceName(elementName);
            if (string.IsNullOrEmpty(sanitized))
            {
                return null;
            }

            return Resources.Load<RectTransform>(ElementRoot + sanitized)
                ?? Resources.Load<RectTransform>(ElementRoot + ResolveFamilyTemplateName(sanitized));
        }

        private static Text GetOrAddText(GameObject target)
        {
            var text = target.GetComponent<Text>();
            if (text == null)
            {
                throw new InvalidOperationException(
                    $"Authored UI object '{target.name}' is missing a legacy Text component.");
            }

            return text;
        }

        private static TMP_Text GetOrAddTmpText(GameObject target)
        {
            var text = target.GetComponent<TMP_Text>();
            if (text == null)
            {
                throw new InvalidOperationException(
                    $"Authored UI object '{target.name}' is missing a TMP_Text component.");
            }

            return text;
        }

        private static TextAlignmentOptions ToTmpAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center,
            };
        }

        private static RectTransform LoadTemplate(KickLuckyCubeUiTemplateKind kind)
        {
            if (kind == KickLuckyCubeUiTemplateKind.Empty)
            {
                return null;
            }

            return Resources.Load<RectTransform>(TemplateRoot + GetTemplateName(kind));
        }

        private static string GetTemplateName(KickLuckyCubeUiTemplateKind kind)
        {
            return kind switch
            {
                KickLuckyCubeUiTemplateKind.Backdrop => "KLC_UiBackdropFrame",
                KickLuckyCubeUiTemplateKind.Window => "KLC_UiWindowFrame",
                KickLuckyCubeUiTemplateKind.Viewport => "KLC_UiViewportFrame",
                KickLuckyCubeUiTemplateKind.Card => "KLC_UiCardFrame",
                KickLuckyCubeUiTemplateKind.ButtonFrame => "KLC_UiButtonFrame",
                KickLuckyCubeUiTemplateKind.IconFrame => "KLC_UiIconFrame",
                _ => string.Empty,
            };
        }

        private static KickLuckyCubeUiTemplateKind GuessKind(string elementName)
        {
            var name = elementName ?? string.Empty;
            if (Contains(name, "Backdrop"))
            {
                return KickLuckyCubeUiTemplateKind.Backdrop;
            }

            if (Contains(name, "Window"))
            {
                return KickLuckyCubeUiTemplateKind.Window;
            }

            if (Contains(name, "Viewport") || Contains(name, "Grid") || Contains(name, "Strip"))
            {
                return KickLuckyCubeUiTemplateKind.Viewport;
            }

            if (Contains(name, "Button") || Contains(name, "Close") || Contains(name, "Action") || Contains(name, "Buy") || Contains(name, "Claim"))
            {
                return KickLuckyCubeUiTemplateKind.ButtonFrame;
            }

            if (Contains(name, "Icon") || Contains(name, "Preview") || Contains(name, "Swatch"))
            {
                return KickLuckyCubeUiTemplateKind.IconFrame;
            }

            if (Contains(name, "Card") || Contains(name, "Slot") || Contains(name, "Tier") || Contains(name, "Style_") || Contains(name, "EpicMob") || Contains(name, "SpeedUpgrade") || Contains(name, "Pill"))
            {
                return KickLuckyCubeUiTemplateKind.Card;
            }

            return KickLuckyCubeUiTemplateKind.Empty;
        }

        private static void ResetRect(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition3D = Vector3.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static bool Contains(string value, string token)
        {
            return value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string SanitizeResourceName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value.ToCharArray();
            for (var index = 0; index < chars.Length; index++)
            {
                var c = chars[index];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    chars[index] = '_';
                }
            }

            return new string(chars);
        }

        private static string ResolveFamilyTemplateName(string sanitizedName)
        {
            if (StartsWith(sanitizedName, "SpeedUpgradePlus_"))
            {
                return "SpeedUpgradePlus";
            }

            if (StartsWith(sanitizedName, "ToolTier_"))
            {
                return "ToolTier";
            }

            if (StartsWith(sanitizedName, "Style_"))
            {
                return "StyleCard";
            }

            if (StartsWith(sanitizedName, "EpicMob_"))
            {
                return "EpicMob";
            }

            if (StartsWith(sanitizedName, "KLC_AlbumCard_"))
            {
                return "KLC_AlbumCard";
            }

            if (StartsWith(sanitizedName, "KLC_SellShopCard_"))
            {
                return "KLC_SellShopCard";
            }

            if (StartsWith(sanitizedName, "KLC_InventorySlot_"))
            {
                return "KLC_InventorySlot";
            }

            if (StartsWith(sanitizedName, "KLC_InventoryCategory_"))
            {
                return "KLC_InventoryCategory";
            }

            if (StartsWith(sanitizedName, "KLC_PowerBand_"))
            {
                return "KLC_PowerBand";
            }

            if (StartsWith(sanitizedName, "KLC_WaveDangerVignette_"))
            {
                return "KLC_WaveDangerVignette_Edge";
            }

            if (Contains(sanitizedName, "Label")
                || Contains(sanitizedName, "Text")
                || Contains(sanitizedName, "Title")
                || Contains(sanitizedName, "Status")
                || Contains(sanitizedName, "Detail")
                || Contains(sanitizedName, "Requirement")
                || Contains(sanitizedName, "Timer")
                || Contains(sanitizedName, "Charges")
                || Contains(sanitizedName, "Price")
                || Contains(sanitizedName, "Value")
                || Contains(sanitizedName, "Income")
                || Contains(sanitizedName, "Level")
                || Contains(sanitizedName, "Name")
                || Contains(sanitizedName, "Rarity")
                || Contains(sanitizedName, "Hint")
                || Contains(sanitizedName, "Summary")
                || Contains(sanitizedName, "Source"))
            {
                return "Label";
            }

            return sanitizedName;
        }

        private static bool StartsWith(string value, string prefix)
        {
            return value.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRuntimeFxCanvas(Canvas canvas)
        {
            return canvas != null && canvas.name == "KLC_CurrencyFxCanvas_Runtime";
        }

        private static InvalidOperationException MissingPrefab(string elementName)
        {
            return new InvalidOperationException(
                $"No authored UI prefab exists for '{elementName}'. Add it under Resources/{ElementRoot} instead of constructing visual UI at runtime.");
        }
    }
}
