using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ult_Resurrection", menuName = "Project/Enemy Data/Ultimates/Resurrection")]
public class UltimateResurrection : BossUltimateData
{
    [Header("Necromancy Settings")]
    public float ChargeTime = 1.0f;
    public float StaggerDelay = 0.15f;

    public override async Awaitable ExecuteUltimateAsync(BossBrain boss)
    {
        if (boss.CurrentWave == null || boss.CurrentWave.Batches.Count == 0) return;

        Debug.Log($"[BossUltimate] {boss.name} is scanning for dead enemies...");

        boss.SetSwayEnabled(false);
        await Awaitable.WaitForSecondsAsync(ChargeTime);
        if (!boss.IsActive) return;

        HashSet<Vector2Int> occupiedSeats = new HashSet<Vector2Int>();

        if (FastCollisionManager.Instance != null)
        {
            foreach (var entity in FastCollisionManager.Instance.Enemies)
            {
                if (entity is EnemyBrain drone && drone.IsActive)
                {
                    occupiedSeats.Add(new Vector2Int(drone.AssignedCol, drone.AssignedRow));
                }
            }
        }

        if (WaveSpawner.Instance != null)
        {
            foreach (BatchData batch in boss.CurrentWave.Batches)
            {
                if (!WaveSpawner.Instance.FullySpawnedBatches.Contains(batch)) continue;

                for (int i = 0; i < batch.TargetSeats.Count; i++)
                {
                    Vector2Int seat = batch.TargetSeats[i];

                    if (!occupiedSeats.Contains(seat))
                    {
                        WaveSpawner.Instance.SpawnGhostForSeat(batch, i);
                        await Awaitable.WaitForSecondsAsync(StaggerDelay);
                    }
                }
            }
        }

        boss.SetSwayEnabled(true);
    }
}