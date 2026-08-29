using System.Collections;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMirraGhostIdentity : MonoBehaviour
    {
        public string ProfileId { get; private set; }
        public string Nickname { get; private set; }
        public bool IsFriend { get; private set; }
        public bool FriendRequestInFlight { get; private set; }

        public void Configure(string profileId, string nickname, bool isFriend)
        {
            ProfileId = profileId;
            Nickname = nickname;
            IsFriend = isFriend;
        }

        public void RequestFriendship()
        {
            if (!IsFriend && !FriendRequestInFlight && !string.IsNullOrWhiteSpace(ProfileId))
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
                Debug.LogWarning($"Kick Lucky Cube: friend request to {Nickname} failed: {operation.Result.Error?.Message}");
            }
        }
    }
}
