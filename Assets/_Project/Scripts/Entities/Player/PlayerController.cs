using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader;

    [Header("Stats")]
    [SerializeField] private float _baseMoveSpeed = 7f;

    [Tooltip("Distance from center to the edge of the ship sprite. Prevents half the ship from leaving the screen.")]
    [SerializeField] private float _shipHalfWidth = 0.5f;

    public Stat MoveSpeed { get; private set; }

    private float _minBoundsX;
    private float _maxBoundsX;
    private Camera _mainCamera;

    private void Awake()
    {
        MoveSpeed = new Stat(_baseMoveSpeed);
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.EnablePlayerInput();
        }
        else
        {
            Debug.LogError("[PlayerController] InputReader is not assigned in the Inspector!");
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.DisablePlayerInput();
        }
    }

    private void Start()
    {
        CalculateScreenBounds();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_inputReader == null) return;

        float directionX = _inputReader.MoveVector.x;

        if (directionX == 0f) return;

        Vector3 currentPos = transform.position;
        float deltaX = directionX * MoveSpeed.Value * Time.deltaTime;
        float targetX = currentPos.x + deltaX;

        targetX = Mathf.Clamp(targetX, _minBoundsX + _shipHalfWidth, _maxBoundsX - _shipHalfWidth);

        transform.position = new Vector3(targetX, currentPos.y, currentPos.z);
    }

    private void CalculateScreenBounds()
    {
        if (_mainCamera == null)
        {
            Debug.LogError("[PlayerController] Main Camera missing. Cannot calculate bounds.");
            return;
        }

        float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);

        Vector3 leftBottom = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 rightTop = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        _minBoundsX = leftBottom.x;
        _maxBoundsX = rightTop.x;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 leftEdge = transform.position - new Vector3(_shipHalfWidth, 0, 0);
        Vector3 rightEdge = transform.position + new Vector3(_shipHalfWidth, 0, 0);
        Gizmos.DrawLine(leftEdge, rightEdge);
        Gizmos.DrawWireSphere(leftEdge, 0.1f);
        Gizmos.DrawWireSphere(rightEdge, 0.1f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 leftClamp = new Vector3(_minBoundsX + _shipHalfWidth, transform.position.y, transform.position.z);
            Vector3 rightClamp = new Vector3(_maxBoundsX - _shipHalfWidth, transform.position.y, transform.position.z);

            Gizmos.DrawLine(leftClamp + Vector3.up * 2, leftClamp + Vector3.down * 2);
            Gizmos.DrawLine(rightClamp + Vector3.up * 2, rightClamp + Vector3.down * 2);
        }
    }
}