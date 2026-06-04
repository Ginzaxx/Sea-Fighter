using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerInputs PIA;
    public Vector2 MoveInput;
    public Vector3 NodeInput;
    public bool UseFixedInputs = true;

    [Header("NodeMCU Connections")]
    [SerializeField] private bool NewData;
    [SerializeField] private string IPAddress = "";
    [SerializeField] private int RemotePort = 25666;
    [SerializeField] private int SourcePort = 25666;
    private readonly UDPReceive receiver = new();

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
        DontDestroyOnLoad(this);

        PIA = new();
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

        receiver.ClosePorts();
    }

    private void OnApplicationQuit()
    {
        receiver.ClosePorts();
    }

    void Start()
    {
        receiver.Init(IPAddress, RemotePort, SourcePort);
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        NewData = receiver.newdatahereboys;
        if (NewData)
        {
            byte[] raw = receiver.GetLatestUDPBytes();
            receiver.newdatahereboys = false;

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
        NodeInput = new Vector3(x, y, z);

        if (!UseFixedInputs)
        {
            MoveInput = new Vector2(y/-2, x/-2);
            return;
        }

        float newX = 0, newY = 0;

        if (y > 2.0f)       newX = -1;
        else if (y < -2.0f) newX = 1;
        if (x > 2.0f)       newY = -1;
        else if (x < -2.0f) newY = 1;

        MoveInput = new Vector2(newX, newY);
        OnMove?.Invoke();
    }
}