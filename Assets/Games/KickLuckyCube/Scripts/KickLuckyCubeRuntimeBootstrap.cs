using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-10000)]
    public sealed class KickLuckyCubeRuntimeBootstrap : MonoBehaviour
    {
        private const float NormalTimeScale = 1f;

        [SerializeField] private bool resetTimeScaleOnAwake = true;
        [SerializeField] private bool runInBackground = true;
        [SerializeField, Min(0)] private int startupFramesToNormalize = 5;

        private int remainingStartupFrames;

        private void Awake()
        {
            remainingStartupFrames = startupFramesToNormalize;
            NormalizeTimeScale();
        }

        private void OnEnable()
        {
            NormalizeTimeScale();
        }

        private void Start()
        {
            NormalizeTimeScale();
        }

        private void Update()
        {
            if (remainingStartupFrames <= 0)
            {
                return;
            }

            remainingStartupFrames--;
            NormalizeTimeScale();
        }

        private void NormalizeTimeScale()
        {
            if (runInBackground)
            {
                Application.runInBackground = true;
            }

            if (!resetTimeScaleOnAwake)
            {
                return;
            }

            Time.timeScale = NormalTimeScale;

#if UNITY_EDITOR
            EditorPrefs.SetFloat("CustomToolbar.ToolbarTimeSlider.Value", NormalTimeScale);
#endif
        }
    }
}
