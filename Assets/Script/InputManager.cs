using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInputs PIA;
    private UDPReceive receiver;

    [Header("NodeMCU & Move Inputs")]
    public Vector2 MoveInput;
    public bool UseFixedInputs;
    public bool NewData;

    public event Action OnMove, OffMove, OnConfirm;
    private Action<InputAction.CallbackContext> onMove, offMove, onConfirm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        Application.targetFrameRate = 60;
        receiver = new();
        PIA = new();
    }

    private void OnEnable()
    {
        PIA.Player.Move.performed       += onMove       = ctx =>
            { MoveInput = ctx.ReadValue<Vector2>(); OnMove?.Invoke(); };
        PIA.Player.Move.canceled        += offMove      = ctx =>
            { MoveInput = Vector2.zero;             OffMove?.Invoke(); };
        PIA.Player.Confirm.performed    += onConfirm    = ctx => 
            { OnConfirm?.Invoke(); };

        receiver.OpenPorts();
        PIA.Player.Enable();
    }

    private void OnDisable()
    {
        PIA.Player.Move.performed       -= onMove;
        PIA.Player.Move.canceled        -= offMove;
        PIA.Player.Confirm.performed    -= onConfirm;

        receiver.ClosePorts();
        PIA.Player.Disable();
        PIA.Dispose();
    }

    private void Update()
    {
        if (NewData = receiver.newDataReceived)
        {
            byte[] raw = receiver.lastReceivedBytes;
            receiver.newDataReceived = false;

            if (raw != null && raw.Length == 12)
            {
                float x = BitConverter.ToSingle(raw, 0);
                float y = BitConverter.ToSingle(raw, 4);
                float z = BitConverter.ToSingle(raw, 8);

                HandleNodeInput(x, y, z);
            }
        }
    }

    private void HandleNodeInput(float x, float y, float z)
    {
        if (!UseFixedInputs)
        {
            MoveInput = new Vector2(-y, -x);
            return;
        }

        float newX = 0;
        if (y < -2.0f)      newX = 1;
        else if (y > 2.0f)  newX = -1;

        float newY = 0;
        if (x < -2.0f)      newY = 1;
        else if (x > 2.0f)  newY = -1;

        MoveInput = new Vector2(newX, newY);
        OnMove?.Invoke();
    }

    public void ToggleFixedInputs()
    {
        // UseFixedInputs = !UseFixedInputs;
    }
}