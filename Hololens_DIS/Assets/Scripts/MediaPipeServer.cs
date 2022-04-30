using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Text;
using System;

/// <summary>
/// MediaPipe servers will be fetching pose datas and streamed videos from socket
/// </summary>
public class MediaPipeServer: MonoBehaviour
{
    public bool runOnStart = false;
    public int frame = 0;
    public IpCamera ipCamera;
    public PoseEditor poseEditor;
    public Helper.Pose.Landmarks poseLandmarks;
    TcpServer server;
    Helper.StatFile<Helper.StatStruct.ParseData> statParsData;

    private Thread ReceiveThread;
    public bool ReceiveThreadEnd = false;
    byte[] recvData;

    public bool receiveStop = false;

    public HumanoidController humanoidController;

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

        if (poseLandmarks.landmarks.Count > 0)
        {
            ProcessPoseFrame();
            ProcessPoseRecord();
            // ProcessHumanoid();
        }
        // HandleImage(recvData, new Vector2(720, 1280));
    }

    private void ProcessHumanoid()
    {
        // humanoidController.ProcessHumanoid(poseLandmarks);
    }

    public void StartServer()
    {
        server.InitSocket(Helper.Socket.port, false);
        ReceiveThread = new Thread(GetLandmarks);
        ReceiveThread.Start();
    }

    public void GetLandmarks()
    {
        while (!ReceiveThreadEnd)
        {
            if (server != null && server.IsConnected)
            {
                //if (server.GetLargeDatasCount() > 0)
                if (server.isLargeDataValid())
                    ParseData();
                // else
                // Log("Server: No landmarks detected");
            }
            // else
            // Log("Server: No connected");
        }
    }

    public void ParseData()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // TODO: Encode data to landmarks
        recvData = server.DequeLargeData();
        string recvDataString = Encoding.UTF8.GetString(recvData, 1, recvData.Length - 2);

        if (receiveStop == false)
            poseLandmarks = JsonConvert.DeserializeObject<Helper.Pose.Landmarks>(recvDataString);

        /*
        var count_list = Helper.ConvertIntWithCount(recvData);
        int bytes_length = (count_list.Count + 1) * 4;


        int idx = 1;
        var resolution = new Vector2();
        // 1. resolution, 2. landmarks, 3. image
        foreach (var count in count_list)
        {
            switch (idx)
            {
                case 1:
                    byte[] resolutionData = new byte[count];
                    resolutionData = recvData.Skip(bytes_length).Take(count).ToArray();
                    var resolutionList = Helper.ConvertInt(resolutionData);
                    resolution.x = resolutionList[0];
                    resolution.y = resolutionList[1];
                    break;
                case 2:
                    byte[] imageData = new byte[count];
                    imageData = recvData.Skip(bytes_length).Take(count).ToArray();
                    HandleImage(imageData,resolution);
                    break;
                default:
                    break;
            }

            idx++;
            bytes_length += count;
        }

        */

        sw.Stop();
        statParsData.AddBuffer(new Helper.StatStruct.ParseData(frame, server.readTimes, server.GetLargeDatasCount(), sw.ElapsedMilliseconds));
    }

    /*
    private void HandleImage(Texture2D texture)
    {
        Debug.Log(texture);
        if (texture == null)
            return;

        Destroy(streamImage.texture);
        // Texture2D texture2D = new Texture2D(0, 0);
        // texture2D.LoadImage(bytes);
        streamImage.texture = texture;
        streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
    }
    private void HandleImage(byte[] bytes, Vector2 resolution)
    {
        Destroy(streamImage.texture);
        Texture2D texture2D = new Texture2D((int)resolution.x, (int)resolution.y);
        texture2D.LoadImage(bytes);
        streamImage.texture = texture2D;
        streamImage.GetComponent<RectTransform>().sizeDelta = resolution;
    }

    public void InitStream()
    {
        texture2D = new Texture2D(0, 0);
        streamImage.texture = texture2D;
    }

    public void HandleStream(byte[] bytes)
    {
        texture2D.LoadImage(bytes);
        streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(texture2D.width, texture2D.height);
    }
    */

    private void HandleLandmarks(string str)
    {
        var deserializedLandmarks = JsonConvert.DeserializeObject<Helper.Pose.Landmarks>(str);
        Log(deserializedLandmarks.GetString());
    }

    public void Log(string str)
    {
        Debug.Log(str);
        // landmarkText.text = str;
    }

    public PoseFrame lastPoseFrame;
    public PoseFrame currentPoseFrame;

    public PoseRecorder poseRecorder;
    public bool isPoseStartRecording = false;
    public bool isPoseNowRecording = false;

    public void ProcessPoseFrame()
    {
        // Initialize and calculate pose frames
        if (currentPoseFrame == null)
            currentPoseFrame = PoseFrame.CreateInstance();

        lastPoseFrame = currentPoseFrame;
        currentPoseFrame = PoseFrame.CreateInstance(lastPoseFrame, Time.deltaTime, poseLandmarks, ipCamera.GetWidthFactor(), ipCamera.GetHeightFactor(), ipCamera.GetDepthFactor());
    }

    public void ProcessPoseRecord()
    {
        // Pose frames flow control
        if (isPoseStartRecording)
        {
            if (!isPoseNowRecording)
                poseRecorder = new PoseRecorder();
            else
            {
                poseRecorder.SaveSequence();
                poseEditor.RefreshPoseBrowserContent();
            }

            isPoseNowRecording = !isPoseNowRecording;

            isPoseStartRecording = false;
        }

        if (isPoseNowRecording)
            poseRecorder.RecordFrame(currentPoseFrame);
    }


    public void TogglePoseRecording()
    {
        isPoseStartRecording = true;
    }


    private void OnApplicationQuit()
    {
        ReceiveThreadEnd = true;

        ReceiveThread.Join();
        statParsData.BeginWrite();
    }
}
