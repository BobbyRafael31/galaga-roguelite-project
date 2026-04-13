using UnityEngine;

[CreateAssetMenu(fileName = "Ult_DeathSpiral", menuName = "Project/Enemy Data/Ultimates/Death Spiral")]
public class UltimateDeathSpiral : BossUltimateData
{
    [Header("Spiral Settings")]
    public float ChargeTime = 1.0f;
    public EnemyBullet BulletPrefab;
    public float BulletSpeed = 6f;

    [Header("Danmaku Mathematics")]
    public int SpiralStreams = 2;
    public int FiringsPerRotation = 24;
    public float TotalRotations = 3f;
    public float FireDelay = 0.05f;

    public override async Awaitable ExecuteUltimateAsync(BossBrain boss)
    {
        Debug.Log($"[BossUltimate] {boss.name} is initiating the Death Spiral!");

        boss.SetSwayEnabled(false);
        await Awaitable.WaitForSecondsAsync(ChargeTime);

        if (!boss.IsActive) return;

        int totalShots = Mathf.FloorToInt(FiringsPerRotation * TotalRotations);
        float angleStep = 360f / FiringsPerRotation;

        for (int i = 0; i < totalShots; i++)
        {
            if (!boss.IsActive) break;

            float currentBaseAngle = i * angleStep;

            for (int s = 0; s < SpiralStreams; s++)
            {
                float streamOffset = (360f / SpiralStreams) * s;
                float finalAngle = currentBaseAngle + streamOffset;

                Vector3 fireDirection = Quaternion.Euler(0, 0, finalAngle) * Vector3.down;

                if (PoolManager.Instance != null && BulletPrefab != null)
                {
                    Vector3 spawnPos = boss.transform.position + Vector3.down * 0.5f;
                    EnemyBullet bullet = PoolManager.Instance.Get(BulletPrefab, spawnPos, Quaternion.identity);
                    bullet.SetSpeedAndDirection(BulletSpeed, fireDirection);
                }
            }

            await Awaitable.WaitForSecondsAsync(FireDelay);
        }

        await Awaitable.WaitForSecondsAsync(0.5f);
        boss.SetSwayEnabled(true);
    }
}