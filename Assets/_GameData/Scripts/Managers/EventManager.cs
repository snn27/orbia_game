using System;
using UnityEngine;

// MonoBehaviour'dan kalıtım ALMIYOR. Tamamen statik bir sınıf.
public static class EventManager
{
    // --- OYUN AKIŞI OLAYLARI ---
    public static event Action<LevelDataSo, Transform> OnLevelStart;
    public static event Action OnLevelWon;
    public static event Action OnPlayerDied;
    public static event Action OnRestartLevel;
    public static event Action OnGoToMainMenu;

    // --- OYUNCU EYLEM OLAYLARI ---
    public static event Action OnPlanetReached;

    // --- UI GÜNCELLEME OLAYLARI ---
    public static event Action<int, int> OnScoreUpdated;
    public static event Action<int> OnLevelDisplayUpdated;


    // --- BU METOTLAR DJ'LERİN ANONSLARI GEÇECEĞİ YERLERDİR ---

    public static void TriggerLevelStart(LevelDataSo levelData, Transform startPoint)
    {
        OnLevelStart?.Invoke(levelData, startPoint);
    }
    public static void TriggerLevelWon() => OnLevelWon?.Invoke();
    public static void TriggerPlayerDied() => OnPlayerDied?.Invoke();
    public static void TriggerRestartLevel() => OnRestartLevel?.Invoke();
    public static void TriggerGoToMainMenu() => OnGoToMainMenu?.Invoke();

    public static void TriggerPlanetReached() => OnPlanetReached?.Invoke();

    public static void TriggerScoreUpdated(int newScore, int targetScore) => OnScoreUpdated?.Invoke(newScore, targetScore);
    public static void TriggerLevelDisplayUpdated(int levelNumber) => OnLevelDisplayUpdated?.Invoke(levelNumber);
    
    // <<< EN ÖNEMLİ METOT: TEHLİKEYİ ÖNLEMEK İÇİN >>>
    // Sahne temizlenirken veya yeni bir seviye başlarken çağrılacak.
    // Tüm olaylardaki abone listelerini sıfırlar.
    public static void ClearAllEvents()
    {
        OnLevelStart = null;
        OnLevelWon = null;
        OnPlayerDied = null;
        OnRestartLevel = null;
        OnGoToMainMenu = null;
        OnPlanetReached = null;
        OnScoreUpdated = null;
        OnLevelDisplayUpdated = null;
        Debug.Log("<color=orange>EventManager: Tüm olaylar temizlendi.</color>");
    }
}