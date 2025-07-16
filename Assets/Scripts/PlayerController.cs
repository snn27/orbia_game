using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Dashing }

    [Header("Movement")]
    [SerializeField] private float dashSpeed = 25f;

    [Header("Setup")]
    [SerializeField] private Transform startingPlanet;
    [SerializeField] private Transform initialTarget;

    // --- Private Değişkenler (Yeni Mantık) ---
    private Rigidbody2D rb;
    private PlayerState currentState;
    private Transform nextTarget;

    // Üzerinde bulunduğumuz gezegeni hafızada tutacağız.
    private Transform currentPlanet;

    // Üzerinde bulunduğumuz gezegenin döndürücü script'ini hafızada tutacağız.
    private Rotator currentPlanetRotator;

    // Bir onceki gezegeni hafizada tutmak icin bir degisken
    private Planet previousPlanet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 1. Oyunu ilk gezegene kenetle (sadece başlangıç için pozisyon ayarı)
        AttachToPlanet(startingPlanet);

        // 2. İlk hedefi belirle
        nextTarget = initialTarget;
    }

    private void Update()
    {
        // Eğer fırlatılmadıysak VE bir gezegene bağlıysak...
        if (currentState == PlayerState.Idle && currentPlanet != null && currentPlanetRotator != null)
        {
            // O gezegenin etrafında, onun hızıyla dön.
            transform.RotateAround(currentPlanet.position, Vector3.forward, currentPlanetRotator.rotationSpeed * Time.deltaTime);
        }

        // Eğer fareye basıldıysa VE bir hedefimiz varsa... fırla!
        if (currentState == PlayerState.Idle && nextTarget != null && Input.GetMouseButtonDown(0))
        {
            Launch();
        }
    }

    private void Launch()
    {
        // Artık SetParent(null) yok!
        rb.isKinematic = false; // Fiziği aç
        currentState = PlayerState.Dashing; // Durumu "Fırlatıldı" yap

        // Rotasyon bağlantısını kopar
        currentPlanet = null;
        currentPlanetRotator = null;

        // Hedefe doğru hız ver
        Vector2 direction = (nextTarget.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Eger bir 'Guardian' etiketli dusmana carparsak...
        if (other.CompareTag("Guardian"))
        {
            Debug.Log("Dusmana carptin! Oyun Bitti.");

            // SceneManager.GetActiveScene().buildIndex -> su anki sahnenin sira numarasini verir.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            // Kodun devam etmesini engellemek icin buradan cik.
            return;

        }
        // Sadece fırlatılmış durumdaysak ve doğru hedefe çarptıysak...
        if (currentState == PlayerState.Dashing && other.transform == nextTarget)
        {
            // Eger bir onceki gezegen hafizada varsa (StartPlanet haric diger hepsi icin gecerli)...
            if (previousPlanet != null)
            {
                // Bir onceki gezegene, kendi dusman setini yok etmesini soyle.
                previousPlanet.DestroyMyEnemySet();
            }
            // Yeni gezegene kenetlen
            AttachToPlanet(other.transform);

            // Ulaştığımız gezegenden bir sonraki hedefi isteyelim.
            Planet targetPlanet = other.GetComponent<Planet>();
            if (targetPlanet != null)
            {
                // Ulastigimiz yeni gezegeni, bir sonraki adimda 'bir onceki gezegen' olarak atamak icin hafizaya aliyoruz.
                previousPlanet = targetPlanet;

                nextTarget = targetPlanet.ActivateAndSpawnNext();
            }
        }
    }

    // Oyuncuyu durduran, kenetleyen ve bekleme moduna alan yeni yardımcı fonksiyonumuz.
    private void AttachToPlanet(Transform planetTransform)
    {
        // Hızı sıfırla, fiziği durdur.
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        currentState = PlayerState.Idle;

        // Yeni gezegeni ve onun döndürücüsünü hafızaya al.
        currentPlanet = planetTransform;
        currentPlanetRotator = currentPlanet.GetComponent<Rotator>();

        // Pozisyonu tam gezegenin üstüne alalım (isteğe bağlı ama güzel görünür)
        // Bunu yapabilmek için gezegenin yarıçapını bilmemiz gerekir, şimdilik pozisyonunu alalım yeter.
        // Daha iyi görünüm için gezegenin collider yarıçapı kadar yukarıya taşıyabilirsiniz.
    }
}