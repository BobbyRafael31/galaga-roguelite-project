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
    [HideInInspector] public List<BatchData> Batches = new List<BatchData>();
}

[Serializable]
public struct CombatSettings
{
    [Header("Dive Pacing")]
    [Tooltip("Maximum number of enemies allowed to dive simultaneously.")]
    public int MaxActiveDives;

    [Tooltip("Seconds the Director waits before handing out another Dive Token.")]
    public float DiveTokenCooldown;

    [Header("Shooting Pacing")]
    [Tooltip("Maximum enemy projectiles allowed on screen at once.")]
    public int MaxActiveProjectiles;

    [Tooltip("Seconds the Director waits before handing out another Shoot Token.")]
    public float ShootTokenCooldown;

    [Tooltip("How fast the enemy bullets travel in this stage.")]
    public float EnemyProjectileSpeed;
}

[CreateAssetMenu(fileName = "Stage_", menuName = "Project/Enemy Data/Stage Data")]
public class StageData : ScriptableObject
{
    public CombatSettings Aggression = new CombatSettings
    {
        MaxActiveDives = 2,
        DiveTokenCooldown = 3f,
        MaxActiveProjectiles = 3,
        ShootTokenCooldown = 1.5f,
        EnemyProjectileSpeed = 6f
    };

    public List<WaveData> Waves = new List<WaveData>();
}