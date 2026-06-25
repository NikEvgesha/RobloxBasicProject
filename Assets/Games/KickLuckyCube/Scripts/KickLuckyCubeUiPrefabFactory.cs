using UnityEngine;
using UnityEngine.UI;

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
                ResetRect(existingChild);
                return existingChild;
            }

            var kind = GuessKind(name);
            var template = LoadElementTemplate(name) ?? LoadTemplate(kind);
            RectTransform rect;

            if (template != null)
            {
                rect = Object.Instantiate(template, parent, false);
                rect.name = name;
            }
            else
            {
                var gameObject = new GameObject(name, typeof(RectTransform));
                rect = gameObject.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
            }

            ResetRect(rect);
            return rect;
        }

        public static RectTransform CreateRectInstance(string templateName, string instanceName, Transform parent)
        {
            var kind = GuessKind(templateName);
            var template = LoadElementTemplate(templateName) ?? LoadTemplate(kind);
            RectTransform rect;

            if (template != null)
            {
                rect = Object.Instantiate(template, parent, false);
                rect.name = string.IsNullOrWhiteSpace(instanceName) ? templateName : instanceName;
            }
            else
            {
                var gameObject = new GameObject(string.IsNullOrWhiteSpace(instanceName) ? templateName : instanceName, typeof(RectTransform));
                rect = gameObject.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
            }

            ResetRect(rect);
            return rect;
        }

        public static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target != null ? target.GetComponent<T>() : null;
            return component != null ? component : target.AddComponent<T>();
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
            var rect = FindDirectChild(parent, name) ?? CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

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

        public static Button GetOrCreateButton(RectTransform parent, string name, string value, Font font, Vector2 size, Color fallbackColor, int labelFontSize)
        {
            var rect = FindDirectChild(parent, name) ?? CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var image = KickLuckyCubeUiTheme.AddImage(rect.gameObject, fallbackColor);
            var button = GetOrAddComponent<Button>(rect.gameObject);
            button.targetGraphic = image;
            KickLuckyCubeUiTheme.StyleButton(button, name);
            GetOrCreateLabel(rect, "Label", font, value, labelFontSize, TextAnchor.MiddleCenter, size, Vector2.zero);
            return button;
        }

        private static RectTransform FindDirectChild(Transform parent, string childName)
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
            if (text != null)
            {
                return text;
            }

            var graphics = target.GetComponents<Graphic>();
            foreach (var graphic in graphics)
            {
                if (graphic != null && graphic is not Text)
                {
                    DestroyComponent(graphic);
                }
            }

            return target.AddComponent<Text>();
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(component);
            }
            else
            {
                Object.DestroyImmediate(component);
            }
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
    }
}
