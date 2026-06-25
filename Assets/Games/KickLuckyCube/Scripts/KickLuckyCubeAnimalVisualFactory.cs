using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal static class KickLuckyCubeAnimalVisualFactory
    {
        private static readonly Vector3 ImportedVisualLocalEuler = Vector3.zero;

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
                usedImportedVisual = true;
                bodyRenderer = visual.GetComponentInChildren<Renderer>(true);
                ApplyGradeVisuals(parent, option, visualTargetHeight, includeGradeEffects);
                return bodyRenderer;
            }

            bodyRenderer = CreateFallbackCapsule(parent);
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

        private static Renderer CreateFallbackCapsule(Transform parent)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(parent, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.72f, 0.58f, 1.05f);

            var bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                DestroyUnityObject(bodyCollider);
            }

            return body.GetComponent<Renderer>();
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

            var radius = Mathf.Clamp(targetHeight * 0.42f, 0.42f, 1.65f);
            CreateGroundGlow(root, option.Grade, radius);
            CreateGradeParticles(root, option.Grade, radius, targetHeight);
            CreateGradeLight(root, option.Grade, targetHeight);
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

        private static void CreateGroundGlow(Transform root, KickLuckyCubeAnimalGrade grade, float radius)
        {
            var glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glow.name = "KLC_AnimalGradeGlow_" + grade;
            glow.transform.SetParent(root, false);
            glow.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            glow.transform.localRotation = Quaternion.identity;
            glow.transform.localScale = new Vector3(radius, 0.012f, radius);

            var collider = glow.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyUnityObject(collider);
            }

            var renderer = glow.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateUnlitMaterial(KickLuckyCubeAnimalGradeUtility.GetGlowColor(grade), true);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void CreateGradeParticles(
            Transform root,
            KickLuckyCubeAnimalGrade grade,
            float radius,
            float targetHeight)
        {
            var particlesObject = new GameObject("KLC_AnimalGradeVfx_" + grade);
            particlesObject.transform.SetParent(root, false);
            particlesObject.transform.localPosition = new Vector3(0f, Mathf.Max(0.18f, targetHeight * 0.42f), 0f);
            particlesObject.transform.localRotation = Quaternion.identity;

            var particles = particlesObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.25f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.42f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.105f);
            main.startColor = KickLuckyCubeAnimalGradeUtility.GetGlowColor(grade);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particles.emission;
            emission.rateOverTime = grade == KickLuckyCubeAnimalGrade.Fire ? 22f : 14f;

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.16f, radius * 0.72f);
            shape.arc = 360f;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.22f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            var glow = KickLuckyCubeAnimalGradeUtility.GetGlowColor(grade);
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(glow, 0f),
                    new GradientColorKey(Color.white, 0.5f),
                    new GradientColorKey(glow, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.85f, 0.18f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = particlesObject.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateUnlitMaterial(glow, true);
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            particles.Play();
        }

        private static void CreateGradeLight(Transform root, KickLuckyCubeAnimalGrade grade, float targetHeight)
        {
            var lightObject = new GameObject("KLC_AnimalGradeLight_" + grade);
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, Mathf.Max(0.3f, targetHeight * 0.55f), 0f);
            var gradeLight = lightObject.AddComponent<Light>();
            gradeLight.type = LightType.Point;
            gradeLight.color = KickLuckyCubeAnimalGradeUtility.GetTintColor(grade);
            gradeLight.range = Mathf.Clamp(targetHeight * 1.8f, 1.2f, 4f);
            gradeLight.intensity = grade == KickLuckyCubeAnimalGrade.Fire ? 1.15f : 0.72f;
            gradeLight.shadows = LightShadows.None;
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
