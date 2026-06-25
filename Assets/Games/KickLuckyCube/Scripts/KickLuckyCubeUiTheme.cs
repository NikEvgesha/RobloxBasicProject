using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeUiButtonStyle
    {
        Primary,
        Secondary,
        Side,
        Close,
        Disabled
    }

    public enum KickLuckyCubeUiTextStyle
    {
        Title,
        Body,
        Muted,
        Button,
        Card
    }

    public static class KickLuckyCubeUiTheme
    {
        public static readonly Color Ink = new(0.06f, 0.12f, 0.16f, 1f);
        public static readonly Color Outline = new(0.015f, 0.05f, 0.065f, 0.96f);
        public static readonly Color Window = new(0.96f, 0.985f, 1f, 0.96f);
        public static readonly Color Viewport = new(0.82f, 0.94f, 1f, 0.72f);
        public static readonly Color Card = new(0.19f, 0.68f, 0.94f, 0.96f);
        public static readonly Color CardDark = new(0.12f, 0.20f, 0.27f, 0.94f);
        public static readonly Color ActionPlate = new(0.08f, 0.14f, 0.18f, 0.92f);
        public static readonly Color Primary = new(0.20f, 0.92f, 0.30f, 0.96f);
        public static readonly Color Secondary = new(0.20f, 0.68f, 0.94f, 0.96f);
        public static readonly Color Side = new(0.16f, 0.62f, 0.90f, 0.96f);
        public static readonly Color Close = new(0.96f, 0.12f, 0.22f, 0.98f);
        public static readonly Color Disabled = new(0.42f, 0.45f, 0.50f, 0.86f);
        public static readonly Color Warning = new(1f, 0.82f, 0.16f, 0.96f);
        public static readonly Color SoftCurrency = new(1f, 0.86f, 0.06f, 1f);
        public static readonly Color HardCurrency = new(0.35f, 0.95f, 1f, 1f);
        public static readonly Color Strength = new(0.24f, 1f, 0.14f, 0.95f);

        private static Font cachedFont;
        private static TMP_FontAsset cachedTmpFont;

        public static Font Font => cachedFont != null ? cachedFont : cachedFont = ResolveFont();
        public static TMP_FontAsset TmpFont => cachedTmpFont != null ? cachedTmpFont : cachedTmpFont = ResolveTmpFont();

        public static Image AddImage(GameObject target, Color color)
        {
            var image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.color = ResolveImageColor(target.name, color);
            ApplyImageEffects(target.name, image);
            return image;
        }

        public static void StyleTree(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                image.color = ResolveImageColor(image.gameObject.name, image.color);
                ApplyImageEffects(image.gameObject.name, image);
            }

            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                StyleText(text, text.gameObject.name);
            }

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                StyleText(text, text.gameObject.name);
            }

            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                StyleButton(button, button.gameObject.name);
            }
        }

        public static void StyleButton(Button button, string elementName)
        {
            if (button == null)
            {
                return;
            }

            var image = button.targetGraphic as Image;
            if (image == null)
            {
                image = button.GetComponent<Image>();
                if (image == null)
                {
                    image = button.gameObject.AddComponent<Image>();
                }

                button.targetGraphic = image;
            }

            var style = GuessButtonStyle(elementName);
            var color = GetButtonColor(style);
            if (image != null)
            {
                image.color = color;
                ApplyOutline(image, Outline, new Vector2(2.2f, -2.2f));
            }

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Disabled;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            foreach (var text in button.GetComponentsInChildren<Text>(true))
            {
                StyleText(text, text.gameObject.name);
            }

            foreach (var text in button.GetComponentsInChildren<TMP_Text>(true))
            {
                StyleText(text, text.gameObject.name);
            }
        }

        public static void StyleText(Text text, string elementName)
        {
            if (text == null)
            {
                return;
            }

            text.font = Font;
            text.fontStyle = FontStyle.Bold;
            text.alignByGeometry = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            var style = GuessTextStyle(elementName);
            switch (style)
            {
                case KickLuckyCubeUiTextStyle.Body:
                    text.color = Ink;
                    ApplyOutline(text, Color.white, new Vector2(1.2f, -1.2f));
                    break;
                case KickLuckyCubeUiTextStyle.Muted:
                    text.color = new Color(0.13f, 0.22f, 0.28f, 1f);
                    ApplyOutline(text, Color.white, new Vector2(0.9f, -0.9f));
                    break;
                case KickLuckyCubeUiTextStyle.Title:
                    text.color = Color.white;
                    ApplyOutline(text, Outline, new Vector2(2.2f, -2.2f));
                    ApplyShadow(text, new Color(0f, 0f, 0f, 0.28f), new Vector2(2.5f, -3f));
                    break;
                case KickLuckyCubeUiTextStyle.Button:
                case KickLuckyCubeUiTextStyle.Card:
                default:
                    text.color = Color.white;
                    ApplyOutline(text, Outline, new Vector2(1.8f, -1.8f));
                    break;
            }
        }

        public static void StyleHudText(Text text)
        {
            StyleText(text, "Title");
        }

        public static void StyleText(TMP_Text text, string elementName)
        {
            if (text == null)
            {
                return;
            }

            text.font = TmpFont;
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;

            var style = GuessTextStyle(elementName);
            switch (style)
            {
                case KickLuckyCubeUiTextStyle.Body:
                    text.color = Ink;
                    ApplyTmpOutline(text, Color.white, 0.10f);
                    break;
                case KickLuckyCubeUiTextStyle.Muted:
                    text.color = new Color(0.13f, 0.22f, 0.28f, 1f);
                    ApplyTmpOutline(text, Color.white, 0.08f);
                    break;
                case KickLuckyCubeUiTextStyle.Title:
                    text.color = Color.white;
                    ApplyTmpOutline(text, Outline, 0.18f);
                    break;
                case KickLuckyCubeUiTextStyle.Button:
                case KickLuckyCubeUiTextStyle.Card:
                default:
                    text.color = Color.white;
                    ApplyTmpOutline(text, Outline, 0.14f);
                    break;
            }
        }

        public static void StyleHudText(TMP_Text text)
        {
            StyleText(text, "Title");
        }

        public static void StyleFloatingText(Text text, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.font = Font;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.raycastTarget = false;
            ApplyOutline(text, Outline, new Vector2(2f, -2f));
            ApplyShadow(text, new Color(0f, 0f, 0f, 0.82f), new Vector2(3f, -3f));
        }

        public static void StyleFloatingText(TMP_Text text, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.font = TmpFont;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            ApplyTmpOutline(text, Outline, 0.16f);
        }

        public static void StyleWorldText(TextMesh textMesh, Color color, float outlineDistance)
        {
            if (textMesh == null)
            {
                return;
            }

            textMesh.fontStyle = FontStyle.Bold;
            KickLuckyCubeWorldTextOutline.ApplyTextColor(textMesh, color);

            var outline = textMesh.GetComponent<KickLuckyCubeWorldTextOutline>()
                ?? textMesh.gameObject.AddComponent<KickLuckyCubeWorldTextOutline>();
            outline.Configure(Outline, Mathf.Max(0.003f, outlineDistance));
        }

        public static Color CardColorForRarity(KickLuckyCubeRarity rarity, bool discovered = true)
        {
            if (!discovered)
            {
                return CardDark;
            }

            return rarity switch
            {
                KickLuckyCubeRarity.Common => new Color(0.20f, 0.68f, 0.94f, 0.96f),
                KickLuckyCubeRarity.Uncommon => new Color(0.18f, 0.82f, 0.45f, 0.96f),
                KickLuckyCubeRarity.Rare => new Color(0.30f, 0.54f, 1f, 0.96f),
                KickLuckyCubeRarity.Epic => new Color(0.72f, 0.35f, 1f, 0.96f),
                KickLuckyCubeRarity.Legendary => new Color(1f, 0.66f, 0.10f, 0.96f),
                _ => Card,
            };
        }

        private static Color ResolveImageColor(string elementName, Color requested)
        {
            var name = elementName ?? string.Empty;
            if (Contains(name, "Window"))
            {
                return Window;
            }

            if (Contains(name, "Viewport") || Contains(name, "Grid"))
            {
                return Viewport;
            }

            if (Contains(name, "ActionLabel"))
            {
                return ActionPlate;
            }

            if (Contains(name, "Card") || Contains(name, "Slot") || Contains(name, "Tier") || Contains(name, "Style_") || Contains(name, "EpicMob") || Contains(name, "SpeedUpgrade"))
            {
                return requested.a > 0.01f ? requested : Card;
            }

            return requested;
        }

        private static void ApplyImageEffects(string elementName, Image image)
        {
            var name = elementName ?? string.Empty;
            if (Contains(name, "Window"))
            {
                ApplyOutline(image, Outline, new Vector2(3f, -3f));
                ApplyShadow(image, new Color(0f, 0f, 0f, 0.20f), new Vector2(7f, -9f));
                return;
            }

            if (Contains(name, "Viewport") || Contains(name, "Grid") || Contains(name, "Pill"))
            {
                ApplyOutline(image, Outline, new Vector2(2f, -2f));
                return;
            }

            if (Contains(name, "Button") || Contains(name, "Close") || Contains(name, "Action") || Contains(name, "Card") || Contains(name, "Slot") || Contains(name, "Tier") || Contains(name, "Style_") || Contains(name, "Icon") || Contains(name, "Swatch") || Contains(name, "Preview") || Contains(name, "SpeedUpgrade"))
            {
                ApplyOutline(image, Outline, new Vector2(2f, -2f));
            }
        }

        private static KickLuckyCubeUiButtonStyle GuessButtonStyle(string elementName)
        {
            var name = elementName ?? string.Empty;
            if (Contains(name, "Close"))
            {
                return KickLuckyCubeUiButtonStyle.Close;
            }

            if (Contains(name, "OpenButton") || Contains(name, "Toggle"))
            {
                return KickLuckyCubeUiButtonStyle.Side;
            }

            if (Contains(name, "Action") || Contains(name, "Buy") || Contains(name, "Exchange") || Contains(name, "Sell") || Contains(name, "Spin") || Contains(name, "Claim"))
            {
                return KickLuckyCubeUiButtonStyle.Primary;
            }

            return KickLuckyCubeUiButtonStyle.Secondary;
        }

        private static KickLuckyCubeUiTextStyle GuessTextStyle(string elementName)
        {
            var name = elementName ?? string.Empty;
            if (Contains(name, "Title"))
            {
                return KickLuckyCubeUiTextStyle.Title;
            }

            if (Contains(name, "Label"))
            {
                return KickLuckyCubeUiTextStyle.Button;
            }

            if (Contains(name, "Status") || Contains(name, "Hint") || Contains(name, "Summary") || Contains(name, "Search"))
            {
                return KickLuckyCubeUiTextStyle.Body;
            }

            return KickLuckyCubeUiTextStyle.Card;
        }

        private static Color GetButtonColor(KickLuckyCubeUiButtonStyle style)
        {
            return style switch
            {
                KickLuckyCubeUiButtonStyle.Primary => Primary,
                KickLuckyCubeUiButtonStyle.Side => Side,
                KickLuckyCubeUiButtonStyle.Close => Close,
                KickLuckyCubeUiButtonStyle.Disabled => Disabled,
                _ => Secondary,
            };
        }

        private static void ApplyOutline(Graphic graphic, Color color, Vector2 distance)
        {
            if (graphic == null)
            {
                return;
            }

            var outline = graphic.GetComponent<Outline>();
            if (outline == null)
            {
                outline = graphic.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void ApplyShadow(Graphic graphic, Color color, Vector2 distance)
        {
            if (graphic == null)
            {
                return;
            }

            Shadow shadow = null;
            var shadows = graphic.GetComponents<Shadow>();
            for (var index = 0; index < shadows.Length; index++)
            {
                if (shadows[index] != null && shadows[index] is not UnityEngine.UI.Outline)
                {
                    shadow = shadows[index];
                    break;
                }
            }

            if (shadow == null)
            {
                shadow = graphic.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void ApplyTmpOutline(TMP_Text text, Color color, float width)
        {
            if (text == null)
            {
                return;
            }

            text.outlineColor = color;
            text.outlineWidth = Mathf.Max(0f, width);
        }

        private static Font ResolveFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static TMP_FontAsset ResolveTmpFont()
        {
            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF")
                ?? TMP_Settings.defaultFontAsset;
        }

        private static bool Contains(string source, string value)
        {
            return source.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
