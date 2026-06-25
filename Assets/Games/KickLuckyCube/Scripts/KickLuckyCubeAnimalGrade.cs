using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeAnimalGrade
    {
        Normal = 0,
        Golden = 1,
        Diamond = 2,
        Fire = 3
    }

    public static class KickLuckyCubeAnimalGradeUtility
    {
        public static readonly KickLuckyCubeAnimalGrade[] ProgressionGrades =
        {
            KickLuckyCubeAnimalGrade.Normal,
            KickLuckyCubeAnimalGrade.Golden,
            KickLuckyCubeAnimalGrade.Diamond,
            KickLuckyCubeAnimalGrade.Fire
        };

        public static string GetDisplayName(KickLuckyCubeAnimalGrade grade)
        {
            return grade switch
            {
                KickLuckyCubeAnimalGrade.Golden => "Golden",
                KickLuckyCubeAnimalGrade.Diamond => "Diamond",
                KickLuckyCubeAnimalGrade.Fire => "Fire",
                _ => "Normal",
            };
        }

        public static string GetShortName(KickLuckyCubeAnimalGrade grade)
        {
            return grade switch
            {
                KickLuckyCubeAnimalGrade.Golden => "Gold",
                KickLuckyCubeAnimalGrade.Diamond => "Diamond",
                KickLuckyCubeAnimalGrade.Fire => "Fire",
                _ => "Normal",
            };
        }

        public static string FormatAnimalName(string animalName, KickLuckyCubeAnimalGrade grade)
        {
            var safeName = string.IsNullOrWhiteSpace(animalName) ? "Animal" : animalName;
            return grade == KickLuckyCubeAnimalGrade.Normal
                ? safeName
                : GetDisplayName(grade) + " " + safeName;
        }

        public static int GetIncomeMultiplier(KickLuckyCubeAnimalGrade grade)
        {
            return 1;
        }

        public static int ApplyIncomeMultiplier(int baseIncome, KickLuckyCubeAnimalGrade grade)
        {
            return Mathf.Max(0, baseIncome);
        }

        public static float GetVisualHeightMultiplier(KickLuckyCubeAnimalGrade grade)
        {
            return grade switch
            {
                KickLuckyCubeAnimalGrade.Golden => 1.2f,
                KickLuckyCubeAnimalGrade.Diamond => 1.42f,
                KickLuckyCubeAnimalGrade.Fire => 1.68f,
                _ => 1f,
            };
        }

        public static Color GetTintColor(KickLuckyCubeAnimalGrade grade)
        {
            return grade switch
            {
                KickLuckyCubeAnimalGrade.Golden => new Color(1f, 0.76f, 0.12f, 1f),
                KickLuckyCubeAnimalGrade.Diamond => new Color(0.2f, 0.78f, 1f, 1f),
                KickLuckyCubeAnimalGrade.Fire => new Color(1f, 0.12f, 0.04f, 1f),
                _ => Color.white,
            };
        }

        public static Color GetGlowColor(KickLuckyCubeAnimalGrade grade)
        {
            var tint = GetTintColor(grade);
            return new Color(tint.r, tint.g, tint.b, grade == KickLuckyCubeAnimalGrade.Normal ? 0f : 0.58f);
        }

        public static Color BlendBodyColor(Color baseColor, KickLuckyCubeAnimalGrade grade)
        {
            if (grade == KickLuckyCubeAnimalGrade.Normal)
            {
                return baseColor;
            }

            return Color.Lerp(baseColor, GetTintColor(grade), 0.68f);
        }

        public static string MakeVariantId(string catalogId, KickLuckyCubeAnimalGrade grade)
        {
            var safeCatalogId = string.IsNullOrWhiteSpace(catalogId) ? "animal" : catalogId;
            return grade == KickLuckyCubeAnimalGrade.Normal
                ? safeCatalogId
                : safeCatalogId + "_" + grade.ToString().ToLowerInvariant();
        }

        public static bool TryParse(string value, out KickLuckyCubeAnimalGrade grade)
        {
            if (Enum.TryParse(value, true, out grade))
            {
                return true;
            }

            grade = KickLuckyCubeAnimalGrade.Normal;
            return false;
        }
    }
}
