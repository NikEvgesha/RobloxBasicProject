using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeCharacterCustomization : MonoBehaviour
    {
        private const string SelectedKeyPrefix = "KickLuckyCube.CharacterCustomization.Selected.";
        private const string OwnedKeyPrefix = "KickLuckyCube.CharacterCustomization.Owned.";

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeCharacterAppearance playerAppearance;
        [SerializeField, Min(0.1f)] private float bindingRefreshSeconds = 0.5f;

        private readonly HashSet<int> randomizedBotIds = new();
        private KickLuckyCubeAppearanceLoadout selectedLoadout;
        private float nextBindingRefresh;

        public static KickLuckyCubeCharacterCustomization Instance { get; private set; }

        public event Action Changed;

        public KickLuckyCubeAppearanceLoadout SelectedLoadout => selectedLoadout?.Clone();
        public KickLuckyCubeWallet Wallet => wallet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying
                || FindFirstObjectByType<KickLuckyCubeCharacterCustomization>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_CharacterCustomization_Runtime")
                .AddComponent<KickLuckyCubeCharacterCustomization>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
            LoadSelections();
            ApplyCurrentLoadout();
            EnsureBotAppearances();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < nextBindingRefresh)
            {
                return;
            }

            nextBindingRefresh = Time.unscaledTime + Mathf.Max(0.1f, bindingRefreshSeconds);
            ResolveReferences();
            ApplyCurrentLoadout();
            EnsureBotAppearances();
        }

        public string GetSelectedId(KickLuckyCubeAppearanceSlot slot)
        {
            EnsureLoaded();
            return selectedLoadout.Get(slot);
        }

        public bool IsOwned(string itemId)
        {
            var definition = KickLuckyCubeCharacterSkinCatalog.Get(itemId);
            return definition != null
                && (definition.IsFree || PlayerPrefs.GetInt(OwnedKeyPrefix + definition.Id, 0) != 0);
        }

        public bool TryPurchaseAndEquip(string itemId, out string status)
        {
            EnsureLoaded();
            ResolveReferences();

            var definition = KickLuckyCubeCharacterSkinCatalog.Get(itemId);
            if (definition == null)
            {
                status = "This appearance item does not exist.";
                return false;
            }

            var purchasedNow = false;
            if (!IsOwned(definition.Id))
            {
                if (wallet == null)
                {
                    status = "Wallet is not ready yet.";
                    return false;
                }

                if (!wallet.TrySpendHard(definition.HardCost))
                {
                    status = $"Need {definition.HardCost} hard currency for {definition.DisplayName}.";
                    return false;
                }

                PlayerPrefs.SetInt(OwnedKeyPrefix + definition.Id, 1);
                purchasedNow = true;
            }

            EquipDefinition(definition);
            SaveSelections();
            ApplyCurrentLoadout();
            Changed?.Invoke();
            status = purchasedNow
                ? $"{definition.DisplayName} purchased and equipped."
                : $"{definition.DisplayName} equipped.";
            return true;
        }

        public bool EquipOwned(string itemId, out string status)
        {
            var definition = KickLuckyCubeCharacterSkinCatalog.Get(itemId);
            if (definition == null || !IsOwned(itemId))
            {
                status = "Purchase this item first.";
                return false;
            }

            EnsureLoaded();
            EquipDefinition(definition);
            SaveSelections();
            ApplyCurrentLoadout();
            Changed?.Invoke();
            status = $"{definition.DisplayName} equipped.";
            return true;
        }

        public void ApplyCurrentLoadout()
        {
            EnsureLoaded();
            ResolvePlayerAppearance();
            playerAppearance?.ApplyLoadout(selectedLoadout);
        }

        public bool ValidateState(out string report)
        {
            EnsureLoaded();
            var problems = new List<string>(KickLuckyCubeCharacterSkinCatalog.ValidateCatalog());
            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                var definition = KickLuckyCubeCharacterSkinCatalog.Get(selectedLoadout.Get(slot));
                if (definition == null || definition.Slot != slot)
                {
                    problems.Add("Invalid selected item for slot " + slot);
                }
                else if (!IsOwned(definition.Id))
                {
                    problems.Add("Selected item is not owned: " + definition.Id);
                }
            }

            report = problems.Count == 0
                ? "Catalog, ownership and selected loadout are valid."
                : string.Join("\n", problems);
            return problems.Count == 0;
        }

        private void EquipDefinition(KickLuckyCubeAppearanceItemDefinition definition)
        {
            if (definition.Slot != KickLuckyCubeAppearanceSlot.Body)
            {
                selectedLoadout.Set(definition.Slot, definition.Id);
                return;
            }

            var female = KickLuckyCubeCharacterSkinCatalog.IsFemaleBody(definition.Id);
            selectedLoadout.Set(KickLuckyCubeAppearanceSlot.Body, definition.Id);

            var genderSlots = new[]
            {
                KickLuckyCubeAppearanceSlot.Torso,
                KickLuckyCubeAppearanceSlot.Legs,
                KickLuckyCubeAppearanceSlot.Boots,
                KickLuckyCubeAppearanceSlot.Gloves,
                KickLuckyCubeAppearanceSlot.Hair,
            };

            for (var index = 0; index < genderSlots.Length; index++)
            {
                var slot = genderSlots[index];
                var currentDefinition = KickLuckyCubeCharacterSkinCatalog.Get(selectedLoadout.Get(slot));
                if (currentDefinition == null || currentDefinition.IsFree)
                {
                    selectedLoadout.Set(slot, KickLuckyCubeCharacterSkinCatalog.GetDefaultId(slot, female));
                }
            }
        }

        private void LoadSelections()
        {
            var savedBody = PlayerPrefs.GetString(
                SelectedKeyPrefix + KickLuckyCubeAppearanceSlot.Body,
                KickLuckyCubeCharacterSkinCatalog.MaleBodyId);
            var female = KickLuckyCubeCharacterSkinCatalog.IsFemaleBody(savedBody);
            selectedLoadout = new KickLuckyCubeAppearanceLoadout();

            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                var fallbackId = KickLuckyCubeCharacterSkinCatalog.GetDefaultId(slot, female);
                var savedId = PlayerPrefs.GetString(SelectedKeyPrefix + slot, fallbackId);
                var definition = KickLuckyCubeCharacterSkinCatalog.Get(savedId);
                selectedLoadout.Set(slot,
                    definition != null && definition.Slot == slot && IsOwned(definition.Id)
                        ? definition.Id
                        : fallbackId);
            }

            SaveSelections();
        }

        private void SaveSelections()
        {
            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                PlayerPrefs.SetString(SelectedKeyPrefix + slot, selectedLoadout.Get(slot));
            }

            PlayerPrefs.Save();
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            ResolvePlayerAppearance();
        }

        private void ResolvePlayerAppearance()
        {
            if (playerAppearance != null)
            {
                return;
            }

            var player = FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            if (player == null)
            {
                return;
            }

            playerAppearance = player.GetComponent<KickLuckyCubeCharacterAppearance>()
                ?? player.gameObject.AddComponent<KickLuckyCubeCharacterAppearance>();
        }

        private void EnsureBotAppearances()
        {
            var bots = FindObjectsByType<KickLuckyCubeFakeOnlineBot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < bots.Length; index++)
            {
                var bot = bots[index];
                if (bot == null || !bot.gameObject.scene.IsValid() || !randomizedBotIds.Add(bot.GetInstanceID()))
                {
                    continue;
                }

                var appearance = bot.GetComponent<KickLuckyCubeCharacterAppearance>()
                    ?? bot.gameObject.AddComponent<KickLuckyCubeCharacterAppearance>();
                appearance.ApplyRandomLoadout(StableHash(bot.BotName) ^ (index * 397), true);
            }
        }

        private void EnsureLoaded()
        {
            if (selectedLoadout == null)
            {
                LoadSelections();
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = (int)2166136261;
                value ??= string.Empty;
                for (var index = 0; index < value.Length; index++)
                {
                    hash = (hash ^ value[index]) * 16777619;
                }

                return hash;
            }
        }
    }
}
