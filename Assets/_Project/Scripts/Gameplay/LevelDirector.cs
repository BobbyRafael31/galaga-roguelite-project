using System.Collections.Generic;
using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    public static LevelDirector Instance { get; private set; }

    [Header("Campaign Configuration")]
    [Tooltip("Make sure to add leveldata in exact order")]
    [SerializeField] private List<LevelData> _campaignLevels = new List<LevelData>();

    public int CurrentLevelIndex { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public int CurrentLoopCount { get; private set; } = 1;

    // Campaign loop solution (for know atleast)
    // The mathematical multipliers applied when the player beats the entire campaign and it loops
    public float EnemyHealthMultiplier => 1f + ((CurrentLoopCount - 1) * 0.5f); // +50% HP per loop
    public float EnemyScoreMultiplier => 1f + ((CurrentLoopCount - 1) * 0.2f); // +20% Score per loop

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnGameStarted += ResetCampaign;
        EventBus.OnMainMenuEntered += ResetCampaign;
        EventBus.OnStageCleared += AdvanceProgression;
    }
    private void OnDisable()
    {
        EventBus.OnGameStarted -= ResetCampaign;
        EventBus.OnMainMenuEntered -= ResetCampaign;
        EventBus.OnStageCleared -= AdvanceProgression;
    }

    private void ResetCampaign()
    {
        CurrentLevelIndex = 0;
        CurrentStageIndex = 0;
        CurrentLoopCount = 1;

        BroadcastProgressionUI();
    }

    public StageData GetNextStage()
    {
        if (_campaignLevels == null || _campaignLevels.Count == 0) return null;

        LevelData currentLevel = _campaignLevels[CurrentLevelIndex];

        // Failsafe if a level is completely empty
        if (currentLevel.Stages.Count == 0) return null;

        BroadcastProgressionUI();
        return currentLevel.Stages[CurrentStageIndex];
    }

    private void AdvanceProgression()
    {
        if (_campaignLevels == null || _campaignLevels.Count == 0) return;

        CurrentStageIndex++;

        // Have we beaten all stages in the current level?
        if (CurrentStageIndex >= _campaignLevels[CurrentLevelIndex].Stages.Count)
        {
            CurrentStageIndex = 0;
            CurrentLevelIndex++;

            // Have we beaten all levels in the campaign? INFINITE LOOP!!!!!! (O_O;)
            if (CurrentLevelIndex >= _campaignLevels.Count)
            {
                CurrentLevelIndex = 0;
                CurrentLoopCount++;
                Debug.Log($"[LevelDirector] CAMPAIGN CLEARED! Entering Loop {CurrentLoopCount}. Difficulty increased.");
            }
        }
    }
    private void BroadcastProgressionUI()
    {
        // Format "1-1", "2-3", or "L2: 1-1" for loops
        string loopPrefix = CurrentLoopCount > 1 ? $"LOOP {CurrentLoopCount}: " : "";
        string displayString = $"{loopPrefix}{CurrentLevelIndex + 1} - {CurrentStageIndex + 1}";

        EventBus.OnProgressionChanged?.Invoke(displayString);
    }
}
