using System;

public static class EventBus
{
    // Gameplay Events
    public static Action<int> OnEnemyDestroyed;
    public static Action<int, int, UnityEngine.Sprite> OnPlayerHealthInitialized;
    public static Action<int> OnPlayerHit;
    public static Action OnPlayerDeath;

    // Progression Events
    public static Action OnGameStarted;
    public static Action<int> OnWaveStarted;
    public static Action<int> OnWaveCompleted;
    public static Action OnStageCleared;
    public static Action OnClearArena;

    // UI or Economy Events
    public static Action<int> OnScoreChanged;

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
    }
}
