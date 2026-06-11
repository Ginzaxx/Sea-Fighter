// Source - https://stackoverflow.com/a/68976049
// Posted by YGreater, modified by community
// Retrieved 2026-04-30, License - CC BY-SA 4.0

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive
{
    Thread receiveThread;
    UdpClient client;
    public byte[] lastReceivedBytes = null;
    public bool newDataReceived = false;

    public void OpenPorts()
    {
        client = new UdpClient();
        receiveThread = new Thread(new ThreadStart(ReceiveData))
        { IsBackground = true };
        receiveThread.Start();
        Debug.Log("Opening UDP Bytes Receiver");
    }

    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                lastReceivedBytes = data;
                newDataReceived = true;
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    public void ClosePorts()
    {
        Debug.Log("Closing UDP Bytes Receiver");
        receiveThread?.Abort();
        client.Close();
    }
}