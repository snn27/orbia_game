using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarlari")]
    [Tooltip("Kameranin takip edecegi hedef (Player nesnesi)")]
    [SerializeField] private Transform target;

    [Tooltip("Takip yumusakligi. Dusuk degerler kamerayi daha hizli, yuksek degerler daha 'tembel' ve yumusak yapar.")]
    [SerializeField] private float smoothTime = 0.3f;

    [Header("Kamera Ofseti")]
    [Tooltip("Kameranin hedefe gore duracagi pozisyon farki. 2D oyunlar icin Z ekseninde -10 standarttir.")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    // SmoothDamp fonksiyonunun kendi hizini takip edebilmesi icin gereken dahili bir degisken.
    // Dokunmaniza gerek yoktur, Unity arka planda kendisi kullanir.
    private Vector3 currentVelocity = Vector3.zero;

    // LateUpdate, diger tum Update fonksiyonlari calistiktan sonra her karenin sonunda cagirilir.
    // Bu, oyuncu hareketini tamamladiKtan SONRA kameranin onu takip etmesini garanti ederek
    // olasi titreme (jitter) sorunlarini onler. Kamera takibi icin en iyi yontem budur.
    private void LateUpdate()
    {
        // Bir hedef atanmamissa, kodun devaminda hata almamak icin hicbir sey yapma.
        if (target == null)
        {
            return;
        }

        // 1. Hedef Pozisyonunu Belirle: Kameranin gitmek ISTEDIGI yer burasidir.
        Vector3 desiredPosition = target.position + offset;

        // 2. Yumusak Hareketi Uygula: Mevcut kameranin pozisyonunu, hedeflenen pozisyona dogru yumusakca hareket ettir.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}