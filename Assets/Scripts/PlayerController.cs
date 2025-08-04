using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Dashing }
    public PlayerState currentState;
    
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Transform currentPlanet;
    private float dashSpeed;
    private LevelData currentLevelData;
    
    // Hedef ve Cizgi
    private Transform nextTarget;
    public Transform NextTarget => nextTarget; // Kamera Takibi için
    private LineRenderer activeGuideline;
    [SerializeField] private GameObject guidelinePrefab;
    
    // Efektler ve Sesler
    [Header("Assets")]
    [SerializeField] private GameObject destructionEffectPrefab;//ParticleSystem
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip destroyEnemiesSound;
    [SerializeField] private AudioClip deathSound;
    
    private GameObject activeEnemySet;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Oyuncu sadece "Playing" durumunda hareket edebilir.
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        // Fırlatma
        if (currentState == PlayerState.Idle && nextTarget != null && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // UI uzerine tiklamayi engelle
            {
                Launch();
            }
        }
        
        // line Guncelleme
        if (currentState == PlayerState.Idle && activeGuideline != null)
        {
            activeGuideline.SetPosition(0, transform.position);
            activeGuideline.SetPosition(1, nextTarget.position);
        }
    }

    public void InitializeLevel(LevelData levelData, Transform start)
    {
        this.currentLevelData = levelData; //level data bilgisini alır
        this.dashSpeed = levelData.dashSpeed_inLevelData; 
        ResetPlayer(start);
    }
    
    public void ResetPlayer(Transform startPlanet)
    {
        gameObject.SetActive(true);
        transform.position = startPlanet.position;
        StopAllCoroutines(); // Kalan animasyonlari durdur
        
        AttachToPlanet(startPlanet); 
        
        nextTarget = null;
        if(activeGuideline != null)
        {
            Destroy(activeGuideline.gameObject);
        }
    }
    
    public void SetNextTarget(Transform newTarget)
    {
        nextTarget = newTarget;
        
        // Onceki cizgiyi temizle
        if(activeGuideline != null) Destroy(activeGuideline.gameObject);
        
        // Yeni cizgiyi olustur
        GameObject guidelineObject = Instantiate(guidelinePrefab, Vector3.zero, Quaternion.identity);
        activeGuideline = guidelineObject.GetComponent<LineRenderer>();
    }
    
    private void Launch()
    {
        if (launchSound != null) audioSource.PlayOneShot(launchSound);
        
        rb.isKinematic = false;
        currentState = PlayerState.Dashing;
        currentPlanet = null;

        Vector2 direction = (nextTarget.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;

        if (activeGuideline != null)
        {
            float estimatedTravelTime = Vector2.Distance(transform.position, nextTarget.position) / dashSpeed; //mesafeyi hıza bölerek zamanı hesaplar
            StartCoroutine(AnimateLineDisappearance(activeGuideline, estimatedTravelTime));
            activeGuideline = null;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GeneratedEnemySet"))
        {
            if (deathSound != null) audioSource.PlayOneShot(deathSound);
            GameManager.Instance.PlayerDied();
            gameObject.SetActive(false);
            return;
        }

        if (currentState == PlayerState.Dashing && other.transform == nextTarget)
        {
            AttachToPlanet(other.transform);
            
            Planet targetPlanet = other.GetComponent<Planet>();
            if (targetPlanet != null) targetPlanet.Activate();
            
            DestroyPreviousEnemySet();
            SpawnNewPlanetAndEnemies(other.transform);
        }
    }
    
    private void AttachToPlanet(Transform planetTransform)
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        currentState = PlayerState.Idle;
        currentPlanet = planetTransform;
    }
    
    private void SpawnNewPlanetAndEnemies(Transform originPlanet)
    {
        // Rastgele pozisyon hesaplama
        float minDistance = currentLevelData.minSpawnMesafe;
        float maxDistance = currentLevelData.maxSpawnMesafe;
        float randomAngle = Random.Range(-60f, 90f);
        float randomDistance = Random.Range(minDistance, maxDistance);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)originPlanet.position + (direction * randomDistance);
        
        // Yeni gezegeni olustur ve hedef olarak ata
        GameObject newPlanetObject = Instantiate(currentLevelData.planetPrefab, spawnPosition, Quaternion.identity);
        SetNextTarget(newPlanetObject.transform);

        // Yeni dusman setini olustur
        List<GameObject> enemySets = currentLevelData.enemtSetPrefaps_levelData;
        if (enemySets != null && enemySets.Count > 0)
        {
            GameObject chosenEnemySetPrefab = enemySets[Random.Range(0, enemySets.Count)]; 
            activeEnemySet = Instantiate(chosenEnemySetPrefab, newPlanetObject.transform.position, Quaternion.identity);
        }
    }

    private void DestroyPreviousEnemySet()
    {
        GameManager.Instance.PlanetReached(); 

        if (destroyEnemiesSound != null) audioSource.PlayOneShot(destroyEnemiesSound);
        if (activeEnemySet != null)
        {
            DOTween.Kill(activeEnemySet.transform);
            if (destructionEffectPrefab != null)
            {
                foreach (Transform guardian in activeEnemySet.transform)
                {
                    Instantiate(destructionEffectPrefab, guardian.position, Quaternion.identity);
                }
            }
            Destroy(activeEnemySet);
        }
    }
    
    private System.Collections.IEnumerator AnimateLineDisappearance(LineRenderer line, float duration)
    {
        Vector3 startPoint = line.GetPosition(0);
        Vector3 endPoint = line.GetPosition(1);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // Cizginin null olup olmadigini her adimda kontrol et.
            if(line == null) yield break; // Eger cizgi yok edildiyse, coroutine'den cik.
            
            float t = elapsedTime / duration;
            Vector3 newStartPoint = Vector3.Lerp(startPoint, endPoint, t);
            line.SetPosition(0, newStartPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Animasyon bittiginde cizginin hala var olup olmadigini kontrol et.
        if (line != null)
        {
            Destroy(line.gameObject);
        }
    }
    
    public void StopAllRunningCoroutines()
    {
        StopAllCoroutines(); //bu komut dosyasının (script) başlattığı tüm Coroutine (eşyordam) işlemlerini derhal durdurur.
    }
}