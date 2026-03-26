using UnityEngine;

public class PlayerHealth : MonoBehaviour, IAABBEntity
{
    [Header("Health Settings")]
    [SerializeField] private float _baseMaxHealth = 3f;
    [SerializeField] private float _iFrameDuration = 2.0f;
    [SerializeField] private float _respawnDelay = 1.5f;

    [Header("UI Settings")]
    [SerializeField] private Sprite _defaultIconSprite;

    [Header("Collision Bounds")]
    [SerializeField] private Vector2 _extents = new Vector2(0.4f, 0.4f);

    public Stat MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public bool HasKineticPlating = false;

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy && !_isDead && !IsInvincible;
    private bool IsInvincible => Time.time < _lastHitTime + _iFrameDuration;

    private float _lastHitTime = -999f;
    private bool _isDead = false;
    private Vector3 _startPosition;

    private SpriteRenderer _spriteRenderer;
    private PlayerController _controller;
    private PlayerShooter _shooter;


    private void Awake()
    {
        MaxHealth = new Stat(_baseMaxHealth);

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _controller = GetComponentInParent<PlayerController>() ?? GetComponentInChildren<PlayerController>();
        _shooter = GetComponentInParent<PlayerShooter>() ?? GetComponentInChildren<PlayerShooter>();

        if (_defaultIconSprite == null && _spriteRenderer != null)
        {
            _defaultIconSprite = _spriteRenderer.sprite;
        }
    }

    private void OnEnable()
    {
        _isDead = false;
        CurrentHealth = MaxHealth.Value;
        _startPosition = transform.position;

        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        if (_controller != null) _controller.enabled = true;
        if (_shooter != null) _shooter.enabled = true;

        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterPlayer(this);

        EventBus.OnPlayerHealthInitialized?.Invoke(Mathf.FloorToInt(CurrentHealth), Mathf.FloorToInt(MaxHealth.Value), _defaultIconSprite);
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterPlayer(this);
    }

    public void OnCollide(IAABBEntity other)
    {
        if (IsInvincible || _isDead) return;
        TakeDamage(1f);
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth.Value);
        EventBus.OnPlayerHit?.Invoke(Mathf.FloorToInt(CurrentHealth));
    }

    private void TakeDamage(float amount)
    {
        if (HasKineticPlating)
        {
            Debug.Log("[PlayerHealth] Kinetic Plating absorbed the hit!");
            HasKineticPlating = false;
            _lastHitTime = Time.time;
            return;
        }

        CurrentHealth -= amount;

        _ = ExecuteDeathAndRespawnSequence();
        EventBus.OnPlayerHit?.Invoke(Mathf.FloorToInt(CurrentHealth));
    }

    private async Awaitable ExecuteDeathAndRespawnSequence()
    {
        _isDead = true;

        if (_controller != null) _controller.enabled = false;
        if (_shooter != null) _shooter.enabled = false;
        if (_spriteRenderer != null) _spriteRenderer.enabled = false;

        if (CurrentHealth <= 0)
        {
            Debug.Log("[PlayerHealth] Zero lives remaining. GAME OVER.");
            EventBus.OnGameOver?.Invoke();
            return;
        }

        await Awaitable.WaitForSecondsAsync(_respawnDelay);

        transform.position = _startPosition;
        _isDead = false;
        _lastHitTime = Time.time;

        if (_controller != null) _controller.enabled = true;
        if (_shooter != null) _shooter.enabled = true;

        float flickerEndTime = Time.time + _iFrameDuration;
        while (Time.time < flickerEndTime)
        {
            if (this == null || !gameObject.activeInHierarchy) return;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}