using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 MoveInput => NodeInput != Vector2.zero ? NodeInput : KeyInput;
    public Vector2 NodeInput;
    public Vector2 KeyInput;
    public PlayerInputs PIA;
    public bool UseFixedInputs = true;
    public bool NewData;

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

        PIA = new();
    }

    private void OnEnable()
    {
        PIA.Player.Move.performed       += onMove       = ctx => { KeyInput = ctx.ReadValue<Vector2>(); OnMove?.Invoke(); };
        PIA.Player.Move.canceled        += offMove      = ctx => KeyInput = Vector2.zero;
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
        sender.SendString("Hello from Unity");
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (NewData = sender.newdatahereboys)
        {
            byte[] raw = sender.GetLatestUDPBytes();
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
        if (UseFixedInputs)
        {
            float newX = 0, newY = 0;

            if (y > 2.0f)       newX = -1;
            else if (y < -2.0f) newX = 1;

            if (x > 2.0f)       newY = -1;
            else if (x < -2.0f) newY = 1;

            NodeInput = new Vector2(newX, newY);
        }
        else
        {
            NodeInput = new Vector2(y/-2, x/-2);
        }

        Debug.Log($"Move Input Value : " + NodeInput);
    }
}