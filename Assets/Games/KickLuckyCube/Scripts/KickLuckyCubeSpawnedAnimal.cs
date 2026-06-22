using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSpawnedAnimal : MonoBehaviour
    {
        [SerializeField] private string animalName;
        [SerializeField] private string catalogId;
        [SerializeField] private KickLuckyCubeRarity rarity;
        [SerializeField, Min(0)] private int sellValue;
        [SerializeField, Min(0)] private int incomePerSecond;
        [SerializeField, Min(0f)] private float runnerSpeed;
        [SerializeField] private Color bodyColor = Color.white;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer outlineRenderer;

        public string AnimalName => animalName;
        public string CatalogId => string.IsNullOrWhiteSpace(catalogId)
            ? KickLuckyCubeAnimalCatalog.MakeStableId(AnimalName, rarity)
            : catalogId;
        public KickLuckyCubeRarity Rarity => rarity;
        public int SellValue => sellValue;
        public int IncomePerSecond => incomePerSecond;
        public float RunnerSpeed => runnerSpeed;
        public Color BodyColor => bodyColor;

        public void Configure(KickLuckyCubeAnimalOption option, float baseRunnerSpeed)
        {
            animalName = option.AnimalName;
            catalogId = option.CatalogId;
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

        public void EnsureBlackOutline(float scaleMultiplier = 1.08f)
        {
            if (outlineRenderer != null || bodyRenderer == null)
            {
                return;
            }

            var outlineName = bodyRenderer.gameObject.name + "_BlackOutline";
            var existingOutline = bodyRenderer.transform.parent != null
                ? bodyRenderer.transform.parent.Find(outlineName)
                : null;
            if (existingOutline != null)
            {
                outlineRenderer = existingOutline.GetComponent<Renderer>();
                return;
            }

            var sourceFilter = bodyRenderer.GetComponent<MeshFilter>();
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
            {
                return;
            }

            var outlineObject = Instantiate(bodyRenderer.gameObject, bodyRenderer.transform.parent);
            outlineObject.name = outlineName;
            outlineObject.transform.localPosition = bodyRenderer.transform.localPosition;
            outlineObject.transform.localRotation = bodyRenderer.transform.localRotation;
            outlineObject.transform.localScale = bodyRenderer.transform.localScale * Mathf.Max(1f, scaleMultiplier);
            outlineObject.transform.SetSiblingIndex(Mathf.Max(0, bodyRenderer.transform.GetSiblingIndex()));

            foreach (var collider in outlineObject.GetComponentsInChildren<Collider>())
            {
                DestroyUnityObject(collider);
            }

            var outlineFilter = outlineObject.GetComponent<MeshFilter>();
            if (outlineFilter != null)
            {
                var outlineMesh = Instantiate(sourceFilter.sharedMesh);
                for (var subMesh = 0; subMesh < outlineMesh.subMeshCount; subMesh++)
                {
                    var triangles = outlineMesh.GetTriangles(subMesh);
                    for (var index = 0; index < triangles.Length; index += 3)
                    {
                        (triangles[index], triangles[index + 1]) = (triangles[index + 1], triangles[index]);
                    }

                    outlineMesh.SetTriangles(triangles, subMesh);
                }

                outlineMesh.RecalculateNormals();
                outlineFilter.sharedMesh = outlineMesh;
            }

            outlineRenderer = outlineObject.GetComponent<Renderer>();
            if (outlineRenderer == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return;
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.black);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", Color.black);
            }

            outlineRenderer.sharedMaterial = material;
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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

        private static void DestroyUnityObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }
    }
}
