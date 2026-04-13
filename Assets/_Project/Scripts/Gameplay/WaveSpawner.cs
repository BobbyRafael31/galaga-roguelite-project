using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private Vector3 _offScreenSpawnPoint = new Vector3(0, 15, 0);

    private int _activeEnemiesInWave = 0;
    private int _activeSpawners = 0;
    private bool _isStageRunning = false;

    private int _currentSequenceID = 0;
    public HashSet<BatchData> FullySpawnedBatches { get; private set; } = new HashSet<BatchData>();
    public HashSet<Vector2Int> SpawnedSeats { get; private set; } = new HashSet<Vector2Int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        EventBus.OnEnemyDestroyed -= HandleEnemyDestroyed;
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
        FullySpawnedBatches.Clear();
        SpawnedSeats.Clear();

        _currentSequenceID++;
    }

    public async void StartStageSequence(StageData stageToPlay)
    {
        if (_isStageRunning || stageToPlay == null) return;
        _isStageRunning = true;
        _currentSequenceID++;
        int mySeq = _currentSequenceID;

        Debug.Log($"[WaveSpawner] Stage Started: {stageToPlay.Waves.Count} Waves.");

        for (int w = 0; w < stageToPlay.Waves.Count; w++)
        {
            await SpawnWaveAsync(stageToPlay.Waves[w], w);

            if (mySeq != _currentSequenceID || !_isStageRunning) return;
        }

        if (_isStageRunning)
        {
            Debug.Log("[WaveSpawner] Stage Cleared!");
            EventBus.OnStageCleared?.Invoke();
            _isStageRunning = false;
        }
    }

    public async Awaitable SpawnSingleWaveAsync(WaveData wave)
    {
        _isStageRunning = true;
        _currentSequenceID++;
        int mySeq = _currentSequenceID;

        _activeEnemiesInWave = 0;
        _activeSpawners = 0;
        FullySpawnedBatches.Clear();
        SpawnedSeats.Clear();

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
            if (mySeq != _currentSequenceID) return;
        }

        if (_isStageRunning) await Awaitable.WaitForSecondsAsync(1.5f);
    }

    private async Awaitable SpawnWaveAsync(WaveData wave, int waveIndex)
    {
        int mySeq = _currentSequenceID;

        EventBus.OnWaveStarted?.Invoke(waveIndex);

        if (FormationManager.Instance != null && wave.Formation != null) FormationManager.Instance.SetFormation(wave.Formation);
        if (CombatDirector.Instance != null) CombatDirector.Instance.SetAggression(wave.Aggression);

        _activeEnemiesInWave = 0;
        _activeSpawners = 0;
        FullySpawnedBatches.Clear();
        SpawnedSeats.Clear();

        if (wave.BossEncounter != null && wave.BossEncounter.BossPrefab != null)
        {
            Vector3 bossSpawnPos = new Vector3(0, 15, 0);
            BossBrain boss = Instantiate(wave.BossEncounter.BossPrefab, bossSpawnPos, Quaternion.identity);
            boss.Initialize(wave.BossEncounter, wave);
            _activeEnemiesInWave++;
        }

        foreach (BatchData batch in wave.Batches)
        {
            _activeSpawners++;
            _ = SpawnBatchAsync(batch);
        }

        while ((_activeSpawners > 0 || _activeEnemiesInWave > 0) && _isStageRunning)
        {
            await Awaitable.NextFrameAsync();
            if (mySeq != _currentSequenceID) return;
        }

        if (!_isStageRunning) return;

        EventBus.OnWaveCompleted?.Invoke(waveIndex);
        await Awaitable.WaitForSecondsAsync(1.5f);
    }

    public async Awaitable SpawnBatchAsync(BatchData batch, bool isHollow = false)
    {
        int mySeq = _currentSequenceID;

        if (batch.WaveStartTime > 0)
        {
            await Awaitable.WaitForSecondsAsync(batch.WaveStartTime);
            if (mySeq != _currentSequenceID || !_isStageRunning) return;
        }

        Vector3 spawnPosition = _offScreenSpawnPoint;
        if (batch.EntrancePath != null && batch.EntrancePath.BakedPath != null && batch.EntrancePath.BakedPath.Length > 0)
        {
            spawnPosition = (Vector3)batch.EntrancePath.BakedPath[0];
        }

        for (int i = 0; i < batch.TargetSeats.Count; i++)
        {
            if (mySeq != _currentSequenceID || !_isStageRunning) return;

            EnemyBrain enemy = PoolManager.Instance.Get(batch.EnemyPrefab, spawnPosition, Quaternion.identity);

            if (isHollow) enemy.SetHollow(true);

            enemy.InitializeFormationSeat(batch.TargetSeats[i].y, batch.TargetSeats[i].x, batch.EntrancePath);
            SpawnedSeats.Add(batch.TargetSeats[i]);
            _activeEnemiesInWave++;

            if (i < batch.TargetSeats.Count - 1 && batch.SpawnDelay > 0)
            {
                await Awaitable.WaitForSecondsAsync(batch.SpawnDelay);
            }
        }

        if (mySeq != _currentSequenceID) return;

        if (!isHollow) FullySpawnedBatches.Add(batch);

        _activeSpawners = Mathf.Max(0, _activeSpawners - 1);
    }

    public void SpawnGhostForSeat(BatchData batch, int seatIndex)
    {
        if (!_isStageRunning) return;

        Vector2Int seat = batch.TargetSeats[seatIndex];
        Vector3 spawnPosition = _offScreenSpawnPoint;
        if (batch.EntrancePath != null && batch.EntrancePath.BakedPath != null && batch.EntrancePath.BakedPath.Length > 0)
        {
            spawnPosition = (Vector3)batch.EntrancePath.BakedPath[0];
        }

        EnemyBrain enemy = PoolManager.Instance.Get(batch.EnemyPrefab, spawnPosition, Quaternion.identity);
        enemy.SetHollow(true);
        enemy.InitializeFormationSeat(seat.y, seat.x, batch.EntrancePath);
        _activeEnemiesInWave++;
    }
}