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
        // Adım 1: Önceki seviyeden kalan tüm dinamik objeleri temizle
        ClearSceneObjects();
    
        // Adım 2: Yeni seviye için gerekli verileri ve durumu ayarla
        currentLevelIndex = levelIndex;
        LevelDataSo levelData = levelsDataSo.levels[currentLevelIndex];

        // Güvenlik kontrolleri
        if (levelData == null) {
            Debug.LogError($"HATA: LevelsDataSo içinde {levelIndex}. eleman (seviye) için veri bulunamadı!");
            return;
        }
        if (playerController == null) {
            Debug.LogError("GameManager'da PlayerController referansı atanmamış!");
            return;
        }
    
        // Adım 3: Oyun durumunu ve temel değişkenleri ayarla
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        targetScore = levelData.hedefeUlasmaSayisi;
        currentScore = 0;
    
        Debug.Log($"<color=green>--- SEVİYE {levelIndex + 1} BAŞLATILIYOR: {levelData.name} ---</color>");

        // Adım 4: DOĞRUDAN KOMUTLARLA OYUNCUYU KURMA SÜRECİ
    
        // 4a: Oyuncuyu başlangıç noktasına resetle ve hayata döndür.
        playerController.ResetPlayer(initialStartTransform);

        // 4b: Kameranın boşluğa düşmemesi için ona anında geçici bir hedef ver.
        // Bu, oyuncu ve hedef arasındaki ani boşluğu doldurur.
        playerController.SetTemporaryTarget(initialStartTransform);

        // 4c: Seviye verilerini yüklemesini ve asıl ilk hedefi oluşturmasını söyle.
        // Bu metodun içindeki SpawnNewPlanetAndEnemies, SetNextTarget'ı çağırarak 4b'deki
        // geçici hedefi anında doğru olanla günceller.
        playerController.SetupLevel(levelData);
        playerController.CreateFirstPlanetAndEnemies(initialStartTransform);
    
        // Adım 5: Gerekli UI güncellemeleri için genel anonsları geç.
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
    
    public void AddBonusScore(int amount)
    {
        // Oyunun oynanır durumda olduğundan emin ol.
        if (CurrentState != GameState.Playing) return;

        Debug.Log($"<color=yellow>BONUS:</color> {amount} puan eklendi!");
        
        
        // currentScore += amount;
        
        // Puan değiştiği için, UI'ın güncellenmesi gerektiğini herkese bildir.
        // EventManager.TriggerScoreUpdated(currentScore, targetScore);
    }
    
    // --- TEMİZLİK FONKSİYONU ---
    private void ClearSceneObjects()
    {
        DOTween.KillAll();
        CancelInvoke();
    
        GameObject[] allPlanets = GameObject.FindGameObjectsWithTag("GeneratedPlanet");
        foreach (GameObject planet in allPlanets) Destroy(planet);
    
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("GeneratedEnemySet");
        foreach (GameObject enemy in allEnemies) Destroy(enemy);

        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer line in allLines) Destroy(line.gameObject);

        Debug.Log("<color=red>Dinamik sahne objeleri temizlendi.</color>");
    }
}