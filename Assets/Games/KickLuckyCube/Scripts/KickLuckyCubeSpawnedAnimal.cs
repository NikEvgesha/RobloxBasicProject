using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSpawnedAnimal : MonoBehaviour
    {
        [SerializeField] private string animalName;
        [SerializeField] private KickLuckyCubeRarity rarity;
        [SerializeField, Min(0)] private int sellValue;
        [SerializeField, Min(0)] private int incomePerSecond;
        [SerializeField, Min(0f)] private float runnerSpeed;
        [SerializeField] private Color bodyColor = Color.white;
        [SerializeField] private Renderer bodyRenderer;

        public string AnimalName => animalName;
        public KickLuckyCubeRarity Rarity => rarity;
        public int SellValue => sellValue;
        public int IncomePerSecond => incomePerSecond;
        public float RunnerSpeed => runnerSpeed;
        public Color BodyColor => bodyColor;

        public void Configure(KickLuckyCubeAnimalOption option, float baseRunnerSpeed)
        {
            animalName = option.AnimalName;
            rarity = option.Rarity;
            sellValue = option.SellValue;
            incomePerSecond = option.IncomePerSecond;
            runnerSpeed = Mathf.Max(0f, baseRunnerSpeed) * option.SpeedMultiplier;
            bodyColor = option.BodyColor;
            ApplyColor(option.BodyColor);
        }

        public void SetBodyRenderer(Renderer renderer)
        {
            bodyRenderer = renderer;
        }

        public void SetCarried(Transform carryAnchor)
        {
            if (carryAnchor == null)
            {
                return;
            }

            transform.SetParent(carryAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * 0.72f;
        }

        private void ApplyColor(Color color)
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer == null)
            {
                return;
            }

            var material = new Material(bodyRenderer.sharedMaterial);
            material.color = color;
            bodyRenderer.sharedMaterial = material;
        }
    }
}
