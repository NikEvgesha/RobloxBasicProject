using MirraGames.SDK;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private SaveProvider saveProvider; // Ќазначаем в инспекторе нужный провайдер (YG2SaveProvider, DebugSaveProvider и т.д.)
    [SerializeField] private bool _newPlayer;
    public bool IsNewPlayer => saveProvider.CheckProgress() == false;
    public bool IsReady => saveProvider != null && saveProvider.IsInitialized;

    private bool _pendingSaveFlagSet;
    private bool _pendingSaveFlagValue;
    private bool _hasCachedBackendProfile;
    private bool _pendingBackendProfilePersist;
    private string _cachedBackendPlayerId;
    private string _cachedBackendFriendCode;
    private string _cachedBackendDisplayName;

    private void Awake()
    {

        if (_newPlayer)
        {
            MirraSDK.Data.DeleteAll();
        }

        if (G.Save == null)
        {
            G.Save = this;
            DontDestroyOnLoad(gameObject);
            saveProvider.Initialize();
            StartCoroutine(ProgressSavingRoutine());
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private IEnumerator ProgressSavingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (saveProvider != null && saveProvider.IsInitialized)
            {
                if (_pendingSaveFlagSet)
                {
                    saveProvider.SetSave(_pendingSaveFlagValue);
                    _pendingSaveFlagSet = false;
                }

                //if (_pendingBackendProfilePersist && _hasCachedBackendProfile)
                //{
                //    saveProvider.SaveBackendProfile(_cachedBackendPlayerId, _cachedBackendFriendCode, _cachedBackendDisplayName);
                //    _pendingBackendProfilePersist = false;
                //}
            }

            saveProvider.SaveProgress();
        }
    }

    public void SetSave(bool haveSave)
    {
        if (saveProvider != null && saveProvider.IsInitialized)
        {
            saveProvider.SetSave(haveSave);
            _pendingSaveFlagSet = false;
            return;
        }

        _pendingSaveFlagSet = true;
        _pendingSaveFlagValue = haveSave;
    }

    // ѕример методов, которые делегируют работу провайдеру:
    public float[] GetVolume()
    {
        return saveProvider.LoadVolume();
    }
    public void SaveQuestProgress(int step = 0)
    {
        saveProvider.SaveQuestProgress(step);
    }
    public int LoadQuestProgress()
    {
        return saveProvider.LoadQuestProgress();
    }
    public void SaveMusicVolume(float volume)
    {
        var volumes = saveProvider.LoadVolume();
        saveProvider.SaveVolume(volume, volumes[1]);
    }

    public void SaveSoundVolume(float volume)
    {
        var volumes = saveProvider.LoadVolume();
        saveProvider.SaveVolume(volumes[0], volume);
    }


    public float[] LoadVolume()
    {
        return saveProvider.LoadVolume();
    }


    public void SaveSensivity(float sens)
    {
        saveProvider.SaveSensivity(sens);
    }

    public float LoadSensivity()
    {
        return saveProvider.LoadSensivity();
    }

    public bool GetTutorialProgress()
    {
        return saveProvider.GetTutorialProgress();
    }
    public void SaveTutorialProgress(bool endTutorial)
    {
        saveProvider.SaveTutorialProgress(endTutorial);
    }
    public void SaveGems(double amount)
    {
        saveProvider.SaveGems(amount);
        //LeaderboardManager.Instance.SaveScore(LBName.gems.ToString(), amount);
    }

    public double GetGems()
    {
        return saveProvider.LoadGems();
    }

    public void SaveGameCoin(double coin)
    {
        saveProvider.SaveGameCoin(coin);
    }
    public double LoadGameCoin()
    {
        return saveProvider.LoadGameCoin();
    }

    public void SaveRouletteDate(DateTime date)
    {
        saveProvider.SaveRouletteDate(date);
    }

    public DateTime LoadRouletteDate()
    {
        return saveProvider.LoadRouletteDate();
    }

    //public void SaveInventory(Item type, List<ItemSaveData> items)
    //{
    //    saveProvider.SaveItemsList(type, JsonConvert.SerializeObject(items));
    //}

    //public List<ItemSaveData> LoadInventory(Item type)
    //{
    //    return saveProvider.LoadItemsList(type);
    //}

}
