using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// DOTween'i kod icinden kontrol edebilmek icin bu kutuphaneyi ekliyoruz.
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Dashing } //enum state durumunu belirtmek için kullanılır ilk parametresi int değerinde 0 dır.

    [Header("Movement")]
    [SerializeField] private float dashSpeed = 25f;

    [Header("Setup")]
    [SerializeField] private Transform startingPlanet;
    [SerializeField] private Transform initialTarget;

    // ---  ---
    private Rigidbody2D rb;
    private PlayerState currentState;
    private Transform nextTarget;

    // Üzerinde bulunduğumuz gezegeni hafızada tutacağız.
    private Transform currentPlanet;

    //score text
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI NextLevelTest;

    private int score;
    private int nextLevel;

    private int whatLevel;//level değiştirmek için

    // --- Planet ---
    [Header("Core Dependencies")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private List<GameObject> enemySetPrefabs;
    [SerializeField] private GameObject destructionEffectPrefab; // anim efegi için

    
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

        score = -1;

        whatLevel = SceneManager.GetActiveScene().buildIndex;

    }

    private void Update()
    {

        // Eğer fareye basıldıysa VE bir hedefimiz varsa... fırla!
        if (currentState == PlayerState.Idle && nextTarget != null && Input.GetMouseButtonDown(0))
        {
            Launch();
        }
    }

    private void Launch()
    {
        rb.isKinematic = false; // Fiziği aç, çünkü uçuş modunda
        currentState = PlayerState.Dashing; // Durumu "Fırlatıldı" yap

        // Rotasyon bağlantısını kopar
        currentPlanet = null;

        // Hedefe doğru hız ver
        Vector2 direction = (nextTarget.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Dusmana carpma mantigi
        if (other.CompareTag("Guardian"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //enemye carptıgı için game over!
            return;
        }

        // Sadece firlatilmis durumdaysak ve dogru hedefe carptiysak...
        if (currentState == PlayerState.Dashing && other.transform == nextTarget)
        {
            // --- YENI MANTIK AKISI ---

            // 1. Yeni gezegene kenetlen
            AttachToPlanet(other.transform); // hızını ve kinetigi durdurarak bu işlmei yapmış oluyor

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
        float minDistance = 10f, maxDistance = 16f;
        float randomAngle = Random.Range(-60f, 90f);
        float randomDistance = Random.Range(minDistance, maxDistance);

        //açıyı bir vektöre dönüştürme
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)originPlanet.position + (direction * randomDistance);

        GameObject newPlanetObject = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
        nextTarget = newPlanetObject.transform; // Oyuncunun yeni hedefini belirle.

        // 2. O yeni gezegenin etrafinda bir dusman seti olustur.
        if (enemySetPrefabs != null && enemySetPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, enemySetPrefabs.Count);
            GameObject chosenEnemySetPrefab = enemySetPrefabs[randomIndex]; //random seçmiş oldugu enemy setini chosenEnemySetPrefab aktardı

            // Dusman setini, yeni olusan 'nextTarget'in pozisyonunda olusturuyoruz!
            // ve bir sonraki adimda silebilmek icin hafizaya aliyoruz.
            activeEnemySet = Instantiate(chosenEnemySetPrefab, nextTarget.position, Quaternion.identity);
        }
    }

    private void DestroyPreviousEnemySet()
    {
        UpdateScore();

        // Eger hafizada yok edilecek bir dusman seti varsa...
        if (activeEnemySet != null)
        {

            // 'activeEnemySet' objesini yok etmeden HEMEN ONCE,
            // DOTween'e bu nesne ve cocuklari uzerindeki tum animasyonlari durdurmasini soyluyoruz.
            // Bu, "hayalet animasyon" hatasini %100 cozer.
            DOTween.Kill(activeEnemySet.transform);

            // Her bir dusman icin bir yok olma efekti olustur (animasyon)
            if (destructionEffectPrefab != null)
            {
                foreach (Transform guardian in activeEnemySet.transform)
                {
                    Instantiate(destructionEffectPrefab, guardian.position, Quaternion.identity); //Bu, rotasyonun 0 olduğu, yani prefab’ın orijinal yönüyle kullanılacağı anlamına gelir.
                }
            }

            // Ve ana dusman seti objesini GUVENLE yok et.
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

        // Pozisyonu tam gezegenin üstüne alalım (isteğe bağlı ama güzel görünür)
        // Bunu yapabilmek için gezegenin yarıçapını bilmemiz gerekir, şimdilik pozisyonunu alalım yeter.
        // Daha iyi görünüm için gezegenin collider yarıçapı kadar yukarıya taşıyabilirsiniz.
    }

    private void UpdateScore()
    {
        score += 1;
        text.text = "Score:" + score.ToString();

        nextLevel = 10 - score;
        NextLevelTest.text = "For Next Level:" + nextLevel.ToString();
        if (score >= 10)
        {

            //Highscore = score;
            whatLevel++;
            SceneManager.LoadScene(whatLevel);
        }
    }
}