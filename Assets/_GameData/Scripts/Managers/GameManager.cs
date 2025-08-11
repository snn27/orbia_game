using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, Paused, LevelComplete }
    public GameState CurrentState { get; private set; } // diger scripler değiştiremez
    
    [FormerlySerializedAs("levelsData")] [Header("Level Settings")]
    public LevelsDataSo levelsDataSo;
    public int currentLevelIndex = 0;
    
    [Header("Game State Variables")]
    private int currentScore = 0;
    private int targetScore;
    
    [SerializeField] private Transform initialStartTransform; 
    // GameManager'ın tekil (singleton) örneğini yönetir. Birden fazla kopyayı engeller.
    private void Awake() {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    
    private void OnEnable()
    {
        EventManager.OnPlanetReached += HandlePlanetReached;
        EventManager.OnPlayerDied += HandlePlayerDied;
        EventManager.OnRestartLevel += RestartCurrentLevel;
        EventManager.OnGoToMainMenu += ReturnToMainMenu;
        // Kendi iç yönetimi için NextLevel'ı da buradan tetikleyebilir
    }

    private void OnDisable()
    {
        EventManager.OnPlanetReached -= HandlePlanetReached;
        EventManager.OnPlayerDied -= HandlePlayerDied;
        EventManager.OnRestartLevel -= RestartCurrentLevel;
        EventManager.OnGoToMainMenu -= ReturnToMainMenu;
    }
    
    private void Start()
    {
        StartNewGameSession();
    }
    
    private void StartNewGameSession() {
        CurrentState = GameState.Playing;
        StartLevel(levelsDataSo.levels[currentLevelIndex]);
    }
    
    // Her yeni seviye kurulduğunda bu ana metot çalışır, başlangıçta currentLevelIndex o oldugundan 1 level gelir..
    private void StartLevel(LevelDataSo levelDataSo)
    {
        if (levelDataSo == null)
        {
            Debug.LogError("[GameManager] HATA: StartLevel metoduna gelen 'levelData' NULL! Inspector'da 'All Levels' dizisini kontrol edin!");
            return; // Fonksiyonun devam etmesini engelle
        }
        Debug.Log("[GameManager] StartLevel çağrıldı. Gelen LevelData: " + levelDataSo.name);
        
        ClearPreviousLevel();
        
        LevelDataSo levelData = levelsDataSo.levels[currentLevelIndex];
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        targetScore = levelData.hedefeUlasmaSayisi;
        currentScore = 0;
        
        // 3. ADIM: ANONSU GEÇ
        // PlayerController, UIManager gibi mevcut aboneler bu anonsu duyacak ve kurulumlarını yapacak.
        EventManager.TriggerLevelStart(levelsDataSo.levels[currentLevelIndex], initialStartTransform.transform);
        EventManager.TriggerScoreUpdated(currentScore, targetScore);
        EventManager.TriggerLevelDisplayUpdated(currentLevelIndex + 1);
    }
    
    private void HandlePlanetReached()
    {
        if (CurrentState != GameState.Playing) return;
        currentScore++;
        EventManager.TriggerScoreUpdated(currentScore, targetScore);

        if (currentScore >= targetScore)
        {
            CurrentState = GameState.LevelComplete;
            Time.timeScale = 0f;
            EventManager.TriggerLevelWon();
        }
    }
    
    private void HandlePlayerDied()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.LevelComplete; 
        Invoke(nameof(RestartCurrentLevel), 0.2f); 
    }
    
    public void StartNextLevel()
    {
        //state koruması yapılır 
        if (CurrentState != GameState.LevelComplete) return;

        // Kilit: level  basladigi an durumu degistirerek ikinci bir cagriyi engeller.
        CurrentState = GameState.Playing; 
        
        Debug.Log($"<color=green>[GameManager]</color> StartNextLevel ÇAĞRILDI! Yeni seviyeye geçiliyor...");
        
        Time.timeScale = 1f;
        currentLevelIndex++;
        if (currentLevelIndex >= levelsDataSo.levels.Count)
        {
            Debug.Log("TEBRIKLER! Tum seviyeleri bitirdin! Başa dönülüyor.");
            currentLevelIndex = 0;
        }
        EventManager.ClearAllEvents();

        StartLevel(levelsDataSo.levels[currentLevelIndex]);
    }
    
    public void RestartCurrentLevel()
    {
        StartLevel(levelsDataSo.levels[currentLevelIndex]);
    }

    public void ReturnToMainMenu()
    {
        EventManager.ClearAllEvents();
        currentLevelIndex=0;
        StartNewGameSession();
    }
    
    public void PauseGame() 
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }
    
    //--- Yardımcı Fonksiyon ---
    private void ClearPreviousLevel()
    {
        DOTween.KillAll();
        CancelInvoke();
        
        // Sonra olay listelerini temizle ki eski objeler yeni anonsları duymasın.
        EventManager.ClearAllEvents();

        // Sahnedeki tüm dinamik objeleri yok et.
        GameObject[] allPlanets = GameObject.FindGameObjectsWithTag("GeneratedPlanet");
        foreach (GameObject planet in allPlanets) Destroy(planet);
        
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("GeneratedEnemySet");
        foreach (GameObject enemy in allEnemies) Destroy(enemy);

        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer line in allLines) Destroy(line.gameObject);

        // UIManager'a kendini resetlemesi için anons geç.
        EventManager.TriggerGoToMainMenu(); // ResetPanelsToDefault'u tetikler.

        Debug.Log("<color=red>Tüm sahne ve eventler yeni seviye için temizlendi.</color>");
    }
}