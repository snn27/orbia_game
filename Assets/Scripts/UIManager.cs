using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager InstanceUIManager;

    [Header("1. OYUN İÇİ ARAYÜZ (Sürekli Görünenler)")]
    //  Oyuncunun skorunu vb. bilgileri gösteren ve hep açık olan ana canvas.
    [SerializeField] private GameObject scoreCanvas;
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    
    [SerializeField] private Button menuPanelButton; //3 cizgili


    [Header("2. DURAKLATMA PANELİ (Pause Tuşuna Basınca Çıkan)")]
     public GameObject pausePanel;//3 cizginin canvası
    
    //  Duraklatma Panelindeki "Devam Et" butonu.
    [SerializeField] private Button playButton;
    
    //  Duraklatma Panelindeki "Yeniden Başlat" butonu.
    [SerializeField] private Button restartButton_Pause;
    
    //  Duraklatma Panelindeki "Ana Menü" butonu.
    [SerializeField] private Button mainMenuButton_Pause;


    [Header("3. KAZANMA PANELİ (Seviye Bitince Çıkan)")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton_Win;
    [SerializeField] private TextMeshProUGUI whatLevelText;

    private void Start()
    {
        
        /*  // onClick gözüksün diye
        menuPanelButton.onClick.AddListener(OnOpenPausePanelPressed);
        playButton.onClick.AddListener(OnContinuePressed);
        restartButton_Pause.onClick.AddListener(OnRestartPressed);
        mainMenuButton_Pause.onClick.AddListener(OnMainMenuPressed);
        nextLevelButton.onClick.AddListener(OnNextLevelPressed);
        mainMenuButton_Win.onClick.AddListener(OnMainMenuPressed); */
        // Oyun başlangıcında sadece oyun içi HUD görünsün, diğerleri kapalı olsun.
        scoreCanvas.SetActive(true);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        if (GameManager.Instance.currentLevelIndex >2)
        {
            nextLevelText.text = "Hard Mode Level";
        }
    }
    
    //=================================================================
    // BUTONLARA ATANAN FONKSİYONLAR - Bu kısım butonların beynidir
    //=================================================================
    
    public void OnOpenPausePanelPressed()  { GameManager.Instance.PauseGame(); }
    public void OnContinuePressed() { GameManager.Instance.ResumeGame(); }
    public void OnRestartPressed() { GameManager.Instance.RestartCurrentLevel(); }
    public void OnMainMenuPressed() { GameManager.Instance.ReturnToMainMenu(); }
    public void OnNextLevelPressed()
    {
        winPanel.SetActive(false);
        GameManager.Instance.StartNextLevel();
    }

    //===========================================================================
    // DIŞARIDAN ÇAĞRILAN KOMUTLAR - GameManager bu fonksiyonları çağırır
    //===========================================================================
    
    public void ShowPausePanel() { pausePanel.SetActive(true); }
    public void HidePausePanel() { pausePanel.SetActive(false); }

    public void ShowWinPanel()
    {
        scoreCanvas.SetActive(false); // Karışıklık olmasın diye oyun içi skoru gizle

        winPanel.SetActive(true);
        winText.text = "LEVEL COMPLETE!";
    }
    
    public void UpdateLevelDisplay(int levelNumber)
    {
        if (whatLevelText != null)
        {
            whatLevelText.text = "LEVEL " + levelNumber;
        }
    }
    
    public void UpdateScoreDisplay(int newScore, int targetScore)
    {
        if (GameManager.Instance.currentLevelIndex >2 )
        {
            nextLevelText.text = "Hard Mode Level";
        }
        scoreText.text = "Score: " + newScore.ToString();
        int remaining = Mathf.Max(0, targetScore - newScore);
        nextLevelText.text = "For Next Level: " + remaining.ToString();
    }
    
    public void ResetPanelsToDefault()
    {
        scoreCanvas.SetActive(true);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
    }
}
