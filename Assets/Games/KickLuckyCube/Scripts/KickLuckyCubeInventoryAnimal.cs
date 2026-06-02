using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [Serializable]
    public struct KickLuckyCubeInventoryAnimal
    {
        [SerializeField] private string id;
        [SerializeField] private string animalName;
        [SerializeField] private KickLuckyCubeRarity rarity;
        [SerializeField] private Color bodyColor;
        [SerializeField, Min(0)] private int sellValue;
        [SerializeField, Min(0)] private int incomePerSecond;

        public KickLuckyCubeInventoryAnimal(
            string id,
            string animalName,
            KickLuckyCubeRarity rarity,
            Color bodyColor,
            int sellValue,
            int incomePerSecond)
        {
            this.id = id;
            this.animalName = animalName;
            this.rarity = rarity;
            this.bodyColor = bodyColor;
            this.sellValue = Mathf.Max(0, sellValue);
            this.incomePerSecond = Mathf.Max(0, incomePerSecond);
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(id);
        public string Id => id;
        public string AnimalName => string.IsNullOrWhiteSpace(animalName) ? "Animal" : animalName;
        public KickLuckyCubeRarity Rarity => rarity;
        public Color BodyColor => bodyColor;
        public int SellValue => sellValue;
        public int IncomePerSecond => incomePerSecond;

        public static KickLuckyCubeInventoryAnimal FromSpawnedAnimal(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null)
            {
                return default;
            }

            return new KickLuckyCubeInventoryAnimal(
                Guid.NewGuid().ToString("N"),
                animal.AnimalName,
                animal.Rarity,
                animal.BodyColor,
                animal.SellValue,
                animal.IncomePerSecond);
        }
    }
}
