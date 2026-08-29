using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMirraGhostIdentity : MonoBehaviour
    {
        public string ProfileId { get; private set; }
        public string Nickname { get; private set; }
        public bool IsFriend { get; private set; }
        public bool FriendRequestInFlight { get; private set; }
        public bool FriendRequestSent { get; private set; }

        private KickLuckyCubePlayerController player;
        private GUIStyle promptStyle;

        public void Configure(string profileId, string nickname, bool isFriend)
        {
            ProfileId = profileId;
            Nickname = nickname;
            IsFriend = isFriend;
            EnsureClickCollider();
        }

        private void Update()
        {
            if (IsFriend || FriendRequestSent || FriendRequestInFlight || !IsPlayerNearby())
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.F))
#endif
            {
                RequestFriendship();
            }
        }

        private void OnMouseDown()
        {
            if (IsPlayerNearby())
            {
                RequestFriendship();
            }
        }

        private void OnGUI()
        {
            if (IsFriend || FriendRequestSent || !IsPlayerNearby())
            {
                return;
            }

            promptStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
            };
            GUI.Box(
                new Rect(Screen.width * 0.5f - 170f, Screen.height - 118f, 340f, 42f),
                $"F / Click — add {Nickname} as friend",
                promptStyle);
        }

        public void RequestFriendship()
        {
            if (!IsFriend && !FriendRequestSent && !FriendRequestInFlight && !string.IsNullOrWhiteSpace(ProfileId))
            {
                StartCoroutine(SendFriendRequest());
            }
        }

        private IEnumerator SendFriendRequest()
        {
            var cloud = KickLuckyCubeMirraCloudService.Instance;
            if (cloud == null || !cloud.IsAuthenticated)
            {
                yield break;
            }

            FriendRequestInFlight = true;
            var operation = cloud.Sdk.Friends.SendAsync(ProfileId);
            yield return operation;
            FriendRequestInFlight = false;
            if (!operation.Result.IsSuccess)
            {
                KickLuckyCubeMirraAnalyticsController.Track("klc_friend_request", "result", "failed");
                Debug.LogWarning($"Kick Lucky Cube: friend request to {Nickname} failed: {operation.Result.Error?.Message}");
                yield break;
            }

            FriendRequestSent = true;
            KickLuckyCubeMirraAnalyticsController.Track("klc_friend_request", "result", "sent");
        }

        private bool IsPlayerNearby()
        {
            player ??= FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            return player != null && Vector3.SqrMagnitude(player.transform.position - transform.position) <= 16f;
        }

        private void EnsureClickCollider()
        {
            if (GetComponentInChildren<Collider>(true) != null)
            {
                return;
            }

            var capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.height = 2f;
            capsule.radius = 0.45f;
        }
    }
}
