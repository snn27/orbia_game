// DOTween kutuphanesini kullanabilmek icin bu satiri ekliyoruz.
using DG.Tweening;
using UnityEngine;

public class DynamicEnemyMovement : MonoBehaviour
{
    // --- GENEL HAREKET AYARLARI ---
    [Header("General Settings")]
    [Tooltip("Animasyonun baslamasi icin bir gecikme suresi (saniye)")]
    [SerializeField] private float startDelay = 0f;

    // --- ROTASYON AYARLARI ---
    [Header("Rotation")]
    [Tooltip("Bu set donsun mu?")]
    [SerializeField] private bool shouldRotate = true;
    [Tooltip("Bir tam donusun ne kadar surecegi (saniye)")]
    [SerializeField] private float rotationDuration = 5f;
    [Tooltip("Donus yonu (1 = saat yonunun tersi, -1 = saat yonu)")]
    [SerializeField] private int rotationDirection = 1;

    // --- NABIZ (SCALE) AYARLARI ---
    [Header("Pulsing Scale")]
    [Tooltip("Bu set nabiz gibi atsin mi (buyuyup kuculsun mu?)")]
    [SerializeField] private bool shouldPulse = false;
    [Tooltip("Ne kadar buyuyecegi (1.2 = %20 buyume)")]
    [SerializeField] private float pulseScale = 1.2f;
    [Tooltip("Bir nabiz atisinin ne kadar surecegi (saniye)")]
    [SerializeField] private float pulseDuration = 2f;

    // --- UYARI: Kodu baslatmak icin Start() fonksiyonunu kullaniyoruz. ---
    void Start()
    {
        // Hareketleri birkac saniye gecikmeli baslatmak icin DOTween'in Delay ozelligini kullanabiliriz.
        // Eger startDelay 0 ise animasyonlar hemen baslar.

        // ROTASYON HAREKETI
        
        if (shouldRotate)
        {
            // transform.DORotate: Nesnenin rotasyonunu animasyonlu olarak degistirir.
            // new Vector3(0, 0, 360 * rotationDirection): Z ekseninde 360 derece don. Yonu biz belirliyoruz.
            // rotationDuration: Bu donusun ne kadar surecegini belirtir.
            // RotateMode.LocalAxisAdd: Mevcut rotasyonun UZERINE ekleyerek don. Bu, surekli donus icin gereklidir.
            transform.DORotate(new Vector3(0, 0, 360 * rotationDirection), rotationDuration, RotateMode.LocalAxisAdd)
                .SetDelay(startDelay)       // Belirtilen sure kadar gecikme ekle.
                .SetEase(Ease.Linear)       // Hizi sabit tut, yavaslayip hizlanma olmasin (Rotator.cs gibi).
                .SetLoops(-1);              // Sonsuz dongu (-1 = sonsuz).
        }

        // NABIZ HAREKETI (SCALE)
        if (shouldPulse)
        {
            // transform.DOScale: Nesnenin boyutunu animasyonlu olarak degistirir.
            // pulseScale: Hedef boyut.
            // pulseDuration: Bu animasyonun ne kadar surecegini belirtir.
            transform.DOScale(pulseScale, pulseDuration)
                .SetDelay(startDelay)       // Gecikme ekle.
                .SetEase(Ease.InOutSine)    // Cok yumusak, estetik bir hizlanma/yavaslama efekti.
                .SetLoops(-1, LoopType.Yoyo); // Sonsuz dongu ve Yoyo tipi (sona gelince basa donmek yerine geri sarar).
        }
    }
}