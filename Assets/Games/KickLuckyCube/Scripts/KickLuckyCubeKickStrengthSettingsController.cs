using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-80)]
    public sealed class KickLuckyCubeKickStrengthSettingsController : MonoBehaviour
    {
        private const string SelectedStrengthKey = "KickLuckyCube.Kick.SelectedStrength";
        private const string UseMaximumKey = "KickLuckyCube.Kick.UseMaximum";

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string windowResourcePath = "KickLuckyCube/UI/Elements/KLC_KickStrengthWindow_Runtime";

        private RectTransform windowRoot;
        private Slider strengthSlider;
        private TMP_Text valueText;
        private TMP_Text detailText;
        private Button decreaseButton;
        private Button increaseButton;
        private Button maximumButton;
        private Button closeButton;
        private Button backdropButton;
        private int selectedStrength = 1;
        private bool useMaximum = true;
        private bool suppressSliderCallback;

        public int MaximumStrength => Mathf.Max(1, Mathf.FloorToInt(stats != null ? stats.Strength : 1f));
        public int SelectedStrength => useMaximum ? MaximumStrength : Mathf.Clamp(selectedStrength, 1, MaximumStrength);
        public float EffectiveStrength => stats == null
            ? SelectedStrength
            : useMaximum
                ? Mathf.Max(0f, stats.Strength)
                : Mathf.Min(Mathf.Max(0f, stats.Strength), SelectedStrength);
        public bool UseMaximum => useMaximum;
        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

        public event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying
                || FindFirstObjectByType<KickLuckyCubeKickStrengthSettingsController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_KickStrengthSettingsController_Runtime")
                .AddComponent<KickLuckyCubeKickStrengthSettingsController>();
        }

        private void Awake()
        {
            ResolveReferences();
            Load();
            BuildFromPrefab();
            CloseWindow();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (stats != null)
            {
                stats.Changed += OnStatsChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.Changed -= OnStatsChanged;
            }
        }

        private void OnDestroy()
        {
            UnwireButtons();
        }

        public void OpenWindow()
        {
            ResolveReferences();
            BuildFromPrefab();
            if (windowRoot == null)
            {
                return;
            }

            windowRoot.gameObject.SetActive(true);
            windowRoot.SetAsLastSibling();
            Refresh();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }
        }

        public void SetSelectedStrength(int value)
        {
            selectedStrength = Mathf.Clamp(value, 1, MaximumStrength);
            useMaximum = false;
            Save();
            Refresh();
            Changed?.Invoke();
        }

        public void SelectMaximum()
        {
            useMaximum = true;
            selectedStrength = MaximumStrength;
            Save();
            Refresh();
            Changed?.Invoke();
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
        }

        private void Load()
        {
            useMaximum = PlayerPrefs.GetInt(UseMaximumKey, 1) != 0;
            selectedStrength = Mathf.Clamp(PlayerPrefs.GetInt(SelectedStrengthKey, MaximumStrength), 1, MaximumStrength);
            if (useMaximum)
            {
                selectedStrength = MaximumStrength;
            }
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SelectedStrengthKey, Mathf.Clamp(selectedStrength, 1, MaximumStrength));
            PlayerPrefs.SetInt(UseMaximumKey, useMaximum ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void BuildFromPrefab()
        {
            if (windowRoot != null || canvas == null || string.IsNullOrWhiteSpace(windowResourcePath))
            {
                return;
            }

            var prefab = Resources.Load<RectTransform>(windowResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Kick strength settings prefab is missing at Resources/{windowResourcePath}.prefab.");
                return;
            }

            windowRoot = Instantiate(prefab, canvas.transform, false);
            windowRoot.name = "KLC_KickStrengthWindow_Runtime";
            strengthSlider = FindComponent<Slider>("KLC_KickStrengthSlider");
            valueText = FindComponent<TMP_Text>("KLC_KickStrengthValue");
            detailText = FindComponent<TMP_Text>("KLC_KickStrengthDetail");
            decreaseButton = FindComponent<Button>("KLC_KickStrengthDecreaseButton");
            increaseButton = FindComponent<Button>("KLC_KickStrengthIncreaseButton");
            maximumButton = FindComponent<Button>("KLC_KickStrengthMaximumButton");
            closeButton = FindComponent<Button>("KLC_KickStrengthCloseButton");
            backdropButton = FindComponent<Button>("KLC_KickStrengthBackdrop");
            WireButtons();
            Refresh();
        }

        private T FindComponent<T>(string objectName)
            where T : Component
        {
            if (windowRoot == null)
            {
                return null;
            }

            return windowRoot
                .GetComponentsInChildren<T>(true)
                .FirstOrDefault(component => component != null && string.Equals(component.name, objectName, StringComparison.Ordinal));
        }

        private void WireButtons()
        {
            UnwireButtons();
            decreaseButton?.onClick.AddListener(Decrease);
            increaseButton?.onClick.AddListener(Increase);
            maximumButton?.onClick.AddListener(SelectMaximum);
            closeButton?.onClick.AddListener(CloseWindow);
            backdropButton?.onClick.AddListener(CloseWindow);
            strengthSlider?.onValueChanged.AddListener(OnSliderChanged);
        }

        private void UnwireButtons()
        {
            decreaseButton?.onClick.RemoveListener(Decrease);
            increaseButton?.onClick.RemoveListener(Increase);
            maximumButton?.onClick.RemoveListener(SelectMaximum);
            closeButton?.onClick.RemoveListener(CloseWindow);
            backdropButton?.onClick.RemoveListener(CloseWindow);
            strengthSlider?.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void Decrease()
        {
            SetSelectedStrength(SelectedStrength - 1);
        }

        private void Increase()
        {
            var next = Mathf.Min(MaximumStrength, SelectedStrength + 1);
            if (next >= MaximumStrength)
            {
                SelectMaximum();
                return;
            }

            SetSelectedStrength(next);
        }

        private void OnSliderChanged(float value)
        {
            if (!suppressSliderCallback)
            {
                SetSelectedStrength(Mathf.RoundToInt(value));
            }
        }

        private void OnStatsChanged()
        {
            var clamped = Mathf.Clamp(selectedStrength, 1, MaximumStrength);
            if (clamped != selectedStrength || useMaximum)
            {
                selectedStrength = useMaximum ? MaximumStrength : clamped;
                Save();
                Changed?.Invoke();
            }

            Refresh();
        }

        private void Refresh()
        {
            if (strengthSlider != null)
            {
                suppressSliderCallback = true;
                strengthSlider.wholeNumbers = true;
                strengthSlider.minValue = 1f;
                strengthSlider.maxValue = MaximumStrength;
                strengthSlider.value = SelectedStrength;
                suppressSliderCallback = false;
            }

            if (valueText != null)
            {
                valueText.text = useMaximum
                    ? $"{MaximumStrength} (MAX)"
                    : $"{SelectedStrength} / {MaximumStrength}";
            }

            if (detailText != null)
            {
                detailText.text = useMaximum
                    ? "Every kick can use all trained strength."
                    : $"Kick distance is capped at {SelectedStrength} strength.";
            }

            if (decreaseButton != null)
            {
                decreaseButton.interactable = SelectedStrength > 1;
            }

            if (increaseButton != null)
            {
                increaseButton.interactable = SelectedStrength < MaximumStrength;
            }

            if (maximumButton != null)
            {
                maximumButton.interactable = !useMaximum;
            }
        }
    }
}
