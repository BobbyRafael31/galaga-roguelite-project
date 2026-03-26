using UnityEngine;

public enum MechanicType
{
    Piercing,          // SS-1
    KineticPlating,    // SS-2
    ScoreScavenger,    // SS-3
    TwinBlasters,      // SSS-1
    PhaseGenerator,    // SSS-2 (Will implement the shield logic later )
    GlassCannon,       // SSS-3
    HeavyOrdinance     // SSS-4
}

[CreateAssetMenu(fileName = "Upgrade_Mechanic", menuName = "Project/Roguelite/Mechanical Upgrade Data")]
public class MechanicalUpgradeData : UpgradeData
{
    [Header("Mechanical Buff")]
    public MechanicType Mechanic;

    [Tooltip("How much the mechanic improves per level (e.g., +1 pierce)")]
    public int PowerPerLevel = 1;

    public override void ApplyUpgrade(PlayerController controller, PlayerShooter shooter, PlayerHealth health)
    {
        switch (Mechanic)
        {
            case MechanicType.Piercing:
                shooter.BonusPiercing += PowerPerLevel;
                break;
            case MechanicType.KineticPlating:
                health.HasKineticPlating = true;
                break;
            case MechanicType.ScoreScavenger:
                ScoreManager.Instance.ScoreMultiplier += (0.2f * PowerPerLevel);
                break;
        }

        Debug.Log($"[UpgradeData] Applied Mechanic: {Mechanic} Level up.");
    }
}
