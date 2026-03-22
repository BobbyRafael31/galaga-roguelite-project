using System.Collections.Generic;
using UnityEngine;

public class DummyBot : MonoBehaviour, IAABBEntity
{
    [Header("Bot Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _fireRate = 0.15f; [Tooltip("Maximum bullets the bot can have on screen, mirroring the player.")]
    [SerializeField] private int _maxBulletsOnScreen = 2; [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private Transform _firePoint; [Header("AI Heuristics")]
    [SerializeField] private float _dangerZoneY = 4f;
    [SerializeField] private float _dangerZoneX = 1.2f;

    public Vector2 Position => transform.position;
    public Vector2 Extents => new Vector2(0.4f, 0.4f);
    public bool IsActive => gameObject.activeInHierarchy;

    private float _nextFireTime;
    private Camera _mainCamera;
    private float _minBoundsX;
    private float _maxBoundsX;

    private readonly List<PlayerBullet> _activeBullets = new List<PlayerBullet>(10);

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterPlayer(this);
        EventBus.OnClearArena += Despawn;

        CalculateScreenBounds();
        _activeBullets.Clear();
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterPlayer(this);
        EventBus.OnClearArena -= Despawn;
    }

    private void Update()
    {
        CleanActiveBulletList();

        float targetX = CalculateAITargetX();

        float newX = Mathf.MoveTowards(transform.position.x, targetX, _moveSpeed * Time.deltaTime);
        newX = Mathf.Clamp(newX, _minBoundsX + 0.5f, _maxBoundsX - 0.5f);
        transform.position = new Vector3(newX, transform.position.y, 0);

        if (Time.time >= _nextFireTime && _activeBullets.Count < _maxBulletsOnScreen)
        {
            _nextFireTime = Time.time + _fireRate;
            if (PoolManager.Instance != null)
            {
                PlayerBullet newBullet = PoolManager.Instance.Get(_bulletPrefab, _firePoint.position, Quaternion.identity);
                _activeBullets.Add(newBullet);
            }
        }
    }

    private void CleanActiveBulletList()
    {
        for (int i = _activeBullets.Count - 1; i >= 0; i--)
        {
            if (!_activeBullets[i].gameObject.activeInHierarchy)
            {
                _activeBullets.RemoveAt(i);
            }
        }
    }

    private float CalculateAITargetX()
    {
        if (FastCollisionManager.Instance == null) return transform.position.x;

        var bullets = FastCollisionManager.Instance.EnemyBullets;
        for (int i = 0; i < bullets.Count; i++)
        {
            if (!bullets[i].IsActive) continue;

            float deltaY = bullets[i].Position.y - transform.position.y;
            float deltaX = bullets[i].Position.x - transform.position.x;

            if (deltaY > 0 && deltaY < _dangerZoneY && Mathf.Abs(deltaX) < _dangerZoneX)
            {
                float fleeDirection = deltaX > 0 ? -1f : 1f;
                return transform.position.x + (fleeDirection * 3f);
            }
        }

        var enemies = FastCollisionManager.Instance.Enemies;
        float lowestY = float.MaxValue;
        bool foundTarget = false;
        float optimalTargetX = 0f;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].IsActive) continue;

            if (enemies[i].Position.y < lowestY)
            {
                lowestY = enemies[i].Position.y;
                optimalTargetX = enemies[i].Position.x;
                foundTarget = true;
            }
        }

        if (foundTarget) return optimalTargetX;

        return 0f;
    }

    private void CalculateScreenBounds()
    {
        if (_mainCamera == null) return;
        float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        _minBoundsX = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance)).x;
        _maxBoundsX = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, zDistance)).x;
    }

    public void OnCollide(IAABBEntity other)
    {
        EventBus.OnPlayerDeath?.Invoke();
        Despawn();
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}