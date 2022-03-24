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

        // ����һ��Socketʵ����������ָ���ķ���ˡ�������Ҫʹ��IPEndPoint��(ip�Ͷ˿ںŵķ�װ)
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
            Debug.Log("����¼�룺Keypose �ѻظ�");
        }

        /*
        while (true)
        {
            // ���δ��ʼ¼�룬�������Ӻ�һֱ����ѭ��
            if (StartInputFlag == false)
                continue;

            // ����ģ����������
            if (FirstInputFlag == false)
            {
                FirstInputFlag = true;
                string modelSequenceStr = "";
                for (int i = 0; i < modelVisualizerManager.ModelNames.Count; i++)
                    modelSequenceStr += modelVisualizerManager.ModelNames[i] + " ";

                byte[] modelSequence = System.Text.Encoding.Default.GetBytes(modelSequenceStr);
                client.Send(modelSequence);
            }

            Debug.Log("��¼��ѭ����");



            // ��¼��ѭ������ ��־λ == false ʱ������ѭ��
            while (FinishInputFlag == false)
            {
                Debug.Log("����¼�룺����Ƿ�ʼ¼��");


                // ��ʼ����¼�룬����ѭ��һ��
                if (ModelStartInputFlag == true)
                {

                    Debug.Log("����¼�룺��ʼ¼��");
                    ModelStartInputFlag = false;

                    // �û�ͨ�� ModelFinishInputFlag �����ô�ѭ���Ƿ����
                    var currModelVisualizer = modelVisualizerManager.StartInputModel();
                    bool currModelFinish = false;

                    Debug.Log("����¼�룺֪ͨ Keypose");
                    // ֪ͨ Keypose �˿�ʼ����¼��
                    byte[] startKeypose = System.Text.Encoding.Default.GetBytes("start current model");
                    client.Send(startKeypose);

                    while (currModelFinish == false)
                    {


                        Debug.Log("����¼�룺�ȴ� Keypose");
                        byte[] receivedKeypose = System.Text.Encoding.Default.GetBytes("model prediction received");

                        client.Send(receivedKeypose);

                        var messLenCount = client.Receive(messTmp);



                        if (messLenCount != 0)
                        {
                            Points_Data_Wrapper frame = (Points_Data_Wrapper)JsonUtility.FromJson(Encoding.UTF8.GetString(messTmp, 1, messLenCount - 2), typeof(Points_Data_Wrapper));
                            currModelVisualizer.points_data_wrapper = frame;
                            currModelVisualizer.predFlag = true;
                            Array.Clear(messTmp, 0, messLenCount);
                            Debug.Log("����¼�룺Keypose �ѻظ�");
                        }
                        else
                            Debug.Log("����¼�룺Keypose δ�ظ�");

                        // ������ÿ����ͬģ�͵�¼�����������
                        Debug.Log("����¼�룺�鿴�Ƿ��ѽ���¼��");
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