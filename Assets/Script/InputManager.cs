using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 MoveInput;
    public Vector3 NodeInput;
    public PlayerInputs PIA;
    public bool UseFixedInputs = true;

    [Header("NodeMCU Connections")]
    [SerializeField] private bool NewData;
    [SerializeField] private int IPIndex = 2;
    private readonly UDPSend sender = new();
    private readonly string IPAddress1 = "10.87.8.231";
    private readonly string IPAddress2 = "10.11.183.231";
    [SerializeField] private string IPAddress = "";
    [SerializeField] private int RemotePort = 25666;
    [SerializeField] private int SourcePort = 25666;

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

        sender.ClosePorts();
    }

    private void OnApplicationQuit()
    {
        sender.ClosePorts();
    }

    void Start()
    {
        switch (IPIndex)
        {
            case 1:
                IPAddress = IPAddress1;
                break;
            case 2:
                IPAddress = IPAddress2;
                break;
        }

        sender.Init(IPAddress, RemotePort, SourcePort);
        sender.SendString("Hello from Unity");
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        NewData = sender.newdatahereboys;
        if (NewData)
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