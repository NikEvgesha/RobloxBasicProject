using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeWaveChaseController : MonoBehaviour
    {
        [SerializeField] private Transform waveVisual;
        [SerializeField, Min(0f)] private float startBehindDistance = 14f;
        [SerializeField, Min(0f)] private float catchDistance = 1.25f;
        [SerializeField, Min(0f)] private float speedMultiplier = 0.72f;
        [SerializeField, Min(0f)] private float minimumWaveSpeed = 4.5f;
        [SerializeField] private bool hideWhenIdle = true;

        private KickLuckyCubeAnimalRunner runner;
        private float waveSpeed;
        private bool chasing;

        public event Action<KickLuckyCubeAnimalRunner> AnimalCaught;

        public bool IsChasing => chasing;

        private void Awake()
        {
            if (waveVisual == null)
            {
                waveVisual = transform;
            }

            if (hideWhenIdle && waveVisual != null)
            {
                waveVisual.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!chasing || runner == null || waveVisual == null)
            {
                return;
            }

            waveVisual.position += Vector3.back * (waveSpeed * Time.deltaTime);

            if (waveVisual.position.z <= runner.transform.position.z + catchDistance)
            {
                var caughtRunner = runner;
                StopChase();
                AnimalCaught?.Invoke(caughtRunner);
            }
        }

        public void BeginChase(KickLuckyCubeAnimalRunner targetRunner)
        {
            runner = targetRunner;
            if (runner == null || waveVisual == null)
            {
                chasing = false;
                return;
            }

            waveSpeed = Mathf.Max(minimumWaveSpeed, runner.Speed * speedMultiplier);
            waveVisual.gameObject.SetActive(true);
            waveVisual.position = runner.transform.position + Vector3.forward * startBehindDistance + Vector3.up * 0.2f;
            chasing = true;
        }

        public void StopChase()
        {
            chasing = false;
            runner = null;

            if (hideWhenIdle && waveVisual != null)
            {
                waveVisual.gameObject.SetActive(false);
            }
        }

        public void ForceCatchForPrototype()
        {
            if (runner == null)
            {
                return;
            }

            var caughtRunner = runner;
            StopChase();
            AnimalCaught?.Invoke(caughtRunner);
        }
    }
}
