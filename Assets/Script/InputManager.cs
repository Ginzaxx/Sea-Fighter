using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public PlayerInputs PIA = new();
    public bool UseFixedInputs;
    public bool UseNodeMCUInputs;

    [Header("NodeMCU Connections")]
    public UDPSend sender = new();
    public int Remoteport = 25666;

    public event Action OnMove, OnConfirm;
    private Action<InputAction.CallbackContext> onMove, offMove, onConfirm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        PIA.Player.Move.performed       += onMove       = ctx => { MoveInput = ctx.ReadValue<Vector2>(); OnMove?.Invoke(); };
        PIA.Player.Move.canceled        += offMove      = ctx => MoveInput = Vector2.zero;
        PIA.Player.Confirm.performed    += onConfirm    = ctx => OnConfirm?.Invoke();

        PIA.Player.Enable();
    }

    private void OnDisable()
    {
        PIA.Player.Move.performed       -= onMove;
        PIA.Player.Move.canceled        -= offMove;
        PIA.Player.Confirm.performed    -= onConfirm;

        PIA.Player.Disable();
        PIA.Dispose();

        sender.ClosePorts();
    }

    private void OnApplicationQuit()
    {
        sender.ClosePorts();
    }

    void Start()
    {
        sender.init("10.87.8.231", Remoteport, 25666);
        sender.sendString("Hello from Unity");
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (!UseNodeMCUInputs) return;

        if (sender.newdatahereboys)
        {
            byte[] raw = sender.getLatestUDPBytes();
            sender.newdatahereboys = false;

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
        Debug.Log($"Move Input Value : " + MoveInput);

        if (UseFixedInputs)
        {
            if (y > 2.0f)       MoveInput = new Vector2(-1, MoveInput.y);
            else if (y < -2.0f) MoveInput = new Vector2(1, MoveInput.y);

            if (x > 2.0f)       MoveInput = new Vector2(MoveInput.x, -1);
            else if (x < -2.0f) MoveInput = new Vector2(MoveInput.x, 1);
        }
        else
        {
            MoveInput = new Vector2(y / -2, x / -2);
        }
    }
}