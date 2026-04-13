using UnityEngine;

[CreateAssetMenu(fileName = "Ult_TemporalJamming", menuName = "Project/Enemy Data/Ultimates/Temporal Jamming")]
public class UltimateTemporalJamming : BossUltimateData
{
    [Header("Jamming Settings")]
    public float ChargeTime = 1.0f;
    public DebuffProjectile WavePrefab;

    public override async Awaitable ExecuteUltimateAsync(BossBrain boss)
    {
        Debug.Log($"[BossUltimate] {boss.name} is charging Temporal Jammer!");

        boss.SetSwayEnabled(false);
        await Awaitable.WaitForSecondsAsync(ChargeTime);

        if (!boss.IsActive) return;

        if (PoolManager.Instance != null && WavePrefab != null)
        {
            Vector3 spawnPos = boss.transform.position + Vector3.down * 1.5f;
            PoolManager.Instance.Get(WavePrefab, spawnPos, Quaternion.identity);
        }

        await Awaitable.WaitForSecondsAsync(0.5f);
        boss.SetSwayEnabled(true);
    }
}