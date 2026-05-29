using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveProvider : MonoBehaviour
{
    public bool Changed;
    public abstract bool IsInitialized { get; }
    public abstract void Initialize();

    // Методы для работы с громкостью
    public abstract float[] LoadVolume();
    public abstract void SaveVolume(float musicVolume, float soundVolume);

    public abstract void SaveSensivity(float sens);

    public abstract float LoadSensivity();

    // Прогресс квестов
    public abstract bool GetTutorialProgress();
    public abstract void SaveTutorialProgress(bool endTutorial);
    public abstract void SaveQuestProgress(int step);
    public abstract int LoadQuestProgress();


    // Общий метод сохранения прогресса
    public abstract void SaveProgress();
    public abstract bool CheckProgress();

    // Сохранение валюты
    public abstract void SaveGems(double amount);
    public abstract double LoadGems();
    public abstract void SaveGameCoin(double coin);
    public abstract double LoadGameCoin();

    // инвентарь
    //public abstract void SaveInventory(List<ItemData> items);
    //public abstract List<string> LoadInventory();


    // остальное
    public abstract void SetSave(bool save);

    public abstract void SaveRouletteDate(DateTime date);

    public abstract DateTime LoadRouletteDate();

}
