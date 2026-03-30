using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Level_", menuName = "Project/Enemy Data/Level Data")]
public class LevelData : ScriptableObject
{
    public string LevelName = "World 1";
    public List<StageData> Stages = new List<StageData>();
}