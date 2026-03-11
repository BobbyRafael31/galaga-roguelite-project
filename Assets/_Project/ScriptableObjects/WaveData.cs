using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BatchData
{
    public EnemyBrain EnemyPrefab;
    public PathData EntrancePath;
    public float WaveStartTime = 0f;
    public float SpawnDelay = 0.2f;

    public List<Vector2Int> TargetSeats = new List<Vector2Int>();
}
[CreateAssetMenu(fileName = "Wave_", menuName = "Project/Enemy Data/Wave Data")]
public class WaveData : ScriptableObject
{
    public FormationData Formation;

    [HideInInspector]
    public List<BatchData> Batches = new List<BatchData>();
}

[CreateAssetMenu(fileName = "Stage_", menuName = "Project/Enemy Data/Stage Data")]
public class StageData : ScriptableObject
{
    public List<WaveData> Waves = new List<WaveData>();
}