using UnityEngine;
using TCPLib;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using UnityEngine.UI;

/// <summary>
/// MediaPipe clients will be fetching pose datas from streamed videos, and send to server
/// </summary>
public class MediaPipeClient: MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 6001;

    public bool worldLandmarks = true;
    public bool runOnStart = false;

    public RawImage displaySendImage;

    TCPClient client;
    public PoseVisuallizer3D poseVisuallizer3D;

    public TcpClient landmarkTcpClient;
    public TcpClient imageTcpClient;

    private void Start()
    {
        if (runOnStart)
            StartClient();
    }

    private void Update()
    {
        SendLandmarks();
    }

    public void StartClient()
    {
        landmarkTcpClient.InitSocket(ip, port);
        imageTcpClient.InitSocket(ip, port + 1);
    }

    public void SendLandmarks()
    {
        if (poseVisuallizer3D != null)
        {
            Texture text = poseVisuallizer3D.GetTexture();
            if (landmarkTcpClient != null && landmarkTcpClient.IsConnected && text != null)
            {
                // Send Landmarks
                var landmarks = poseVisuallizer3D.GetLandmarks(true);
                var serializedLandmarks = JsonConvert.SerializeObject(landmarks, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                SendData(serializedLandmarks);


                // Send Image
                Texture2D img_text = TextureToTexture2D(text);
                byte[] img_bytes = img_text.EncodeToJPG(100);
                SendData(img_bytes);
            }
        }
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

    void SendData(string message)
    {
        landmarkTcpClient.SocketSend(message);
    }

    void SendData(byte[] data)
    {
        imageTcpClient.SocketSend(data);
    }

}
