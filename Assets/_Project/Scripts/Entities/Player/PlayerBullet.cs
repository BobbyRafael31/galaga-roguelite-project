using UnityEngine;

public class PlayerBullet : MonoBehaviour, IAABBEntity
{
    [Header("Setting")]
    [SerializeField] private float _speed = 15f;

    [Tooltip("Half-width and half-height for the AABB hit detection.")]
    [SerializeField] private Vector2 _extents = new Vector2(0.1f, 0.4f);

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy;

    private Camera _mainCamera;
    private float _despawnY;

    private void Awake()
    {
        _mainCamera = Camera.main;
        CalculateTopBound();
    }

    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.RegisterPlayerBullet(this);

        EventBus.OnClearArena += Despawn;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.UnregisterPlayerBullet(this);

        EventBus.OnClearArena -= Despawn;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * (_speed * Time.deltaTime));

        if (transform.position.y > _despawnY)
            Despawn();
    }

    public void OnCollide(IAABBEntity other)
    {
        Despawn();
    }

    private void Despawn()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.Release(this);
        else
            gameObject.SetActive(false);
    }

    private void CalculateTopBound()
    {
        if (_mainCamera != null)
        {
            float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            _despawnY = _mainCamera.ViewportToWorldPoint(new Vector3(0, 1.1f, zDistance)).y;
        }
        else
            _despawnY = 15f; // Fallback hardcode
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}
