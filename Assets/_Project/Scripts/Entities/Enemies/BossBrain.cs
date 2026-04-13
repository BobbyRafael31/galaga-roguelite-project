using UnityEngine;
using System.Collections.Generic;

public enum BossFiringMode
{
    Synchronized,
    Randomized
}

public class BossBrain : MonoBehaviour, IAABBEntity
{
    [Header("Physical Configuration")]
    [SerializeField] private float _combatYPosition = 3.5f;
    [SerializeField] private Vector2 _extents = new Vector2(2.0f, 1.5f);

    [Header("Movement (Independent Sway)")]
    [SerializeField] private float _swaySpeed = 1.5f;
    [SerializeField] private float _swayAmplitude = 4.0f;

    [Header("Arsenal & Fire Points")]
    [SerializeField] private BossFiringMode _firingMode = BossFiringMode.Synchronized;
    [SerializeField] private List<Transform> _firePoints = new List<Transform>();
    [SerializeField] private EnemyBullet _bossBulletPrefab;
    [SerializeField] private GameObject _explosionPrefab;

    [Header("Ultimates")]
    public List<BossUltimateData> Ultimates = new List<BossUltimateData>();

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy && !_isDead && !_isEntering;

    private BossEncounterData _data;
    private WaveData _currentWave;

    private float _currentHealth;
    private float _maxCalculatedHealth;
    private bool _isDead = true;
    private bool _isEntering = false;
    private Vector3 _startSwayPosition;
    private float _swayTimer;
    private bool _isExecutingUltimate = false;
    private bool _isSwayEnabled = true;
    public WaveData CurrentWave => _currentWave;

    private float _minBoundsX;
    private float _maxBoundsX;

    private class UltimateTracker
    {
        public BossUltimateData Data;
        public bool IsUnlocked;
        public float NextFireTime;
    }
    private List<UltimateTracker> _ultimateTrackers = new List<UltimateTracker>();

    public void Initialize(BossEncounterData encounterData, WaveData currentWave)
    {
        _data = encounterData;
        _currentWave = currentWave;

        float loopHpMult = LevelDirector.Instance != null ? LevelDirector.Instance.EnemyHealthMultiplier : 1f;
        _maxCalculatedHealth = _data.MaxHealth * loopHpMult;
        _currentHealth = _maxCalculatedHealth;

        _isDead = false;
        _swayTimer = 0f;
        _isSwayEnabled = true;
        _isExecutingUltimate = false;

        _ultimateTrackers.Clear();
        foreach (var ult in Ultimates)
        {
            _ultimateTrackers.Add(new UltimateTracker { Data = ult, IsUnlocked = false, NextFireTime = 0f });
        }

        if (_firePoints.Count == 0) Debug.LogError($"[BossBrain] {_data.BossName} has ZERO FirePoints assigned!");

        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemy(this);
        EventBus.OnBossSpawned?.Invoke(_data.BossName, _currentHealth, _maxCalculatedHealth);

        _ = EntranceAndCombatSequenceAsync();
    }

    private void OnEnable()
    {
        EventBus.OnClearArena += ForceDespawn;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemy(this);
        EventBus.OnClearArena -= ForceDespawn;
    }

    private void ForceDespawn()
    {
        _isDead = true;
        EventBus.OnBossDefeated?.Invoke();
        Destroy(gameObject);
    }

    private void Update()
    {
        if (_isDead || _isEntering || _data == null) return;

        if (_isSwayEnabled)
        {
            _startSwayPosition.x = Mathf.MoveTowards(_startSwayPosition.x, 0f, 1.5f * Time.deltaTime);

            _swayTimer += Time.deltaTime;

            float xOffset = Mathf.Sin(_swayTimer * _swaySpeed) * _swayAmplitude;
            float targetX = _startSwayPosition.x + xOffset;

            targetX = Mathf.Clamp(targetX, ArenaBounds.MinX + Extents.x, ArenaBounds.MaxX - Extents.x);

            transform.position = new Vector3(targetX, _startSwayPosition.y, 0);
        }
    }

    public void SetSwayEnabled(bool isEnabled)
    {
        _isSwayEnabled = isEnabled;
        if (isEnabled)
        {
            _startSwayPosition = transform.position;
            _swayTimer = 0f;
        }
    }

    private async Awaitable EntranceAndCombatSequenceAsync()
    {
        _isEntering = true;

        Vector3 targetPos = new Vector3(0, _combatYPosition, 0);
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (_isDead) return;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 3f * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }

        _startSwayPosition = transform.position;
        _isEntering = false;

        int attackIndex = 0;
        while (!_isDead)
        {
            if (_isExecutingUltimate)
            {
                await Awaitable.NextFrameAsync();
                continue;
            }

            BossUltimateData readyUltimate = GetReadyUltimate();
            if (readyUltimate != null)
            {
                await TriggerUltimateAsync(readyUltimate);
                continue;
            }

            if (_data.AttackPatterns.Count > 0)
            {
                BossAttackData currentAttack = _data.AttackPatterns[attackIndex];
                await ExecuteAttackPatternAsync(currentAttack);

                attackIndex++;
                if (attackIndex >= _data.AttackPatterns.Count) attackIndex = 0;
            }
            else
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }

    private BossUltimateData GetReadyUltimate()
    {
        float hpPercentage = _currentHealth / _maxCalculatedHealth;

        foreach (var tracker in _ultimateTrackers)
        {
            if (!tracker.IsUnlocked && hpPercentage <= tracker.Data.HealthTriggerPercentage)
            {
                tracker.IsUnlocked = true;
                tracker.NextFireTime = Time.time;
            }

            if (tracker.IsUnlocked && Time.time >= tracker.NextFireTime)
            {
                tracker.NextFireTime = tracker.Data.Cooldown > 0 ? Time.time + tracker.Data.Cooldown : float.MaxValue;
                return tracker.Data;
            }
        }
        return null;
    }

    private async Awaitable TriggerUltimateAsync(BossUltimateData ultimate)
    {
        _isExecutingUltimate = true;
        await Awaitable.WaitForSecondsAsync(0.5f);
        await ultimate.ExecuteUltimateAsync(this);
        _isExecutingUltimate = false;
    }

    private async Awaitable ExecuteAttackPatternAsync(BossAttackData attack)
    {
        if (_isDead || _firePoints.Count == 0) return;

        List<Transform> activeGuns = new List<Transform>();
        if (_firingMode == BossFiringMode.Synchronized) activeGuns.AddRange(_firePoints);
        else if (_firingMode == BossFiringMode.Randomized) activeGuns.Add(_firePoints[Random.Range(0, _firePoints.Count)]);

        Vector3 playerPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : transform.position + Vector3.down;

        switch (attack.AttackType)
        {
            case BossAttackType.Targeted:
                foreach (Transform gun in activeGuns) FireBullet(gun.position, (playerPos - gun.position).normalized, attack.BulletSpeed);
                break;
            case BossAttackType.Spread:
                float startAngle = -attack.SpreadAngle / 2f;
                float angleStep = attack.ProjectileCount > 1 ? attack.SpreadAngle / (attack.ProjectileCount - 1) : 0;
                for (int i = 0; i < attack.ProjectileCount; i++)
                {
                    Vector3 spreadDir = Quaternion.Euler(0, 0, startAngle + (angleStep * i)) * Vector3.down;
                    foreach (Transform gun in activeGuns) FireBullet(gun.position, spreadDir, attack.BulletSpeed);
                }
                break;
            case BossAttackType.Burst:
                for (int i = 0; i < attack.ProjectileCount; i++)
                {
                    if (_isDead) return;
                    foreach (Transform gun in activeGuns) FireBullet(gun.position, Vector3.down, attack.BulletSpeed);
                    await Awaitable.WaitForSecondsAsync(attack.BurstDelay);
                }
                break;
        }
        await Awaitable.WaitForSecondsAsync(attack.PostAttackCooldown);
    }

    private void FireBullet(Vector3 spawnPos, Vector3 direction, float speed)
    {
        if (PoolManager.Instance != null && _bossBulletPrefab != null)
        {
            EnemyBullet bullet = PoolManager.Instance.Get(_bossBulletPrefab, spawnPos, Quaternion.identity);
            bullet.SetSpeedAndDirection(speed, direction);
        }
    }
    public void OnCollide(IAABBEntity other)
    {
        if (_isDead || _isEntering) return;
        TakeDamage(1f);
    }

    private void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        EventBus.OnBossHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0 && !_isDead)
        {
            _ = ExecuteDeathSequenceAsync();
        }
    }

    private async Awaitable ExecuteDeathSequenceAsync()
    {
        _isDead = true;
        EventBus.OnBossDefeated?.Invoke();

        int explosionCount = 15;
        for (int i = 0; i < explosionCount; i++)
        {
            if (_explosionPrefab != null)
            {
                float randX = Random.Range(-Extents.x, Extents.x);
                float randY = Random.Range(-Extents.y, Extents.y);
                Instantiate(_explosionPrefab, transform.position + new Vector3(randX, randY, -1f), Quaternion.identity);
            }
            await Awaitable.WaitForSecondsAsync(Random.Range(0.05f, 0.15f));
        }

        float loopScoreMult = LevelDirector.Instance != null ? LevelDirector.Instance.EnemyScoreMultiplier : 1f;
        EventBus.OnEnemyDestroyed?.Invoke(Mathf.FloorToInt(_data.ScoreValue * loopScoreMult));
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(Extents.x * 2, Extents.y * 2, 0));
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-5, _combatYPosition, 0), new Vector3(5, _combatYPosition, 0));
    }
}