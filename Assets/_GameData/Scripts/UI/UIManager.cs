using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // <<< CanvasGroup animasyonları için DOTween'i ekliyoruz
using System.Collections;

public class UIManager : MonoBehaviour
{
    // --- UI SİSTEM REFERANSLARI ---
    [Header("UI System References")]
    [SerializeField] private ExperienceBarUI experienceBar;
    
    // --- CANVAS GROUP REFERANSLARI ---
    // Artık GameObject'leri değil, CanvasGroup'ları kontrol edeceğiz.
    [Header("Panels & Canvases")]
    [SerializeField] private CanvasGroup scoreCanvasGroup;
    [SerializeField] private CanvasGroup pausePanelGroup;
    [SerializeField] private CanvasGroup winPanelGroup;
    [SerializeField] private float fadeDuration = 0.3f; // Animasyon süresi

    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI winText;
    
    // --- BUTON REFERANSLARI (Sadeleştirildi) ---
    [Header("Buttons")]
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private Button playButton;         // Continue button
    [SerializeField] private Button repeatButton;       // Restart button
    [SerializeField] private Button nextLevelButton;    // Win Panel's NextLevel button
    
    // --- KURULUM ---
    private void Awake() {
        SetPanelState(pausePanelGroup, false, 0f);
        SetPanelState(winPanelGroup, false, 0f);
    }
    
    // --- OLAY YÖNETİMİ ---
    private void OnEnable() {
        EventManager.OnScoreUpdated += HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated += HandleLevelDisplayUpdated;
        EventManager.OnLevelWon += HandleLevelWon;
    
        // <<< EN ÖNEMLİ DÜZELTME: BU ABONELİĞİ GERİ EKLEMEK >>>
        // Yeni bir seviye başladığında arayüzü resetlemek için bu olayı dinlemeliyiz.
        EventManager.OnLevelStart += HandleLevelStart;
    }

    private void OnDisable() {
        EventManager.OnScoreUpdated -= HandleScoreUpdated;
        EventManager.OnLevelDisplayUpdated -= HandleLevelDisplayUpdated;
        EventManager.OnLevelWon -= HandleLevelWon;
    
        // <<< EKLEDİĞİMİZ ABONELİĞİ BURADAN DA ÇIKARIYORUZ >>>
        EventManager.OnLevelStart -= HandleLevelStart;
    }
    
    // --- OLAY İŞLEYİCİLERİ ---
    private void HandleLevelStart(LevelDataSo levelData, Transform startPoint) {
        
        Debug.Log("<color=yellow>UIManager:</color> Seviye başlangıç anonsunu duydum, UI resetleniyor.");
    
        FadeInPanel(scoreCanvasGroup);   // OYUN İÇİ ARAYÜZÜNÜ GÖSTER
        FadeOutPanel(pausePanelGroup);  // Pause panelinin kapalı olduğundan emin ol
        FadeOutPanel(winPanelGroup);    // Win panelinin kapalı olduğundan emin 
    }
    
    private void HandleLevelWon() {
        FadeOutPanel(scoreCanvasGroup);
        FadeInPanel(winPanelGroup);
        if(winText != null) winText.text = "LEVEL COMPLETE!";
    }

    // HandleScoreUpdated ve HandleLevelDisplayUpdated'da değişiklik yok, aynı kalabilir.
    private void HandleScoreUpdated(int newScore, int targetScore) { if(experienceBar != null) experienceBar.UpdateExperience(newScore, targetScore); }
    private void HandleLevelDisplayUpdated(int levelNumber) { if(experienceBar != null) experienceBar.UpdateLevel(levelNumber); }
    
    // --- BUTON METOTLARI ---
    public void OnOpenPausePanelPressed() {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        FadeInPanel(pausePanelGroup);
        Time.timeScale = 0f;
    }
    
    public void OnContinuePressed() {
        FadeOutPanel(pausePanelGroup);
        Time.timeScale = 1f;
    }
    
    public void OnRestartPressed() {
        StartCoroutine(FadeOutAndDoAction(() => {
            Time.timeScale = 1f;
            EventManager.TriggerRestartLevelRequest();
        }));
    }
    
    // Ana Menü butonu kalktığı için bu fonksiyon da kaldırıldı.
    // public void OnMainMenuPressed() { ... }

    public void OnNextLevelPressed()
    {
        StartCoroutine(FadeOutAndDoAction(() => {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextLevel();
            }
        }));
        FadeOutPanel(winPanelGroup);
    
        // SONRA oyun içi panelini anında görünür yap.
        FadeInPanel(scoreCanvasGroup);
    }
    
    private IEnumerator FadeOutAndDoAction(Action actionToDo)
    {
        // Önce açık olan paneli bul
        CanvasGroup activePanel = null;
        if(pausePanelGroup.alpha > 0) activePanel = pausePanelGroup;
        else if (winPanelGroup.alpha > 0) activePanel = winPanelGroup;

        // Eğer kapatılacak bir panel varsa, onun kapanmasını bekle
        if (activePanel != null)
        {
            FadeOutPanel(activePanel);
            
            // Animasyon süresi kadar bekle.
            yield return new WaitForSecondsRealtime(fadeDuration); // WaitForSecondsRealtime, Time.timeScale=0 iken de çalışır.
        }
        
        // Animasyon bittikten sonra asıl görevi yap.
        actionToDo?.Invoke();
    }

    // --- YARDIMCI ANİMASYON METOTLARI ---
    private void FadeInPanel(CanvasGroup canvasGroup) {
        if (canvasGroup == null) return;
        // DOTween kullanarak alpha'yı 1 yap (görünür)
        canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true); // SetUpdate(true) Time.timeScale=0 iken de çalışmasını sağlar
        SetPanelState(canvasGroup, true);
    }
    
    private void FadeOutPanel(CanvasGroup canvasGroup) {
        if (canvasGroup == null)
        {
            Debug.Log("panel düzgün kapatılmadı");
            return;
        }
        // DOTween kullanarak alpha'yı 0 yap (görünmez)
        canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        SetPanelState(canvasGroup, false);
    }
    
    private void SetPanelState(CanvasGroup canvasGroup, bool isInteractable, float? alpha = null)
    {
        if(canvasGroup == null) return;
        if(alpha.HasValue) canvasGroup.alpha = alpha.Value;
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;
    }
}