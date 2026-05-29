using MirraGames.SDK;
using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public Action<float> ChangeMouseSensitivity;
    public Action<float> ChangeVolume;

    public bool IsReady;
    public Action Ready;

    public float Sensivity;
    private void Awake()
    {

        if (G.Settings == null)
        {
            G.Settings = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        
        MirraSDK.WaitForProviders(static () => {
            G.Settings.OnGameStart();
            // Методы SDK не должны вызывать вылет или NullReferenceException,
            // делегат будет вызван только когда все провайдеры имеют статус IsInitialized.
        });
    }

    private void OnGameStart()
    {
        Sensitivity(G.Save.LoadSensivity());
        float[] volumes = G.Save.LoadVolume();
        MusicVolume(volumes[0]);
        SoundVolume(volumes[1]);
        IsReady = true;
        Ready?.Invoke();
    }
   
    public void Sensitivity(float sens)
    {
        Sensivity = sens;
        ChangeMouseSensitivity?.Invoke(sens);
        G.Save.SaveSensivity(sens);
    }

    public void SoundVolume(float volume)
    {
        G.Sound.SoundVolume = volume;
    }
    public void MusicVolume(float volume)
    {
        G.Sound.MusicVolume = volume;
    }

}
