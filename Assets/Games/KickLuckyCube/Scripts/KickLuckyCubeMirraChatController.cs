using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MirraCloud.Core.Chats.Dto;
using UnityEngine;

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
            CloseChat();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
