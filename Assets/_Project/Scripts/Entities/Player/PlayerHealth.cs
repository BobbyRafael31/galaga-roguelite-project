using UnityEngine;

public class PlayerHealth : MonoBehaviour, IAABBEntity
{
    [Header("Health Settings")]
    [SerializeField] private float _baseMaxHealth = 3f;

    [Tooltip("How long the ship is invicible after taking a hit.")]
    [SerializeField] private float _iFrameDuration = 1.5f;

    [Header("Collison Bounds")]
    [Tooltip("Half-width and half-height for the player's hurtbox. Usually smaller than the sprite for arcade fairness.")]
    [SerializeField] private Vector2 _extents = new Vector2(0.4f, 0.4f);

    [Header("UI Settings")]
    [SerializeField] private Sprite _defaultIconSprite;
    private SpriteRenderer _spriteRenderer;

    public Stat MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;

    private bool _isDead = false;
    private float _lastHitTime = -999f;
    public bool IsActive => gameObject.activeInHierarchy && !IsInvincible & !_isDead;
    private bool IsInvincible => Time.time < _lastHitTime + _iFrameDuration;

    private void Awake()
    {
        MaxHealth = new Stat(_baseMaxHealth);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_defaultIconSprite == null && _spriteRenderer != null)
        {
            _defaultIconSprite = _spriteRenderer.sprite;
        }
    }

    private void OnEnable()
    {
        _isDead = false;
        CurrentHealth = MaxHealth.Value;

        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.RegisterPlayer(this);

        EventBus.OnPlayerHealthInitialized?.Invoke(Mathf.FloorToInt(CurrentHealth),
            Mathf.FloorToInt(MaxHealth.Value), _defaultIconSprite);

    }

    public void OnCollide(IAABBEntity other)
    {
        if (IsInvincible || _isDead) return;
        TakeDamage(1f);
    }

    private void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        _lastHitTime = Time.time;

        if (CurrentHealth <= 0f)
            Die();
        else
        {
            EventBus.OnPlayerHit?.Invoke(Mathf.FloorToInt(CurrentHealth));
            Debug.Log($"[PlayerHealth] Hit! HP remaining: {CurrentHealth}. I-Frames active for {_iFrameDuration}s.");
        }
    }

    private void Die()
    {
        _isDead = true;
        EventBus.OnPlayerDeath?.Invoke();

        Debug.Log("[PlayerHealth] Player Destroyed!");

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}
