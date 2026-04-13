using UnityEngine;

public enum DebuffType
{
    TemporalJamming, // -50% Move Speed
    EMPOverload      // Disables Weapons
}

public class DebuffProjectile : MonoBehaviour, IAABBEntity
{
    [Header("Payload Type")]
    public DebuffType Type = DebuffType.TemporalJamming;

    [Header("Settings")]
    public float Speed = 8f;
    public Vector2 HitboxExtents = new Vector2(5.0f, 0.5f);

    [Header("Debuff Values")]
    public float SpeedPenalty = -0.5f;
    public float Duration = 3.0f;

    public Vector2 Position => transform.position;
    public Vector2 Extents => HitboxExtents;
    public bool IsActive => gameObject.activeInHierarchy;


    private void OnEnable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.RegisterEnemyBullet(this);

        EventBus.OnClearArena += Despawn;
    }

    private void OnDisable()
    {
        if (FastCollisionManager.Instance != null) FastCollisionManager.Instance.UnregisterEnemyBullet(this);

        EventBus.OnClearArena -= Despawn;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (Speed * Time.deltaTime), Space.World);
        if (transform.position.y < ArenaBounds.MinY - 1f) Despawn();
    }

    public void OnCollide(IAABBEntity other)
    {
        if (other is PlayerHealth)
        {
            if (Type == DebuffType.TemporalJamming && PlayerController.Instance != null)
            {
                PlayerController.Instance.ApplyTemporalJamming(SpeedPenalty, Duration);
            }
            else if (Type == DebuffType.EMPOverload && PlayerShooter.Instance != null)
            {
                PlayerShooter.Instance.ApplyEMP(Duration);
            }
        }

        Despawn();
    }

    private void Despawn()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.Release(this);
        else gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Type == DebuffType.TemporalJamming ? Color.cyan : Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(Extents.x * 2, Extents.y * 2, 0));
    }
}