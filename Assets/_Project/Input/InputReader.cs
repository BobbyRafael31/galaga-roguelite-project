using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Project.Input;

[CreateAssetMenu(fileName = "InputReader", menuName = "Project/Input/Input Reader")]
public class InputReader : ScriptableObject, GameInput.IPlayerActions
{
    public event Action OnFireEvent;
    public event Action OnMenuEvent;

    public Vector2 MoveVector { get; private set; }

    private GameInput _gameInput;

    public void EnablePlayerInput()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Player.SetCallbacks(this);
        }

        _gameInput.Player.Enable();
    }

    public void DisablePlayerInput()
    {
        _gameInput?.Player.Disable();
        MoveVector = Vector2.zero; // Reset vector to prevent infinite drifting
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveVector = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            OnFireEvent?.Invoke();
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            OnMenuEvent?.Invoke();
    }
}