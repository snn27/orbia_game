using System.Collections.Generic;
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

    // --- Planet ---
    [Header("Core Dependencies")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private List<GameObject> enemySetPrefabs;
    [SerializeField] private GameObject destructionEffectPrefab;

    // --- Hafiza icin yeni degiskenler ---
    private GameObject activeEnemySet; // Sahnedeki aktif dusman setini hafizada tutacagiz.

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
        // Dusmana carpma mantigi
        if (other.CompareTag("Guardian"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        // Sadece firlatilmis durumdaysak ve dogru hedefe carptiysak...
        if (currentState == PlayerState.Dashing && other.transform == nextTarget)
        {
            // --- YENI MANTIK AKISI ---

            // 1. Yeni gezegene kenetlen
            AttachToPlanet(other.transform);

            // 2. Ulastigimiz gezegeni 'aktif' et (rengini kirmizi yap).
            Planet targetPlanet = other.GetComponent<Planet>();
            if (targetPlanet != null)
            {
                targetPlanet.Activate();
            }

            // 3. Bir onceki (artik aktif olan) dusman setini yok et.
            DestroyPreviousEnemySet();

            // 4. Yeni bir gezegen ve onun etrafinda bir dusman seti olustur.
            SpawnNewPlanetAndEnemies(other.transform);
        }
    }

    private void SpawnNewPlanetAndEnemies(Transform originPlanet)
    {
        // 1. Bir sonraki hedef gezegeni olustur
        float minDistance = 8f, maxDistance = 14f;
        float randomAngle = Random.Range(-60f, 90f);
        float randomDistance = Random.Range(minDistance, maxDistance);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)originPlanet.position + (direction * randomDistance);

        GameObject newPlanetObject = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
        nextTarget = newPlanetObject.transform; // Oyuncunun yeni hedefini belirle.

        // 2. O yeni gezegenin etrafinda bir dusman seti olustur.
        if (enemySetPrefabs != null && enemySetPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, enemySetPrefabs.Count);
            GameObject chosenEnemySetPrefab = enemySetPrefabs[randomIndex];

            // Dusman setini, yeni olusan 'nextTarget'in pozisyonunda olusturuyoruz!
            // ve bir sonraki adimda silebilmek icin hafizaya aliyoruz.
            activeEnemySet = Instantiate(chosenEnemySetPrefab, nextTarget.position, Quaternion.identity);
        }
    }

    private void DestroyPreviousEnemySet()
    {
        // Eger hafizada yok edilecek bir dusman seti varsa...
        if (activeEnemySet != null)
        {
            // Her bir dusman icin bir yok olma efekti olustur (animasyon)
            if (destructionEffectPrefab != null)
            {
                foreach (Transform guardian in activeEnemySet.transform)
                {
                    Instantiate(destructionEffectPrefab, guardian.position, Quaternion.identity);
                }
            }
            // Ve ana dusman seti objesini yok et.
            Destroy(activeEnemySet);
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