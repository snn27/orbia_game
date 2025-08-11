using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // --- CANVAS & PANEL REFERANSLARI ---
    [Header("Panels & Canvases")]
    [SerializeField] private GameObject scoreCanvas;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;

    // --- TEXT REFERANSLARI ---
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI winText;
    
    // --- BUTON REFERANSLARI ---
    [Header("Buttons")]
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button repeatButton;
    [SerializeField] private Button baseButton_Pause;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button baseButton_Win;

    private void Awake()
    {
        // Butonların olay dinleyicilerini (listeners) kod üzerinden atamak en güvenli yoldur.
        // Bu, Inspector'daki olası hataları ve unutkanlıkları engeller.
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(OnOpenPausePanelPressed);
        if (playButton != null) playButton.onClick.AddListener(OnContinuePressed);
        if (repeatButton != null) repeatButton.onClick.AddListener(OnRestartPressed);
        if (baseButton_Pause != null) baseButton_Pause.onClick.AddListener(OnMainMenuPressed);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelPressed);
        if (baseButton_Win != null) baseButton_Win.onClick.AddListener(OnMainMenuPressed);
    }
    
    // --- OLAY YÖNETİMİ ---
    private void OnEnable()
    {
        // GameManager'dan gelen bilgilendirme olaylarını dinle
        EventManager.OnScoreUpdated += HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated += HandleLevelDisplayUpdated;
        EventManager.OnLevelWon += HandleLevelWon;
        
        // Seviye başladığında panelleri sıfırla
        EventManager.OnLevelStart += HandleLevelStart;
    }

    private void OnDisable()
    {
        // Obje yok edildiğinde veya pasif olduğunda dinlemeyi bırakmayı ASLA unutma!
        EventManager.OnScoreUpdated -= HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated -= HandleLevelDisplayUpdated;
        EventManager.OnLevelWon -= HandleLevelWon;
        EventManager.OnLevelStart -= HandleLevelStart;
    }

    // --- OLAY İŞLEYİCİLERİ (EVENT HANDLERS) ---
    private void HandleLevelStart(LevelDataSo levelData, Transform startPoint)
    {
        // Seviye başladığında UIManager'ın yapması gereken tek şey, arayüzü varsayılan haline getirmektir.
        ResetPanelsToDefault();
    }
    
    private void HandleScoreUpdated(int newScore, int targetScore)
    {
        if(scoreText != null) scoreText.text = "Score: " + newScore.ToString();
        if(nextLevelText != null) 
        {
            int remaining = Mathf.Max(0, targetScore - newScore);
            nextLevelText.text = "For Next Level: " + remaining.ToString();
        }
    }
    
    private void HandleLevelDisplayUpdated(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = "LEVEL " + levelNumber;
        }
    }
    
    private void HandleLevelWon() 
    {
        if(scoreCanvas != null) scoreCanvas.SetActive(false);
        if(winPanel != null) winPanel.SetActive(true);
        if(winText != null) winText.text = "LEVEL COMPLETE!";
    }
    
    // --- BUTONLARA BAĞLI METOTLAR ---
    // Bu metotların görevi, ya arayüzü anında değiştirmek ya da GameManager'ın dinlediği bir isteği göndermektir.
    
    private void OnOpenPausePanelPressed()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if(pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }
    
    private void OnContinuePressed()
    {
        if(pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    private void OnRestartPressed()
    {
        Time.timeScale = 1f;
        // GameManager'a "Yeniden Başlatma İsteği" gönderiyoruz.
        EventManager.TriggerRestartLevelRequest();
    }
    
    private void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        // GameManager'a "Ana Menüye Dönme İsteği" gönderiyoruz.
        EventManager.TriggerGoToMainMenuRequest();
    }
    
    public void OnNextLevelPressed()
    {
        Time.timeScale = 1f;
        // Bu doğrudan GameManager'ı çağırabilir, çünkü oyun akışıyla ilgili çok spesifik bir komuttur
        // ve sadece GameManager'ın nasıl yapılacağını bildiği bir iştir.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNextLevel();
        }
    }

    // --- YARDIMCI METOT ---
    private void ResetPanelsToDefault()
    {
        if(scoreCanvas != null) scoreCanvas.SetActive(true);
        if(pausePanel != null) pausePanel.SetActive(false);
        if(winPanel != null) winPanel.SetActive(false);
    }
}