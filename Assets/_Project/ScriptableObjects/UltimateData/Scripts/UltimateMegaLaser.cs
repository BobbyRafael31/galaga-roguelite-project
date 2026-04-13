using UnityEngine;

[CreateAssetMenu(fileName = "Ult_MegaLaser", menuName = "Project/Enemy Data/Ultimates/Mega Laser")]
public class UltimateMegaLaser : BossUltimateData
{
    [Header("Laser Settings")]
    public float ChargeTime = 1.5f;
    public float FiringDuration = 2.0f;
    public float LaserSpeed = 15f;

    public EnemyBullet LaserSegmentPrefab;

    public override async Awaitable ExecuteUltimateAsync(BossBrain boss)
    {
        Debug.Log($"[BossUltimate] {boss.name} is charging {UltimateName}!");

        boss.SetSwayEnabled(false);

        Vector3 targetPos = boss.transform.position;
        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy)
        {
            targetPos = new Vector3(PlayerController.Instance.transform.position.x, boss.transform.position.y, 0);
        }

        while (Vector3.Distance(boss.transform.position, targetPos) > 0.05f)
        {
            if (!boss.IsActive) return;
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, targetPos, 10f * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }

        await Awaitable.WaitForSecondsAsync(ChargeTime);

        if (!boss.IsActive) return;

        float endTime = Time.time + FiringDuration;
        while (Time.time < endTime)
        {
            if (!boss.IsActive) break;

            if (PoolManager.Instance != null && LaserSegmentPrefab != null)
            {
                EnemyBullet segment = PoolManager.Instance.Get(LaserSegmentPrefab, boss.transform.position + Vector3.down * 1.5f, Quaternion.identity);
                segment.SetSpeedAndDirection(LaserSpeed, Vector3.down);
            }

            await Awaitable.WaitForSecondsAsync(0.05f);
        }

        boss.SetSwayEnabled(true);
    }
}