using System;

public static class EventBus
{
    // Gameplay Events
    public static Action<int> OnEnemyDestroyed;
    public static Action<int, int, UnityEngine.Sprite> OnPlayerHealthInitialized;
    public static Action<int> OnPlayerHit;
    public static Action OnPlayerDeath;
    public static Action OnGameOver;
    public static Action OnMainMenuEntered;

    // Progression Events
    public static Action OnGameStarted;
    public static Action<int> OnWaveStarted;
    public static Action<int> OnWaveCompleted;
    public static Action OnStageCleared;
    public static Action OnClearArena;
    public static Action OnShopEntered;
    public static System.Action<string> OnProgressionChanged;

    // UI or Economy Events
    public static Action<int> OnScoreChanged;
    public static Action<UpgradeData[]> OnDraftGenerated;

    // Boss UI Events
    public static Action<string, float, float> OnBossSpawned;
    public static Action<float> OnBossHealthChanged;
    public static Action OnBossDefeated;

    public static void ClearAll()
    {
        OnEnemyDestroyed = null;
        OnPlayerHit = null;
        OnPlayerDeath = null;
        OnGameStarted = null;
        OnWaveStarted = null;
        OnWaveCompleted = null;
        OnStageCleared = null;
        OnScoreChanged = null;
        OnClearArena = null;
        OnGameOver = null;
        OnShopEntered = null;
        OnDraftGenerated = null;
        OnProgressionChanged = null;
        OnMainMenuEntered = null;
        OnBossSpawned = null;
        OnBossHealthChanged = null;
        OnBossDefeated = null;
    }
}
