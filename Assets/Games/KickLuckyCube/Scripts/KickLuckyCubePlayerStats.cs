using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePlayerStats : MonoBehaviour
    {
        private const string StrengthKey = "Strength";
        private const string AnimalSpeedKey = "AnimalSpeed";
        private const string StrengthToolTierKey = "StrengthToolTier";
        private const string SelectedStrengthToolTierKey = "SelectedStrengthToolTier";
        private const string SpeedUpgradeLevelKey = "SpeedUpgradeLevel";

        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.PlayerStats.";
        [SerializeField, Min(0f)] private float initialStrength = 120f;
        [SerializeField, Min(0f)] private float initialAnimalSpeed = 7f;
        [SerializeField, Min(1)] private int initialStrengthToolTier = 1;
        [SerializeField, Min(0)] private int initialSpeedUpgradeLevel;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private float strength;
        private float animalSpeed;
        private int strengthToolTier;
        private int selectedStrengthToolTier;
        private int speedUpgradeLevel;
        private bool initialized;

        public event Action Changed;

        public float Strength => initialized ? strength : Mathf.Max(0f, initialStrength);
        public float AnimalSpeed => initialized ? animalSpeed : Mathf.Max(0f, initialAnimalSpeed);
        public int StrengthToolTier => initialized ? strengthToolTier : Mathf.Max(1, initialStrengthToolTier);
        public int SelectedStrengthToolTier => initialized
            ? Mathf.Clamp(selectedStrengthToolTier, 1, StrengthToolTier)
            : Mathf.Max(1, initialStrengthToolTier);
        public int SpeedUpgradeLevel => initialized ? speedUpgradeLevel : Mathf.Max(0, initialSpeedUpgradeLevel);

        private void Awake()
        {
            LoadProgression();
        }

        public void AddStrength(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetStrength(strength + amount);
        }

        public void SetStrength(float value)
        {
            strength = Mathf.Max(0f, value);
            CommitChange();
        }

        public void SetAnimalSpeed(float value)
        {
            animalSpeed = Mathf.Max(0f, value);
            CommitChange();
        }

        public void AddAnimalSpeed(float amount)
        {
            AddAnimalSpeedLevels(1, amount);
        }

        public void AddAnimalSpeedLevels(int levels, float amountPerLevel)
        {
            if (levels <= 0 || amountPerLevel <= 0f)
            {
                return;
            }

            animalSpeed = Mathf.Max(0f, AnimalSpeed + amountPerLevel * levels);
            speedUpgradeLevel = Mathf.Max(0, SpeedUpgradeLevel + levels);
            CommitChange();
        }

        public void SetSpeedUpgradeLevel(int value)
        {
            speedUpgradeLevel = Mathf.Max(0, value);
            CommitChange();
        }

        public void UpgradeStrengthTool()
        {
            var nextTier = StrengthToolTier + 1;
            SetStrengthToolTier(nextTier);
            SelectStrengthToolTier(nextTier);
        }

        public void SetStrengthToolTier(int value)
        {
            strengthToolTier = Mathf.Max(1, value);
            selectedStrengthToolTier = Mathf.Clamp(SelectedStrengthToolTier, 1, strengthToolTier);
            CommitChange();
        }

        public void SelectStrengthToolTier(int value)
        {
            selectedStrengthToolTier = Mathf.Clamp(value, 1, StrengthToolTier);
            CommitChange();
        }

        public void ResetProgression()
        {
            strength = Mathf.Max(0f, initialStrength);
            animalSpeed = Mathf.Max(0f, initialAnimalSpeed);
            strengthToolTier = Mathf.Max(1, initialStrengthToolTier);
            selectedStrengthToolTier = strengthToolTier;
            speedUpgradeLevel = Mathf.Max(0, initialSpeedUpgradeLevel);
            CommitChange();
        }

        public void ResetStrengthForRebirth()
        {
            strength = Mathf.Max(0f, initialStrength);
            CommitChange();
        }

        private void LoadProgression()
        {
            if (Application.isPlaying && saveInPlayerPrefs)
            {
                strength = Mathf.Max(0f, PlayerPrefs.GetFloat(saveKeyPrefix + StrengthKey, initialStrength));
                animalSpeed = Mathf.Max(0f, PlayerPrefs.GetFloat(saveKeyPrefix + AnimalSpeedKey, initialAnimalSpeed));
                strengthToolTier = Mathf.Max(1, PlayerPrefs.GetInt(saveKeyPrefix + StrengthToolTierKey, initialStrengthToolTier));
                speedUpgradeLevel = Mathf.Max(0, PlayerPrefs.GetInt(saveKeyPrefix + SpeedUpgradeLevelKey, initialSpeedUpgradeLevel));
                selectedStrengthToolTier = Mathf.Clamp(
                    PlayerPrefs.GetInt(saveKeyPrefix + SelectedStrengthToolTierKey, strengthToolTier),
                    1,
                    strengthToolTier);
            }
            else
            {
                strength = Mathf.Max(0f, initialStrength);
                animalSpeed = Mathf.Max(0f, initialAnimalSpeed);
                strengthToolTier = Mathf.Max(1, initialStrengthToolTier);
                selectedStrengthToolTier = strengthToolTier;
                speedUpgradeLevel = Mathf.Max(0, initialSpeedUpgradeLevel);
            }

            initialized = true;
            Changed?.Invoke();
        }

        private void CommitChange()
        {
            initialized = true;
            SaveProgression();
            Changed?.Invoke();
        }

        private void SaveProgression()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            PlayerPrefs.SetFloat(saveKeyPrefix + StrengthKey, strength);
            PlayerPrefs.SetFloat(saveKeyPrefix + AnimalSpeedKey, animalSpeed);
            PlayerPrefs.SetInt(saveKeyPrefix + StrengthToolTierKey, strengthToolTier);
            PlayerPrefs.SetInt(saveKeyPrefix + SelectedStrengthToolTierKey, SelectedStrengthToolTier);
            PlayerPrefs.SetInt(saveKeyPrefix + SpeedUpgradeLevelKey, speedUpgradeLevel);
        }
    }
}
