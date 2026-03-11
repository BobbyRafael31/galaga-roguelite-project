using UnityEngine;

public enum EnemyState
{
    Entrance,
    Formation,
    Dive
}

public class EnemyBrain : MonoBehaviour, IAABBEntity
{
    [Header("Pathing & State")]
    [SerializeField] private PathData _entrancePath;
    [SerializeField] private float _moveSpeed = 10f;

    [Header("Formation Assignment")]
    [Tooltip("Which row this enemy will lock into (0 is the bottom)")]
    [SerializeField] private int _assignedRow = 0;

    [Tooltip("Which column this enemy will lock into (0 is the bottom)")]
    [SerializeField] private int _assignedCol = 0;

    [Tooltip("How fast the enemy flies from the end of its path to its seat")]
    [SerializeField] private float _formationSnapSpeed = 10f;


    [Header("Stats")]
    [SerializeField] private float _maxHealth = 1f;
    [SerializeField] private int _scoreValue = 100;

    [Tooltip("Invulnerability duration upon spawning to prevent cheap corner kills.")]
    [SerializeField] private float _spawnVulnerability = 1.0f;

    [Header("Collision Bounds")]
    [SerializeField] private Vector2 _extents = new Vector2(0.4f, 0.4f);

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy && !_isDead && Time.time > _spawnTime +
        _spawnVulnerability;

    private EnemyState _currentState;
    private int _currentPathIndex;
    private float _currentHealth;
    private bool _isDead;
    private bool _isLockedInFormation;
    private Transform _originalPoolParent;
    private float _spawnTime;

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

        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.RegisterEnemy(this);

        if (_entrancePath != null && _entrancePath.BakedPath != null && _entrancePath.BakedPath.Length > 0)
            transform.position = _entrancePath.BakedPath[0];
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.UnregisterEnemy(this);
    }

    public void InitializeFormationSeat(int row, int col, PathData path)
    {
        _assignedRow = row;
        _assignedCol = col;
        _entrancePath = path;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case EnemyState.Entrance:
                HandleEntranceState();
                break;
            case EnemyState.Formation:
                HandleFormationState();
                break;
            case EnemyState.Dive:
                // TO DO: Add dive state where enemy can start to attack out from formation
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

        if((transform.position - targetPos).sqrMagnitude < 0.001f)
        {
            _currentPathIndex++;

            if(_currentPathIndex >= _entrancePath.BakedPath.Length)
            {
                _currentState = EnemyState.Formation;
                Debug.Log($"[{gameObject.name}] Path finished. Entering Formation State.");
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

            if ((transform.position - targetSeat).sqrMagnitude < 0.001f)
                _isLockedInFormation = true;
        }
        else
            transform.position = targetSeat;
    }

    public void OnCollide(IAABBEntity other)
    {
        if (_isDead) return;

        TakeDamage(1f);
    }

    private void TakeDamage(float amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _isDead = true;

        EventBus.OnEnemyDestroyed?.Invoke(_scoreValue);

        if (PoolManager.Instance != null)
            PoolManager.Instance.Release(this);
        else
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));

        if (_entrancePath != null && _entrancePath.BakedPath != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _entrancePath.BakedPath.Length - 1; i++)
            {
                Gizmos.DrawLine(_entrancePath.BakedPath[i], _entrancePath.BakedPath[i + 1]);
            }
        }
    }
}
