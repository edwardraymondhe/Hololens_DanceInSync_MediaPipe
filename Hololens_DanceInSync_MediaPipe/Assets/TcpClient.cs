using UnityEngine;
using System.Collections;
//�����
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpClient : MonoBehaviour
{
    public bool IsConnected = false;

    string editString = "hello wolrd"; //�༭������

    Socket serverSocket; //��������socket
    IPAddress ip; //����ip
    IPEndPoint ipEnd;
    public string recvStr; //���յ��ַ���
    public string sendStr; //���͵��ַ���
    byte[] recvData = new byte[4096]; //���յ����ݣ�����Ϊ�ֽ�
    byte[] sendData = new byte[4096]; //���͵����ݣ�����Ϊ�ֽ�
    int recvLen; //���յ����ݳ���
    Thread connectThread; //�����߳�

    //��ʼ��
    public void InitSocket(string ip_str, int port)
    {
        //�����������IP�Ͷ˿ڣ��˿����������Ӧ
        ip = IPAddress.Parse(ip_str); //�����Ǿ�����������ip���˴��Ǳ���
        ipEnd = new IPEndPoint(ip, port);


        //����һ���߳����ӣ�����ģ��������߳̿���
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketConnect()
    {
        IsConnected = false;

        if (serverSocket != null)
            serverSocket.Close();
        //�����׽�������,���������߳��ж���
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        print("ready to connect");
        
        //����
        serverSocket.Connect(ipEnd);
        IsConnected = true;

        //������������յ����ַ���
        recvLen = serverSocket.Receive(recvData);
        recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
        print(recvStr);
    }

    public void SocketSend(string sendStr)
    {
        //��շ��ͻ���
        sendData = new byte[4096];
        //��������ת��
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //����

        Debug.Log("Sending from client str: " + sendData.Length);
        serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    public void SocketSend(byte[] data)
    {
        Debug.Log("Sending from client byte: " + data.Length);
        serverSocket.Send(data, data.Length, SocketFlags.None);
    }

    void SocketReceive()
    {
        SocketConnect();
        //���Ͻ��շ���������������
        while (true)
        {
            recvData = new byte[4096];
            recvLen = serverSocket.Receive(recvData);
            if (recvLen == 0)
            {
                SocketConnect();
                continue;
            }
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            print(recvStr);
        }
    }

    void SocketQuit()
    {
        IsConnected = false;

        //�ر��߳�
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //���رշ�����
        if (serverSocket != null)
            serverSocket.Close();
        print("diconnect");
    }

    void OnGUI()
    {
        editString = GUI.TextField(new Rect(10, 10, 100, 20), editString);
        if (GUI.Button(new Rect(10, 30, 60, 20), "send"))
            SocketSend(editString);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //�����˳���ر�����
    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
