using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [Serializable]
    public struct KickLuckyCubeAnimalOption
    {
        [SerializeField] private KickLuckyCubeRarity rarity;
        [SerializeField] private string animalName;
        [SerializeField] private Color bodyColor;
        [SerializeField, Min(0)] private int sellValue;
        [SerializeField, Min(0)] private int incomePerSecond;
        [SerializeField, Min(0.1f)] private float speedMultiplier;

        public KickLuckyCubeAnimalOption(
            KickLuckyCubeRarity rarity,
            string animalName,
            Color bodyColor,
            int sellValue,
            int incomePerSecond,
            float speedMultiplier)
        {
            this.rarity = rarity;
            this.animalName = animalName;
            this.bodyColor = bodyColor;
            this.sellValue = sellValue;
            this.incomePerSecond = incomePerSecond;
            this.speedMultiplier = speedMultiplier;
        }

        public KickLuckyCubeRarity Rarity => rarity;
        public string AnimalName => string.IsNullOrWhiteSpace(animalName) ? rarity.ToString() + " Animal" : animalName;
        public Color BodyColor => bodyColor;
        public int SellValue => sellValue;
        public int IncomePerSecond => incomePerSecond;
        public float SpeedMultiplier => Mathf.Max(0.1f, speedMultiplier);
    }
}
