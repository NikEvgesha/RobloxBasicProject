using System;
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
        [SerializeField] private Vector3 headerLocalPosition = new(0f, 2.15f, -0.08f);
        [SerializeField] private Vector3 firstLineLocalPosition = new(0f, 1.55f, -0.08f);
        [SerializeField] private float lineSpacing = 0.34f;
        [SerializeField, Min(1)] private int lineCount = 5;
        [SerializeField] private Color textColor = new(1f, 0.95f, 0.78f);

        private TextMesh headerText;
        private TextMesh[] lineTexts = Array.Empty<TextMesh>();
        private float refreshTimer;

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
            var board = GameObject.Find(boardName);
            if (board == null)
            {
                return;
            }

            var visualRoot = ResolveVisualRoot(board.transform);
            headerText = CreateOrGetLine(visualRoot, HeaderLineName, headerLocalPosition, 0.18f, TextAnchor.MiddleCenter);
            lineTexts = new TextMesh[lineCount];
            for (var index = 0; index < lineCount; index++)
            {
                lineTexts[index] = CreateOrGetLine(
                    visualRoot,
                    LineNamePrefix + (index + 1).ToString("00"),
                    firstLineLocalPosition + Vector3.down * (lineSpacing * index),
                    0.12f,
                    TextAnchor.MiddleLeft);
            }
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

            var fallback = new GameObject(VisualRootName).transform;
            fallback.SetParent(board, false);
            fallback.localPosition = Vector3.zero;
            fallback.localRotation = Quaternion.identity;
            fallback.localScale = Vector3.one;
            return fallback;
        }

        private TextMesh CreateOrGetLine(Transform parent, string name, Vector3 localPosition, float characterSize, TextAnchor anchor)
        {
            var existing = parent.Find(name);
            var created = existing == null;
            var textTransform = created ? new GameObject(name).transform : existing;
            if (created)
            {
                textTransform.SetParent(parent, false);
                textTransform.localPosition = localPosition;
                textTransform.localRotation = Quaternion.identity;
                textTransform.localScale = Vector3.one;
            }

            var text = textTransform.GetComponent<TextMesh>();
            if (text == null)
            {
                text = textTransform.gameObject.AddComponent<TextMesh>();
            }

            text.anchor = anchor;
            text.alignment = anchor == TextAnchor.MiddleLeft ? TextAlignment.Left : TextAlignment.Center;
            if (created)
            {
                text.characterSize = characterSize;
            }

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
            var score = Mathf.Max(strength, 0) + soft / 10 + mobs * 250;

            headerText.text = "TOP KICKERS";
            var lines = new[]
            {
                $"1. You   {FormatScore(score)}",
                $"2. BoxLord   {FormatScore(score + 1850)}",
                $"3. CubeQueen   {FormatScore(score + 940)}",
                $"4. LuckyNoob   {FormatScore(Mathf.Max(0, score - 420))}",
                $"5. GrassRunner   {FormatScore(Mathf.Max(0, score - 980))}",
            };

            for (var index = 0; index < lineTexts.Length; index++)
            {
                if (lineTexts[index] != null)
                {
                    lineTexts[index].text = index < lines.Length ? lines[index] : string.Empty;
                }
            }
        }

        private static string FormatScore(int score)
        {
            if (score >= 1000000)
            {
                return (score / 1000000f).ToString("0.0M");
            }

            if (score >= 1000)
            {
                return (score / 1000f).ToString("0.0K");
            }

            return score.ToString();
        }
    }
}
