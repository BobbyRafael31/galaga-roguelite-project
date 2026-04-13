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

    private Vector3 _flightDirection = Vector3.down;

    public bool IsIndestructible = false;
    public bool IsBossProjectile = false;

    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemyBullet(this);

        EventBus.OnClearArena += Despawn;
        EventBus.OnBossDefeated += HandleBossDefeated;

        _flightDirection = Vector3.down;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemyBullet(this);

        EventBus.OnClearArena -= Despawn;
        EventBus.OnBossDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated()
    {
        if (IsBossProjectile)
        {
            Despawn();
        }
    }

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
    public void SetSpeedAndDirection(float newSpeed, Vector3 direction)
    {
        _speed = newSpeed;
        _flightDirection = direction.normalized;
    }

    private void Update()
    {
        transform.Translate(_flightDirection * (_speed * Time.deltaTime), Space.World);

        if (transform.position.y < ArenaBounds.MinY - 1f) Despawn();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(_extents.x * 2, _extents.y * 2, 0));
    }
}
