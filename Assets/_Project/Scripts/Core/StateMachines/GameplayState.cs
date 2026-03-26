using UnityEngine;

public class GameplayState : IGameState
{
    private GameStateManager _context;

    public GameplayState(GameStateManager context) => _context = context;

    public async void Enter()
    {
        _context.IsTransitioning = true;

        EventBus.OnClearArena?.Invoke();
        _context.WaveSpawner.StopAndReset();

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToBlack(0.8f);

        _context.SpawnRealPlayer();

        EventBus.OnGameStarted?.Invoke();

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToClear(0.8f);

        _context.IsTransitioning = false;
        _context.WaveSpawner.StartStageSequence();
    }

    public void Tick() { }
    public void Exit()
    {
        EventBus.OnGameOver -= HandleGameOver;
    }

    private async void HandleGameOver()
    {
        Debug.Log("[GameState] Processing Game Over sequence...");

        await Awaitable.WaitForSecondsAsync(3.0f);

        _context.ChangeState(_context.MenuState);
    }
}