using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Yumuşak geçişler için DOTween

public class ExperienceBarUI : MonoBehaviour
{
    [Header("UI Element References")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Animation Settings")]
    [Tooltip("Barın dolma animasyonunun süresi (saniye)")]
    [SerializeField] private float fillDuration = 0.5f;

    // Seviye yazısını günceller (Örn: Yıldızın içindeki "4")
    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }
    }

    // Puan/XP çubuğunu ve metnini günceller
    public void UpdateExperience(int currentXP, int targetXP)
    {
        // Metni güncelle (örneğin "324 / 400")
        if (progressText != null)
        {
            progressText.text = $"{currentXP} / {targetXP}";
        }
        
        // Doluluk oranını hesapla (0.0 ile 1.0 arasında)
        float targetFillAmount = (targetXP > 0) ? (float)currentXP / targetXP : 0f;

        // DOTween ile barı yumuşak bir şekilde doldur
        if (progressBarFill != null)
        {
            progressBarFill.DOKill(); // Devam eden bir animasyon varsa kes
            progressBarFill.DOFillAmount(targetFillAmount, fillDuration).SetEase(Ease.OutCubic);
        }
    }
}