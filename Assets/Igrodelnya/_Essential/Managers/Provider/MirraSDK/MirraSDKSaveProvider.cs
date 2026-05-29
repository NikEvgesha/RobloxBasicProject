using MirraGames.SDK;
using System;  // доступ к MirraSDK.Data
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListSaver
{
    public List<string> list = new();
}

public class MirraSDKSaveProvider : SaveProvider
{
    public override bool IsInitialized => isInitialize;
    private bool isInitialize;
    public override void Initialize()
    {
        // Дождёмся полной готовности системы сохранений
        MirraSDK.WaitForProviders(() =>
        {
            //Debug.Log("MirraSDKSaveProvider initialized");
            isInitialize = true;
        });
    }

    public override float[] LoadVolume()
    {
        if (!isInitialize)
            return new float[] { 0.5f, 0.5f };
        // Достаём значения, с дефолтом 0.5f
        float music = MirraSDK.Data.GetFloat(SaveKey.MusicVolume.ToString(), 0.5f);
        float sound = MirraSDK.Data.GetFloat(SaveKey.SoundVolume.ToString(), 0.5f);
        return new float[] { music, sound };
    }

    public override void SaveVolume(float musicVolume, float soundVolume)
    {
        if (!isInitialize) return;
        MirraSDK.Data.SetFloat(SaveKey.MusicVolume.ToString(), musicVolume);
        MirraSDK.Data.SetFloat(SaveKey.SoundVolume.ToString(), soundVolume);
        Changed = true;
    }


    public override void SaveSensivity(float sens)
    {
        if (!isInitialize) return;
        MirraSDK.Data.SetFloat(SaveKey.Sensivity.ToString(), sens);
        Changed = true;
    }

    public override float LoadSensivity()
    {
        if (!isInitialize)
            return 0.5f;
        float sens = MirraSDK.Data.GetFloat(SaveKey.Sensivity.ToString(), 0.5f);
        return sens;
    }

    public override void SaveGems(double amount)
    {
        if (!isInitialize) return;
        Changed = true;
        MirraSDK.Data.SetString(SaveKey.Gems.ToString(), amount.ToString());
    }

    public override double LoadGems()
    {
        double res = 0;

        if (isInitialize)
        {
            string resStr = MirraSDK.Data.GetString(SaveKey.Gems.ToString(), "0");
            res = Double.Parse(resStr);
        }
        return res;
    }

    public override void SaveProgress()
    {
        if (!isInitialize) return;
        // Синхронизировать все изменения с провайдером (локальным или облачным)
        if (Changed)
        {
            MirraSDK.Data.Save();
            Changed = false;
        }
    }

    public override bool CheckProgress()
    {
        if (!isInitialize) return false;
        // Есть ли хоть что-то из основных ключей?
        return MirraSDK.Data.GetBool(SaveKey.Save.ToString(), false);
    }

    public override void SaveGameCoin(double coin)
    {
        if (!isInitialize) return;
        Changed = true;
        MirraSDK.Data.SetString(SaveKey.Coins.ToString(), coin.ToString());
    }

    public override double LoadGameCoin()
    {
        double res = -1;
        if (isInitialize)
        {
            string coinsStr = MirraSDK.Data.GetString(SaveKey.Coins.ToString(), "-1");
            Double.TryParse(coinsStr, out res);
        }
        return res;
    }

    public override void SaveRouletteDate(DateTime date)
    {
        if (!isInitialize) return;
        Changed = true;
        MirraSDK.Data.SetString(SaveKey.RouletteLastDate.ToString(), date.Date.ToString());
        Debug.Log("Date saved: " + date.ToString());
    }

    public override DateTime LoadRouletteDate()
    {
        if (!isInitialize) return DateTime.Today.AddDays(-1);

        string date = MirraSDK.Data.GetString(SaveKey.RouletteLastDate.ToString());
        //Debug.Log("Date loaded: " + date);
        if (date.Length == 0)
        {
            return DateTime.Today.AddDays(-1);
        }
        return DateTime.Parse(date);
    }

    public override void SetSave(bool save)
    {
        if (!isInitialize) return;
        Changed = true;
        MirraSDK.Data.SetBool(SaveKey.Save.ToString(), save);
    }

    public override void SaveTutorialProgress(bool endTutorial)
    {
        if (!isInitialize) return;
        MirraSDK.Data.SetBool(SaveKey.EndTutorial.ToString(), endTutorial);
        Changed = true;
    }

    public override bool GetTutorialProgress()
    {
        if (!isInitialize) return false;
        return MirraSDK.Data.GetBool(SaveKey.EndTutorial.ToString(), false);
    }

    public override void SaveQuestProgress(int step)
    {
        if (!isInitialize) return;
        MirraSDK.Data.SetInt(SaveKey.QuestProgress.ToString(), step);
        Changed = true;
    }
    public override int LoadQuestProgress()
    {
        if (!isInitialize) return 0;
        return MirraSDK.Data.GetInt(SaveKey.QuestProgress.ToString(), 0);
    }
}
