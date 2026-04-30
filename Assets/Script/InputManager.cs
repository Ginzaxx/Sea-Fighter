using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerInputs PIA { get; private set; }
    public Vector2 MoveInput { get; private set; }

    public event Action OnMove;
    public event Action OnConfirm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PIA = new PlayerInputs();
    }
    private void OnEnable()
    {
        PIA.Player.Move.performed       += ctx => MoveInput = ctx.ReadValue<Vector2>();
        PIA.Player.Move.canceled        += ctx => MoveInput = Vector2.zero;
        PIA.Player.Move.performed       += ctx => OnMove?.Invoke();
        PIA.Player.Confirm.performed    += ctx => OnConfirm?.Invoke();

        PIA.Player.Enable();
    }

    void OnDisable()
    {
        PIA.Player.Move.performed       -= ctx => MoveInput = ctx.ReadValue<Vector2>();
        PIA.Player.Move.canceled        -= ctx => MoveInput = Vector2.zero;
        PIA.Player.Move.performed       -= ctx => OnMove?.Invoke();
        PIA.Player.Confirm.performed    -= ctx => OnConfirm?.Invoke();

        PIA.Player.Disable();
    }
}