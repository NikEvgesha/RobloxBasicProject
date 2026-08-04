using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeSaveTools
    {
        private static readonly string[] ExplicitKeys =
        {
            "KickLuckyCube.Settings.Music",
            "KickLuckyCube.Settings.Sfx",
            "KickLuckyCube.Settings.Language",
            "KickLuckyCube.Wheel.NextSpinAt",
            "KickLuckyCube.Wheel.NextSpinAtUnix",
            "KickLuckyCube.PlayerStats.Strength",
            "KickLuckyCube.PlayerStats.AnimalSpeed",
            "KickLuckyCube.PlayerStats.StrengthToolTier",
            "KickLuckyCube.PlayerStats.SelectedStrengthToolTier",
            "KickLuckyCube.PlayerStats.SpeedUpgradeLevel",
            "KickLuckyCube.Kick.SelectedStrength",
            "KickLuckyCube.Kick.UseMaximum",
            "KickLuckyCube.Wallet.Soft",
            "KickLuckyCube.Wallet.Hard",
            "KickLuckyCube.Wallet.Soft64",
            "KickLuckyCube.Wallet.Hard64",
            "KickLuckyCube.Rebirth.Count",
            "KickLuckyCube.Rewards.StartedAt",
            "KickLuckyCube.Rewards.ElapsedSeconds",
            "KickLuckyCube.Rewards.ClaimedMask",
            "KickLuckyCube.Inventory.State",
            "KickLuckyCube.Future.WeatherEndsAt",
            "KickLuckyCube.Future.ExchangeCharges",
            "KickLuckyCube.Future.ExchangeLastAt",
            "KickLuckyCube.Future.RatingGiftClaimed",
            "KickLuckyCube.Future.EpicMobBought.0",
            "KickLuckyCube.Future.EpicMobBought.1",
            "KickLuckyCube.Future.EpicMobBought.2",
            "KickLuckyCube.Style.Selected",
            "KickLuckyCube.Style.Owned.0",
            "KickLuckyCube.Style.Owned.1",
            "KickLuckyCube.Style.Owned.2",
            "KickLuckyCube.Style.Owned.3",
            "KickLuckyCube.KickStyle.SelectedId",
            "KickLuckyCube.KickStyle.Owned.classic",
            "KickLuckyCube.KickStyle.Owned.roundhouse",
            "KickLuckyCube.KickStyle.Owned.tornado",
            "KickLuckyCube.KickStyle.Owned.bicycle",
            "KickLuckyCube.KickStyle.Owned.scorpion",
            "KickLuckyCube.KickStyle.Owned.lightning_spiral",
        };

        private static readonly string[] StableSuffixes =
        {
            "AnimalName",
            "AnimalJson",
            "PendingSoft",
            "SavedAt",
            "UpgradeLevel",
        };

        [MenuItem("Tools/Kick Lucky Cube/Clear Prototype Save")]
        public static void ClearPrototypeSave()
        {
            foreach (var key in ExplicitKeys)
            {
                PlayerPrefs.DeleteKey(key);
            }

            var stableKeyCount = ClearOpenSceneStableKeys();
            PlayerPrefs.Save();
            Debug.Log($"Kick Lucky Cube prototype save cleared. Keys: {ExplicitKeys.Length + stableKeyCount}.");
        }

        private static int ClearOpenSceneStableKeys()
        {
            var count = 0;
            var stableSlots = Resources
                .FindObjectsOfTypeAll<KickLuckyCubeStableSlot>()
                .Where(slot => slot != null && slot.gameObject.scene.IsValid());

            foreach (var slot in stableSlots)
            {
                var serializedSlot = new SerializedObject(slot);
                var prefixProperty = serializedSlot.FindProperty("saveKeyPrefix");
                var idProperty = serializedSlot.FindProperty("stableSlotId");
                var prefix = prefixProperty != null && !string.IsNullOrWhiteSpace(prefixProperty.stringValue)
                    ? prefixProperty.stringValue
                    : "KickLuckyCube.Stable.";
                var slotId = idProperty != null && !string.IsNullOrWhiteSpace(idProperty.stringValue)
                    ? idProperty.stringValue
                    : slot.gameObject.name;

                foreach (var suffix in StableSuffixes)
                {
                    PlayerPrefs.DeleteKey(prefix + slotId + "." + suffix);
                    count++;
                }
            }

            return count;
        }
    }
}
