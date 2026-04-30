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

            if (raw != null && raw.Length == 4)
            {
                int value = (raw[0] << 24) | (raw[1] << 16) | (raw[2] << 8) | raw[3];
                HandleNodeInput(value);
            }
        }
    }

    private void HandleNodeInput(int value)
    {
        if (playerMovement == null) return;

        Debug.Log($"Value : " + value);

        if (value < 400)       playerMovement.MoveLeft();
        else if (value > 600)  playerMovement.MoveRight();
    }

    public void OnDisable()       { sender.ClosePorts(); }
    public void OnApplicationQuit() { sender.ClosePorts(); }
}