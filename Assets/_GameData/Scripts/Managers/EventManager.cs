using System;
using UnityEngine;

// Tamamen statik bir sınıf, MonoBehaviour'dan kalıtım almıyor.
public static class EventManager
{
    // OYUN AKIŞI OLAYLARI
    // Bir seviye başladığında, hangi seviye verisiyle ve nerede başlayacağını bildirir.
    public static event Action<LevelDataSo, Transform> OnLevelStart;
    public static event Action OnLevelWon;
    public static event Action OnPlayerDied;

    // BUTONLARDAN GELEN İSTEKLER İÇİN OLAYLAR
    public static event Action OnRestartLevelRequest;
    public static event Action OnGoToMainMenuRequest;

    // OYUNCU EYLEM OLAYLARI
    public static event Action OnPlanetReached;

    // UI GÜNCELLEME OLAYLARI
    public static event Action<int, int> OnScoreUpdated;
    public static event Action<int> OnLevelDisplayUpdated;

    // --- TETİKLEME METOTLARI ---

    public static void TriggerLevelStart(LevelDataSo levelData, Transform startPoint) => OnLevelStart?.Invoke(levelData, startPoint);
    public static void TriggerLevelWon() => OnLevelWon?.Invoke();
    public static void TriggerPlayerDied() => OnPlayerDied?.Invoke();
    public static void TriggerRestartLevelRequest() => OnRestartLevelRequest?.Invoke();
    public static void TriggerGoToMainMenuRequest() => OnGoToMainMenuRequest?.Invoke();
    public static void TriggerPlanetReached() => OnPlanetReached?.Invoke();
    public static void TriggerScoreUpdated(int newScore, int targetScore) => OnScoreUpdated?.Invoke(newScore, targetScore);
    public static void TriggerLevelDisplayUpdated(int levelNumber) => OnLevelDisplayUpdated?.Invoke(levelNumber);
}