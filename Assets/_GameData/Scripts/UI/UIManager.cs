using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // [Header("1. OYUN İÇİ ARAYÜZ")]
    [SerializeField] private GameObject scoreCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private TextMeshProUGUI levelText; // Önceden "kazanma" panelindeydi, buraya daha uygun

    // [Header("2. DURAKLATMA PANELİ")]
    [SerializeField] private GameObject pausePanel; // Hiyerarşideki 'MenuCanvas' olabilir.

    // [Header("3. KAZANMA PANELİ")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    
    // --- BUTON REFERANSLARI (İsimler hiyerarşiye uygun hale getirildi) ---
    [Header("Buttons")]
    [SerializeField] private Button pauseMenuButton;    // Sahnedeki üç çizgili buton
    [SerializeField] private Button playButton;         // Pause Panel -> PlayButton
    [SerializeField] private Button repeatButton;       // Pause Panel -> RepeatButton
    [SerializeField] private Button baseButton_Pause;   // Pause Panel -> BaseButton
    
    [SerializeField] private Button nextLevelButton;    // Win Panel -> NextLevelButton
    [SerializeField] private Button baseButton_Win;     // Win Panel -> BaseButton

    private void Awake()
    {
        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(OnOpenPausePanelPressed);

        // PAUSE PANELİ BUTONLARI
        if (playButton != null)
            playButton.onClick.AddListener(OnContinuePressed);

        if (repeatButton != null)
            repeatButton.onClick.AddListener(OnRestartPressed);
            
        if (baseButton_Pause != null)
            baseButton_Pause.onClick.AddListener(OnMainMenuPressed);
            
        // WIN PANELİ BUTONLARI
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelPressed);
            
        if (baseButton_Win != null)
            baseButton_Win.onClick.AddListener(OnMainMenuPressed); // Bu da Ana Menü'ye döner
    }
    private void OnEnable()
    {
        EventManager.OnScoreUpdated += HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated += HandleLevelDisplayUpdated;
        EventManager.OnLevelWon += HandleLevelWon;
    
        // Ana menüye dönüldüğünde veya seviye yeniden başladığında panelleri resetlemek için.
        EventManager.OnGoToMainMenu += ResetPanelsToDefault;
        EventManager.OnRestartLevel += ResetPanelsToDefault_Event; // Yeni bir handler ekliyoruz
    }

    private void OnDisable()
    {
        EventManager.OnScoreUpdated -= HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated -= HandleLevelDisplayUpdated;
        EventManager.OnLevelWon -= HandleLevelWon;
        EventManager.OnGoToMainMenu -= ResetPanelsToDefault;
        EventManager.OnRestartLevel -= ResetPanelsToDefault_Event;
    }

    private void Start()
    {
        ResetPanelsToDefault();
    }
    
    // --- OLAY İŞLEYİCİLERİ (EVENT HANDLERS) ---
    // GameManager "Skor Güncellendi" dediğinde burası otomatik çalışır.
    private void HandleScoreUpdated(int newScore, int targetScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
        int remaining = Mathf.Max(0, targetScore - newScore);
        nextLevelText.text = "For Next Level: " + remaining.ToString();
    }
    
    // GameManager "Seviye Yazısı Güncellendi" dediğinde burası otomatik çalışır.
    private void HandleLevelDisplayUpdated(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = "LEVEL " + levelNumber;
        }
    }
    
    // GameManager "Seviye Kazanıldı" dediğinde burası otomatik çalışır.
    private void HandleLevelWon() {
        scoreCanvas.SetActive(false);
        winPanel.SetActive(true);
        if(winText != null) winText.text = "LEVEL COMPLETE!";
    }
    
    public void OnOpenPausePanelPressed()
    {
        // Bu panel yönetimi artık UIManager'ın kendi sorumluluğunda.
        // GameManager'a sormasına gerek yok.
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Oyunun zamanını durdurmak önemli.
    }
    
    public void OnContinuePressed()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Zamanı tekrar başlat.
    }
    
    public void OnRestartPressed()
    {
        // Bu buton bir anons yapar. GameManager bu anonsu duyup seviyeyi yeniden başlatır.
        Time.timeScale = 1f; // Zamanı başlatmayı unutma!
        EventManager.TriggerRestartLevel();
    }
    
    public void OnMainMenuPressed()
    {
        // Bu buton da bir anons yapar.
        Time.timeScale = 1f;
        EventManager.TriggerGoToMainMenu();
    }
    
    public void OnNextLevelPressed()
    {
        ResetPanelsToDefault();

        // <<< 2. ADIM: SONRA GÖREVİ GAMEMANAGER'A DEVRET >>>
        // Zamanı tekrar başlat.
        Time.timeScale = 1f;

        // GameManager'a yeni seviyeyi başlatması için sinyal gönder.
        if(GameManager.Instance != null)
        {
            GameManager.Instance.StartNextLevel();
        }
    }
    private void ResetPanelsToDefault_Event()
    {
        // Bu metot, sadece event tarafından tetiklendiğinde çalışır
        ResetPanelsToDefault();
    }
    private void ResetPanelsToDefault()
    {
        scoreCanvas.SetActive(true);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        Debug.Log("<color=yellow>UIManager:</color> Paneller resetlendi.");
    }
}