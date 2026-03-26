using UnityEngine;

public class ShopState : IGameState
{
    private GameStateManager _context;

    public ShopState(GameStateManager context) => _context = context;

    public void Enter()
    {
        Debug.Log("[GameState] Entering Shop State");
        Time.timeScale = 0f;

        if (_context.ShopUI != null) _context.ShopUI.SetActive(true);

        EventBus.OnShopEntered?.Invoke();
    }

    public void Tick()
    {

    }

    public void Exit()
    {
        Debug.Log("[GameState] Exiting Shop State");
        Time.timeScale = 1f;

        if (_context.ShopUI != null) _context.ShopUI.SetActive(false);
    }
}
