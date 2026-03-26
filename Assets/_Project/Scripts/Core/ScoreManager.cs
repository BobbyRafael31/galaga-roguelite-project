using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }

    public float ScoreMultiplier = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnEnemyDestroyed += AddScore;
        EventBus.OnGameStarted += ResetScore;
    }

    private void OnDisable()
    {
        EventBus.OnEnemyDestroyed -= AddScore;
        EventBus.OnGameStarted -= ResetScore;
    }

    private void AddScore(int amount)
    {
        int finalAmount = Mathf.FloorToInt(amount * ScoreMultiplier);

        CurrentScore += finalAmount;
        EventBus.OnScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        ScoreMultiplier = 1.0f; // Reset on new run
        EventBus.OnScoreChanged?.Invoke(CurrentScore);
    }

    public bool TrySpendScore(int amount)
    {
        if (CurrentScore >= amount)
        {
            CurrentScore -= amount;
            EventBus.OnScoreChanged?.Invoke(CurrentScore);
            return true;
        }
        return false;
    }
}