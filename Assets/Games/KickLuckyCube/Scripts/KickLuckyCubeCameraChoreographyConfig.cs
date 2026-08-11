using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [CreateAssetMenu(
        fileName = "KickLuckyCubeCameraChoreographyConfig",
        menuName = "Kick Lucky Cube/Camera Choreography Config")]
    public sealed class KickLuckyCubeCameraChoreographyConfig : ScriptableObject
    {
        public enum YawBasis
        {
            Absolute,
            TargetForward,
            DirectionToHome,
        }

        [Serializable]
        public sealed class Shot
        {
            [SerializeField] private string displayName = "Shot";
            [SerializeField] private Vector3 focusOffset = new(0f, 1f, 0f);
            [SerializeField, Min(0.5f)] private float distance = 6f;
            [SerializeField, Range(-20f, 70f)] private float pitch = 12f;
            [SerializeField] private YawBasis yawBasis = YawBasis.Absolute;
            [SerializeField] private float yawOffset;
            [SerializeField, Min(0f)] private float transitionSeconds = 0.8f;
            [SerializeField, Min(0f)] private float holdSeconds = 0.25f;
            [SerializeField, Range(20f, 100f)] private float fieldOfView = 60f;
            [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private bool snapOnEnter;

            public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? "Shot" : displayName;
            public Vector3 FocusOffset => focusOffset;
            public float Distance => Mathf.Max(0.5f, distance);
            public float Pitch => pitch;
            public YawBasis Basis => yawBasis;
            public float YawOffset => yawOffset;
            public float TransitionSeconds => Mathf.Max(0f, transitionSeconds);
            public float HoldSeconds => Mathf.Max(0f, holdSeconds);
            public float FieldOfView => Mathf.Clamp(fieldOfView, 20f, 100f);
            public AnimationCurve Easing => easing;
            public bool SnapOnEnter => snapOnEnter;

            public float ResolveYaw(Transform target, Vector3 homePosition)
            {
                var basisYaw = yawBasis switch
                {
                    YawBasis.TargetForward when target != null => target.eulerAngles.y,
                    YawBasis.DirectionToHome when target != null
                        && (homePosition - target.position).sqrMagnitude > 0.001f
                        => Quaternion.LookRotation(homePosition - target.position, Vector3.up).eulerAngles.y,
                    _ => 0f,
                };

                return basisYaw + yawOffset;
            }
        }

        [Header("Wave reveal")]
        [SerializeField] private Shot waveReveal = new();

        [Header("Runner ready")]
        [SerializeField] private Shot runnerReady = new();

        [Header("Sequence")]
        [SerializeField, Min(0f)] private float revealToWaveDelay = 0.1f;
        [SerializeField, Min(0f)] private float runnerGroundingPause = 0.08f;

        public Shot WaveReveal => waveReveal;
        public Shot RunnerReady => runnerReady;
        public float RevealToWaveDelay => Mathf.Max(0f, revealToWaveDelay);
        public float RunnerGroundingPause => Mathf.Max(0f, runnerGroundingPause);
    }
}
