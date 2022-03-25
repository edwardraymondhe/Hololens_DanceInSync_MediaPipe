using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPLib;
using Newtonsoft.Json;
using UnityEngine.UI;

public class MediaPipeServer: MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 6001;

    public Text displayText;
    public RawImage rawImage;

    public bool runOnStart = false;

    TCPServer server;

    public TcpServer landmarkTcpServer;
    public TcpServer imageTcpServer;

    private void Start()
    {
        if (runOnStart)
            StartServer();
    }

    private void Update()
    {
        GetLandmarks();
        GetImage();
    }

    public void StartServer()
    {
        landmarkTcpServer.InitSocket(port, false);
        imageTcpServer.InitSocket(port + 1, true);
    }

    private void GetImage()
    {
        if (imageTcpServer != null && imageTcpServer.IsConnected)
        {
            if (imageTcpServer.GetLargeDatasCount() > 0)
            {
                Texture2D texture2D = new Texture2D(1980, 1020);
                texture2D.LoadImage(imageTcpServer.DequeLargeData());
                rawImage.texture = texture2D;
            }
        }
    }

    public void GetLandmarks()
    {
        if (landmarkTcpServer != null && landmarkTcpServer.IsConnected)
        {
            if (landmarkTcpServer.recvStr != "")
            {
                var deserializedLandmarks = JsonConvert.DeserializeObject<DataStructures.PoseLandmarks>(landmarkTcpServer.recvStr);
                Log("Server: Landmarks detected");
                Log(deserializedLandmarks.GetString());
                // return deserializedLandmarks.landmarks;
            }
            return;
        }
        Log("Server: No landmarks detected");
        // return null;
    }

    public void Log(string str)
    {
        Debug.Log(str);
        displayText.text = str;
    }
}
