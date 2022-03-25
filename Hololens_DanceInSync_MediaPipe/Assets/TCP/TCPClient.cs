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
        /// 客户端IP
        /// </summary>
        private string ip;
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }
        /// <summary>
        /// 客户端端口号
        /// </summary>
        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }


        /// <summary>
        /// IP终端
        /// </summary>
        private IPEndPoint ipEndPoint;

        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket mClientSocket;

        //是否连接到服务器
        private bool isConnected = false;
        public bool IsConnected { get => isConnected; set => isConnected = value; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public TCPClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            //初始化IP终端
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            //初始化客户端
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            //创建一个线程以不断连接服务器
            mConnectThread = new Thread(this.ConnectToServer);
            //开启线程
            mConnectThread.Start();
        }

        public Thread mReceiveThread;
        public Thread mConnectThread;

        //连接服务器
        private void ConnectToServer()
        {
            Debug.Log("客户端：新建线程...");
            while (!IsConnected)
            {
                try
                {
                    Debug.Log("客户端：连接中");
                    mClientSocket.Connect(this.ipEndPoint);
                    Debug.Log("客户端：连接成功");
                    this.IsConnected = true;
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("客户端：因为一个错误的发生，暂时无法连接到服务器，错误信息为：{0}", e.Message));
                    mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.IsConnected = false;
                }
                //等待5秒后尝试再次连接
                Thread.Sleep(5000);
                Debug.Log("客服端：正在尝试重新连接...");
            }
            Debug.Log("客户端：连接服务器成功，现在可以和服务器进行会话了");

            //开启线程监听数据接收
            mReceiveThread = new Thread(this.ReceiveMessage);
            mReceiveThread.Start();

            while (mReceiveThread.IsAlive)
            {
                Debug.Log("客户端：线程任务活跃，等待下次检查...");
                Thread.Sleep(1000);
            }

            Debug.Log("客户端：线程任务结束...");
            Debug.Log("客户端：关闭线程...");
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
                    //获取数据长度
                    int receiveLength = this.mClientSocket.Receive(result);
                    //获取服务器消息
                    string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                    //输出服务器消息
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
