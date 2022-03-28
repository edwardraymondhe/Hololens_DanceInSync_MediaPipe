using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// MediaPipe servers will be fetching pose datas and streamed videos from socket
/// </summary>
public class MediaPipeServer: MonoBehaviour
{
    public bool runOnStart = false;

    public Text landmarkText;
    public RawImage streamImage;

    TcpServer server;

    public int frame = 0;
    Helper.StatFile<Helper.StatStruct.ParseData> statParsData;

    private void Start()
    {
        server = gameObject.AddComponent<TcpServer>();
        statParsData = new Helper.StatFile<Helper.StatStruct.ParseData>("ParseData With Pure Image with Queue");

        if (runOnStart)
            StartServer();
    }

    private void Update()
    {
        frame++;
        GetLandmarks();
    }

    public void StartServer()
    {
        server.InitSocket(Helper.Socket.port, false);
    }

    public void GetLandmarks()
    {
        if (server != null && server.IsConnected)
        {
            if (server.GetLargeDatasCount() > 0)
                ParseData();
            else
                Log("Server: No landmarks detected");
        }
        else
            Log("Server: No connected");
    }

    public void HandleImage(byte[] bytes)
    {
        Destroy(streamImage.texture);
        Texture2D texture2D = new Texture2D(1980, 1020);
        texture2D.LoadImage(bytes);
        streamImage.texture = texture2D;
    }

    public void HandleLandmarks(string str)
    {
        var deserializedLandmarks = JsonConvert.DeserializeObject<Helper.Pose.Landmarks>(str);
        Log(deserializedLandmarks.GetString());
    }

    public void ParseData()
    {
        Stopwatch sw = new Stopwatch();
        Stopwatch resSW = new Stopwatch();
        Stopwatch landmarkSW = new Stopwatch();
        Stopwatch imageSW = new Stopwatch();

        sw.Start();

        byte[] recvData = server.DequeLargeData();

        var count_list = Helper.ConvertIntWithCount(recvData);
        int bytes_length = (count_list.Count + 1) * 4;


        int idx = 1;
        // 1. resolution, 2. landmarks, 3. image
        foreach (var count in count_list)
        {
            switch (idx)
            {
                case 1:
                    resSW.Start();

                    byte[] resolutionData = new byte[count];
                    resolutionData = recvData.Skip(bytes_length).Take(count).ToArray();
                    var resolution = Helper.ConvertInt(resolutionData);
                    HandleResolution(resolution);

                    resSW.Stop();
                    break;
                case 2:
                    landmarkSW.Start();

                    string landmarksData = new UTF8Encoding().GetString(recvData, bytes_length, count);
                    HandleLandmarks(landmarksData);

                    landmarkSW.Stop();
                    break;
                case 3:
                    imageSW.Start();

                    byte[] imageData = new byte[count];
                    imageData = recvData.Skip(bytes_length).Take(count).ToArray();
                    HandleImage(imageData);

                    imageSW.Stop();
                    break;
                default:
                    break;
            }

            idx++;
            bytes_length += count;
        }

        
        sw.Stop();

        statParsData.AddBuffer(new Helper.StatStruct.ParseData(frame, server.readTimes, server.GetLargeDatasCount(), sw.ElapsedMilliseconds));
    }

    private void HandleResolution(List<int> resolution)
    {
        streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(resolution[0], resolution[1]);
    }

    public void Log(string str)
    {
        // Debug.Log(str);
        landmarkText.text = str;
    }

    private void OnApplicationQuit()
    {
        statParsData.BeginWrite();
    }
}
