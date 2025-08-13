using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // --- OYUNCU İÇ DURUMU ---
    public enum PlayerState { Idle, Dashing }
    private PlayerState currentState;
    public Transform NextTarget => nextTarget; // Kamera için public erişim

    // --- ÖZEL DEĞİŞKENLER ---
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Transform nextTarget;
    private LineRenderer activeGuideline;
    private GameObject activeEnemySet;
    private LevelDataSo _currentLevelDataSo;
    private float dashSpeed;
    
    // --- INSPECTOR REFERANSLARI ---
    [Header("Asset & Prefab References")]
    [SerializeField] private GameObject guidelinePrefab;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip destroyEnemiesSound;
    [SerializeField] private AudioClip deathSound;
    
    private bool _hasBonusPointChance = false; 
    [Header("Bonus Point Assets")] //[SerializeField] private AudioClip bonusPointSound;
    [SerializeField] private GameObject bonusPointEffectPrefab;
    [SerializeField] private int bonusScoreValue = 5;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (currentState == PlayerState.Idle && nextTarget != null && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Launch();
            }
        }
        UpdateGuideline();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (currentState != PlayerState.Dashing) return;

        if (other.transform == nextTarget)
        {
            // Gezegene ulaşmadan önce, bonus hakkımız var mıydı diye kontrol et.
            if (_hasBonusPointChance)
            {
                // Evet, hakkımız vardı! Puanı ver.
                GameManager.Instance.AddBonusScore(bonusScoreValue);
                _hasBonusPointChance = false; // Hakkı kullandık, tekrar sıfırla.
            }
            
            AttachToPlanet(other.transform);
            other.GetComponent<Planet>()?.Activate();
            DestroyPreviousEnemySet();
            SpawnNewPlanetAndEnemies(other.transform);
        }
        // --- BONUS HAKKI KAZANMA DURUMU ---
        else if (other.CompareTag("Puan"))
        {
            // Puan trigger'ına değdik. Bayrağı kaldır.
            _hasBonusPointChance = true;

            // Oyuncuya görsel/işitsel geri bildirim ver (bu çok önemlidir!) //if (bonusPointSound != null) audioSource.PlayOneShot(bonusPointSound);
            if (bonusPointEffectPrefab != null)
                Instantiate(bonusPointEffectPrefab, other.transform.position, Quaternion.identity);
        }
        else if (other.CompareTag("GeneratedEnemySet"))
        {
            HandleDeath();
        }
    }
    
    // --- OLAY İŞLEYİCİSİ ---
    private void HandleLevelStart(LevelDataSo levelData, Transform startPoint)
    {
        Debug.Log($"<color=cyan>PlayerController:</color> 'OnLevelStart' anonsunu duydum. '{levelData.name}' kuruluyor.");
        _currentLevelDataSo = levelData;
        dashSpeed = _currentLevelDataSo.dashSpeed_inLevelData;
        
        ResetPlayer(startPoint);
        SpawnNewPlanetAndEnemies(startPoint);
    }
    
    // --- İÇ (PRIVATE) YARDIMCI METOTLAR ---
    public void ResetPlayer(Transform startPlanet)
    {
        gameObject.SetActive(true); // Her ihtimale karşı aktif olduğundan emin ol
        transform.position = startPlanet.position;
        StopAllCoroutines();

        // Tüm iç durumu (hedef, çizgi vb.) temizle
        if (activeGuideline != null) Destroy(activeGuideline.gameObject);
        activeGuideline = null;
        nextTarget = null;
        if (activeEnemySet != null) Destroy(activeEnemySet);
        activeEnemySet = null;

        // Oyuncuyu gezegene fiziksel olarak bağla ve bekleme durumuna al
        AttachToPlanet(startPlanet);
    }
    
    private void Launch()
    {

        _hasBonusPointChance = false;
        if (nextTarget == null) return;
        
        currentState = PlayerState.Dashing;
        rb.isKinematic = false;
        if (launchSound != null) audioSource.PlayOneShot(launchSound);
        
        Vector2 direction = (nextTarget.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;
        
        if (activeGuideline != null)
        {
            float travelTime = Vector2.Distance(transform.position, nextTarget.position) / dashSpeed;
            StartCoroutine(AnimateLineDisappearance(activeGuideline, travelTime));
            activeGuideline = null;
        }
    }
    
    private void AttachToPlanet(Transform planetTransform)
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        currentState = PlayerState.Idle;
        transform.position = planetTransform.position;
    }
    
    private void HandleDeath()
    {
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        // rb.velocity = Vector2.zero;
        // rb.isKinematic = true;

        // Durumu GameManager'a bildir.
        EventManager.TriggerPlayerDied();
    }
    
    private void DestroyPreviousEnemySet()
    {
        EventManager.TriggerPlanetReached(); 
        
        if (destroyEnemiesSound != null) audioSource.PlayOneShot(destroyEnemiesSound);
        if (activeEnemySet != null)
        {
            foreach (Transform guardian in activeEnemySet.transform)
            {
                if (destructionEffectPrefab != null)
                {
                    Instantiate(destructionEffectPrefab, guardian.position, Quaternion.identity);
                }
            }
            DOTween.Kill(activeEnemySet.transform, true);
            Destroy(activeEnemySet);
        }
    }
    
    private void SpawnNewPlanetAndEnemies(Transform originPlanet)
    {
        if (_currentLevelDataSo == null) { return; }
        
        float minDistance = _currentLevelDataSo.minSpawnMesafe;
        float maxDistance = _currentLevelDataSo.maxSpawnMesafe;
        float randomAngle = Random.Range(-60f, 90f);
        float randomDistance = Random.Range(minDistance, maxDistance);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)originPlanet.position + (direction * randomDistance);
        
        GameObject newPlanetObject = Instantiate(_currentLevelDataSo.planetPrefab, spawnPosition, Quaternion.identity);
        SetNextTarget(newPlanetObject.transform);

        List<GameObject> enemySets = _currentLevelDataSo.enemtSetPrefaps_levelData;

        if (enemySets != null && enemySets.Count > 0)
        {
            GameObject chosenEnemySetPrefab = enemySets[Random.Range(0, enemySets.Count)];
            if(chosenEnemySetPrefab != null)
            {
                activeEnemySet = Instantiate(chosenEnemySetPrefab, newPlanetObject.transform.position, Quaternion.identity);
            }
        }
    }

    private void SetNextTarget(Transform newTarget)
    {
        nextTarget = newTarget;
        if(activeGuideline != null) Destroy(activeGuideline.gameObject);
        if (guidelinePrefab != null)
        {
            GameObject guidelineObject = Instantiate(guidelinePrefab, Vector3.zero, Quaternion.identity);
            activeGuideline = guidelineObject.GetComponent<LineRenderer>();
        }
    }

    private void UpdateGuideline()
    {
        if (currentState == PlayerState.Idle && activeGuideline != null && nextTarget != null)
        {
            activeGuideline.SetPosition(0, transform.position);
            activeGuideline.SetPosition(1, nextTarget.position);
        }
    }

    private IEnumerator AnimateLineDisappearance(LineRenderer line, float duration)
    {
        Vector3 startPoint = line.GetPosition(0);
        Vector3 endPoint = line.GetPosition(1);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (line == null) yield break;
            float t = elapsedTime / duration;
            line.SetPosition(0, Vector3.Lerp(startPoint, endPoint, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (line != null) Destroy(line.gameObject);
    }
    public void SetupFirstTarget(LevelDataSo levelData)
    {
        // OnLevelStart'taki kod buraya taşınır.
        _currentLevelDataSo = levelData;
        dashSpeed = _currentLevelDataSo.dashSpeed_inLevelData;
        SpawnNewPlanetAndEnemies(transform); // Artık başlangıç noktası kendi transformu
    }
    
    public void SetupLevel(LevelDataSo levelData)
    {
        Debug.Log($"<color=cyan>PlayerController:</color> GameManager'dan kurulum verileri alındı: '{levelData.name}'");
        _currentLevelDataSo = levelData;
        dashSpeed = _currentLevelDataSo.dashSpeed_inLevelData;
    }

    public void CreateFirstPlanetAndEnemies(Transform originPoint)
    {
        // İlk gezegeni ve düşmanı oluşturur.
        SpawnNewPlanetAndEnemies(originPoint);
    }
    
    public void SetTemporaryTarget(Transform temporaryTarget)
    {
        // Bu, kameranın ve nişan çizgisinin, asıl hedef yaratılmadan önce
        // geçici bir noktaya odaklanmasını sağlar.
        nextTarget = temporaryTarget;
    }
    
}