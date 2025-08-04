using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, Paused, LevelComplete }
    public GameState CurrentState { get; private set; }
    
    [Header("Level Settings")]
    public LevelData[] allLevels;
    public int currentLevelIndex = 0;
    
    [Header("Game State Variables")]
    private int currentScore = 0;
    private int targetScore;
    private bool isHardModeActive = false; // Artik bu degiskenin de bir anlami kalmadi

    [Header("Manager References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private StartPlanet initialStartPlanet;

    // GameManager'ın tekil (singleton) örneğini yönetir. Birden fazla kopyayı engeller.
    private void Awake() {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        currentLevelIndex = 0;
        StartNewGameSession();
    }
    
    // Oyun ilk açıldığında veya Ana Menü'den başlandığında çağrılır.
    private void StartNewGameSession() {
        // Yeni bir oyun seansı başladığında durumu ve zamanı ayarla.
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        StartLevel(allLevels[currentLevelIndex]);
    }
    
    // Her yeni seviye kurulduğunda bu ana metot çalışır, başlangıçta currentLevelIndex o oldugundan 1 level gelir..
    private void StartLevel(LevelData levelData)
    {
        CurrentState = GameState.Playing;
        uiManager.ResetPanelsToDefault(); //paneller olması gerketigi şekilde düzenlenir

        // 2. Oyuncuyu kur(leveli başlat)
        playerController.InitializeLevel(levelData, initialStartPlanet.transform);

        // 3. İlk hedefi oluştur ve oyuncuya bildir
        float randomAngle = Random.Range(-45f, 45f);
        float randomDistance = Random.Range(14f, 18f);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)initialStartPlanet.transform.position + (direction * randomDistance);
        GameObject firstTargetObject = Instantiate(levelData.planetPrefab, spawnPosition, Quaternion.identity);
        playerController.SetNextTarget(firstTargetObject.transform);

        // 4. Bu seviyenin kurallarını ve UI'yi ayarla
        targetScore = levelData.hedefeUlasmaSayisi;
        
        // Hard Mode kontrolü seviye başında burada yapılır.
        if (currentLevelIndex >= 2) // 3. seviye (index 2) ve sonrası Hard Mode'dur.
        {
            targetScore = 15; // Zorluk ayarını direkt buradan yap.
        }

        currentScore = 0;
        uiManager.UpdateLevelDisplay(currentLevelIndex + 1);
        uiManager.UpdateScoreDisplay(currentScore, targetScore);
    }
    
    //--- Oyun Akışını Yöneten Fonksiyonlar ---

    public void PlanetReached()
    {
        if (CurrentState != GameState.Playing) return;

        currentScore++;
        uiManager.UpdateScoreDisplay(currentScore, targetScore);

        if (currentScore >= targetScore)
        {
            CurrentState = GameState.LevelComplete;
            Time.timeScale = 0f;
            uiManager.ShowWinPanel();
        }
    }
    
    public void PlayerDied()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.LevelComplete; // Durumu degistirerek tekrar olmesini engelle
        Invoke("RestartCurrentLevel", 0.1f); 
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
        if (currentLevelIndex >= allLevels.Length)
        {
            Debug.Log("TEBRIKLER! Tum seviyeleri bitirdin! Başa dönülüyor.");
            currentLevelIndex = 0;
        }
        ClearPreviousLevel();
        StartLevel(allLevels[currentLevelIndex]);
    }
    
    public void RestartCurrentLevel()
    {
        // Eger oyun zaten oynanir durumdaysa (ornegin PlayerDied'den Invoke beklenirken oyuncu restart tusuna basti), Invoke'u iptal et.
        if (IsInvoking("RestartCurrentLevel"))
        {
            CancelInvoke("RestartCurrentLevel");
        }

        Time.timeScale = 1f;
        ClearPreviousLevel();
        StartLevel(allLevels[currentLevelIndex]);
    }

    public void ReturnToMainMenu()
    {
        currentLevelIndex = 0;
        RestartCurrentLevel();
    }
    
    //--- Duraklatma Yönetimi ---
    
    public void PauseGame() 
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        uiManager.ShowPausePanel(); 
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        uiManager.HidePausePanel();
    }
    
    //--- Yardımcı Fonksiyon ---
    private void ClearPreviousLevel()
    {
        playerController.StopAllRunningCoroutines();// sahnedeki tüm objeleri yok etmeden çalışan coroutiens'leri durdur.
        
        LineRenderer[] lines = FindObjectsOfType<LineRenderer>();
        foreach(var line in lines) Destroy(line.gameObject);
        
        GameObject[] oldPlanets = GameObject.FindGameObjectsWithTag("GeneratedPlanet");
        foreach(GameObject planet in oldPlanets) Destroy(planet);
        
        GameObject[] oldEnemySets = GameObject.FindGameObjectsWithTag("GeneratedEnemySet");
        foreach(GameObject enemySet in oldEnemySets)
        {
            DOTween.Kill(enemySet.transform, true);
            Destroy(enemySet);
        }

        playerController.ResetPlayer(initialStartPlanet.transform);
    }
}