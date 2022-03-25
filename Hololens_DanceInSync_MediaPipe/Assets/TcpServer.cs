using UnityEngine;
using System.Collections;
//�����
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
    
    //����Ĭ�϶���˽�еĳ�Ա
    Socket serverSocket; //��������socket
    Socket clientSocket; //�ͻ���socket
    IPEndPoint ipEnd; //�����˿�
    public string recvStr; //���յ��ַ���
    public string sendStr; //���͵��ַ���
    byte[] recvData = new byte[1024 * 1024 * 10]; //���յ����ݣ�����Ϊ�ֽ�
    byte[] sendData = new byte[4096]; //���͵����ݣ�����Ϊ�ֽ�
    int recvLen; //���յ����ݳ���
    Thread connectThread; //�����߳�
    Thread connectThread2; //�����߳�

    //��ʼ��
    public void InitSocket(int port, bool largeData)
    {
        //���������˿�,�����κ�IP
        ipEnd = new IPEndPoint(IPAddress.Any, port);
        //�����׽�������,�����߳��ж���
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //����
        serverSocket.Bind(ipEnd);
        //��ʼ����,���10������
        serverSocket.Listen(10);


        //����һ���߳����ӣ�����ģ��������߳̿���
        if (largeData)
            connectThread = new Thread(new ThreadStart(SocketReceiveImage));
        else
            connectThread = new Thread(new ThreadStart(SocketReceiveText));

        connectThread.Start();
    }

    //����
    void SocketConnect()
    {
        if (clientSocket != null)
            clientSocket.Close();

        IsConnected = false;

        //����̨�������״̬
        print("Waiting for a client");
        //һ���������ӣ�����һ���ͻ���
        clientSocket = serverSocket.Accept();

        IsConnected = true;

        //��ȡ�ͻ��˵�IP�Ͷ˿�
        IPEndPoint ipEndClient = (IPEndPoint)clientSocket.RemoteEndPoint;
        //����ͻ��˵�IP�Ͷ˿�
        print("Connect with " + ipEndClient.Address.ToString() + ":" + ipEndClient.Port.ToString());
        //���ӳɹ���������
        sendStr = "Welcome to my server";
        SocketSend(sendStr);
    }

    public void SocketSend(string sendStr)
    {
        //��շ��ͻ���
        sendData = new byte[4096];
        //��������ת��
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //����
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
    //����������
    void SocketReceiveImage()
    {
        //����
        SocketConnect();
        //�������ѭ��
        while (true)
        {
            //��data����
            recvData = new byte[1024 * 1024 * 10];
            //��ȡ�յ������ݵĳ���
            recvLen = clientSocket.Receive(recvData);

            //����յ������ݳ���Ϊ0����������������һ��ѭ��
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
        // ����
        SocketConnect();
        // �������ѭ��
        while(true)
        {
            //��data����
            recvData = new byte[1024 * 1024 * 10];
            //��ȡ�յ������ݵĳ���
            recvLen = clientSocket.Receive(recvData);

            //����յ������ݳ���Ϊ0����������������һ��ѭ��
            if (recvLen == 0)
            {
                SocketConnect();
                continue;
            }

            //������յ�������
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            //�����յ������ݾ��������ٷ��ͳ�ȥ
            sendStr = "Received Landmarks from client.";
            SocketSend(sendStr);
        }
    }

    //���ӹر�
    void SocketQuit()
    {
        IsConnected = false;

        //�ȹرտͻ���
        if (clientSocket != null)
            clientSocket.Close();
        //�ٹر��߳�
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

        //���رշ�����
        serverSocket.Close();
        print("diconnect");
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
