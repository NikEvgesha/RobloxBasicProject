using System;
using System.Collections;
using System.Linq;
using MirraCloud.Core.DailyRewards.Dto;
using MirraCloud.Core.Friends.Dto;
using MirraCloud.Core.PromoCodes.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    /// <summary>
    /// A deliberately lazy Mirra test panel. It performs no polling: reads happen on open/refresh,
    /// while claims, redemptions and friendship changes require a player button press.
    /// </summary>
    public sealed class KickLuckyCubeMirraLiveOpsController : MonoBehaviour
    {
        private const string CalendarKey = "klc_welcome_week";
        private const float AutomaticRefreshCooldownSeconds = 60f;

        private KickLuckyCubeMirraCloudService cloud;
        private RectTransform windowRoot;
        private Text titleText;
        private Text bodyText;
        private Text statusText;
        private InputField promoInput;
        private Button claimButton;
        private Button redeemButton;
        private Button acceptButton;
        private Button rejectButton;
        private Button revokeButton;
        private Button removeButton;
        private DailyRewardCalendarDto calendar;
        private DailyRewardStatusDto dailyStatus;
        private GetPlayerDto[] friends = Array.Empty<GetPlayerDto>();
        private GetFriendRequestDto[] incoming = Array.Empty<GetFriendRequestDto>();
        private GetFriendRequestDto[] outgoing = Array.Empty<GetFriendRequestDto>();
        private float lastAutomaticRefreshAt = float.NegativeInfinity;
        private bool busy;

        private void Awake()
        {
            cloud = KickLuckyCubeMirraCloudService.Instance;
        }

        private void Start()
        {
            BuildRuntimeUi();
        }

        private void OpenWindow()
        {
            if (windowRoot == null)
            {
                return;
            }

            windowRoot.gameObject.SetActive(true);
            if (Time.realtimeSinceStartup - lastAutomaticRefreshAt >= AutomaticRefreshCooldownSeconds)
            {
                RefreshDaily(false);
            }
        }

        private void RefreshDaily(bool force)
        {
            SetActionVisibility(true, false, false);
            if (!busy && (force || Time.realtimeSinceStartup - lastAutomaticRefreshAt >= AutomaticRefreshCooldownSeconds))
            {
                StartCoroutine(RefreshDailyRoutine());
            }
        }

        private IEnumerator RefreshDailyRoutine()
        {
            if (!BeginOperation("DAILY REWARDS", "Loading calendar…")) yield break;

            var calendars = cloud.Sdk.DailyRewards.GetCalendarsAsync();
            yield return calendars;
            if (!calendars.Result.IsSuccess)
            {
                EndOperation("Daily Rewards unavailable: " + ErrorOf(calendars.Result.Error?.Message));
                yield break;
            }

            calendar = calendars.Result.Data?.FirstOrDefault(item => item.key == CalendarKey);
            if (calendar == null)
            {
                EndOperation("Calendar '" + CalendarKey + "' is not deployed on this branch.");
                yield break;
            }

            var status = cloud.Sdk.DailyRewards.GetStatusAsync(calendar.id);
            yield return status;
            if (!status.Result.IsSuccess)
            {
                EndOperation("Daily status unavailable: " + ErrorOf(status.Result.Error?.Message));
                yield break;
            }

            dailyStatus = status.Result.Data;
            lastAutomaticRefreshAt = Time.realtimeSinceStartup;
            bodyText.text = $"{calendar.name}\nDay {dailyStatus.currentDayNumber} / {dailyStatus.cycleLengthDays}\n" +
                (dailyStatus.canClaimToday ? "A reward is ready." : $"Next reset: {dailyStatus.nextResetTime:u}");
            claimButton.interactable = dailyStatus.canClaimToday;
            EndOperation(dailyStatus.canClaimToday ? "Ready to claim" : "Up to date");
        }

        private void ClaimDaily()
        {
            if (!busy && calendar != null && dailyStatus != null && dailyStatus.canClaimToday)
            {
                StartCoroutine(ClaimDailyRoutine());
            }
        }

        private IEnumerator ClaimDailyRoutine()
        {
            if (!BeginOperation("DAILY REWARDS", "Claiming…")) yield break;
            var operation = cloud.Sdk.DailyRewards.ClaimAsync(calendar.id);
            yield return operation;
            if (!operation.Result.IsSuccess)
            {
                EndOperation("Claim failed: " + ErrorOf(operation.Result.Error?.Message));
                yield break;
            }

            var result = operation.Result.Data;
            KickLuckyCubeMirraAnalyticsController.Track("klc_daily_reward", new System.Collections.Generic.Dictionary<string, string>
            {
                ["result"] = "claimed",
                ["value"] = result.dayNumberClaimed.ToString(),
            });
            KickLuckyCubeMirraCloudGameplaySync.Instance?.RefreshEconomyAfterExternalGrant();
            bodyText.text = $"Day {result.dayNumberClaimed} claimed.\nTotal claim days: {result.newTotalClaimDays}.";
            claimButton.interactable = false;
            EndOperation("Reward received");
        }

        private void ShowPromo()
        {
            titleText.text = "PROMO CODE";
            bodyText.text = "Enter a Mirra Hub promo code. Redemption is sent only when you press REDEEM.";
            statusText.text = "No background requests";
            SetActionVisibility(false, true, false);
        }

        private void RedeemPromo()
        {
            if (!busy && promoInput != null && !string.IsNullOrWhiteSpace(promoInput.text))
            {
                StartCoroutine(RedeemPromoRoutine(promoInput.text.Trim()));
            }
        }

        private IEnumerator RedeemPromoRoutine(string code)
        {
            if (!BeginOperation("PROMO CODE", "Redeeming…")) yield break;
            var operation = cloud.Sdk.PromoCodes.RedeemAsync(code);
            yield return operation;
            if (!operation.Result.IsSuccess)
            {
                TrackPromo("transport_failed");
                EndOperation("Promo request failed: " + ErrorOf(operation.Result.Error?.Message));
                yield break;
            }

            var result = operation.Result.Data;
            var accepted = result != null && result.status == RedemptionStatus.Success;
            TrackPromo(accepted ? "success" : (result?.status.ToString() ?? "empty_response"));
            if (!accepted)
            {
                EndOperation("Code refused: " + (result?.status.ToString() ?? "unknown"));
                yield break;
            }

            KickLuckyCubeMirraCloudGameplaySync.Instance?.RefreshEconomyAfterExternalGrant();
            bodyText.text = $"{result.campaignDisplayName}\nRewards: {result.rewards?.Count ?? 0} · Effects: {result.effects?.Count ?? 0}";
            EndOperation("Code accepted");
        }

        private void RefreshFriends()
        {
            if (!busy) StartCoroutine(RefreshFriendsRoutine());
        }

        private IEnumerator RefreshFriendsRoutine()
        {
            if (!BeginOperation("FRIENDS", "Loading on demand…")) yield break;

            var friendOp = cloud.Sdk.Friends.GetFriendsAsync(true);
            yield return friendOp;
            if (!friendOp.Result.IsSuccess)
            {
                EndOperation("Friends backend error: " + ErrorOf(friendOp.Result.Error?.Message));
                yield break;
            }

            var incomingOp = cloud.Sdk.Friends.GetIncomingAsync();
            yield return incomingOp;
            if (!incomingOp.Result.IsSuccess)
            {
                EndOperation("Incoming requests error: " + ErrorOf(incomingOp.Result.Error?.Message));
                yield break;
            }

            var outgoingOp = cloud.Sdk.Friends.GetOutgoingAsync();
            yield return outgoingOp;
            if (!outgoingOp.Result.IsSuccess)
            {
                EndOperation("Outgoing requests error: " + ErrorOf(outgoingOp.Result.Error?.Message));
                yield break;
            }

            friends = friendOp.Result.Data ?? Array.Empty<GetPlayerDto>();
            incoming = incomingOp.Result.Data ?? Array.Empty<GetFriendRequestDto>();
            outgoing = outgoingOp.Result.Data ?? Array.Empty<GetFriendRequestDto>();
            bodyText.text = $"Friends: {friends.Length}\nIncoming: {incoming.Length}\nOutgoing: {outgoing.Length}\n" + FirstSocialName();
            acceptButton.interactable = incoming.Length > 0;
            rejectButton.interactable = incoming.Length > 0;
            revokeButton.interactable = outgoing.Length > 0;
            removeButton.interactable = friends.Length > 0;
            SetActionVisibility(false, false, true);
            EndOperation("Loaded once — press REFRESH to read again");
        }

        private void RunFriendAction(int action)
        {
            if (!busy) StartCoroutine(FriendActionRoutine(action));
        }

        private IEnumerator FriendActionRoutine(int action)
        {
            if (!BeginOperation("FRIENDS", "Updating…")) yield break;
            Plugins.MirraCloud.Core.General.AsyncOperations.AsyncOperation<MirraCloud.Core.RestApiResult> operation;
            if (action == 0 && incoming.Length > 0) operation = cloud.Sdk.Friends.AcceptAsync(incoming[0].SourcePlayerId);
            else if (action == 1 && incoming.Length > 0) operation = cloud.Sdk.Friends.RejectAsync(incoming[0].SourcePlayerId);
            else if (action == 2 && outgoing.Length > 0) operation = cloud.Sdk.Friends.RevokeAsync(outgoing[0].TargetPlayerId);
            else if (action == 3 && friends.Length > 0) operation = cloud.Sdk.Friends.RemoveFriendAsync(friends[0].PlayerId);
            else
            {
                EndOperation("Nothing to update");
                yield break;
            }

            yield return operation;
            KickLuckyCubeMirraAnalyticsController.Track("klc_friend_request", "result", operation.Result.IsSuccess ? "updated" : "failed");
            if (!operation.Result.IsSuccess)
            {
                EndOperation("Friend action failed: " + ErrorOf(operation.Result.Error?.Message));
                yield break;
            }

            busy = false;
            StartCoroutine(RefreshFriendsRoutine());
        }

        private bool BeginOperation(string title, string status)
        {
            cloud ??= KickLuckyCubeMirraCloudService.Instance;
            if (busy || cloud == null || !cloud.IsAuthenticated)
            {
                if (statusText != null) statusText.text = "Mirra Cloud session is not ready.";
                return false;
            }

            busy = true;
            titleText.text = title;
            statusText.text = status;
            return true;
        }

        private void EndOperation(string status)
        {
            busy = false;
            if (statusText != null) statusText.text = status;
        }

        private void BuildRuntimeUi()
        {
            var canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas();
            if (canvas == null) return;
            var canvasRect = canvas.transform as RectTransform;

            var open = CreateButton(canvasRect, "KLC_MirraLiveOpsOpenButton", "LIVE", new Vector2(116f, 48f), KickLuckyCubeUiTheme.Warning, 17);
            var openRect = open.transform as RectTransform;
            openRect.anchorMin = openRect.anchorMax = openRect.pivot = new Vector2(1f, 1f);
            openRect.anchoredPosition = new Vector2(-22f, -142f);
            open.onClick.AddListener(OpenWindow);

            windowRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_MirraLiveOpsWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = windowRoot.anchorMax = windowRoot.pivot = new Vector2(1f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(-22f, 0f);
            windowRoot.sizeDelta = new Vector2(540f, 430f);
            KickLuckyCubeUiTheme.AddImage(windowRoot.gameObject, new Color(0.04f, 0.08f, 0.11f, 0.98f));

            titleText = Label(windowRoot, "Title", "MIRRA LIVEOPS", 26, new Vector2(360f, 42f), new Vector2(-55f, 181f));
            titleText.color = KickLuckyCubeUiTheme.Ink;
            var close = CreateButton(windowRoot, "CloseButton", "X", new Vector2(44f, 36f), KickLuckyCubeUiTheme.Close, 18);
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(238f, 181f);
            close.onClick.AddListener(() => windowRoot.gameObject.SetActive(false));

            var daily = CreateButton(windowRoot, "DailyTab", "DAILY", new Vector2(120f, 38f), KickLuckyCubeUiTheme.Primary, 14);
            daily.GetComponent<RectTransform>().anchoredPosition = new Vector2(-145f, 132f);
            daily.onClick.AddListener(() => RefreshDaily(true));
            var promo = CreateButton(windowRoot, "PromoTab", "PROMO", new Vector2(120f, 38f), KickLuckyCubeUiTheme.Secondary, 14);
            promo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 132f);
            promo.onClick.AddListener(ShowPromo);
            var social = CreateButton(windowRoot, "FriendsTab", "FRIENDS", new Vector2(120f, 38f), KickLuckyCubeUiTheme.Side, 14);
            social.GetComponent<RectTransform>().anchoredPosition = new Vector2(145f, 132f);
            social.onClick.AddListener(RefreshFriends);

            bodyText = Label(windowRoot, "Body", "Open a tab to load Mirra Cloud data.", 17, new Vector2(474f, 150f), new Vector2(0f, 38f));
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.color = KickLuckyCubeUiTheme.Ink;
            statusText = Label(windowRoot, "Status", "No background requests", 14, new Vector2(470f, 28f), new Vector2(0f, -62f));
            statusText.color = KickLuckyCubeUiTheme.Ink;

            promoInput = CreateInput(windowRoot);
            claimButton = CreateButton(windowRoot, "ClaimButton", "CLAIM", new Vector2(145f, 42f), KickLuckyCubeUiTheme.Primary, 15);
            claimButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -142f);
            claimButton.onClick.AddListener(ClaimDaily);
            redeemButton = CreateButton(windowRoot, "RedeemButton", "REDEEM", new Vector2(145f, 42f), KickLuckyCubeUiTheme.Warning, 15);
            redeemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(165f, -142f);
            redeemButton.onClick.AddListener(RedeemPromo);

            acceptButton = SocialButton("Accept", "ACCEPT", -180f, () => RunFriendAction(0));
            rejectButton = SocialButton("Reject", "REJECT", -60f, () => RunFriendAction(1));
            revokeButton = SocialButton("Revoke", "REVOKE", 60f, () => RunFriendAction(2));
            removeButton = SocialButton("Remove", "REMOVE", 180f, () => RunFriendAction(3));
            SetActionVisibility(true, false, false);
            windowRoot.gameObject.SetActive(false);
        }

        private Button SocialButton(string name, string value, float x, UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(windowRoot, name + "Button", value, new Vector2(108f, 38f), KickLuckyCubeUiTheme.Side, 13);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -142f);
            button.onClick.AddListener(action);
            return button;
        }

        private void SetActionVisibility(bool daily, bool promo, bool social)
        {
            if (claimButton != null) claimButton.gameObject.SetActive(daily);
            if (redeemButton != null) redeemButton.gameObject.SetActive(promo);
            if (promoInput != null) promoInput.gameObject.SetActive(promo);
            foreach (var button in new[] { acceptButton, rejectButton, revokeButton, removeButton })
            {
                if (button != null) button.gameObject.SetActive(social);
            }
        }

        private InputField CreateInput(RectTransform parent)
        {
            var rect = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_MirraPromoInput", parent);
            rect.sizeDelta = new Vector2(310f, 42f);
            rect.anchoredPosition = new Vector2(-72f, -142f);
            var image = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Image>(rect.gameObject);
            image.color = new Color(1f, 1f, 1f, 0.94f);
            var text = KickLuckyCubeUiPrefabFactory.FindText(rect, "Text");
            text.font = KickLuckyCubeUiTheme.Font;
            text.fontSize = 16;
            text.color = KickLuckyCubeUiTheme.Ink;
            text.alignment = TextAnchor.MiddleLeft;
            var input = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<InputField>(rect.gameObject);
            input.targetGraphic = image;
            input.textComponent = text;
            input.characterLimit = 64;
            input.text = "KLC-CODEX-TEST";
            return input;
        }

        private static Button CreateButton(RectTransform parent, string name, string value, Vector2 size, Color color, int fontSize)
        {
            var rect = KickLuckyCubeUiPrefabFactory.CreateRectInstance("SellButton", name, parent);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            var button = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Button>(rect.gameObject);
            var image = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Image>(rect.gameObject);
            button.onClick.RemoveAllListeners();
            button.targetGraphic = image;
            image.color = color;
            KickLuckyCubeUiTheme.StyleButton(button, name);
            Label(rect, "Label", value, fontSize, size, Vector2.zero).alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private static Text Label(RectTransform parent, string name, string value, int fontSize, Vector2 size, Vector2 position)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, KickLuckyCubeUiTheme.Font, value, fontSize, TextAnchor.MiddleLeft, size, position);
        }

        private string FirstSocialName()
        {
            if (incoming.Length > 0) return "Incoming from " + Short(incoming[0].SourcePlayerId);
            if (outgoing.Length > 0) return "Outgoing to " + Short(outgoing[0].TargetPlayerId);
            if (friends.Length > 0)
            {
                var info = friends[0].PlayerInfo;
                return "First friend: " + (info != null && !string.IsNullOrWhiteSpace(info.Nickname) ? info.Nickname : Short(friends[0].PlayerId));
            }
            return "No relationships yet.";
        }

        private static string Short(string value) => string.IsNullOrEmpty(value) ? "unknown" : value.Substring(0, Mathf.Min(12, value.Length));
        private static string ErrorOf(string value) => string.IsNullOrWhiteSpace(value) ? "unknown error" : value;
        private static void TrackPromo(string result) => KickLuckyCubeMirraAnalyticsController.Track("klc_promo_redeemed", "result", result);
    }
}
