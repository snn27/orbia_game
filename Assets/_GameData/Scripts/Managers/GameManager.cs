using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, Paused, LevelComplete }
    public GameState CurrentState { get; private set; }

    [Header("Level Settings")]
    public LevelsDataSo levelsDataSo;
    public int currentLevelIndex = 0;
    
    [Header("Scene References")]
    [Tooltip("Oyuncunun ve seviyelerin başlayacağı sabit başlangıç noktası.")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform initialStartTransform; 

    private int currentScore = 0;
    private int targetScore;

    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    
    private void OnEnable()
    {
        // Oyun içindeki olayları dinle
        EventManager.OnPlanetReached += HandlePlanetReached;
        EventManager.OnPlayerDied += HandlePlayerDied;
        
        // UI'dan gelen istekleri dinle
        EventManager.OnRestartLevelRequest += RestartCurrentLevel;
        EventManager.OnGoToMainMenuRequest += GoToMainMenu;
    }

    private void OnDisable()
    {
        EventManager.OnPlanetReached -= HandlePlanetReached;
        EventManager.OnPlayerDied -= HandlePlayerDied;
        EventManager.OnRestartLevelRequest -= RestartCurrentLevel;
        EventManager.OnGoToMainMenuRequest -= GoToMainMenu;
    }
    
    private void Start()
    {
        // Oyunu en baştan, temiz bir oturumla başlat.
        StartNewGameSession();
    }

    public void StartNewGameSession() {
        StartLevel(currentLevelIndex); 
    }
    
    public void StartNextLevel() {
        if (CurrentState != GameState.LevelComplete) return;
        int nextIndex = currentLevelIndex + 1;
        if (nextIndex >= levelsDataSo.levels.Count) {
            Debug.Log("TEBRIKLER! Tum seviyeleri bitirdin! Başa dönülüyor.");
            nextIndex = 0;
        }
        StartLevel(nextIndex);
    }

    private void RestartCurrentLevel() {
        StartLevel(currentLevelIndex); // O anki seviyeyi yeniden başlat.
    }
    
    private void GoToMainMenu() {
        StartNewGameSession(); // Ana menüye dönmek, yeni bir oyun başlatmaktır.
    }

    // --- EN ÖNEMLİ ANA FONKSİYON: SEVİYE KURULUMU ---
    private void StartLevel(int levelIndex)
    {
        // 1. ÖNCEKİ SEVİYEDEN KALAN TÜM DİNAMİK OBJELERİ TEMİZLE
        ClearSceneObjects();
        
        // 2. YENİ SEVİYE DEĞİŞKENLERİNİ AYARLA
        currentLevelIndex = levelIndex;
        LevelDataSo levelData = levelsDataSo.levels[currentLevelIndex];

        if (levelData == null) {
            Debug.LogError($"HATA: LevelsDataSo içinde {levelIndex}. eleman (seviye) için veri bulunamadı!");
            return;
        }
        
        if (playerController != null)
        {
            playerController.ResetPlayer(initialStartTransform);
        }
        else
        {
            Debug.LogError("GameManager'da PlayerController referansı atanmamış!");
            return;
        }
        
        playerController.SetupFirstTarget(levelData);

        // ADIM 3: OYUN DURUMUNU VE UI'ı GÜNCELLEMESİ İÇİN ANONS GEÇ
        EventManager.TriggerScoreUpdated(currentScore, targetScore);
        EventManager.TriggerLevelDisplayUpdated(currentLevelIndex + 1);
        
    }
    
    // --- OLAY İŞLEYİCİLERİ ---
    private void HandlePlanetReached() {
        if (CurrentState != GameState.Playing) return;
        currentScore++;
        EventManager.TriggerScoreUpdated(currentScore, targetScore);

        if (currentScore >= targetScore) {
            CurrentState = GameState.LevelComplete;
            Time.timeScale = 0f;
            EventManager.TriggerLevelWon();
        }
    }
    
    private void HandlePlayerDied() {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.LevelComplete;
        Invoke(nameof(RestartCurrentLevel), 0.4f); // 1.5 saniye sonra seviyeyi yeniden başlat
    }
    
    // --- TEMİZLİK FONKSİYONU ---
    private void ClearSceneObjects() {
        Debug.Log("<color=red>Sahne temizleniyor...</color>");
        
        DOTween.KillAll();
        CancelInvoke(); // Zamanlanmış tüm Invoke'ları iptal et (örneğin oyuncu ölür ölmez restart'a basarsa)

        // SADECE DİNAMİK OLARAK OLUŞTURULAN VE ETİKETLENEN OBJELERİ YOK ET
        GameObject[] allPlanets = GameObject.FindGameObjectsWithTag("GeneratedPlanet");
        foreach (GameObject planet in allPlanets) Destroy(planet);
        
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("GeneratedEnemySet");
        foreach (GameObject enemy in allEnemies) Destroy(enemy);

        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer line in allLines) Destroy(line.gameObject);

        // PlayerController kendini OnLevelStart'ta zaten resetleyecek.
        // UIManager da kendi panellerini OnLevelStart veya diğer eventlerde yönetecek.
    }
}