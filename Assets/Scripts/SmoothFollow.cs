using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarlari")]
    [Tooltip("Kameranin takip edecegi hedef (Player nesnesi)")]
    [SerializeField] private PlayerController target;

    [Tooltip("Takip yumusakligi.")]
    [SerializeField] private float smoothTime = 0.3f;

    [Header("Kamera Ofseti")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    
    private Vector3 currentVelocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // --- BUTUN MANTIK DEGISTI ---
        
        Vector3 desiredPosition;

        // 1. Oyuncunun bir sonraki hedefi var mi diye kontrol et.
        if (target.NextTarget != null)
        {
            // 2. Eger hedef VARSA, oyuncu ile hedefinin tam orta noktasini hesapla.
            Vector3 midpoint = (target.transform.position + target.NextTarget.position) / 2f;
            
            // 3. Kameranin olmasi gereken pozisyon, bu orta noktaya ofset eklenmis halidir.
            desiredPosition = midpoint + offset;
        }
        else
        {
            // 4. Eger hedef YOKSA (oyun sonu, baslangic anı vb.), hata olmamasi icin guvenli bir sekilde direkt oyuncuyu takip et.
            desiredPosition = target.transform.position + offset;
        }

        // Hesaplanan 'desiredPosition'a dogru kamerayi yumusakca hareket ettir.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}