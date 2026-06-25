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
        [SerializeField] private bool ensureToolTraining = true;
        [SerializeField] private bool ensureInventoryUi = true;
        [SerializeField] private bool ensureSellShopUi = true;
        [SerializeField] private bool ensureAudioListener = true;
        [SerializeField, Min(0)] private int startupFramesToNormalize = 5;

        private int remainingStartupFrames;

        private void Awake()
        {
            EnsureToolTrainingController();
            EnsureInventoryController();
            EnsureSellShopController();
            EnsureAudioListener();
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

        private void EnsureToolTrainingController()
        {
            if (!Application.isPlaying || !ensureToolTraining)
            {
                return;
            }

            var existingToolTraining = FindFirstObjectByType<KickLuckyCubeToolTrainingController>(FindObjectsInactive.Include);
            if (existingToolTraining != null)
            {
                return;
            }

            var toolTrainingObject = new GameObject("KLC_ToolTrainingController_Runtime");
            toolTrainingObject.AddComponent<KickLuckyCubeToolTrainingController>();
        }

        private void EnsureInventoryController()
        {
            if (!Application.isPlaying || !ensureInventoryUi)
            {
                return;
            }

            var existingInventory = FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            if (existingInventory != null)
            {
                return;
            }

            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                return;
            }

            var inventoryObject = new GameObject("KLC_InventoryController_Runtime");
            inventoryObject.transform.SetParent(canvas.transform, false);
            inventoryObject.AddComponent<KickLuckyCubeInventoryController>();
        }

        private void EnsureSellShopController()
        {
            if (!Application.isPlaying || !ensureSellShopUi)
            {
                return;
            }

            var existingSellShop = FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include);
            if (existingSellShop != null)
            {
                return;
            }

            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                return;
            }

            var sellShopObject = new GameObject("KLC_SellShopController_Runtime");
            sellShopObject.transform.SetParent(canvas.transform, false);
            sellShopObject.AddComponent<KickLuckyCubeSellShopController>();
        }

        private void EnsureAudioListener()
        {
            if (!Application.isPlaying || !ensureAudioListener)
            {
                return;
            }

            if (FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var targetCamera = Camera.main ?? FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            if (targetCamera != null)
            {
                targetCamera.gameObject.AddComponent<AudioListener>();
            }
        }
    }
}
