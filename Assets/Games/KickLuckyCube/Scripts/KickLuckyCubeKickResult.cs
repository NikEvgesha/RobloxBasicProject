using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public readonly struct KickLuckyCubeKickResult
    {
        public KickLuckyCubeKickResult(
            float distance,
            Vector3 landingPosition,
            KickLuckyCubeRarityZone zone,
            KickLuckyCubeRarity rarity,
            string animalPoolText)
        {
            Distance = distance;
            LandingPosition = landingPosition;
            Zone = zone;
            Rarity = rarity;
            AnimalPoolText = animalPoolText;
        }

        public float Distance { get; }
        public Vector3 LandingPosition { get; }
        public KickLuckyCubeRarityZone Zone { get; }
        public KickLuckyCubeRarity Rarity { get; }
        public string AnimalPoolText { get; }
    }
}
