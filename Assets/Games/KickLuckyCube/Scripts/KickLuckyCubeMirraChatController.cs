using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MirraCloud.Core.Chats.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMirraChatController : MonoBehaviour
    {
        [SerializeField, Range(5, 50)] private int historyLimit = 25;
        [SerializeField, Min(1f)] private float minimumSendIntervalSeconds = 3f;
        [SerializeField, Range(20, 500)] private int maximumMessageLength = 180;

        private readonly List<ChatMessageDto> messages = new();
        private readonly ConcurrentQueue<ChatMessageDto> receivedMessages = new();
        private KickLuckyCubeMirraCloudService cloud;
        private string channelId;
        private float nextSendAt;
        private bool configuredEnabled;
        private bool opening;
        private bool joined;
        private RectTransform windowRoot;
        private Text messagesText;
        private Text statusText;
        private InputField messageInput;

        public static KickLuckyCubeMirraChatController Instance { get; private set; }
        public bool IsOpen { get; private set; }
        public bool IsAvailable => configuredEnabled && !string.IsNullOrWhiteSpace(channelId);
        public IReadOnlyList<ChatMessageDto> Messages => messages;
        public event Action<IReadOnlyList<ChatMessageDto>> MessagesChanged;
        public event Action<string> ChatError;

        private void Awake()
        {
            Instance = this;
            cloud = KickLuckyCubeMirraCloudService.Instance;
        }

        private void Start()
        {
            BuildRuntimeUi();
            MessagesChanged += RefreshMessageView;
            ChatError += SetStatus;
        }

        private void Update()
        {
            var changed = false;
            while (receivedMessages.TryDequeue(out var message))
            {
                UpsertMessage(message);
                changed = true;
            }

            if (changed)
            {
                MessagesChanged?.Invoke(messages);
            }
        }

        public void Configure(bool enabledForBranch, string configuredChannelId)
        {
            configuredEnabled = enabledForBranch;
            channelId = configuredChannelId?.Trim();
        }

        public void OpenChat()
        {
            if (!opening && !IsOpen)
            {
                if (windowRoot != null)
                {
                    windowRoot.gameObject.SetActive(true);
                }

                SetStatus(IsAvailable ? "Connecting…" : "Chat is disabled for this branch.");
                StartCoroutine(OpenChatRoutine());
            }
        }

        public void CloseChat()
        {
            if (cloud?.Sdk == null)
            {
                return;
            }

            cloud.Sdk.Chats.OnMessageReceived -= HandleMessageReceived;
            if (IsOpen)
            {
                cloud.Sdk.Chats.UnsubscribeAsync(channelId);
                cloud.Sdk.Chats.DisconnectAsync();
            }

            IsOpen = false;
            SetStatus("Disconnected");
        }

        public void SendChatMessage(string body)
        {
            if (!IsOpen || Time.realtimeSinceStartup < nextSendAt || string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            var sanitizedLength = Mathf.Min(body.Trim().Length, maximumMessageLength);
            var sanitizedBody = body.Trim().Substring(0, sanitizedLength);
            nextSendAt = Time.realtimeSinceStartup + minimumSendIntervalSeconds;
            StartCoroutine(SendMessageRoutine(sanitizedBody));
        }

        private IEnumerator OpenChatRoutine()
        {
            opening = true;
            cloud ??= KickLuckyCubeMirraCloudService.Instance;
            if (!IsAvailable || cloud == null || !cloud.IsAuthenticated)
            {
                opening = false;
                ChatError?.Invoke("Chat is not enabled for this Mirra branch.");
                yield break;
            }

            if (!joined)
            {
                var join = cloud.Sdk.Chats.JoinAsync(channelId);
                yield return join;
                if (!join.Result.IsSuccess && join.Result.Error?.HttpStatusCode != 409)
                {
                    Fail(join.Result.Error?.Message);
                    yield break;
                }

                joined = true;
            }

            var history = cloud.Sdk.Chats.GetMessagesAsync(channelId, limit: historyLimit);
            yield return history;
            if (history.Result.IsSuccess)
            {
                messages.Clear();
                if (history.Result.Data != null)
                {
                    messages.AddRange(history.Result.Data);
                }

                MessagesChanged?.Invoke(messages);
            }

            cloud.Sdk.Chats.OnMessageReceived -= HandleMessageReceived;
            cloud.Sdk.Chats.OnMessageReceived += HandleMessageReceived;
            var connect = cloud.Sdk.Chats.ConnectAsync();
            yield return connect;
            if (!connect.Result.IsSuccess)
            {
                Fail(connect.Result.Message);
                yield break;
            }

            var subscribe = cloud.Sdk.Chats.SubscribeAsync(channelId);
            yield return subscribe;
            if (!subscribe.Result.IsSuccess)
            {
                Fail(subscribe.Result.Message);
                yield break;
            }

            IsOpen = true;
            opening = false;
            SetStatus("Connected");
        }

        private IEnumerator SendMessageRoutine(string body)
        {
            var send = cloud.Sdk.Chats.SendMessageAsync(channelId, body);
            yield return send;
            if (!send.Result.IsSuccess)
            {
                ChatError?.Invoke(send.Result.Message ?? "Chat send failed.");
            }
        }

        private void HandleMessageReceived(ChatMessageDto message)
        {
            if (message != null)
            {
                receivedMessages.Enqueue(message);
            }
        }

        private void UpsertMessage(ChatMessageDto message)
        {
            var index = messages.FindIndex(existing => existing.MessageId == message.MessageId);
            if (index >= 0)
            {
                messages[index] = message;
            }
            else
            {
                messages.Add(message);
            }
        }

        private void BuildRuntimeUi()
        {
            var canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas();
            if (canvas == null)
            {
                return;
            }

            var canvasRect = canvas.transform as RectTransform;
            var openButton = CreateTextButton(
                canvasRect,
                "KLC_MirraChatOpenButton",
                "SellButton",
                "CHAT",
                new Vector2(116f, 48f),
                KickLuckyCubeUiTheme.Side,
                18);
            var openRect = openButton.transform as RectTransform;
            openRect.anchorMin = new Vector2(1f, 1f);
            openRect.anchorMax = new Vector2(1f, 1f);
            openRect.pivot = new Vector2(1f, 1f);
            openRect.anchoredPosition = new Vector2(-22f, -86f);
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenChat);

            windowRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_MirraChatWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = new Vector2(1f, 0.5f);
            windowRoot.anchorMax = new Vector2(1f, 0.5f);
            windowRoot.pivot = new Vector2(1f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(-22f, 0f);
            windowRoot.sizeDelta = new Vector2(520f, 610f);
            KickLuckyCubeUiTheme.AddImage(windowRoot.gameObject, new Color(0.04f, 0.08f, 0.11f, 0.97f));

            KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(
                windowRoot, "Title", KickLuckyCubeUiTheme.Font, "GLOBAL CHAT", 28,
                TextAnchor.MiddleLeft, new Vector2(330f, 44f), new Vector2(-70f, 266f));

            var closeButton = CreateTextButton(
                windowRoot, "CloseButton", "CloseButton", "X",
                new Vector2(44f, 36f), KickLuckyCubeUiTheme.Close, 18);
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(228f, 266f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                CloseChat();
                windowRoot.gameObject.SetActive(false);
            });

            messagesText = KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(
                windowRoot, "ChatMessagesText", KickLuckyCubeUiTheme.Font, "No messages yet.", 17,
                TextAnchor.LowerLeft, new Vector2(464f, 440f), new Vector2(0f, 20f));
            messagesText.verticalOverflow = VerticalWrapMode.Truncate;

            statusText = KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(
                windowRoot, "Status", KickLuckyCubeUiTheme.Font, "Disconnected", 14,
                TextAnchor.MiddleLeft, new Vector2(300f, 26f), new Vector2(-82f, -218f));

            messageInput = CreateInputField(windowRoot);
            var sendButton = CreateTextButton(
                windowRoot, "SendButton", "SellButton", "SEND",
                new Vector2(104f, 44f), KickLuckyCubeUiTheme.Primary, 16);
            sendButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(180f, -260f);
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(SendInputMessage);

            windowRoot.gameObject.SetActive(false);
        }

        private static Button CreateTextButton(
            RectTransform parent,
            string name,
            string templateName,
            string value,
            Vector2 size,
            Color color,
            int fontSize)
        {
            var rect = KickLuckyCubeUiPrefabFactory.FindDirectChild(parent, name)
                ?? KickLuckyCubeUiPrefabFactory.CreateRectInstance(templateName, name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var button = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Button>(rect.gameObject);
            var image = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Image>(rect.gameObject);
            button.targetGraphic = image;
            KickLuckyCubeUiTheme.StyleButton(button, name);
            image.color = color;
            KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(
                rect, "Label", KickLuckyCubeUiTheme.Font, value, fontSize,
                TextAnchor.MiddleCenter, size, Vector2.zero);
            return button;
        }

        private InputField CreateInputField(RectTransform parent)
        {
            var rect = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_MirraChatInput", parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(344f, 44f);
            rect.anchoredPosition = new Vector2(-54f, -260f);
            var image = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Image>(rect.gameObject);
            image.color = new Color(1f, 1f, 1f, 0.94f);
            var text = KickLuckyCubeUiPrefabFactory.FindText(rect, "Text");
            text.font = KickLuckyCubeUiTheme.Font;
            text.fontSize = 17;
            text.color = KickLuckyCubeUiTheme.Ink;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;

            var input = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<InputField>(rect.gameObject);
            input.targetGraphic = image;
            input.textComponent = text;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = maximumMessageLength;
            input.placeholder = null;
            return input;
        }

        private void SendInputMessage()
        {
            if (messageInput == null || string.IsNullOrWhiteSpace(messageInput.text))
            {
                return;
            }

            var body = messageInput.text;
            messageInput.text = string.Empty;
            SendChatMessage(body);
        }

        private void RefreshMessageView(IReadOnlyList<ChatMessageDto> currentMessages)
        {
            if (messagesText == null)
            {
                return;
            }

            var startIndex = Mathf.Max(0, currentMessages.Count - 12);
            var lines = new List<string>();
            for (var index = startIndex; index < currentMessages.Count; index++)
            {
                var message = currentMessages[index];
                if (message == null || message.DeletedAt.HasValue)
                {
                    continue;
                }

                var sender = string.IsNullOrWhiteSpace(message.SenderId)
                    ? "player"
                    : message.SenderId.Substring(0, Mathf.Min(8, message.SenderId.Length));
                lines.Add($"{sender}: {message.Body}");
            }

            messagesText.text = lines.Count > 0 ? string.Join("\n", lines) : "No messages yet.";
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void Fail(string message)
        {
            if (cloud?.Sdk != null)
            {
                cloud.Sdk.Chats.OnMessageReceived -= HandleMessageReceived;
                cloud.Sdk.Chats.DisconnectAsync();
            }

            opening = false;
            IsOpen = false;
            ChatError?.Invoke(string.IsNullOrWhiteSpace(message) ? "Chat operation failed." : message);
        }

        private void OnDestroy()
        {
            MessagesChanged -= RefreshMessageView;
            ChatError -= SetStatus;
            CloseChat();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
