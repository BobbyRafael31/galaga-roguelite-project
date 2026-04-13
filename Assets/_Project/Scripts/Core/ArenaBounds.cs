using UnityEngine;

public class ArenaBounds : MonoBehaviour
{
    public static float MinX { get; private set; } = -8f;
    public static float MaxX { get; private set; } = 8f;
    public static float MinY { get; private set; } = -8f;
    public static float MaxY { get; private set; } = 8f;

    [Header("Arcade Cabinet Settings")]
    [SerializeField] private Vector2 _arenaSize = new Vector2(16f, 16f);

    private void Awake() => UpdateBounds();
    private void OnValidate() => UpdateBounds();

    private void UpdateBounds()
    {
        MinX = -_arenaSize.x / 2f;
        MaxX = _arenaSize.x / 2f;
        MinY = -_arenaSize.y / 2f;
        MaxY = _arenaSize.y / 2f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(_arenaSize.x, _arenaSize.y, 0));
    }
}