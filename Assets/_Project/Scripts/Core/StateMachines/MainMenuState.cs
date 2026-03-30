using UnityEngine;

public class MainMenuState : IGameState
{
    private GameStateManager _context;
    private bool _isActive;

    public MainMenuState(GameStateManager context) => _context = context;

    public void Enter()
    {
        _isActive = true;
        _context.MainMenuUI.SetActive(true);
        EventBus.OnPlayerDeath += HandleDummyDeath;

        EventBus.OnMainMenuEntered?.Invoke();

        _ = RunDemoLoopAsync();
    }

    public void Tick() { }

    public void Exit()
    {
        _isActive = false;
        EventBus.OnPlayerDeath -= HandleDummyDeath;
        _context.MainMenuUI.SetActive(false);
        _context.WaveSpawner.StopAndReset();
    }

    private void HandleDummyDeath()
    {
        _context.WaveSpawner.StopAndReset();
    }

    private async Awaitable RunDemoLoopAsync()
    {
        while (_isActive)
        {
            EventBus.OnClearArena?.Invoke();
            _context.SpawnDummyBot();

            if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToClear(0.5f);
            if (!_isActive) return;

            if (_context.DemoWave != null)
            {
                await _context.WaveSpawner.SpawnSingleWaveAsync(_context.DemoWave);
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(3f);
            }

            if (!_isActive) return;

            if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeToBlack(0.5f);
        }
    }
}