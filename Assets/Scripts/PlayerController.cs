using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// DOTween'i kod icinden kontrol edebilmek icin bu kutuphaneyi ekliyoruz.
using DG.Tweening;
using UnityEngine.EventSystems;

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
    private int passLevel = 10;
    private bool hardMode=false;
    private int nextLevelInLevel = 10;

    private int whatLevel;//level değiştirmek için

    //iki cisim arasında line
    [Header("Guideline")]
    [SerializeField] private GameObject guidelinePrefab; // Az önce oluşturduğumuz prefab'ı buraya atayacağız.

    private LineRenderer activeGuideline; // Sahnede aktif olan çizgiyi hafızada tutmak için

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

        whatLevel = SceneManager.GetActiveScene().buildIndex ;

        if (whatLevel >= 3)
        {
            nextLevelInLevel = 15;
           NextLevelTest.text = "Hard Mode Level" ;
        }

    }

    private void Update()
    {

        // Eğer fareye basıldıysa VE bir hedefimiz varsa... fırla!
        if (currentState == PlayerState.Idle && nextTarget != null && Input.GetMouseButtonDown(0))
        {
            Launch();
        }

        // Eğer bekleme modundaysak ve bir yol gösterici çizgimiz varsa...
        if (currentState == PlayerState.Idle && activeGuideline != null)
        {
            // Çizginin başlangıç noktasını oyuncunun pozisyonuna ayarla.
            activeGuideline.SetPosition(0, transform.position);
            // Çizginin bitiş noktasını hedefin pozisyonuna ayarla.
            activeGuideline.SetPosition(1, nextTarget.position);
        }
    }

    private void Launch()
    {
        // Eğer fare bir UI elementinin üzerindeyse, HİÇBİR ŞEY YAPMA ve fonksiyondan çık.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Bu satır, aşağıdaki kodların çalışmasını engeller.
        }

        AudioManager.instance.Play("Launch");

        rb.isKinematic = false; // Fiziği aç, çünkü uçuş modunda
        currentState = PlayerState.Dashing; // Durumu "Fırlatıldı" yap

        // Rotasyon bağlantısını kopar
        currentPlanet = null;

        // Hedefe doğru hız ver
        Vector2 direction = (nextTarget.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;

        if (activeGuideline != null)
        {
            // Oyuncunun hedefe varma süresini yaklaşık olarak hesaplayalım.
            float estimatedTravelTime = Vector2.Distance(transform.position, nextTarget.position) / dashSpeed;

            // Bu süreye göre animasyonu başlatacak coroutine'i çağır.
            StartCoroutine(AnimateLineDisappearance(activeGuideline, estimatedTravelTime));

            // Animasyon başladığı için artık ana kontrolü bu değişkenden alıyoruz.
            activeGuideline = null;
        }

    }

    private System.Collections.IEnumerator AnimateLineDisappearance(LineRenderer line, float duration)
    {
        // Çizginin başlangıç ve bitiş noktalarını bir kereliğine kaydedelim.
        Vector3 startPoint = line.GetPosition(0);
        Vector3 endPoint = line.GetPosition(1);

        float elapsedTime = 0f;

        // Belirlenen süre boyunca...
        while (elapsedTime < duration)
        {
            // Animasyonun ne kadarının tamamlandığını hesapla (0'dan 1'e).
            float t = elapsedTime / duration;

            // Çizginin BAŞLANGIÇ noktasını, eski başlangıç noktasından bitiş noktasına doğru hareket ettir.
            Vector3 newStartPoint = Vector3.Lerp(startPoint, endPoint, t);
            line.SetPosition(0, newStartPoint);

            // Bir sonraki frame'e geç.
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Süre dolduğunda, çizgi nesnesini tamamen yok et.
        Destroy(line.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Dusmana carpma mantigi
        if (other.CompareTag("Guardian"))
        {
            AudioManager.instance.Play("Death");
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


        // 1. Önceki çizgiyi (varsa) temizleyelim. Güvenlik önlemi.
        if (activeGuideline != null)
        {
            Destroy(activeGuideline.gameObject);
        }

        // 2. Yeni yol gösterici çizgiyi sahnede oluştur.
        GameObject guidelineObject = Instantiate(guidelinePrefab, Vector3.zero, Quaternion.identity);
        activeGuideline = guidelineObject.GetComponent<LineRenderer>();


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

            if (activeEnemySet != null)
            {
                AudioManager.instance.Play("Destroy"); // <-- BU SATIRI EKLE

                DOTween.Kill(activeEnemySet.transform);
                // ... geri kalan kodlar ...
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

        nextLevel = nextLevelInLevel - score;
        NextLevelTest.text = "For Next Level:" + nextLevel.ToString();
        if (score >= passLevel )
        {
            if (whatLevel>=3 && !hardMode)
            {
                passLevel = 15;
                hardMode = true;
                return;
            }
            //Highscore = score;æç
            whatLevel++;
            SceneManager.LoadScene(whatLevel);
        }
    }
}