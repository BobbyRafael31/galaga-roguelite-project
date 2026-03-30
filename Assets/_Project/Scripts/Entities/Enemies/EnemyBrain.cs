using UnityEngine;

public enum EnemyState
{
    Entrance,
    Formation,
    Dive,
    WrapAround,
    Return
}

public enum DivePhase
{
    Breakaway,
    Swerving,
    Looping,
    Homing,
    Hardlock
}

public class EnemyBrain : MonoBehaviour, IAABBEntity
{
    private float ActiveMoveSpeed => _moveSpeed * (CombatDirector.Instance != null ? CombatDirector.Instance.GlobalEnemySpeedMultiplier : 1f);
    private float ActiveSnapSpeed => _formationSnapSpeed * (CombatDirector.Instance != null ? CombatDirector.Instance.GlobalEnemySpeedMultiplier : 1f);

    [Header("Pathing & State")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _formationSnapSpeed = 15f;
    [SerializeField] private float _homingTurnSpeed = 4f;

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
    private Camera _mainCamera;
    private float _cameraZDistance;

    private DivePhase _currentDivePhase;
    private float _divePhaseTimer;
    private float _accumulatedLoopAngle;
    private int _targetLoops;
    private float _diveTurnDirection;
    private Vector3 _diveVelocity;

    private float _swerveTime;
    private float _swerveAmp;
    private float _swerveFreq;
    private Vector3 _loopCurrentDir;
    private float _currentLoopTurnSpeed;

    private void Awake()
    {
        _originalPoolParent = transform.parent;
        _mainCamera = Camera.main;
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
        transform.rotation = Quaternion.identity;

        float loopHpMult = LevelDirector.Instance != null ? LevelDirector.Instance.EnemyHealthMultiplier : 1f;
        _currentHealth = _maxHealth * loopHpMult;

        if (_mainCamera != null) _cameraZDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemy(this);
        if (CombatDirector.Instance != null) CombatDirector.Instance.RegisterEnemy(this);

        EventBus.OnClearArena += Die;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemy(this);
        if (CombatDirector.Instance != null) CombatDirector.Instance.UnregisterEnemy(this);
        if (_originalPoolParent != null) transform.SetParent(_originalPoolParent);

        EventBus.OnClearArena -= Die;
    }

    public void InitializeFormationSeat(int row, int col, PathData path)
    {
        _assignedRow = row;
        _assignedCol = col;
        _entrancePath = path;
    }

    private void Update()
    {
        Vector3 movementDelta = transform.position - _previousPosition;
        if (movementDelta.sqrMagnitude > 0.00001f)
        {
            _currentVelocity = movementDelta.normalized;
        }
        _previousPosition = transform.position;

        if (_currentState == EnemyState.Formation && _isLockedInFormation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, 360f * Time.deltaTime);
        }
        else if (movementDelta.sqrMagnitude > 0.00001f)
        {
            float angle = Mathf.Atan2(_currentVelocity.y, _currentVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        switch (_currentState)
        {
            case EnemyState.Entrance: HandleEntranceState(); break;
            case EnemyState.Formation: HandleFormationState(); break;
            case EnemyState.Dive: HandleProceduralDive(); break;
            case EnemyState.WrapAround: HandleWrapAroundState(); break;
            case EnemyState.Return: HandleReturnState(); break;
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
        transform.position = Vector3.MoveTowards(transform.position, targetPos, ActiveMoveSpeed * Time.deltaTime);

        if ((transform.position - targetPos).sqrMagnitude < 0.001f)
        {
            _currentPathIndex++;
            if (_currentPathIndex >= _entrancePath.BakedPath.Length) _currentState = EnemyState.Formation;
        }
    }

    private void HandleFormationState()
    {
        if (FormationManager.Instance == null) return;

        Vector3 targetSeat = FormationManager.Instance.GetSeatPosition(_assignedRow, _assignedCol);

        if (!_isLockedInFormation)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetSeat, ActiveSnapSpeed * Time.deltaTime);
            if ((transform.position - targetSeat).sqrMagnitude < 0.001f) _isLockedInFormation = true;
        }
        else
            transform.position = targetSeat;

    }


    public void StartDive()
    {
        bool isFromEntrance = (_currentState == EnemyState.Entrance);
        if (_currentState != EnemyState.Formation && !isFromEntrance) return;

        _currentState = EnemyState.Dive;
        _isLockedInFormation = false;

        _targetLoops = Random.Range(0, 3);

        _swerveTime = 0f;
        _swerveAmp = Random.Range(30f, 60f);
        _swerveFreq = Random.Range(2f, 3.5f);
        _divePhaseTimer = 0f;

        _currentLoopTurnSpeed = Random.Range(250f, 600f);

        _diveTurnDirection = transform.position.x > 0 ? 1f : -1f;

        if (isFromEntrance)
        {
            _diveVelocity = _currentVelocity.normalized * ActiveMoveSpeed;
            _currentDivePhase = DivePhase.Swerving;
        }
        else
        {
            _currentDivePhase = DivePhase.Breakaway;
            _diveVelocity = new Vector3(_diveTurnDirection * 0.8f, 1f, 0).normalized * ActiveMoveSpeed;
        }
    }

    private void HandleProceduralDive()
    {
        float dt = Time.deltaTime;
        Vector3 playerPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : transform.position + Vector3.down * 10f;
        float distToPlayerY = transform.position.y - playerPos.y;

        switch (_currentDivePhase)
        {
            case DivePhase.Breakaway:
                _divePhaseTimer += dt;

                _diveVelocity = Quaternion.Euler(0, 0, -180f * _diveTurnDirection * dt) * _diveVelocity;

                if (_divePhaseTimer > 0.4f)
                {
                    _currentDivePhase = DivePhase.Swerving;
                    _divePhaseTimer = 0f;
                }
                break;

            case DivePhase.Swerving:
                _divePhaseTimer += dt;
                _swerveTime += dt;

                Vector3 baseDir = (playerPos - transform.position).normalized;
                if (baseDir.y > -0.2f) baseDir = new Vector3(baseDir.x, -1f, 0).normalized;


                float sOffset = Mathf.Cos(_swerveTime * _swerveFreq) * _swerveAmp * _diveTurnDirection;
                Vector3 targetSwerveDir = Quaternion.Euler(0, 0, sOffset) * baseDir;

                _diveVelocity = Vector3.RotateTowards(_diveVelocity.normalized, targetSwerveDir, _homingTurnSpeed * 1.5f * dt, 0f) * ActiveMoveSpeed;

                if (_targetLoops > 0 && _divePhaseTimer > Random.Range(0.4f, 0.7f))
                {
                    _currentDivePhase = DivePhase.Looping;
                    _accumulatedLoopAngle = 0f;
                    _loopCurrentDir = _diveVelocity.normalized;
                }
                else if (distToPlayerY < 4.5f)
                {
                    _currentDivePhase = DivePhase.Homing;
                }
                break;

           case DivePhase.Looping:
                float turnStep = _currentLoopTurnSpeed * dt;

                _loopCurrentDir = Quaternion.Euler(0, 0, turnStep * _diveTurnDirection) * _loopCurrentDir;

                _diveVelocity = (_loopCurrentDir + (Vector3.down * 0.2f)).normalized * ActiveMoveSpeed;

                _accumulatedLoopAngle += turnStep;
                if (_accumulatedLoopAngle >= 360f)
                {
                    _targetLoops--;
                    _currentDivePhase = DivePhase.Swerving;
                    _divePhaseTimer = 0f;
                }
                break;

            case DivePhase.Homing:
                Vector3 homingDir = (playerPos - transform.position).normalized;
                if (homingDir.y > -0.2f) homingDir = new Vector3(homingDir.x, -1f, 0).normalized;

                _diveVelocity = Vector3.RotateTowards(_diveVelocity.normalized, homingDir, _homingTurnSpeed * dt, 0f) * ActiveMoveSpeed;

                if (distToPlayerY < 2.5f) _currentDivePhase = DivePhase.Hardlock;
                break;

            case DivePhase.Hardlock:
                // Blind straight-line vector
                break;
        }

        transform.position += _diveVelocity * dt;
        CheckOffScreen();
    }
    private void CheckOffScreen()
    {
        if (_mainCamera != null)
        {
            float bottomY = _mainCamera.ViewportToWorldPoint(new Vector3(0, -0.1f, _cameraZDistance)).y;
            if (transform.position.y < bottomY) _currentState = EnemyState.WrapAround;
        }
    }

    private void HandleWrapAroundState()
    {
        if (_mainCamera != null)
        {
            float topY = _mainCamera.ViewportToWorldPoint(new Vector3(0, 1.1f, _cameraZDistance)).y;
            transform.position = new Vector3(transform.position.x, topY, transform.position.z);
        }
        _currentState = EnemyState.Return;
    }

    private void HandleReturnState()
    {
        if (FormationManager.Instance == null) return;

        Vector3 targetSeat = FormationManager.Instance.GetSeatPosition(_assignedRow, _assignedCol);
        transform.position = Vector3.MoveTowards(transform.position, targetSeat, ActiveSnapSpeed * Time.deltaTime);

        if ((transform.position - targetSeat).sqrMagnitude < 0.001f)
        {
            _isLockedInFormation = true;
            _currentState = EnemyState.Formation;
            if (CombatDirector.Instance != null) CombatDirector.Instance.ReportDiveCompleted();
        }
    }

    public bool TryShoot(EnemyBullet bulletPrefab, float bulletSpeed)
    {
        float dotProduct = Vector3.Dot(Vector3.down, _currentVelocity);
        if (dotProduct < 0.5f && _currentState != EnemyState.Formation) return false;

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
        if (_currentState == EnemyState.Dive && CombatDirector.Instance != null) CombatDirector.Instance.ReportDiveCompleted();

        // Apply Infinite Loop Multiplier to Score
        float loopScoreMult = LevelDirector.Instance != null ? LevelDirector.Instance.EnemyScoreMultiplier : 1f;
        int finalScore = Mathf.FloorToInt(_scoreValue * loopScoreMult);

        EventBus.OnEnemyDestroyed?.Invoke(finalScore);

        if (PoolManager.Instance != null) PoolManager.Instance.Release(this);
        else gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}