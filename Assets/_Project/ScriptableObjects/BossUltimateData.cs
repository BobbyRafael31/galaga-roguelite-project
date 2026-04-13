using UnityEngine;

public abstract class BossUltimateData : ScriptableObject
{
    [Header("Ultimate Identity")]
    public string UltimateName = "Unknown Ultimate";
    [Range(0f, 1f)] public float HealthTriggerPercentage = 0.5f;

    [Header("Repeatability")]
    public float Cooldown = 10f;
    public abstract Awaitable ExecuteUltimateAsync(BossBrain boss);
}