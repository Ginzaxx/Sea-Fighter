using System;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    public int Remoteport = 25666;
    public UDPSend sender = new UDPSend();
    public string datafromnode;

    [SerializeField] private PlayerMovement playerMovement; // drag in Inspector

    void Start()
    {
        // Replace with your PC's local IP and NodeMCU's IP if needed
        sender.init("10.87.8.231", Remoteport, 25666);
        sender.sendString("Hello from Unity");
        Application.targetFrameRate = 60;
    }

    void Update()
    {
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
        if (playerMovement == null) return;

        Debug.Log($"X : " + x);

        if (x < -2.0f)      playerMovement.MoveLeft();
        else if (x > 2.0f)  playerMovement.MoveRight();
    }

    public void OnDisable()       { sender.ClosePorts(); }
    public void OnApplicationQuit() { sender.ClosePorts(); }
}