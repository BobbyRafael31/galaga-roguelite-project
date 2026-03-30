using UnityEngine;

public enum BossAttackType
{
    Targeted,
    Spread,
    Burst
}

[CreateAssetMenu(fileName = "BossAttack_", menuName = "Project/Enemy Data/Boss Attack Data")]
public class BossAttackData : ScriptableObject
{
    public BossAttackType AttackType;

    [Header("Projectile Settings")]
    public float BulletSpeed = 8f;
    public int ProjectileCount = 1;

    [Header("Pattern Math")]
    [Tooltip("For Spread: The total angle arc (e.g., 90 degrees). For Burst/Targeted: Ignored")]
    public float SpreadAngle = 45f;

    [Header("Cooldown")]
    public float BurstDelay = 0.1f;
    public float PostAttackCooldown = 2.0f;
}
