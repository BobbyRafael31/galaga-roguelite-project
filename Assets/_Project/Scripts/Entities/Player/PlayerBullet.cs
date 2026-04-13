using UnityEngine;

public class PlayerBullet : MonoBehaviour, IAABBEntity
{
    [Header("Settings")]
    [SerializeField] private float _speed = 15f;
    [SerializeField] private Vector2 _extents = new Vector2(0.1f, 0.4f);

    public Vector2 Position => transform.position;
    public Vector2 Extents => IsHeavy ? _extents * 2.5f : _extents;
    public bool IsActive => gameObject.activeInHierarchy;
    public bool IsHeavy { get; private set; }

    private Vector3 _originalScale;

    private int _maxPierces = 0;
    private int _currentPiercesLeft = 0;

    private float _despawnY;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterPlayerBullet(this);
        EventBus.OnClearArena += Despawn;

        _currentPiercesLeft = _maxPierces;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterPlayerBullet(this);
        EventBus.OnClearArena -= Despawn;
    }

    public void SetPiercingLevel(int pierceCount)
    {
        _maxPierces = pierceCount;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * (_speed * Time.deltaTime));
        if (transform.position.y > ArenaBounds.MaxY + 1f) Despawn();
    }

    public void OnCollide(IAABBEntity other)
    {
        if (_currentPiercesLeft > 0)
        {
            _currentPiercesLeft--;
        }
        else
        {
            Despawn();
        }
    }
    public void SetHeavyOrdinance(bool isHeavy)
    {
        IsHeavy = isHeavy;
        transform.localScale = isHeavy ? _originalScale * 2.5f : _originalScale;
    }

    private void Despawn()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.Release(this);
        else gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}