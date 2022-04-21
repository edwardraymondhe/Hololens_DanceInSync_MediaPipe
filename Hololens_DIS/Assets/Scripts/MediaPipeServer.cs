using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Text;

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

    private void Start()
    {
        // var str = "{ \"landmarks\": [{ \"score\": 0.999931812286377, \"x\": 0.530025064945221, \"y\": 0.3351213037967682, \"z\": -0.4886830747127533}, { \"score\": 0.9999023675918579, \"x\": 0.5492587089538574, \"y\": 0.31880253553390503, \"z\": -0.43882423639297485}]}";
        // var str = "{ \"landmarks\": [{ \"score\": 0.999931812286377, \"x\": 0.530025064945221, \"y\": 0.3351213037967682, \"z\": -0.4886830747127533}, { \"score\": 0.9999023675918579, \"x\": 0.5492587089538574, \"y\": 0.31880253553390503, \"z\": -0.43882423639297485}, { \"score\": 0.9998643398284912, \"x\": 0.5596959590911865, \"y\": 0.3186560273170471, \"z\": -0.43858927488327026}, { \"score\": 0.9998856782913208, \"x\": 0.5679664611816406, \"y\": 0.31926441192626953, \"z\": -0.438568115234375}, { \"score\": 0.999923586845398, \"x\": 0.5123602151870728, \"y\": 0.32080188393592834, \"z\": -0.43782898783683777}, { \"score\": 0.9999071359634399, \"x\": 0.49866512417793274, \"y\": 0.322055846452713, \"z\": -0.43773671984672546}, { \"score\": 0.9999325275421143, \"x\": 0.48735517263412476, \"y\": 0.3234778940677643, \"z\": -0.437755286693573}, { \"score\": 0.9998414516448975, \"x\": 0.585321307182312, \"y\": 0.3292527198791504, \"z\": -0.18990327417850494}, { \"score\": 0.9999492168426514, \"x\": 0.4731421172618866, \"y\": 0.3335225284099579, \"z\": -0.18985134363174438}, { \"score\": 0.9999216794967651, \"x\": 0.5559679269790649, \"y\": 0.35614195466041565, \"z\": -0.4019724428653717}, { \"score\": 0.9999487400054932, \"x\": 0.5145332217216492, \"y\": 0.3571571707725525, \"z\": -0.401839017868042}, { \"score\": 0.9983030557632446, \"x\": 0.664055585861206, \"y\": 0.41937676072120667, \"z\": -0.11114896833896637}, { \"score\": 0.9993422627449036, \"x\": 0.4066779613494873, \"y\": 0.4214462339878082, \"z\": -0.15164656937122345}, { \"score\": 0.9906184077262878, \"x\": 0.8565105199813843, \"y\": 0.4202900528907776, \"z\": -0.5052555799484253}, { \"score\": 0.9957263469696045, \"x\": 0.2164468616247177, \"y\": 0.40117448568344116, \"z\": -0.5659931302070618}, { \"score\": 0.8435907363891602, \"x\": 0.6958911418914795, \"y\": 0.38233688473701477, \"z\": -0.701551616191864}, { \"score\": 0.824522852897644, \"x\": 0.390993595123291, \"y\": 0.37461867928504944, \"z\": -0.7479051947593689}, { \"score\": 0.54879230260849, \"x\": 0.6657171249389648, \"y\": 0.379499226808548, \"z\": -0.7539389133453369}, { \"score\": 0.5168834328651428, \"x\": 0.4333897829055786, \"y\": 0.3719840943813324, \"z\": -0.8083171248435974}, { \"score\": 0.5000239014625549, \"x\": 0.648918867111206, \"y\": 0.381140798330307, \"z\": -0.657725989818573}, { \"score\": 0.45832645893096924, \"x\": 0.4454614520072937, \"y\": 0.3759561777114868, \"z\": -0.7117816209793091}, { \"score\": 0.4990289509296417, \"x\": 0.6538510322570801, \"y\": 0.3853358328342438, \"z\": -0.6722217202186584}, { \"score\": 0.4726217985153198, \"x\": 0.43538594245910645, \"y\": 0.3795396387577057, \"z\": -0.7206783890724182}, { \"score\": 0.9993556141853333, \"x\": 0.6006811857223511, \"y\": 0.6616551280021667, \"z\": 0.018275484442710876}, { \"score\": 0.999496579170227, \"x\": 0.4377761483192444, \"y\": 0.6585823893547058, \"z\": -0.01851274073123932}, { \"score\": 0.992230236530304, \"x\": 0.6328152418136597, \"y\": 0.8189026117324829, \"z\": 0.18700465559959412}, { \"score\": 0.9973760843276978, \"x\": 0.4013449251651764, \"y\": 0.8198889493942261, \"z\": 0.1361495554447174}, { \"score\": 0.9901279807090759, \"x\": 0.6572417616844177, \"y\": 0.935285747051239, \"z\": 0.5142783522605896}, { \"score\": 0.9915000796318054, \"x\": 0.37837111949920654, \"y\": 0.931155800819397, \"z\": 0.45954078435897827}, { \"score\": 0.9212484955787659, \"x\": 0.6436111927032471, \"y\": 0.9491018056869507, \"z\": 0.5310332775115967}, { \"score\": 0.8969449996948242, \"x\": 0.4015083611011505, \"y\": 0.9492929577827454, \"z\": 0.4792312681674957}, { \"score\": 0.9675736427307129, \"x\": 0.6972454190254211, \"y\": 0.9901121854782104, \"z\": 0.26509785652160645}, { \"score\": 0.9698900580406189, \"x\": 0.3320368230342865, \"y\": 0.9867975115776062, \"z\": 0.21460574865341187}]}";
        // poseLandmarks = JsonConvert.DeserializeObject<Helper.Pose.Landmarks>(str);
        // Debug.Log(poseLandmarks);

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
        }
        // HandleImage(recvData, new Vector2(720, 1280));
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
        currentPoseFrame = PoseFrame.CreateInstance(lastPoseFrame, Time.deltaTime, poseLandmarks, ipCamera.widthFactor, ipCamera.heightFactor, ipCamera.depthFactor);
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
