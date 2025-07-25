using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    // Inspector üzerinden ana kameramızı bu alana sürükleyeceğiz.
    [SerializeField] private Transform cameraTransform;

    // Paralaks etkisinin gücünü ayarlayan çarpan.
    // 0 = Hiç hareket etmez. 1 = Kamera ile aynı hızda hareket eder.
    // Genellikle 0 ile 1 arasında bir değer kullanılır (örn: 0.5)
    [SerializeField] private float parallaxMultiplier;

    // Kameranın bir önceki frame'deki pozisyonunu saklamak için
    private Vector3 lastCameraPosition;

    void Start()
    {
        // Oyun başladığında kameranın ilk pozisyonunu alıyoruz.
        lastCameraPosition = cameraTransform.position;
    }

    // LateUpdate, tüm Update'ler bittikten sonra çalışır.
    // Kamera hareketleri genellikle Update'de yapıldığı için, kameranın son pozisyonunu
    // almak için LateUpdate kullanmak en güvenlisidir.
    void LateUpdate()
    {
        // Kameranın bu frame'de ne kadar hareket ettiğini hesaplıyoruz (delta).
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Arka planın pozisyonunu, kameranın hareket miktarının
        // bizim belirlediğimiz çarpanla çarpılmış hali kadar değiştiriyoruz.
        transform.position += deltaMovement * parallaxMultiplier;

        // Bir sonraki frame'de kullanmak üzere kameranın şu anki pozisyonunu güncelliyoruz.
        lastCameraPosition = cameraTransform.position;
    }
}