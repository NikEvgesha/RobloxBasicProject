using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAnimalGradeVisual : MonoBehaviour
    {
        [SerializeField] private Renderer groundGlow;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Light gradeLight;

        private MaterialPropertyBlock materialBlock;

        public void Configure(KickLuckyCubeAnimalGrade grade, float targetHeight)
        {
            var tint = KickLuckyCubeAnimalGradeUtility.GetTintColor(grade);
            var glow = KickLuckyCubeAnimalGradeUtility.GetGlowColor(grade);
            var radius = Mathf.Clamp(targetHeight * 0.42f, 0.42f, 1.65f);

            if (groundGlow != null)
            {
                groundGlow.transform.localScale = new Vector3(radius, 0.012f, radius);
                materialBlock ??= new MaterialPropertyBlock();
                groundGlow.GetPropertyBlock(materialBlock);
                materialBlock.SetColor("_BaseColor", glow);
                materialBlock.SetColor("_Color", glow);
                groundGlow.SetPropertyBlock(materialBlock);
            }

            if (particles != null)
            {
                particles.transform.localPosition = new Vector3(0f, Mathf.Max(0.18f, targetHeight * 0.42f), 0f);
                var main = particles.main;
                main.startColor = glow;
                var emission = particles.emission;
                emission.rateOverTime = grade == KickLuckyCubeAnimalGrade.Fire ? 22f : 14f;
                var shape = particles.shape;
                shape.radius = Mathf.Max(0.16f, radius * 0.72f);

                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    materialBlock ??= new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(materialBlock);
                    materialBlock.SetColor("_BaseColor", glow);
                    materialBlock.SetColor("_Color", glow);
                    renderer.SetPropertyBlock(materialBlock);
                }

                particles.Play(true);
            }

            if (gradeLight != null)
            {
                gradeLight.transform.localPosition = new Vector3(0f, Mathf.Max(0.3f, targetHeight * 0.55f), 0f);
                gradeLight.color = tint;
                gradeLight.range = Mathf.Clamp(targetHeight * 1.8f, 1.2f, 4f);
                gradeLight.intensity = grade == KickLuckyCubeAnimalGrade.Fire ? 1.15f : 0.72f;
            }
        }

        public void SetReferences(Renderer glow, ParticleSystem particleSystem, Light light)
        {
            groundGlow = glow;
            particles = particleSystem;
            gradeLight = light;
        }
    }
}
