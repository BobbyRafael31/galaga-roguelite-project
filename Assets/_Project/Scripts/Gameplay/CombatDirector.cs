using System.Collections.Generic;
using UnityEngine;

public class CombatDirector : MonoBehaviour
{
    public static CombatDirector Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private EnemyBullet _enemyBulletPrefab;

    private CombatSettings _currentSettings;
    private readonly List<EnemyBrain> _activeEnemies = new List<EnemyBrain>(100);
    private bool _isCombatActive = false;

    private float _diveTimer;
    private float _shootTimer;
    private int _activeDivesCount;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnWaveStarted += HandleWaveStarted;
        EventBus.OnStageCleared += HandleStageCleared;
    }

    private void OnDisable()
    {
        EventBus.OnWaveStarted -= HandleWaveStarted;
        EventBus.OnStageCleared -= HandleStageCleared;
    }

    private void HandleWaveStarted(int waveIndex)
    {
        _activeDivesCount = 0;
    }

    private void HandleStageCleared()
    {
        _isCombatActive = false;
    }

    public void SetAggression(CombatSettings settings)
    {
        _currentSettings = settings;
        _isCombatActive = true;
        _diveTimer = settings.DiveTokenCooldown;
        _shootTimer = settings.ShootTokenCooldown;
    }

    public void RegisterEnemy(EnemyBrain enemy) => _activeEnemies.Add(enemy);
    public void UnregisterEnemy(EnemyBrain enemy) => _activeEnemies.Remove(enemy);

    public void ReportDiveCompleted() => _activeDivesCount = Mathf.Max(0, _activeDivesCount - 1);

    private void Update()
    {
        if (!_isCombatActive || _activeEnemies.Count == 0) return;

        HandleDiveRaffle();
        HandleShootRaffle();
    }

    private void HandleDiveRaffle()
    {
        if (_activeDivesCount >= _currentSettings.MaxActiveDives) return;

        _diveTimer -= Time.deltaTime;
        if (_diveTimer <= 0)
        {
            _diveTimer = _currentSettings.DiveTokenCooldown;

            EnemyBrain candidate = GetRandomEnemyInState(EnemyState.Formation);
            if (candidate != null)
            {
                _activeDivesCount++;
                candidate.StartDive();
            }
        }
    }

    private void HandleShootRaffle()
    {
        if (FastCollisionManager.Instance.GetEnemyBulletCount() >= _currentSettings.MaxActiveProjectiles) return;

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            _shootTimer = _currentSettings.ShootTokenCooldown;

            EnemyBrain candidate = GetRandomEnemyInState(null);
            if (candidate != null)
            {
                bool didShoot = candidate.TryShoot(_enemyBulletPrefab, _currentSettings.EnemyProjectileSpeed);

                if (!didShoot) _shootTimer = 0.1f;
            }
        }
    }

    private EnemyBrain GetRandomEnemyInState(EnemyState? requiredState)
    {
        List<EnemyBrain> validCandidates = new List<EnemyBrain>();

        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            if (requiredState == null || _activeEnemies[i].CurrentState == requiredState)
            {
                validCandidates.Add(_activeEnemies[i]);
            }
        }

        if (validCandidates.Count == 0) return null;

        return validCandidates[Random.Range(0, validCandidates.Count)];
    }
}
