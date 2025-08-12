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
    
    private Vector3 _currentVelocity = Vector3.zero;
    private bool _canFollow = true;

    // --- OLAY YÖNETİMİ ---
    private void OnEnable()
    {
        // Oyuncu öldüğünde takibi durdurmak için dinle.
        EventManager.OnPlayerDied += StopFollowing;

        // <<< DÜZELTME BURADA >>>
        // Yeni bir seviye başladığında takibi tekrar aktif etmek için dinle.
        EventManager.OnLevelStart += StartFollowing; 
    }

    private void OnDisable()
    {
        // Abonelikten çıkmayı unutmuyoruz.
        EventManager.OnPlayerDied -= StopFollowing;
        EventManager.OnLevelStart -= StartFollowing;
    }
    
    // --- YAŞAM DÖNGÜSÜ ---
    private void LateUpdate()
    {
        // Hedefimiz yoksa veya pasifse hiçbir şey yapma.
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return;
        }
        
        Vector3 desiredPosition;
        
        if (target.NextTarget != null)
        {
            // --- Normal Takip Modu ---
            // Oyuncu ve hedefinin tam orta noktasini hesapla.
            Vector3 midpoint = (target.transform.position + target.NextTarget.position) / 2f;
            desiredPosition = midpoint + offset;
        }
        else
        {
            // --- HEDEFİN KAYBOLDUĞU AN (Geçiş veya Ölüm) ---
            // Panik yapıp oyuncuya yapışma! Bunun yerine, sadece oyuncunun
            // mevcut pozisyonunu takip et. Bu, kamera zıplamasını engeller.
            desiredPosition = target.transform.position + offset;
            
            // Daha da iyisi, kamerayı mevcut pozisyonunda tutabiliriz.
            // Ama bu, oyuncu başlangıç noktasına ışınlandığında kameranın
            // anında atlamasına neden olabilir. Bu yüzden yavaşça oyuncuya
            // doğru kayması daha iyi.
        }

        // Hesaplanan 'desiredPosition'a dogru kamerayi yumusakca hareket ettir.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothTime);
    }
    
    // --- OLAY İŞLEYİCİLERİ ---
    
    // OnPlayerDied olayının parametresi yok, bu yüzden bu metot da parametresiz.
    private void StopFollowing()
    {
        Debug.Log("<color=purple>CameraFollow:</color> Oyuncu ölüm anonsunu duydum, takibi durduruyorum.");
        _canFollow = false;
    }
    
    private void StartFollowing(LevelDataSo levelData, Transform startPoint)
    {
        Debug.Log("<color=purple>CameraFollow:</color> Seviye başlangıç anonsunu duydum, takibi yeniden başlatıyorum.");
        _canFollow = true;
    }
    
    
}