using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeUiAuthoringPreview : MonoBehaviour
    {
        public enum PreviewState
        {
            Default,
            Affordable,
            InsufficientFunds,
            Owned,
            Equipped,
            Locked,
            Cooldown,
            Claimed,
        }

        [Serializable]
        public sealed class TextSample
        {
            [SerializeField] private TMP_Text target;
            [SerializeField, TextArea] private string sampleText;

            public TMP_Text Target => target;
            public string SampleText => sampleText;
        }

        [Serializable]
        public sealed class StateObjects
        {
            [SerializeField] private PreviewState state;
            [SerializeField] private GameObject[] show = Array.Empty<GameObject>();
            [SerializeField] private GameObject[] hide = Array.Empty<GameObject>();

            public PreviewState State => state;
            public IReadOnlyList<GameObject> Show => show;
            public IReadOnlyList<GameObject> Hide => hide;
        }

        [SerializeField] private PreviewState previewState;
        [SerializeField] private TextSample[] textSamples = Array.Empty<TextSample>();
        [SerializeField] private StateObjects[] stateObjects = Array.Empty<StateObjects>();
        [SerializeField] private bool applyTextSamples = true;

        public PreviewState State => previewState;

        public void ApplyPreview()
        {
            if (applyTextSamples && textSamples != null)
            {
                foreach (var sample in textSamples)
                    if (sample?.Target != null) sample.Target.text = sample.SampleText;
            }

            if (stateObjects == null) return;
            foreach (var group in stateObjects)
            {
                if (group == null || group.State != previewState) continue;
                foreach (var target in group.Show) if (target != null) target.SetActive(true);
                foreach (var target in group.Hide) if (target != null) target.SetActive(false);
            }
        }

        public void SetPreviewState(PreviewState state)
        {
            previewState = state;
            ApplyPreview();
        }

        public void ApplyHeuristicSampleText()
        {
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
            {
                if (label == null) continue;
                var lower = label.name.ToLowerInvariant();
                if (lower.Contains("price") || lower.Contains("cost")) label.text = "$12.5K";
                else if (lower.Contains("level")) label.text = "Lv 125";
                else if (lower.Contains("income")) label.text = "+2.4K/s";
                else if (lower.Contains("speed")) label.text = "+0.8 Speed";
                else if (lower.Contains("status")) label.text = previewState.ToString();
                else if (lower.Contains("count")) label.text = "3 / 10";
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) ApplyPreview();
        }
    }
}
