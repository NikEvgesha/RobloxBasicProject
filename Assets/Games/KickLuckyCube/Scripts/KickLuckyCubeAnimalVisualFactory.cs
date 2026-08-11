using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal static class KickLuckyCubeAnimalVisualFactory
    {
        private static readonly Vector3 ImportedVisualLocalEuler = Vector3.zero;
        private const string FallbackVisualResourcePath = "KickLuckyCube/Animals/KLC_AnimalFallbackVisual";
        private const string GradeVisualResourceRoot = "KickLuckyCube/Animals/KLC_AnimalGradeVfx_";

        public static Renderer CreateVisual(
            Transform parent,
            KickLuckyCubeAnimalOption option,
            float targetHeight,
            out bool usedImportedVisual,
            bool includeGradeEffects = true)
        {
            usedImportedVisual = false;
            if (parent == null)
            {
                return null;
            }

            var visualTargetHeight = targetHeight * KickLuckyCubeAnimalGradeUtility.GetVisualHeightMultiplier(option.Grade);
            Renderer bodyRenderer;
            var visualPrefab = KickLuckyCubeAnimalCatalog.LoadVisualPrefab(option);
            if (visualPrefab != null)
            {
                var visual = Object.Instantiate(visualPrefab, parent);
                visual.name = "Visual_" + Sanitize(option.AnimalName);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.Euler(ImportedVisualLocalEuler);
                visual.transform.localScale = Vector3.one;
                RemoveColliders(visual);
                NormalizeToHeight(visual.transform, visualTargetHeight);
                ConfigureImportedAnimators(visual);
                AddProceduralMotion(visual, visualTargetHeight);
                usedImportedVisual = true;
                bodyRenderer = visual.GetComponentInChildren<Renderer>(true);
                ApplyGradeVisuals(parent, option, visualTargetHeight, includeGradeEffects);
                return bodyRenderer;
            }

            bodyRenderer = CreateFallbackVisual(parent);
            NormalizeToHeight(bodyRenderer != null ? bodyRenderer.transform : parent, visualTargetHeight);
            ApplyGradeVisuals(parent, option, visualTargetHeight, includeGradeEffects);
            return bodyRenderer;
        }

        public static Renderer CreateVisual(
            Transform parent,
            KickLuckyCubeInventoryAnimal animal,
            float targetHeight,
            out bool usedImportedVisual)
        {
            return CreateVisual(parent, KickLuckyCubeAnimalCatalog.CreateOption(animal), targetHeight, out usedImportedVisual);
        }

        public static void NormalizeToHeight(Transform root, float targetHeight)
        {
            if (root == null)
            {
                return;
            }

            root.localScale = Vector3.one;
            if (!TryGetRendererBounds(root, out var bounds))
            {
                return;
            }

            var safeTargetHeight = Mathf.Max(0.1f, targetHeight);
            var currentHeight = Mathf.Max(0.001f, bounds.size.y);
            root.localScale = Vector3.one * Mathf.Clamp(safeTargetHeight / currentHeight, 0.01f, 100f);

            if (!TryGetRendererBounds(root, out bounds))
            {
                return;
            }

            root.position += Vector3.up * (root.parent.position.y - bounds.min.y);
        }

        private static Renderer CreateFallbackVisual(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(FallbackVisualResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Animal fallback prefab is missing at Resources/{FallbackVisualResourcePath}.prefab.");
                return null;
            }

            var body = Object.Instantiate(prefab, parent, false);
            body.name = "KLC_AnimalFallbackVisual";
            RemoveColliders(body);
            return body.GetComponentInChildren<Renderer>(true);
        }

        private static void ApplyGradeVisuals(
            Transform root,
            KickLuckyCubeAnimalOption option,
            float targetHeight,
            bool includeGradeEffects)
        {
            if (root == null || option.Grade == KickLuckyCubeAnimalGrade.Normal)
            {
                return;
            }

            ApplyGradeTint(root, option.Grade);
            if (!includeGradeEffects)
            {
                return;
            }

            var resourcePath = GradeVisualResourceRoot + option.Grade;
            var prefab = Resources.Load<KickLuckyCubeAnimalGradeVisual>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Animal grade VFX prefab is missing at Resources/{resourcePath}.prefab.");
                return;
            }

            var gradeVisual = Object.Instantiate(prefab, root, false);
            gradeVisual.name = "KLC_AnimalGradeVfx_" + option.Grade;
            gradeVisual.Configure(option.Grade, targetHeight);
        }

        private static void ApplyGradeTint(Transform root, KickLuckyCubeAnimalGrade grade)
        {
            var tint = KickLuckyCubeAnimalGradeUtility.GetTintColor(grade);
            foreach (var targetRenderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (targetRenderer == null
                    || targetRenderer is ParticleSystemRenderer
                    || targetRenderer.gameObject.name.IndexOf("BlackOutline", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || targetRenderer.gameObject.name.IndexOf("Outline", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                var materials = targetRenderer.materials;
                for (var index = 0; index < materials.Length; index++)
                {
                    var material = materials[index] != null
                        ? new Material(materials[index])
                        : CreateUnlitMaterial(Color.white, false);
                    var sourceColor = material.HasProperty("_BaseColor")
                        ? material.GetColor("_BaseColor")
                        : material.HasProperty("_Color")
                            ? material.GetColor("_Color")
                            : Color.white;
                    var color = Color.Lerp(sourceColor, tint, 0.55f);
                    SetMaterialColor(material, color);

                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", tint * 0.85f);
                    }

                    materials[index] = material;
                }

                targetRenderer.materials = materials;
            }
        }

        private static Material CreateUnlitMaterial(Color color, bool transparent)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader);
            SetMaterialColor(material, color);

            if (transparent)
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 1f);
                }

                if (material.HasProperty("_Blend"))
                {
                    material.SetFloat("_Blend", 0f);
                }

                if (material.HasProperty("_SrcBlend"))
                {
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }

                if (material.HasProperty("_DstBlend"))
                {
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }

                if (material.HasProperty("_ZWrite"))
                {
                    material.SetFloat("_ZWrite", 0f);
                }
            }

            return material;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void RemoveColliders(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                DestroyUnityObject(collider);
            }
        }

        private static void ConfigureImportedAnimators(GameObject root)
        {
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                animator.enabled = true;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.applyRootMotion = false;
                animator.keepAnimatorStateOnDisable = true;
                animator.Rebind();
                animator.Play(0, 0, 0f);
                animator.Update(0f);
            }
        }

        private static void AddProceduralMotion(GameObject root, float targetHeight)
        {
            if (root == null || root.GetComponent<KickLuckyCubeAnimalProceduralMotion>() != null)
            {
                return;
            }

            var phase = Random.Range(0f, Mathf.PI * 2f);
            root.AddComponent<KickLuckyCubeAnimalProceduralMotion>()
                .Configure(targetHeight, phase);
        }

        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds(root != null ? root.position : Vector3.zero, Vector3.zero);
            if (root == null)
            {
                return false;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;
            foreach (var renderer in renderers)
            {
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static string Sanitize(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Animal" : value;
            var chars = source.ToCharArray();
            for (var index = 0; index < chars.Length; index++)
            {
                if (!char.IsLetterOrDigit(chars[index]))
                {
                    chars[index] = '_';
                }
            }

            return new string(chars);
        }

        private static void DestroyUnityObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
                return;
            }

            Object.DestroyImmediate(target);
        }
    }
}
