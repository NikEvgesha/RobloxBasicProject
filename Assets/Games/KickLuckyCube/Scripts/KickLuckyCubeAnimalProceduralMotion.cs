using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal sealed class KickLuckyCubeAnimalProceduralMotion : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float bobAmplitude = 0.035f;
        [SerializeField, Min(0.01f)] private float bobFrequency = 1.65f;
        [SerializeField, Min(0f)] private float pitchDegrees = 1.8f;
        [SerializeField, Min(0f)] private float rollDegrees = 2.4f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation = Quaternion.identity;
        private float phase;
        private bool capturedPose;

        public void Configure(float targetHeight, float phaseOffset)
        {
            var safeHeight = Mathf.Max(0.35f, targetHeight);
            bobAmplitude = Mathf.Clamp(safeHeight * 0.022f, 0.018f, 0.065f);
            pitchDegrees = Mathf.Clamp(safeHeight * 1.1f, 1.2f, 3.2f);
            rollDegrees = Mathf.Clamp(safeHeight * 1.45f, 1.5f, 4.5f);
            phase = phaseOffset;
            CapturePose();
        }

        private void OnEnable()
        {
            CapturePose();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!capturedPose)
            {
                CapturePose();
            }

            var time = Time.time * bobFrequency + phase;
            var bob = Mathf.Sin(time) * bobAmplitude;
            var pitch = Mathf.Sin(time * 0.83f + 0.7f) * pitchDegrees;
            var roll = Mathf.Sin(time * 1.17f + 1.1f) * rollDegrees;
            transform.localPosition = baseLocalPosition + Vector3.up * bob;
            transform.localRotation = baseLocalRotation * Quaternion.Euler(pitch, 0f, roll);
        }

        private void OnDisable()
        {
            if (!capturedPose)
            {
                return;
            }

            transform.localPosition = baseLocalPosition;
            transform.localRotation = baseLocalRotation;
        }

        private void CapturePose()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            capturedPose = true;
        }
    }
}
