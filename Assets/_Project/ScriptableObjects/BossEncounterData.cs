using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossEncounter_", menuName = "Project/Enemy Data/Boss Encounter Data")]
public class BossEncounterData : ScriptableObject
{
    [Header("Identity")]
    public string BossName = "Mothership";
    public BossBrain BossPrefab;

    [Header("Stats")]
    public float MaxHealth = 500f;
    public int ScoreValue = 5000;

    [Header("Arsenal")]
    [Tooltip("The Boss will cycle through these attacks sequentially")]
    public List<BossAttackData> AttackPatterns = new List<BossAttackData>();
}