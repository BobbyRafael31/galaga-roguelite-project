using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    [Header("UI Dependencies")]
    public GameObject MainMenuUI;

    [Header("Gameplay Dependencies")]
    public WaveSpawner WaveSpawner;
    public WaveData DemoWave;
    public GameObject RealPlayerPrefab;
    public GameObject DummyBotPrefab;
    public Transform PlayerSpawnPoint;

    private IGameState _currentState;
    public MainMenuState MenuState { get; private set; }
    public GameplayState PlayState { get; private set; }

    public bool IsTransitioning { get; set; }
    private GameObject _activePlayerInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        MenuState = new MainMenuState(this);
        PlayState = new GameplayState(this);
    }

    private void Start() => ChangeState(MenuState);
    private void Update() => _currentState?.Tick();

    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public void SpawnDummyBot()
    {
        if (_activePlayerInstance != null) Destroy(_activePlayerInstance);
        _activePlayerInstance = Instantiate(DummyBotPrefab, PlayerSpawnPoint.position, Quaternion.identity);
    }

    public void SpawnRealPlayer()
    {
        if (_activePlayerInstance != null) Destroy(_activePlayerInstance);
        _activePlayerInstance = Instantiate(RealPlayerPrefab, PlayerSpawnPoint.position, Quaternion.identity);
    }

    public void OnStartGameClicked()
    {
        if (IsTransitioning) return;
        ChangeState(PlayState);
    }

    public void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}