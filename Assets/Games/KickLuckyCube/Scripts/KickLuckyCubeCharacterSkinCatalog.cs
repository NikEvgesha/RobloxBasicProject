using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeAppearanceSlot
    {
        Body,
        SkinTone,
        Torso,
        Legs,
        Boots,
        Gloves,
        Hair,
        Headwear,
        Mask
    }

    [Serializable]
    public sealed class KickLuckyCubeAppearanceItemDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private KickLuckyCubeAppearanceSlot slot;
        [SerializeField, Min(0)] private int hardCost;
        [SerializeField] private Color previewColor;
        [SerializeField] private string visualRootName;

        public KickLuckyCubeAppearanceItemDefinition(
            string id,
            string displayName,
            string description,
            KickLuckyCubeAppearanceSlot slot,
            int hardCost,
            Color previewColor,
            string visualRootName = "")
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.slot = slot;
            this.hardCost = Mathf.Max(0, hardCost);
            this.previewColor = previewColor;
            this.visualRootName = visualRootName ?? string.Empty;
        }

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public KickLuckyCubeAppearanceSlot Slot => slot;
        public int HardCost => Mathf.Max(0, hardCost);
        public Color PreviewColor => previewColor;
        public string VisualRootName => visualRootName;
        public bool IsFree => HardCost == 0;
    }

    [Serializable]
    public sealed class KickLuckyCubeAppearanceLoadout
    {
        [SerializeField] private string body;
        [SerializeField] private string skinTone;
        [SerializeField] private string torso;
        [SerializeField] private string legs;
        [SerializeField] private string boots;
        [SerializeField] private string gloves;
        [SerializeField] private string hair;
        [SerializeField] private string headwear;
        [SerializeField] private string mask;

        public string Get(KickLuckyCubeAppearanceSlot slot)
        {
            return slot switch
            {
                KickLuckyCubeAppearanceSlot.Body => body,
                KickLuckyCubeAppearanceSlot.SkinTone => skinTone,
                KickLuckyCubeAppearanceSlot.Torso => torso,
                KickLuckyCubeAppearanceSlot.Legs => legs,
                KickLuckyCubeAppearanceSlot.Boots => boots,
                KickLuckyCubeAppearanceSlot.Gloves => gloves,
                KickLuckyCubeAppearanceSlot.Hair => hair,
                KickLuckyCubeAppearanceSlot.Headwear => headwear,
                KickLuckyCubeAppearanceSlot.Mask => mask,
                _ => string.Empty,
            };
        }

        public void Set(KickLuckyCubeAppearanceSlot slot, string itemId)
        {
            itemId ??= string.Empty;
            switch (slot)
            {
                case KickLuckyCubeAppearanceSlot.Body: body = itemId; break;
                case KickLuckyCubeAppearanceSlot.SkinTone: skinTone = itemId; break;
                case KickLuckyCubeAppearanceSlot.Torso: torso = itemId; break;
                case KickLuckyCubeAppearanceSlot.Legs: legs = itemId; break;
                case KickLuckyCubeAppearanceSlot.Boots: boots = itemId; break;
                case KickLuckyCubeAppearanceSlot.Gloves: gloves = itemId; break;
                case KickLuckyCubeAppearanceSlot.Hair: hair = itemId; break;
                case KickLuckyCubeAppearanceSlot.Headwear: headwear = itemId; break;
                case KickLuckyCubeAppearanceSlot.Mask: mask = itemId; break;
            }
        }

        public KickLuckyCubeAppearanceLoadout Clone()
        {
            var clone = new KickLuckyCubeAppearanceLoadout();
            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                clone.Set(slot, Get(slot));
            }

            return clone;
        }
    }

    public static class KickLuckyCubeCharacterSkinCatalog
    {
        public const string MaleBodyId = "body_male";
        public const string FemaleBodyId = "body_female";
        public const string DefaultSkinToneId = "skin_light";

        private static readonly KickLuckyCubeAppearanceSlot[] OrderedSlots =
        {
            KickLuckyCubeAppearanceSlot.Body,
            KickLuckyCubeAppearanceSlot.SkinTone,
            KickLuckyCubeAppearanceSlot.Torso,
            KickLuckyCubeAppearanceSlot.Legs,
            KickLuckyCubeAppearanceSlot.Boots,
            KickLuckyCubeAppearanceSlot.Gloves,
            KickLuckyCubeAppearanceSlot.Hair,
            KickLuckyCubeAppearanceSlot.Headwear,
            KickLuckyCubeAppearanceSlot.Mask,
        };

        private static readonly KickLuckyCubeAppearanceItemDefinition[] Definitions =
        {
            new(MaleBodyId, "Male", "Free male mannequin body.", KickLuckyCubeAppearanceSlot.Body, 0, new Color(0.19f, 0.68f, 0.94f), "Mannequin_Male"),
            new(FemaleBodyId, "Female", "Free female mannequin body.", KickLuckyCubeAppearanceSlot.Body, 0, new Color(0.94f, 0.30f, 0.58f), "Mannequin_Female"),

            new(DefaultSkinToneId, "Light", "Free natural skin tone.", KickLuckyCubeAppearanceSlot.SkinTone, 0, new Color(0.965f, 0.765f, 0.604f)),
            new("skin_warm", "Warm", "Free natural skin tone.", KickLuckyCubeAppearanceSlot.SkinTone, 0, new Color(0.804f, 0.576f, 0.455f)),
            new("skin_deep", "Deep", "Free natural skin tone.", KickLuckyCubeAppearanceSlot.SkinTone, 0, new Color(0.29f, 0.16f, 0.12f)),
            new("skin_green", "Neon Green", "Fantasy skin tone.", KickLuckyCubeAppearanceSlot.SkinTone, 120, new Color(0.24f, 0.88f, 0.30f)),
            new("skin_blue", "Electric Blue", "Fantasy skin tone.", KickLuckyCubeAppearanceSlot.SkinTone, 160, new Color(0.16f, 0.56f, 1f)),

            new("torso_male_default", "Lucky Hoodie", "Free default male top.", KickLuckyCubeAppearanceSlot.Torso, 0, new Color(0.08f, 0.72f, 0.66f), "Torso_Current_LuckyHoodie"),
            new("torso_female_default", "Cropped Jacket", "Free default female top.", KickLuckyCubeAppearanceSlot.Torso, 0, new Color(0.94f, 0.30f, 0.58f), "Torso_Female_CroppedJacket"),
            new("torso_varsity", "Varsity", "Unisex varsity top.", KickLuckyCubeAppearanceSlot.Torso, 90, new Color(0.26f, 0.52f, 1f), "Torso_Unisex_Varsity"),

            new("legs_male_default", "Joggers", "Free default male legs.", KickLuckyCubeAppearanceSlot.Legs, 0, new Color(0.10f, 0.18f, 0.28f), "Legs_Current_Joggers"),
            new("legs_female_default", "Skort Leggings", "Free default female legs.", KickLuckyCubeAppearanceSlot.Legs, 0, new Color(0.50f, 0.17f, 0.42f), "Legs_Female_SkortLeggings"),
            new("legs_cargo", "Cargo", "Unisex cargo legs.", KickLuckyCubeAppearanceSlot.Legs, 75, new Color(0.30f, 0.36f, 0.24f), "Legs_Unisex_Cargo"),

            new("boots_male_default", "Sneakers", "Free default male footwear.", KickLuckyCubeAppearanceSlot.Boots, 0, new Color(0.08f, 0.70f, 0.68f), "Boots_Current_Sneakers"),
            new("boots_female_default", "Ankle Boots", "Free default female footwear.", KickLuckyCubeAppearanceSlot.Boots, 0, new Color(0.82f, 0.28f, 0.52f), "Boots_Female_AnkleBoots"),
            new("boots_runners", "Runners", "Unisex running shoes.", KickLuckyCubeAppearanceSlot.Boots, 70, new Color(0.25f, 0.58f, 1f), "Boots_Unisex_Runners"),

            new("gloves_male_default", "Fingerless", "Free default male gloves.", KickLuckyCubeAppearanceSlot.Gloves, 0, new Color(0.08f, 0.12f, 0.16f), "Gloves_Current_Fingerless"),
            new("gloves_female_default", "Wrist Gloves", "Free default female gloves.", KickLuckyCubeAppearanceSlot.Gloves, 0, new Color(0.78f, 0.26f, 0.52f), "Gloves_Female_Wrist"),
            new("gloves_sport", "Sport Gloves", "Unisex sport gloves.", KickLuckyCubeAppearanceSlot.Gloves, 45, new Color(0.22f, 0.62f, 0.94f), "Gloves_Unisex_Sport"),

            new("hair_male_default", "Short", "Free default male hair.", KickLuckyCubeAppearanceSlot.Hair, 0, new Color(0.12f, 0.08f, 0.06f), "Hair_Current_Short"),
            new("hair_female_default", "Ponytail", "Free default female hair.", KickLuckyCubeAppearanceSlot.Hair, 0, new Color(0.24f, 0.12f, 0.08f), "Hair_Female_Ponytail"),
            new("hair_curly", "Curly", "Unisex curly hair.", KickLuckyCubeAppearanceSlot.Hair, 60, new Color(0.18f, 0.10f, 0.06f), "Hair_Unisex_Curly"),

            new("headwear_none", "None", "No headwear.", KickLuckyCubeAppearanceSlot.Headwear, 0, new Color(0.30f, 0.36f, 0.40f)),
            new("hat_beanie", "Beanie", "Soft street beanie.", KickLuckyCubeAppearanceSlot.Headwear, 40, new Color(0.10f, 0.20f, 0.34f), "Hat_Beanie"),
            new("hat_cap", "Cap", "Sport cap.", KickLuckyCubeAppearanceSlot.Headwear, 50, new Color(0.10f, 0.70f, 0.66f), "Hat_Cap"),
            new("hat_bucket", "Bucket Hat", "Wide bucket hat.", KickLuckyCubeAppearanceSlot.Headwear, 60, new Color(0.92f, 0.62f, 0.12f), "Hat_Bucket"),

            new("mask_none", "None", "No face mask.", KickLuckyCubeAppearanceSlot.Mask, 0, new Color(0.30f, 0.36f, 0.40f)),
            new("mask_bandana", "Bandana", "Dark bandana mask.", KickLuckyCubeAppearanceSlot.Mask, 45, new Color(0.08f, 0.15f, 0.28f), "Mask_Bandana"),
            new("mask_visor", "Visor", "Bright sport visor.", KickLuckyCubeAppearanceSlot.Mask, 65, new Color(0.04f, 0.78f, 0.88f), "Mask_Visor"),
            new("mask_respirator", "Respirator", "Tech respirator mask.", KickLuckyCubeAppearanceSlot.Mask, 80, new Color(0.12f, 0.16f, 0.20f), "Mask_Respirator"),
        };

        private static readonly string[] VisualRootNames = BuildVisualRootNames();

        public static IReadOnlyList<KickLuckyCubeAppearanceSlot> SlotOrder => OrderedSlots;
        public static IReadOnlyList<KickLuckyCubeAppearanceItemDefinition> All => Definitions;
        public static IReadOnlyList<string> AllVisualRootNames => VisualRootNames;

        public static KickLuckyCubeAppearanceItemDefinition Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (var index = 0; index < Definitions.Length; index++)
            {
                if (string.Equals(Definitions[index].Id, id, StringComparison.Ordinal))
                {
                    return Definitions[index];
                }
            }

            return null;
        }

        public static IReadOnlyList<KickLuckyCubeAppearanceItemDefinition> GetForSlot(KickLuckyCubeAppearanceSlot slot)
        {
            var matches = new List<KickLuckyCubeAppearanceItemDefinition>();
            for (var index = 0; index < Definitions.Length; index++)
            {
                if (Definitions[index].Slot == slot)
                {
                    matches.Add(Definitions[index]);
                }
            }

            return matches;
        }

        public static string GetDefaultId(KickLuckyCubeAppearanceSlot slot, bool female)
        {
            return slot switch
            {
                KickLuckyCubeAppearanceSlot.Body => female ? FemaleBodyId : MaleBodyId,
                KickLuckyCubeAppearanceSlot.SkinTone => DefaultSkinToneId,
                KickLuckyCubeAppearanceSlot.Torso => female ? "torso_female_default" : "torso_male_default",
                KickLuckyCubeAppearanceSlot.Legs => female ? "legs_female_default" : "legs_male_default",
                KickLuckyCubeAppearanceSlot.Boots => female ? "boots_female_default" : "boots_male_default",
                KickLuckyCubeAppearanceSlot.Gloves => female ? "gloves_female_default" : "gloves_male_default",
                KickLuckyCubeAppearanceSlot.Hair => female ? "hair_female_default" : "hair_male_default",
                KickLuckyCubeAppearanceSlot.Headwear => "headwear_none",
                KickLuckyCubeAppearanceSlot.Mask => "mask_none",
                _ => string.Empty,
            };
        }

        public static KickLuckyCubeAppearanceLoadout CreateDefaultLoadout(bool female = false)
        {
            var loadout = new KickLuckyCubeAppearanceLoadout();
            foreach (var slot in OrderedSlots)
            {
                loadout.Set(slot, GetDefaultId(slot, female));
            }

            return loadout;
        }

        public static KickLuckyCubeAppearanceLoadout CreateRandomLoadout(int seed, bool includePremium = true)
        {
            var random = new System.Random(seed);
            var loadout = new KickLuckyCubeAppearanceLoadout();
            foreach (var slot in OrderedSlots)
            {
                var candidates = GetForSlot(slot);
                var eligible = new List<KickLuckyCubeAppearanceItemDefinition>();
                for (var index = 0; index < candidates.Count; index++)
                {
                    if (includePremium || candidates[index].IsFree)
                    {
                        eligible.Add(candidates[index]);
                    }
                }

                var selected = eligible[random.Next(eligible.Count)];
                loadout.Set(slot, selected.Id);
            }

            return loadout;
        }

        public static bool IsFemaleBody(string bodyId)
        {
            return string.Equals(bodyId, FemaleBodyId, StringComparison.Ordinal);
        }

        public static string GetSlotDisplayName(KickLuckyCubeAppearanceSlot slot)
        {
            return slot switch
            {
                KickLuckyCubeAppearanceSlot.Body => "Body",
                KickLuckyCubeAppearanceSlot.SkinTone => "Skin",
                KickLuckyCubeAppearanceSlot.Torso => "Torso",
                KickLuckyCubeAppearanceSlot.Legs => "Legs",
                KickLuckyCubeAppearanceSlot.Boots => "Boots",
                KickLuckyCubeAppearanceSlot.Gloves => "Gloves",
                KickLuckyCubeAppearanceSlot.Hair => "Hair",
                KickLuckyCubeAppearanceSlot.Headwear => "Hats",
                KickLuckyCubeAppearanceSlot.Mask => "Masks",
                _ => slot.ToString(),
            };
        }

        public static string GetTabIconResourcePath(KickLuckyCubeAppearanceSlot slot)
        {
            return "KickLuckyCube/UI/SkinTabs/KLC_SkinTab_" + slot;
        }

        public static string GetItemIconResourcePath(string itemId)
        {
            return "KickLuckyCube/UI/SkinItems/KLC_SkinItem_" + itemId;
        }

        public static bool RequiresWardrobeIcon(KickLuckyCubeAppearanceSlot slot)
        {
            return slot is KickLuckyCubeAppearanceSlot.Torso
                or KickLuckyCubeAppearanceSlot.Legs
                or KickLuckyCubeAppearanceSlot.Boots
                or KickLuckyCubeAppearanceSlot.Gloves
                or KickLuckyCubeAppearanceSlot.Hair
                or KickLuckyCubeAppearanceSlot.Headwear
                or KickLuckyCubeAppearanceSlot.Mask;
        }

        public static IReadOnlyList<string> ValidateCatalog()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions)
            {
                if (string.IsNullOrWhiteSpace(definition.Id) || !ids.Add(definition.Id))
                {
                    errors.Add("Missing or duplicate appearance id: " + definition.Id);
                }
            }

            foreach (var slot in OrderedSlots)
            {
                if (GetForSlot(slot).Count == 0)
                {
                    errors.Add("Appearance slot has no items: " + slot);
                }

                var maleDefault = Get(GetDefaultId(slot, false));
                var femaleDefault = Get(GetDefaultId(slot, true));
                if (maleDefault == null || maleDefault.Slot != slot || !maleDefault.IsFree)
                {
                    errors.Add("Invalid free male default for slot: " + slot);
                }

                if (femaleDefault == null || femaleDefault.Slot != slot || !femaleDefault.IsFree)
                {
                    errors.Add("Invalid free female default for slot: " + slot);
                }
            }

            if (Get("skin_green")?.HardCost <= 0 || Get("skin_blue")?.HardCost <= 0)
            {
                errors.Add("Fantasy skin tones must cost hard currency.");
            }

            var iconAssets = new HashSet<Sprite>();
            foreach (var definition in Definitions)
            {
                if (!RequiresWardrobeIcon(definition.Slot))
                {
                    continue;
                }

                var icon = Resources.Load<Sprite>(GetItemIconResourcePath(definition.Id));
                if (icon == null)
                {
                    errors.Add("Missing wardrobe icon: " + definition.Id);
                }
                else if (!iconAssets.Add(icon))
                {
                    errors.Add("Duplicate wardrobe icon asset: " + definition.Id);
                }
            }

            return errors;
        }

        private static string[] BuildVisualRootNames()
        {
            var names = new List<string>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions)
            {
                if (!string.IsNullOrWhiteSpace(definition.VisualRootName) && unique.Add(definition.VisualRootName))
                {
                    names.Add(definition.VisualRootName);
                }
            }

            return names.ToArray();
        }
    }
}
