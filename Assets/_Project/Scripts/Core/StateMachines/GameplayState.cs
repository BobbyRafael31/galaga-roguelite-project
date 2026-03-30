using UnityEngine;

public class GameplayState : IGameState
{
    private GameStateManager _context;
    private bool _isResumingFromShop = false;

    public GameplayState(GameStateManager context) => _context = context;

    public void SetResumeFlag(bool isResuming) => _isResumingFromShop = isResuming;

    public async void Enter()
    {
        EventBus.OnGameOver += HandleGameOver;

        if (_isResumingFromShop)
        {
            _isResumingFromShop = false;

            StageData nextStage = LevelDirector.Instance.GetNextStage();
            if (nextStage != null)
            {
                _context.WaveSpawner.StartStageSequence(nextStage);
            }
            return;
        }
        _context.IsTransitioning = true;

        EventBus.OnClearArena?.Invoke();
        _context.WaveSpawner.StopAndReset();

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToBlack(0.8f);

        _context.SpawnRealPlayer();

        EventBus.OnGameStarted?.Invoke();

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToClear(0.8f);

        _context.IsTransitioning = false;

        StageData firstStage = LevelDirector.Instance.GetNextStage();
        if (firstStage != null) _context.WaveSpawner.StartStageSequence(firstStage);
    }

    public void Tick() { }
    public void Exit()
    {
        EventBus.OnGameOver -= HandleGameOver;
    }

    private async void HandleGameOver()
    {
        Debug.Log("[GameState] Processing Game Over sequence...");

        _context.WaveSpawner.StopAndReset();

        await Awaitable.WaitForSecondsAsync(3.0f);

        _context.ChangeState(_context.MenuState);
    }
}