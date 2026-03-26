using UnityEngine;

public enum UpgradeTier
{
    S_Tactical,
    SS_Evolution,
    SSS_Cursed
}

public enum StatType
{
    None,
    MoveSpeed,
    FireCooldown,
    MaxBullets,
    BulletSpeed,
    MaxHealth,
    ScoreMultiplier,
    EnemySpeedMultiplier // For Cursed Coins
}

[CreateAssetMenu(fileName = "Upgrade_", menuName = "Project/Roguelite/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("UI Presentation")]
    public string UpgradeName;
    [TextArea] public string Description;
    public Sprite CoinIcon;
    public UpgradeTier Tier;

    [Header("Economy & Limits")]
    public int BaseCost = 1000;
    public int MaxLevel = 5;

    [Tooltip("How much the cost increases each time you buy a level of this upgrade")]
    public int CostScalingPerLevel = 250;

    [Tooltip("Higher weight means more likely to appear in the shop RNG")]
    public int BaseRNGWeight = 100;

    [Header("Numerical Buff (Optional)")]
    public StatType TargetStat = StatType.None;
    public float StatModifierValue = 0f;
    public StatModType ModifierType = StatModType.Flat;

    public virtual void ApplyUpgrade(PlayerController controller, PlayerShooter shooter, PlayerHealth health)
    {
        if (TargetStat == StatType.None) return;

        StatModifier mod = new StatModifier(StatModifierValue, ModifierType, this);

        switch (TargetStat)
        {
            case StatType.MoveSpeed:
                controller.MoveSpeed.AddModifier(mod);
                break;
            case StatType.FireCooldown:
                shooter.FireCooldown.AddModifier(mod);
                break;
            case StatType.MaxBullets:
                shooter.MaxBulletsOnScreen.AddModifier(mod);
                break;
            case StatType.MaxHealth:
                health.MaxHealth.AddModifier(mod);
                // Hardcoded heal mechanic for S-5
                health.Heal(1f);
                break;
                // Complex mechanical overrides (Piercing, Twin Blasters) will be handled by inheriting classes
        }
        Debug.Log($"[UpgradeData] Applied {UpgradeName} to {TargetStat}. Value: {StatModifierValue} {ModifierType}");
    }
}
