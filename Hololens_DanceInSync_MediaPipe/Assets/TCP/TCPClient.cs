using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace TCPLib
{
    public class TCPClient
    {
        private byte[] result = new byte[4096];

        /// <summary>
        /// �ͻ���IP
        /// </summary>
        private string ip;
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }
        /// <summary>
        /// �ͻ��˶˿ں�
        /// </summary>
        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }


        /// <summary>
        /// IP�ն�
        /// </summary>
        private IPEndPoint ipEndPoint;

        /// <summary>
        /// �ͻ���Socket
        /// </summary>
        private Socket mClientSocket;

        //�Ƿ����ӵ�������
        private bool isConnected = false;
        public bool IsConnected { get => isConnected; set => isConnected = value; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">IP��ַ</param>
        /// <param name="port">�˿ں�</param>
        public TCPClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            //��ʼ��IP�ն�
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            //��ʼ���ͻ���
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            //����һ���߳��Բ������ӷ�����
            mConnectThread = new Thread(this.ConnectToServer);
            //�����߳�
            mConnectThread.Start();
        }

        public Thread mReceiveThread;
        public Thread mConnectThread;

        //���ӷ�����
        private void ConnectToServer()
        {
            Debug.Log("�ͻ��ˣ��½��߳�...");
            while (!IsConnected)
            {
                try
                {
                    Debug.Log("�ͻ��ˣ�������");
                    mClientSocket.Connect(this.ipEndPoint);
                    Debug.Log("�ͻ��ˣ����ӳɹ�");
                    this.IsConnected = true;
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("�ͻ��ˣ���Ϊһ������ķ�������ʱ�޷����ӵ���������������ϢΪ��{0}", e.Message));
                    mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.IsConnected = false;
                }
                //�ȴ�5������ٴ�����
                Thread.Sleep(5000);
                Debug.Log("�ͷ��ˣ����ڳ�����������...");
            }
            Debug.Log("�ͻ��ˣ����ӷ������ɹ������ڿ��Ժͷ��������лỰ��");

            //�����̼߳������ݽ���
            mReceiveThread = new Thread(this.ReceiveMessage);
            mReceiveThread.Start();

            while (mReceiveThread.IsAlive)
            {
                Debug.Log("�ͻ��ˣ��߳������Ծ���ȴ��´μ��...");
                Thread.Sleep(1000);
            }

            Debug.Log("�ͻ��ˣ��߳��������...");
            Debug.Log("�ͻ��ˣ��ر��߳�...");
            mReceiveThread.Join();

            ConnectToServer();
        }

        public void ReceiveMessage()
        {
            bool flag = true;
            int cnt = 0;
            while (flag)
            {
                try
                {
                    //��ȡ���ݳ���
                    int receiveLength = this.mClientSocket.Receive(result);
                    //��ȡ��������Ϣ
                    string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                    //�����������Ϣ
                    cnt++;
                    Debug.Log("Got message: " + cnt);
                    if (cnt > 5)
                    {
                        flag = false;
                        Shutdown();
                    }
                }
                catch (Exception e)
                {
                    flag = false;
                    Shutdown();
                }
            }
        }

        public void SendMessage(string msg)
        {
            try
            {
                if (msg == string.Empty || this.mClientSocket == null) return;

                mClientSocket.Send(Encoding.UTF8.GetBytes(msg));
            }
            catch (Exception e)
            {
                Restart();
                throw;
            }
        }

        private void Shutdown()
        {
            this.mClientSocket.Shutdown(SocketShutdown.Both);
            this.mClientSocket.Close();
            this.IsConnected = false;
        }

        private void Restart()
        {
            Shutdown();

            ConnectToServer();
        }
    }
}
