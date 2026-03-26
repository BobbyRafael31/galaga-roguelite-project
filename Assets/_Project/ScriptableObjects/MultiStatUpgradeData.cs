using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatInjection
{
    public StatType TargetStat;
    public float Value;
    public StatModType ModType;
}

[CreateAssetMenu(fileName = "Upgrade_Multi", menuName = "Project/Roguelite/Multi-Stat Upgrade Data")]
public class MultiStatUpgradeData : UpgradeData
{
    [Header("Multiple Modifications")]
    public List<StatInjection> Injections = new List<StatInjection>();
    public override void ApplyUpgrade(PlayerController controller, PlayerShooter shooter, PlayerHealth health)
    {
        foreach (var injection in Injections)
        {
            StatModifier mod = new StatModifier(injection.Value, injection.ModType, this);

            switch (injection.TargetStat)
            {
                case StatType.MoveSpeed: controller.MoveSpeed.AddModifier(mod); break;
                case StatType.FireCooldown: shooter.FireCooldown.AddModifier(mod); break;
                case StatType.MaxBullets: shooter.MaxBulletsOnScreen.AddModifier(mod); break;
                case StatType.MaxHealth: health.MaxHealth.AddModifier(mod); break;
            }
        }

        Debug.Log($"[UpgradeData] Applied Multi-Stat Upgrade: {UpgradeName}");
    }
}
