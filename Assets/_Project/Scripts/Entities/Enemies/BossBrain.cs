using UnityEngine;
using System.Collections.Generic;

public enum BossFiringMode
{
    Synchronized, // All FirePoints shoot the exact same pattern simultaneously
    Randomized    // A single, random FirePoint is chosen for each attack execution
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

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy && !_isDead && !_isEntering;

    private BossEncounterData _data;
    private float _currentHealth;
    private bool _isDead = true;
    private bool _isEntering = false;
    private Vector3 _startSwayPosition;
    private float _swayTimer;

    public void Initialize(BossEncounterData encounterData)
    {
        _data = encounterData;

        float loopHpMult = LevelDirector.Instance != null ? LevelDirector.Instance.EnemyHealthMultiplier : 1f;
        _currentHealth = _data.MaxHealth * loopHpMult;

        _isDead = false;
        _swayTimer = 0f;

        if (_firePoints.Count == 0)
        {
            Debug.LogError($"[BossBrain] {_data.BossName} has ZERO FirePoints assigned in its Prefab!");
        }

        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemy(this);
        EventBus.OnBossSpawned?.Invoke(_data.BossName, _currentHealth, _currentHealth);

        _ = EntranceAndCombatSequenceAsync();
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemy(this);
    }

    private void Update()
    {
        if (_isDead || _isEntering || _data == null) return;

        _swayTimer += Time.deltaTime;
        float xOffset = Mathf.Sin(_swayTimer * _swaySpeed) * _swayAmplitude;

        transform.position = new Vector3(_startSwayPosition.x + xOffset, _startSwayPosition.y, 0);
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
        while (!_isDead && _data.AttackPatterns.Count > 0)
        {
            BossAttackData currentAttack = _data.AttackPatterns[attackIndex];

            await ExecuteAttackPatternAsync(currentAttack);

            attackIndex++;
            if (attackIndex >= _data.AttackPatterns.Count) attackIndex = 0;
        }
    }

    private async Awaitable ExecuteAttackPatternAsync(BossAttackData attack)
    {
        if (_isDead || _firePoints.Count == 0) return;

        List<Transform> activeGuns = new List<Transform>();
        if (_firingMode == BossFiringMode.Synchronized)
        {
            activeGuns.AddRange(_firePoints);
        }
        else if (_firingMode == BossFiringMode.Randomized)
        {
            activeGuns.Add(_firePoints[Random.Range(0, _firePoints.Count)]);
        }

        Vector3 playerPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : transform.position + Vector3.down;

        switch (attack.AttackType)
        {
            case BossAttackType.Targeted:
                foreach (Transform gun in activeGuns)
                {
                    Vector3 dirToPlayer = (playerPos - gun.position).normalized;
                    FireBullet(gun.position, dirToPlayer, attack.BulletSpeed);
                }
                break;

            case BossAttackType.Spread:
                float startAngle = -attack.SpreadAngle / 2f;
                float angleStep = attack.ProjectileCount > 1 ? attack.SpreadAngle / (attack.ProjectileCount - 1) : 0;

                for (int i = 0; i < attack.ProjectileCount; i++)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    Vector3 spreadDir = Quaternion.Euler(0, 0, currentAngle) * Vector3.down;

                    foreach (Transform gun in activeGuns)
                    {
                        FireBullet(gun.position, spreadDir, attack.BulletSpeed);
                    }
                }
                break;

            case BossAttackType.Burst:
                for (int i = 0; i < attack.ProjectileCount; i++)
                {
                    if (_isDead) return;

                    foreach (Transform gun in activeGuns)
                    {
                        FireBullet(gun.position, Vector3.down, attack.BulletSpeed);
                    }
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

        if (_currentHealth <= 0)
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
                Vector3 explosionPos = transform.position + new Vector3(randX, randY, -1f);

                Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);
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