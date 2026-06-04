// Source - https://stackoverflow.com/a/68976049
// Posted by YGreater, modified by community. See post 'Timeline' for change history
// Retrieved 2026-04-30, License - CC BY-SA 4.0

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive
{
    // === Connection ===
    public string IP { get; private set; }
    public int SourcePort { get; private set; }
    public int RemotePort { get; private set; }

    Thread receiveThread;
    UdpClient client;

    // === Information ===
    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = "";
    public byte[] lastReceivedBytes = null;
    public bool newdatahereboys = false;

    public void Init(string IPAdress, int remotePort, int sourcePort = -1)
    {
        IP = IPAdress;
        SourcePort = sourcePort;
        RemotePort = remotePort;

        if (SourcePort <= -1)
        {
            client = new UdpClient();
            Debug.Log("Sending to " + IP + ": " + RemotePort);
        }
        else
        {
            client = new UdpClient(SourcePort);
            Debug.Log("Sending to " + IP + ": " + RemotePort + " from Source Port: " + SourcePort);
        }

        receiveThread = new Thread(new ThreadStart(ReceiveData)) { IsBackground = true };
        receiveThread.Start();

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
                newdatahereboys = true;
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    public string GetLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }

    public byte[] GetLatestUDPBytes()
    {
        return lastReceivedBytes;
    }

    public void ClosePorts()
    {
        Debug.Log("Closing receiving UDP on Port: " + RemotePort);

        receiveThread?.Abort();

        client.Close();
    }
}