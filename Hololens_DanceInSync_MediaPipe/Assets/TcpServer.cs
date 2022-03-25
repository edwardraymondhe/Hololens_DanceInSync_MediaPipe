using UnityEngine;
using System.Collections;
//引入库
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System;

public class TcpServer : MonoBehaviour
{
    public bool IsConnected = false;
    private Queue<byte[]> largeDatas = new Queue<byte[]>();
    
    //以下默认都是私有的成员
    Socket serverSocket; //服务器端socket
    Socket clientSocket; //客户端socket
    IPEndPoint ipEnd; //侦听端口
    public string recvStr; //接收的字符串
    public string sendStr; //发送的字符串
    byte[] recvData = new byte[1024 * 1024 * 10]; //接收的数据，必须为字节
    byte[] sendData = new byte[4096]; //发送的数据，必须为字节
    int recvLen; //接收的数据长度
    Thread connectThread; //连接线程
    Thread connectThread2; //连接线程

    //初始化
    public void InitSocket(int port, bool largeData)
    {
        //定义侦听端口,侦听任何IP
        ipEnd = new IPEndPoint(IPAddress.Any, port);
        //定义套接字类型,在主线程中定义
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //连接
        serverSocket.Bind(ipEnd);
        //开始侦听,最大10个连接
        serverSocket.Listen(10);


        //开启一个线程连接，必须的，否则主线程卡死
        if (largeData)
            connectThread = new Thread(new ThreadStart(SocketReceiveImage));
        else
            connectThread = new Thread(new ThreadStart(SocketReceiveText));

        connectThread.Start();
    }

    //连接
    void SocketConnect()
    {
        if (clientSocket != null)
            clientSocket.Close();

        IsConnected = false;

        //控制台输出侦听状态
        print("Waiting for a client");
        //一旦接受连接，创建一个客户端
        clientSocket = serverSocket.Accept();

        IsConnected = true;

        //获取客户端的IP和端口
        IPEndPoint ipEndClient = (IPEndPoint)clientSocket.RemoteEndPoint;
        //输出客户端的IP和端口
        print("Connect with " + ipEndClient.Address.ToString() + ":" + ipEndClient.Port.ToString());
        //连接成功则发送数据
        sendStr = "Welcome to my server";
        SocketSend(sendStr);
    }

    public void SocketSend(string sendStr)
    {
        //清空发送缓存
        sendData = new byte[4096];
        //数据类型转换
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //发送
        clientSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    public int GetLargeDatasCount()
    {
        return largeDatas.Count;
    }

    public byte[] DequeLargeData()
    {
        return largeDatas.Dequeue();
    }


    MemoryStream ms = null;
    public int readTimes = 0;
    //服务器接收
    void SocketReceiveImage()
    {
        //连接
        SocketConnect();
        //进入接收循环
        while (true)
        {
            //对data清零
            recvData = new byte[1024 * 1024 * 10];
            //获取收到的数据的长度
            recvLen = clientSocket.Receive(recvData);

            //如果收到的数据长度为0，则重连并进入下一个循环
            if (recvLen == 0)
            {
                SocketConnect();
                continue;
            }

            ms = new MemoryStream(recvData, 0, recvLen);
            largeDatas.Enqueue(ms.ToArray());
            readTimes++;
            if (readTimes > 5000)
            {
                readTimes = 0;
                GC.Collect(2);
            }
            SocketSend("Received Image from client.");
        }
    }

    void SocketReceiveText()
    {
        // 连接
        SocketConnect();
        // 进入接受循环
        while(true)
        {
            //对data清零
            recvData = new byte[1024 * 1024 * 10];
            //获取收到的数据的长度
            recvLen = clientSocket.Receive(recvData);

            //如果收到的数据长度为0，则重连并进入下一个循环
            if (recvLen == 0)
            {
                SocketConnect();
                continue;
            }

            //输出接收到的数据
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            //将接收到的数据经过处理再发送出去
            sendStr = "Received Landmarks from client.";
            SocketSend(sendStr);
        }
    }

    //连接关闭
    void SocketQuit()
    {
        IsConnected = false;

        //先关闭客户端
        if (clientSocket != null)
            clientSocket.Close();
        //再关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        if (connectThread2 != null)
        {
            connectThread2.Interrupt();
            connectThread2.Abort();
        }

        //最后关闭服务器
        serverSocket.Close();
        print("diconnect");
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
