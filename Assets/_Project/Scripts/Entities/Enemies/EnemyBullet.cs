using UnityEngine;

public class EnemyBullet : MonoBehaviour, IAABBEntity
{
    [Header("Settings")]
    [Tooltip("Speed is controlled dynamically by the combat director, but this is the fallback")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private Vector2 _extents = new Vector2(0.2f, 0.2f);

    public Vector2 Position => transform.position;
    public Vector2 Extents => _extents;
    public bool IsActive => gameObject.activeInHierarchy;

    private Camera _mainCamera;
    private float _despawnY;

    private void Awake()
    {
        _mainCamera = Camera.main;
        CalculateBottomBound();
    }

    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.RegisterEnemyBullet(this);
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null)
            FastCollisionManager.Instance.UnregisterEnemyBullet(this);
    }

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (_speed * Time.deltaTime));

        if (transform.position.y < _despawnY)
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

    private void CalculateBottomBound()
    {
        if (_mainCamera != null)
        {
            float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            _despawnY = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0.1f, zDistance)).y;
        }
        else
            _despawnY = -15f; //FALLBACK
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}
