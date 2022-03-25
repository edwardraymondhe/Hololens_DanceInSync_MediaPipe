using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPLib
{
    public class TCPServer
    {
        private byte[] result = new byte[4096];

        private int maxClientCount; //最大的监听数量
        public int MaxClientCount
        {
            get
            {
                return maxClientCount;
            }
            set
            {
                maxClientCount = value;
            }
        }
        //IP地址
        private string ip;
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }
        //端口号
        private int port;
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }
        //客户端列表
        private List<Socket> mClientSockets;
        public List<Socket> ClientSockets
        {
            get
            {
                return mClientSockets;
            }
        }
        //IP终端
        private IPEndPoint iPEndPoint;

        //服务端Socket
        private Socket mServerSocket;

        //当前客户端Socket
        private Socket mClientSocket;
        public Socket ClientSocket
        {
            get
            {
                return mClientSocket;
            }
            set
            {
                mClientSocket = value;
            }
        }

        public string StoredMessage { get => storedMessage; set => storedMessage = value; }

        private string storedMessage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="count">监听的最大数量</param>
        public TCPServer(int port, int count)
        {
            this.ip = IPAddress.Any.ToString();
            this.port = port;
            this.maxClientCount = count;

            this.mClientSockets = new List<Socket>();
            //初始化IP终端
            this.iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //初始化服务端Socket
            this.mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //端口绑定
            this.mServerSocket.Bind(this.iPEndPoint);
            //设置监听数目
            this.mServerSocket.Listen(maxClientCount);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="count">监听的最大数目</param>
        public TCPServer(string ip, int port, int count)
        {
            this.ip = ip;
            this.port = port;
            this.maxClientCount = count;

            this.mClientSockets = new List<Socket>();

            //初始化IP终端
            this.iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //初始化服务端Socket
            this.mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //端口绑定
            this.mServerSocket.Bind(this.iPEndPoint);
            //设置监听数目
            this.mServerSocket.Listen(maxClientCount);

        }

        public void Start()
        {
            //创建服务端线程，实现客户端连接请求的循环监听
            var mServerThread = new Thread(this.ListenClientConnect);
            mServerThread.Start();
        }

        //监听客户端连接
        private void ListenClientConnect()
        {
            //设置循环标志位
            bool flag = true;
            while (flag)
            {
                //获取连接服务端的客户端
                this.ClientSocket = this.mServerSocket.Accept();
                //将获取的客户端添加到客户端列表
                this.mClientSockets.Add(this.ClientSocket);
                this.SendMessage(string.Format("客户端{0}已成功连接到服务器", this.ClientSocket.RemoteEndPoint));
                //创建客户端消息线程，实现客户端消息的循环监听
                var mReceiveThread = new Thread(this.ReceiveClient);
                mReceiveThread.Start(this.ClientSocket);
            }
        }
        //接受客户端消息
        private void ReceiveClient(object obj)
        {
            //获取当前客户端
            var mClientSocket = (Socket)obj;
            bool flag = true;
            while (flag)
            {
                try
                {
                    //获取数据的长度
                    int receiveLength = mClientSocket.Receive(result);
                    //获取客户端消息
                    string clientMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                    storedMessage = clientMessage;
                    //服务端负责将客户端的消息分发给各个客户端
                    this.SendMessage(string.Format("客户端{0}发来消息:{1}", mClientSocket.RemoteEndPoint, clientMessage));
                }
                catch (Exception e)
                {
                    //从客户端列表移除此客户端
                    this.mClientSockets.Remove(mClientSocket);
                    this.SendMessage(string.Format("服务器发来消息:客户端{0}从服务器断开,断开原因:{1}", mClientSocket.RemoteEndPoint, e.Message));
                    //断开连接
                    mClientSocket.Shutdown(SocketShutdown.Both);
                    mClientSocket.Close();
                    break;
                }
            }
        }

        //向所有的客户端群发消息
        public void SendMessage(string msg)
        {
            if (msg == string.Empty || this.mClientSockets.Count <= 0) return;
            foreach (Socket s in this.mClientSockets)
            {
                (s as Socket).Send(Encoding.UTF8.GetBytes(msg));
            }
        }

        /// <summary>
        /// 向指定的客户端发送消息
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">port</param>
        /// <param name="msg">message</param>
        public void SendMessage(string ip, int port, string msg)
        {
            //构造出一个终端地址
            IPEndPoint _IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //遍历所有客户端
            foreach (Socket s in mClientSockets)
            {
                if (_IPEndPoint == (IPEndPoint)s.RemoteEndPoint)
                {
                    s.Send(Encoding.UTF8.GetBytes(msg));
                }
            }
        }
    }
}
