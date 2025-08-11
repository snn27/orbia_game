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
        // Güvenlik koşulumuz aynı.
        if (!_canFollow || target == null || !target.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 desiredPosition;
        
        // Bu takip mantığı doğru ve aynı kalabilir.
        if (target.NextTarget != null)
        {
            Vector3 midpoint = (target.transform.position + target.NextTarget.position) / 2f;
            desiredPosition = midpoint + offset;
        }
        else
        {
            desiredPosition = target.transform.position + offset;
        }

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