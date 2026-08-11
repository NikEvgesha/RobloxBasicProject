using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeLeaderboardController : MonoBehaviour
    {
        private const string VisualRootName = "KLC_LeaderboardBoardVisual_Runtime";
        private const string HeaderLineName = "KLC_Leaderboard_Header";
        private const string LineNamePrefix = "KLC_Leaderboard_Line_";

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private string boardName = "KLC_Board_05_Leaderboard";
        [SerializeField] private string visualResourcePath = "KickLuckyCube/World/KLC_LeaderboardBoardVisual";
        [SerializeField, Min(1)] private int lineCount = 5;
        [SerializeField] private Color textColor = new(1f, 0.95f, 0.78f);

        private TextMesh headerText;
        private TextMesh[] lineTexts = Array.Empty<TextMesh>();
        private float refreshTimer;

        public bool IsBound => headerText != null && lineTexts != null && Array.Exists(lineTexts, line => line != null);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeLeaderboardController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_Leaderboard_Runtime").AddComponent<KickLuckyCubeLeaderboardController>();
        }

        private void Awake()
        {
            ResolveReferences();
            BuildBoardText();
            Refresh();
        }

        private void Update()
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer < 1f)
            {
                return;
            }

            refreshTimer = 0f;
            Refresh();
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
        }

        private void BuildBoardText()
        {
            var board = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate != null
                    && candidate.gameObject.scene.IsValid()
                    && string.Equals(candidate.name, boardName, StringComparison.Ordinal))
                ?.gameObject;
            if (board == null)
            {
                return;
            }

            if (TryBindAuthoredBoardText(board.transform))
            {
                return;
            }

            var visualRoot = ResolveVisualRoot(board.transform);
            headerText = CreateOrGetLine(visualRoot, HeaderLineName, TextAnchor.MiddleCenter);
            lineTexts = new TextMesh[lineCount];
            for (var index = 0; index < lineCount; index++)
            {
                lineTexts[index] = CreateOrGetLine(
                    visualRoot,
                    LineNamePrefix + (index + 1).ToString("00"),
                    TextAnchor.MiddleLeft);
            }
        }

        private bool TryBindAuthoredBoardText(Transform board)
        {
            var texts = board.GetComponentsInChildren<TextMesh>(true);
            headerText = Array.Find(texts, text => text != null
                && (string.Equals(text.name, HeaderLineName, StringComparison.Ordinal)
                    || text.name.EndsWith("_Leaderboard_Header", StringComparison.Ordinal)));

            var authoredLines = texts
                .Where(text => text != null
                    && (text.name.StartsWith(LineNamePrefix, StringComparison.Ordinal)
                        || text.name.IndexOf("_Leaderboard_RankLine_", StringComparison.Ordinal) >= 0))
                .OrderBy(text => ResolveTrailingNumber(text.name))
                .Take(Mathf.Max(1, lineCount))
                .ToArray();

            if (headerText == null || authoredLines.Length == 0)
            {
                headerText = null;
                return false;
            }

            lineTexts = new TextMesh[Mathf.Max(1, lineCount)];
            for (var index = 0; index < lineTexts.Length && index < authoredLines.Length; index++)
            {
                lineTexts[index] = authoredLines[index];
            }

            return true;
        }

        private static int ResolveTrailingNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return int.MaxValue;
            }

            var lastSeparator = value.LastIndexOf('_');
            return lastSeparator >= 0 && int.TryParse(value.Substring(lastSeparator + 1), out var number)
                ? number
                : int.MaxValue;
        }

        private Transform ResolveVisualRoot(Transform board)
        {
            var existing = board.Find(VisualRootName);
            if (existing != null)
            {
                return existing;
            }

            var prefab = !string.IsNullOrWhiteSpace(visualResourcePath)
                ? Resources.Load<Transform>(visualResourcePath)
                : null;
            if (prefab != null)
            {
                var instance = Instantiate(prefab, board, false);
                instance.name = VisualRootName;
                return instance;
            }

            Debug.LogError($"Leaderboard visual prefab is missing at Resources/{visualResourcePath}.prefab.", this);
            return null;
        }

        private TextMesh CreateOrGetLine(Transform parent, string name, TextAnchor anchor)
        {
            if (parent == null)
            {
                return null;
            }

            var textTransform = parent.Find(name);
            if (textTransform == null)
            {
                Debug.LogError($"Leaderboard prefab is missing authored text '{name}'.", parent);
                return null;
            }

            var text = textTransform.GetComponent<TextMesh>();
            if (text == null)
            {
                Debug.LogError($"Leaderboard prefab object '{name}' has no TextMesh component.", textTransform);
                return null;
            }

            text.anchor = anchor;
            text.alignment = anchor == TextAnchor.MiddleLeft ? TextAlignment.Left : TextAlignment.Center;
            text.fontSize = 48;
            text.color = textColor;
            KickLuckyCubeUiTheme.StyleWorldText(text, textColor, Mathf.Max(0.003f, text.characterSize * 0.07f));
            return text;
        }

        private void Refresh()
        {
            if (headerText == null || lineTexts == null || lineTexts.Length == 0)
            {
                BuildBoardText();
                if (headerText == null || lineTexts == null)
                {
                    return;
                }
            }

            var strength = stats != null ? Mathf.RoundToInt(stats.Strength) : 0;
            var soft = wallet != null ? wallet.SoftCurrency : 0;
            var mobs = inventory != null ? inventory.HotbarAnimalCount + inventory.StoredAnimalCount : 0;
            var score = Math.Max(strength, 0L) + soft / 10L + mobs * 250L;

            headerText.text = "TOP KICKERS";
            var entries = new[]
            {
                (Name: "You", Score: score),
                (Name: "BoxLord", Score: Math.Max(0L, score + 1850L)),
                (Name: "CubeQueen", Score: Math.Max(0L, score + 940L)),
                (Name: "LuckyNoob", Score: Math.Max(0L, score - 420L)),
                (Name: "GrassRunner", Score: Math.Max(0L, score - 980L)),
            };
            Array.Sort(entries, (left, right) => right.Score.CompareTo(left.Score));

            for (var index = 0; index < lineTexts.Length; index++)
            {
                if (lineTexts[index] != null)
                {
                    lineTexts[index].text = index < entries.Length
                        ? $"{index + 1}. {entries[index].Name}   {FormatScore(entries[index].Score)}"
                        : string.Empty;
                }
            }
        }

        private static string FormatScore(long score)
        {
            return KickLuckyCubeNumberFormatter.FormatCompact(score);
        }
    }
}
