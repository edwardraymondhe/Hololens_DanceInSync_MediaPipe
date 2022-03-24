using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;


public class SocketClient : MonoBehaviour
{
    [Serializable]
    public class KeyPoint
    {
        public float x;
        public float y;
        public float confident;
    }


    [Serializable]
    public class Points_Data_Wrapper
    {
        public List<KeyPoint> keyPoints;
    }

    public Text text;
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 10086;
    private byte[] messTmp;

    // Use this for initialization
    void Start()
    {
        messTmp = new byte[1024];

        // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        Thread control_thread = new Thread(ListenControl);

        control_thread.Start();
    }

    void ListenControl()
    {
        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        var messLenCount = client.Receive(messTmp);

        if (messLenCount != 0)
        {
            Points_Data_Wrapper frame = (Points_Data_Wrapper)JsonUtility.FromJson(Encoding.UTF8.GetString(messTmp, 1, messLenCount - 2), typeof(Points_Data_Wrapper));
            Array.Clear(messTmp, 0, messLenCount);
            Debug.Log("单次录入：Keypose 已回复");
        }

        /*
        while (true)
        {
            // 如果未开始录入，则建立链接后将一直跳过循环
            if (StartInputFlag == false)
                continue;

            // 发送模型名称数组
            if (FirstInputFlag == false)
            {
                FirstInputFlag = true;
                string modelSequenceStr = "";
                for (int i = 0; i < modelVisualizerManager.ModelNames.Count; i++)
                    modelSequenceStr += modelVisualizerManager.ModelNames[i] + " ";

                byte[] modelSequence = System.Text.Encoding.Default.GetBytes(modelSequenceStr);
                client.Send(modelSequence);
            }

            Debug.Log("总录入循环：");



            // 总录入循环，当 标志位 == false 时，结束循环
            while (FinishInputFlag == false)
            {
                Debug.Log("单次录入：检测是否开始录入");


                // 开始单次录入，并仅循环一次
                if (ModelStartInputFlag == true)
                {

                    Debug.Log("单次录入：开始录入");
                    ModelStartInputFlag = false;

                    // 用户通过 ModelFinishInputFlag 决定该次循环是否完成
                    var currModelVisualizer = modelVisualizerManager.StartInputModel();
                    bool currModelFinish = false;

                    Debug.Log("单次录入：通知 Keypose");
                    // 通知 Keypose 端开始单次录入
                    byte[] startKeypose = System.Text.Encoding.Default.GetBytes("start current model");
                    client.Send(startKeypose);

                    while (currModelFinish == false)
                    {


                        Debug.Log("单次录入：等待 Keypose");
                        byte[] receivedKeypose = System.Text.Encoding.Default.GetBytes("model prediction received");

                        client.Send(receivedKeypose);

                        var messLenCount = client.Receive(messTmp);



                        if (messLenCount != 0)
                        {
                            Points_Data_Wrapper frame = (Points_Data_Wrapper)JsonUtility.FromJson(Encoding.UTF8.GetString(messTmp, 1, messLenCount - 2), typeof(Points_Data_Wrapper));
                            currModelVisualizer.points_data_wrapper = frame;
                            currModelVisualizer.predFlag = true;
                            Array.Clear(messTmp, 0, messLenCount);
                            Debug.Log("单次录入：Keypose 已回复");
                        }
                        else
                            Debug.Log("单次录入：Keypose 未回复");

                        // 现在是每个不同模型的录入出现了问题
                        Debug.Log("单次录入：查看是否已结束录入");
                        if (ModelFinishInputFlag == true)
                        {
                            currModelFinish = true;
                            byte[] finishKeypose = System.Text.Encoding.Default.GetBytes("finish current model");
                            client.Send(finishKeypose);
                        }



                    }
                }
            }

            break;
            */
    }

    bool SaveLocalFile(string fileName, byte[] data)
    {
        string path = Application.dataPath + "/data/" + "fileName";
        if (File.Exists(path))
            File.Delete(path);
        FileStream fs = new FileStream(path, FileMode.CreateNew);
        if (fs == null)
            return false;
        fs.Write(data, 0, data.Length);
        fs.Close();
        return true;
    }

}