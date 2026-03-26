using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade_Cursed", menuName = "Project/Roguelite/Cursed Upgrade Data")]
public class CursedUpgradeData : UpgradeData
{
    [Header("Mechanical Buff (The Game Changer)")]
    public MechanicType Mechanic; [Header("Stat Debuffs (The Curses)")]
    public List<StatInjection> Curses = new List<StatInjection>();

    public override void ApplyUpgrade(PlayerController controller, PlayerShooter shooter, PlayerHealth health)
    {
        switch (Mechanic)
        {
            case MechanicType.TwinBlasters:
                shooter.HasTwinBlasters = true;

                if (CombatDirector.Instance != null)
                    CombatDirector.Instance.GlobalEnemySpeedMultiplier += 0.2f;
                break;

            case MechanicType.HeavyOrdinance:
                shooter.HasHeavyOrdinance = true;
                break;

            case MechanicType.GlassCannon:
                // Hardcode Max HP to 1 and remove all previous health buffs
                health.MaxHealth.BaseValue = 1;
                health.MaxHealth.RemoveAllModifiersFromSource(this); // Failsafe
                break;
        }

        foreach (var injection in Curses)
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

        Debug.Log($"[UpgradeData] Applied Cursed Upgrade: {UpgradeName}. Mechanics and Debuffs injected.");
    }
}