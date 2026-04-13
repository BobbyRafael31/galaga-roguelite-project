using UnityEngine;

[CreateAssetMenu(fileName = "Ult_EMPOverload", menuName = "Project/Enemy Data/Ultimates/EMP Overload")]
public class UltimateEMPOverload : BossUltimateData
{
    [Header("EMP Settings")]
    public float ChargeTime = 1.0f;
    public DebuffProjectile EMPWavePrefab;

    public override async Awaitable ExecuteUltimateAsync(BossBrain boss)
    {
        Debug.Log($"[BossUltimate] {boss.name} is charging EMP Overload!");

        boss.SetSwayEnabled(false);
        await Awaitable.WaitForSecondsAsync(ChargeTime);
        if (!boss.IsActive) return;

        if (PoolManager.Instance != null && EMPWavePrefab != null)
        {
            Vector3 spawnPos = boss.transform.position + Vector3.down * 1.5f;
            PoolManager.Instance.Get(EMPWavePrefab, spawnPos, Quaternion.identity);
        }

        await Awaitable.WaitForSecondsAsync(0.5f);
        boss.SetSwayEnabled(true);
    }
}