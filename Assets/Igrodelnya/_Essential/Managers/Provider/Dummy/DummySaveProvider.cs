using MirraGames.SDK;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DummySaveProvider : SaveProvider
{
    public override bool IsInitialized => true;
    public override void Initialize() { Debug.Log("DummySaveProvider initialized"); }
    public override float[] LoadVolume() {
        float[] volumes = new float[] { 0.5f, 0.5f };
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            volumes[0] = PlayerPrefs.GetFloat("MusicVolume");
        }
        if (PlayerPrefs.HasKey("SoundVolume"))
        {
            volumes[1] = PlayerPrefs.GetFloat("SoundVolume");
        }
        return volumes;
    }
    public override void SaveGems(double amount) {
        PlayerPrefs.SetFloat("Gems", (float)amount);
    }

    public override double LoadGems()
    {
        float gems = 0;
        if (PlayerPrefs.HasKey("Gems"))
        {
            gems = PlayerPrefs.GetFloat("Gems");
        }
        return gems;
    }
    public override void SaveVolume(float musicVolume, float soundVolume)
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
    }

    public override void SaveSensivity(float sens) {
        PlayerPrefs.SetFloat("Sensivity", sens);
    }

    public override float LoadSensivity()
    {
        float sens = 0.5f;
        if (PlayerPrefs.HasKey("Sensivity"))
        {
            sens = PlayerPrefs.GetFloat("Sensivity");
        }
        return sens;
    }
 
    public override void SaveProgress() { }

    public override bool CheckProgress() { return false; }

    public override void SaveGameCoin(double coin)
    {
        throw new NotImplementedException();
    }
    public override double LoadGameCoin()
    {
        throw new NotImplementedException();
    }
    
    public override void SaveRouletteDate(DateTime date)
    {
    }

    public override DateTime LoadRouletteDate()
    {
        return DateTime.Today.AddDays(-1);
    }

    public override void SetSave(bool save)
    {
    }

    public override void SaveTutorialProgress(bool endTutorial)
    {
    }

    public override bool GetTutorialProgress()
    {
        return false;
    }

    public override void SaveQuestProgress(int step)
    {
    }
    public override int LoadQuestProgress()
    {
        return 0;
    }

}
