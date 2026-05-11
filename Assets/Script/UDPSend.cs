// Source - https://stackoverflow.com/a/68976049
// Posted by YGreater, modified by community. See post 'Timeline' for change history
// Retrieved 2026-04-30, License - CC BY-SA 4.0

// Inspired by this thread: https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
// Thanks OP la1n
// Thanks MattijsKneppers for letting me know that I also need to lock my queue while enqueuing
// Adapted during projects according to my needs

using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSend
{
    public string IP { get; private set; }
    public int SourcePort { get; private set; }
    public int RemotePort { get; private set; }

    IPEndPoint remoteEndPoint;
    Thread receiveThread;
    UdpClient client;

    // === Connection ===
    // public string IP = "127.0.0.1";
    public int port = 25666;

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

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), RemotePort);
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

                Debug.Log("New Data = " + newdatahereboys);
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    public void SendString(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);

        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public void SendInt32(int myInt)
    {
        try
        {
            byte[] data = BitConverter.GetBytes(myInt);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public void SendInt32Array(int[] myInts)
    {
        try
        {
            byte[] data = new byte[myInts.Length * sizeof(Int32)];
            Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public void SendInt16Array(short[] myInts)
    {
        try
        {
            byte[] data = new byte[myInts.Length * sizeof(Int16)];
            Buffer.BlockCopy(myInts, 0, data, 0, data.Length);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
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
        Debug.Log("Closing receiving UDP on Port: " + port);

        receiveThread?.Abort();

        client.Close();
    }
}