using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private StageData _currentStage;
    [Tooltip("If true, starts the stage automatically for testing.")]
    [SerializeField] private bool _startOnAwake = false;

    [Header("Dependencies")]
    [Tooltip("A fallback spawn point far off-screen.")]
    [SerializeField] private Vector3 _offScreenSpawnPoint = new Vector3(0, 15, 0);

    private int _activeEnemiesInWave = 0;
    private int _activeSpawners = 0;
    private bool _isStageRunning = false;

    private void OnEnable()
    {
        EventBus.OnEnemyDestroyed += HandleEnemyDestroyed;

        if (!_startOnAwake) EventBus.OnGameStarted += StartStageSequence;
    }

    private void OnDisable()
    {
        EventBus.OnEnemyDestroyed -= HandleEnemyDestroyed;
        EventBus.OnGameStarted -= StartStageSequence;
    }

    private void Start()
    {
        if (_startOnAwake) StartStageSequence();
    }

    private void HandleEnemyDestroyed(int scoreValue)
    {
        _activeEnemiesInWave--;
    }

    public void StopAndReset()
    {
        _isStageRunning = false;
        _activeEnemiesInWave = 0;
        _activeSpawners = 0;
    }

    public async Awaitable SpawnSingleWaveAsync(WaveData wave)
    {
        _isStageRunning = true;
        _activeEnemiesInWave = 0;
        _activeSpawners = 0;

        if (FormationManager.Instance != null && wave.Formation != null) FormationManager.Instance.SetFormation(wave.Formation);
        if (CombatDirector.Instance != null) CombatDirector.Instance.SetAggression(wave.Aggression);

        foreach (BatchData batch in wave.Batches)
        {
            _activeSpawners++;
            _ = SpawnBatchAsync(batch);
        }

        while ((_activeSpawners > 0 || _activeEnemiesInWave > 0) && _isStageRunning)
        {
            await Awaitable.NextFrameAsync();
        }

        if (_isStageRunning) await Awaitable.WaitForSecondsAsync(1.5f);
    }

    private async Awaitable SpawnBatchAsync(BatchData batch)
    {
        if (batch.WaveStartTime > 0) await Awaitable.WaitForSecondsAsync(batch.WaveStartTime);

        if (!_isStageRunning) { _activeSpawners--; return; }

        Vector3 spawnPosition = _offScreenSpawnPoint;
        if (batch.EntrancePath != null && batch.EntrancePath.BakedPath != null && batch.EntrancePath.BakedPath.Length > 0)
            spawnPosition = (Vector3)batch.EntrancePath.BakedPath[0];

        for (int i = 0; i < batch.TargetSeats.Count; i++)
        {
            if (!_isStageRunning) break;

            EnemyBrain enemy = PoolManager.Instance.Get(batch.EnemyPrefab, spawnPosition, Quaternion.identity);
            enemy.InitializeFormationSeat(batch.TargetSeats[i].y, batch.TargetSeats[i].x, batch.EntrancePath);
            _activeEnemiesInWave++;

            if (i < batch.TargetSeats.Count - 1 && batch.SpawnDelay > 0)
                await Awaitable.WaitForSecondsAsync(batch.SpawnDelay);
        }

        _activeSpawners = Mathf.Max(0, _activeSpawners - 1);
    }

    public async void StartStageSequence()
    {
        if (_isStageRunning || _currentStage == null) return;
        _isStageRunning = true;

        Debug.Log($"[WaveSpawner] Stage Started: {_currentStage.Waves.Count} Waves.");

        for (int w = 0; w < _currentStage.Waves.Count; w++)
        {
            WaveData wave = _currentStage.Waves[w];
            await SpawnWaveAsync(wave, w);
        }

        Debug.Log("[WaveSpawner] Stage Cleared!");
        EventBus.OnStageCleared?.Invoke();
        _isStageRunning = false;
    }

    private async Awaitable SpawnWaveAsync(WaveData wave, int waveIndex)
    {
        EventBus.OnWaveStarted?.Invoke(waveIndex);

        if (FormationManager.Instance != null && wave.Formation != null)
        {
            FormationManager.Instance.SetFormation(wave.Formation);
        }

        if (CombatDirector.Instance != null)
        {
            CombatDirector.Instance.SetAggression(wave.Aggression);
        }


        _activeEnemiesInWave = 0;
        _activeSpawners = 0;

        foreach (BatchData batch in wave.Batches)
        {
            _activeSpawners++;


            _ = SpawnBatchAsync(batch);
        }

        while (_activeSpawners > 0 || _activeEnemiesInWave > 0)
        {
            await Awaitable.NextFrameAsync();
        }

        EventBus.OnWaveCompleted?.Invoke(waveIndex);
        await Awaitable.WaitForSecondsAsync(1.5f);
    }

}