using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private List<GameObject> enemySetPrefabs;
    [SerializeField] private Color defaultColor = Color.green;
    [SerializeField] private Color activeColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenActivated = false;

    // Bu gezegenin olusturdugu dusman setini hafizada tutacagiz.
    private GameObject spawnedEnemySet;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor; // Her gezegen yeşil başlar.
        }
    }

    public Transform ActivateAndSpawnNext()
    {
        if (hasBeenActivated) return null; // Eger bu gezegen daha once aktive edildiyse, tekrar islem yapma.

        hasBeenActivated = true;
        if (spriteRenderer != null) spriteRenderer.color = activeColor;// Rengini 'aktif' (kirmizi) olarak degistir.

        SpawnRandomEnemySet();// Bu gezegen aktif oldugunda, etrafina bir dusman seti olustur.

        return SpawnNextPlanet();
    }

    // Bu gezegene ait dusman setini yok etmesi icin cagiracagimiz komut.
    public void DestroyMyEnemySet()
    {
        // Eger bu gezegen tarafindan bir dusman seti olusturulduysa...
        if (spawnedEnemySet != null)
        {
            // O dusman setini sahneden yok et.
            Destroy(spawnedEnemySet);
        }
    }

    private void SpawnRandomEnemySet()
    {
        if (enemySetPrefabs == null || enemySetPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, enemySetPrefabs.Count);
        GameObject chosenEnemySetPrefab = enemySetPrefabs[randomIndex];

        // Olusturdugumuz dusman setini, sonradan silebilmek icin degiskene atiyoruz.
        spawnedEnemySet = Instantiate(chosenEnemySetPrefab, transform.position, Quaternion.identity);
    }

    private Transform SpawnNextPlanet()
    {
        if (planetPrefab == null) return null;

        // Rastgele konum hesaplamaları...
        float minDistance = 12f, maxDistance = 18f;
        float randomAngle = Random.Range(-45f, 90f);
        float randomDistance = Random.Range(minDistance, maxDistance);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)transform.position + (direction * randomDistance);

        GameObject newPlanet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
        return newPlanet.transform;
    }
}