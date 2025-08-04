using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Seviye_", menuName = "Orbia Game/Seviye Verisi")]
public class LevelData : ScriptableObject //unity'ye özel oyuyun özelliklerini tanımlayacabilecegin yapıya erişmeni ağlar
{
    [Header("Oyuncu Ayarları")]
    public float dashSpeed_inLevelData = 25f; 

    [Header("Kazanma Şartı")]
    public int hedefeUlasmaSayisi = 10; // PlayerController'daki 'passLevel'

    [Header("Gezegen/Düşman Üretim Ayarları")]
    public List<GameObject> enemtSetPrefaps_levelData;  // GameObject[] şeklinde dene
    public GameObject planetPrefab;

    [Tooltip("Yeni gezegenin ne kadar uzağa oluşturulacağı")]
    public float minSpawnMesafe = 14f;
    public float maxSpawnMesafe = 18f;
}