using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _hudContainer;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _stageText;

    [Header("Animation Settings")]
    [SerializeField] private float _baseRollSpeed = 1000f;
    [SerializeField] private float _catchupMultiplier = 5f;

    private int _targetScore;
    private float _currentDisplayScore;

    private string _currentWorldStage = "1-1";
    private int _currentWave = 1;

    private void OnEnable()
    {
        EventBus.OnScoreChanged += HandleScoreChanged;
        EventBus.OnGameStarted += ShowHUD;
        EventBus.OnMainMenuEntered += HideHUD;

        EventBus.OnProgressionChanged += HandleProgressionChanged;
        EventBus.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        EventBus.OnScoreChanged -= HandleScoreChanged;
        EventBus.OnGameStarted -= ShowHUD;
        EventBus.OnMainMenuEntered -= HideHUD;

        EventBus.OnProgressionChanged -= HandleProgressionChanged;
        EventBus.OnWaveStarted -= HandleWaveStarted;
    }

    private void Start()
    {
        HideHUD();
    }

    private void Update()
    {
        if (_hudContainer == null || !_hudContainer.activeInHierarchy) return;

        if (_currentDisplayScore < _targetScore)
        {
            float dynamicSpeed = Mathf.Max(_baseRollSpeed, (_targetScore - _currentDisplayScore) * _catchupMultiplier);
            _currentDisplayScore = Mathf.MoveTowards(_currentDisplayScore, _targetScore, dynamicSpeed * Time.deltaTime);
            UpdateScoreText(Mathf.FloorToInt(_currentDisplayScore));
        }
        else if (_currentDisplayScore > _targetScore)
        {
            _currentDisplayScore = _targetScore;
            UpdateScoreText(_targetScore);
        }
    }

    private void HandleScoreChanged(int newScore) => _targetScore = newScore;

    private void UpdateScoreText(int scoreToDisplay)
    {
        if (_scoreText != null) _scoreText.text = scoreToDisplay.ToString("D6");
    }

    private void HandleProgressionChanged(string newStage)
    {
        _currentWorldStage = newStage;
        RefreshProgressionText();
    }

    private void HandleWaveStarted(int waveIndex)
    {
        _currentWave = waveIndex + 1;
        RefreshProgressionText();
    }

    private void RefreshProgressionText()
    {
        if (_stageText != null)
        {
            _stageText.text = $"WORLD {_currentWorldStage} - WAVE {_currentWave}";
        }
    }

    public void ShowHUD()
    {
        if (_hudContainer != null) _hudContainer.SetActive(true);

        _targetScore = 0;
        _currentDisplayScore = 0;
        UpdateScoreText(0);
    }

    public void HideHUD()
    {
        if (_hudContainer != null) _hudContainer.SetActive(false);
    }
}