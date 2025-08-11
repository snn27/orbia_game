using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelsData", menuName = "LevelsData")]

public class LevelsDataSo : ScriptableObject
{
    public List<LevelDataSo> levels;
}
