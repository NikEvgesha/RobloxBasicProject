using System.Collections;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedCurrencyVfx : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private MechanicsTestbedWallet wallet;
        [SerializeField] private Transform target;
        [SerializeField] private Transform spawnCenter;

        [Header("Particles")]
        [SerializeField] private ParticleSystem burstSystem;
        [SerializeField] private int maxFlyingTokens = 18;
        [SerializeField] private float burstRadius = 1.35f;
        [SerializeField] private float targetHeight = 1.25f;
        [SerializeField] private float scatterDuration = 0.18f;
        [SerializeField] private float flightDuration = 0.56f;
        [SerializeField] private float tokenSize = 0.13f;
        [SerializeField] private float trailLifetime = 0.22f;

        [Header("Colors")]
        [SerializeField] private Color softColor = new Color(1f, 0.78f, 0.12f, 1f);
        [SerializeField] private Color hardColor = new Color(1f, 0.18f, 0.95f, 1f);
        [SerializeField] private Color pickupColor = new Color(0.35f, 0.85f, 1f, 1f);

        private Material burstMaterial;
        private Material softMaterial;
        private Material hardMaterial;
        private Material pickupMaterial;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<MechanicsTestbedWallet>();

            if (target == null)
            {
                var carryController = FindFirstObjectByType<MechanicsTestbedCarryController>();
                target = carryController == null ? transform : carryController.transform;
            }

            spawnCenter ??= target;
            ConfigureBurstSystem();
        }

        private void OnEnable()
        {
            if (wallet != null)
            {
                wallet.CurrencyGained += OnCurrencyGained;
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.CurrencyGained -= OnCurrencyGained;
            }
        }

        public void PlayPickupBurst(Vector3 worldPosition)
        {
            PlayBurst(worldPosition, pickupColor, Mathf.Min(8, maxFlyingTokens), pickupMaterial ??= CreateTokenMaterial(pickupColor));
        }

        private void OnCurrencyGained(int softDelta, int hardDelta)
        {
            if (softDelta > 0)
            {
                var tokenCount = Mathf.Clamp(5 + softDelta / 75, 5, maxFlyingTokens);
                PlayBurst(GetEffectCenter(), softColor, tokenCount, softMaterial ??= CreateTokenMaterial(softColor));
            }

            if (hardDelta > 0)
            {
                var tokenCount = Mathf.Clamp(5 + hardDelta * 2, 5, maxFlyingTokens);
                PlayBurst(GetEffectCenter(), hardColor, tokenCount, hardMaterial ??= CreateTokenMaterial(hardColor));
            }
        }

        private void PlayBurst(Vector3 center, Color color, int tokenCount, Material tokenMaterial)
        {
            ConfigureBurstSystem();
            burstSystem.transform.position = center;

            var emitParams = new ParticleSystem.EmitParams
            {
                startColor = color,
                startLifetime = Random.Range(0.35f, 0.55f),
                startSize = Random.Range(0.06f, 0.12f)
            };
            burstSystem.Emit(emitParams, Mathf.Clamp(tokenCount * 3, 12, 48));

            for (var i = 0; i < tokenCount; i++)
            {
                StartCoroutine(FlyToken(center, color, tokenMaterial, i * 0.018f));
            }
        }

        private IEnumerator FlyToken(Vector3 center, Color color, Material tokenMaterial, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            var token = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            token.name = "CurrencyVfxToken";
            token.transform.SetParent(transform, true);
            token.transform.localScale = Vector3.one * tokenSize;

            var collider = token.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = token.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = tokenMaterial;
            }

            var trail = token.AddComponent<TrailRenderer>();
            trail.time = trailLifetime;
            trail.minVertexDistance = 0.015f;
            trail.widthMultiplier = tokenSize * 0.45f;
            trail.material = tokenMaterial;
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            var radial = Random.insideUnitCircle.normalized;
            if (radial.sqrMagnitude < 0.01f)
            {
                radial = Vector2.right;
            }

            var start = center + new Vector3(radial.x, Random.Range(0.55f, 1.55f), radial.y) * Random.Range(0.35f, burstRadius);
            var scatter = start
                + new Vector3(radial.x, Random.Range(0.15f, 0.55f), radial.y) * Random.Range(0.35f, 0.75f);
            token.transform.position = start;

            var elapsed = 0f;
            while (elapsed < scatterDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / scatterDuration);
                token.transform.position = Vector3.LerpUnclamped(start, scatter, EaseOut(t));
                yield return null;
            }

            var flightStart = token.transform.position;
            elapsed = 0f;
            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / flightDuration);
                var destination = GetTargetPosition();
                var arc = Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.35f);
                token.transform.position = Vector3.LerpUnclamped(flightStart, destination, EaseIn(t)) + arc;
                yield return null;
            }

            token.transform.position = GetTargetPosition();
            var shrinkDuration = 0.12f;
            elapsed = 0f;
            var initialScale = token.transform.localScale;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                token.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, elapsed / shrinkDuration);
                yield return null;
            }

            Destroy(token, trailLifetime);
        }

        private Vector3 GetEffectCenter()
        {
            return spawnCenter == null ? transform.position : spawnCenter.position + Vector3.up * targetHeight;
        }

        private Vector3 GetTargetPosition()
        {
            return target == null ? transform.position : target.position + Vector3.up * targetHeight;
        }

        private void ConfigureBurstSystem()
        {
            if (burstSystem == null)
            {
                var burstObject = new GameObject("CurrencyBurstParticles");
                burstObject.transform.SetParent(transform, false);
                burstSystem = burstObject.AddComponent<ParticleSystem>();
            }

            var main = burstSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.65f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 4.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.055f, 0.12f);
            main.gravityModifier = 0.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 96;

            var emission = burstSystem.emission;
            emission.enabled = false;

            var shape = burstSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;
            shape.randomDirectionAmount = 0.65f;

            var colorOverLifetime = burstSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(CreateFadeGradient(Color.white));

            var sizeOverLifetime = burstSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var renderer = burstSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = burstMaterial ??= CreateParticleMaterial();
        }

        private Material CreateTokenMaterial(Color color)
        {
            var material = new Material(FindVfxShader())
            {
                color = color
            };
            return material;
        }

        private Material CreateParticleMaterial()
        {
            return new Material(FindVfxShader())
            {
                color = Color.white
            };
        }

        private static Shader FindVfxShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
        }

        private static Gradient CreateFadeGradient(Color tint)
        {
            return new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(tint, 0f),
                    new GradientColorKey(tint, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.85f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                }
            };
        }

        private static float EaseOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseIn(float t)
        {
            return t * t * t;
        }
    }
}
