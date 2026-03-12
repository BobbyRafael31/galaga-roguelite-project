using UnityEngine;

public enum EnemyState
{
    Entrance,
    Formation,
    Dive,
    WrapAround,
    Return
}

public class EnemyBrain : MonoBehaviour, IAABBEntity
{
    [Header("Pathing & State")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _formationSnapSpeed = 10f;

    [Tooltip("The path the enemy takes when diving (relative to its seat).")]
    public PathData DivePath;

    [Header("Stats")]
    [SerializeField] private float _maxHealth = 1f;
    [SerializeField] private int _scoreValue = 100;
    [SerializeField] private float _spawnInvulnerability = 1.0f;

    [Header("Collision Bounds")]
    [SerializeField] private Vector2 _extents = new Vector2(0.4f, 0.4f);

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy && !_isDead && Time.time > _spawnTime + _spawnInvulnerability;

    public EnemyState CurrentState => _currentState;

    private EnemyState _currentState;
    private int _currentPathIndex;
    private float _currentHealth;
    private bool _isDead;
    private bool _isLockedInFormation;
    private Transform _originalPoolParent;
    private float _spawnTime;

    private int _assignedRow;
    private int _assignedCol;
    private PathData _entrancePath;

    private Vector3 _previousPosition;
    private Vector3 _currentVelocity;

    private void Awake()
    {
        _originalPoolParent = transform.parent;
    }

    private void OnEnable()
    {
        _isDead = false;
        _currentHealth = _maxHealth;
        _currentState = EnemyState.Entrance;
        _currentPathIndex = 0;
        _isLockedInFormation = false;
        _spawnTime = Time.time;
        _entrancePath = null;

        _previousPosition = transform.position;

        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemy(this);

        if (CombatDirector.Instance != null) CombatDirector.Instance.RegisterEnemy(this);
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemy(this);
        if (CombatDirector.Instance != null) CombatDirector.Instance.UnregisterEnemy(this);

        if (_originalPoolParent != null) transform.SetParent(_originalPoolParent);
    }

    public void InitializeFormationSeat(int row, int col, PathData path)
    {
        _assignedRow = row;
        _assignedCol = col;
        _entrancePath = path;
    }

    private void Update()
    {
        _currentVelocity = (transform.position - _previousPosition).normalized;
        _previousPosition = transform.position;

        switch (_currentState)
        {
            case EnemyState.Entrance:
                HandleEntranceState();
                break;
            case EnemyState.Formation:
                HandleFormationState();
                break;
            case EnemyState.Dive:
                HandleDiveState();
                break;
            case EnemyState.WrapAround:
                HandleWrapAroundState();
                break;
            case EnemyState.Return:
                HandleReturnState();
                break;
        }
    }

    private void HandleEntranceState()
    {
        if (_entrancePath == null || _entrancePath.BakedPath == null || _entrancePath.BakedPath.Length == 0)
        {
            _currentState = EnemyState.Formation;
            return;
        }

        Vector3 targetPos = _entrancePath.BakedPath[_currentPathIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);

        if ((transform.position - targetPos).sqrMagnitude < 0.001f)
        {
            _currentPathIndex++;
            if (_currentPathIndex >= _entrancePath.BakedPath.Length)
            {
                _currentState = EnemyState.Formation;
            }
        }
    }

    private void HandleFormationState()
    {
        if (FormationManager.Instance == null) return;

        Vector3 targetSeat = FormationManager.Instance.GetSeatPosition(_assignedRow, _assignedCol);

        if (!_isLockedInFormation)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetSeat, _formationSnapSpeed * Time.deltaTime);
            if ((transform.position - targetSeat).sqrMagnitude < 0.001f) _isLockedInFormation = true;
        }
        else
        {
            transform.position = targetSeat;
        }
    }

    private void HandleDiveState()
    {
        // TO DO
        Debug.Log($"{gameObject.name} is diving");
    }

    private void HandleWrapAroundState()
    {
        // TO DO
    }

    private void HandleReturnState()
    {
        // TO DO
    }

    public void StartDive()
    {
        if (_currentState != EnemyState.Formation) return;

        _currentState = EnemyState.Dive;
        _isLockedInFormation = false;
        _currentPathIndex = 0;

        // The mathematical translation logic for the relative dive will go here
    }

    public bool TryShoot(EnemyBullet bulletPrefab, float bulletSpeed)
    {
        float dotProduct = Vector3.Dot(Vector3.down, _currentVelocity);

        if (dotProduct < 0.5f && _currentState != EnemyState.Formation)
        {
            return false;
        }

        if (PoolManager.Instance != null)
        {
            EnemyBullet bullet = PoolManager.Instance.Get(bulletPrefab, transform.position, Quaternion.identity);
            bullet.SetSpeed(bulletSpeed);
            return true;
        }

        return false;
    }

    public void OnCollide(IAABBEntity other)
    {
        if (_isDead) return;
        TakeDamage(1f);
    }

    private void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        _isDead = true;
        EventBus.OnEnemyDestroyed?.Invoke(_scoreValue);

        if (PoolManager.Instance != null) PoolManager.Instance.Release(this);
        else gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}