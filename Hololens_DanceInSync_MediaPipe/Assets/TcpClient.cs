using UnityEngine;
using System.Collections;
//引入库
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpClient : MonoBehaviour
{
    public bool IsConnected = false;

    string editString = "hello wolrd"; //编辑框文字

    Socket serverSocket; //服务器端socket
    IPAddress ip; //主机ip
    IPEndPoint ipEnd;
    public string recvStr; //接收的字符串
    public string sendStr; //发送的字符串
    byte[] recvData = new byte[4096]; //接收的数据，必须为字节
    byte[] sendData = new byte[4096]; //发送的数据，必须为字节
    int recvLen; //接收的数据长度
    Thread connectThread; //连接线程

    //初始化
    public void InitSocket(string ip_str, int port)
    {
        //定义服务器的IP和端口，端口与服务器对应
        ip = IPAddress.Parse(ip_str); //可以是局域网或互联网ip，此处是本机
        ipEnd = new IPEndPoint(ip, port);


        //开启一个线程连接，必须的，否则主线程卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketConnect()
    {
        IsConnected = false;

        if (serverSocket != null)
            serverSocket.Close();
        //定义套接字类型,必须在子线程中定义
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        print("ready to connect");
        
        //连接
        serverSocket.Connect(ipEnd);
        IsConnected = true;

        //输出初次连接收到的字符串
        recvLen = serverSocket.Receive(recvData);
        recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
        print(recvStr);
    }

    public void SocketSend(string sendStr)
    {
        //清空发送缓存
        sendData = new byte[4096];
        //数据类型转换
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //发送

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
        //不断接收服务器发来的数据
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

        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭服务器
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

    //程序退出则关闭连接
    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
