using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// MediaPipe clients will be fetching pose datas from streamed videos, and send to server
/// </summary>
public class MediaPipeClient: MonoBehaviour
{
    public bool worldLandmarks = true;
    public bool runOnStart = false;

    public RawImage displaySendImage;
    public WebCamInput webCamInput;

    public PoseVisuallizer3D poseVisuallizer3D;

    TcpClient landmarkClient;
    TcpClient client;

    private void Start()
    {
        landmarkClient = gameObject.AddComponent<TcpClient>();
        client = gameObject.AddComponent<TcpClient>();

        if (runOnStart)
            StartClient();
    }

    private void Update()
    {
        SendLandmarks();

        var rect = displaySendImage.rectTransform.rect;
        var resolution = webCamInput.GetResolution();
        displaySendImage.GetComponent<RectTransform>().sizeDelta = resolution;
    }

    public void StartClient()
    {
        client.InitSocket(Helper.Socket.ip, Helper.Socket.port);
    }

    public void SendLandmarks()
    {
        if (poseVisuallizer3D != null)
        {
            Texture text = poseVisuallizer3D.GetTexture();
            if (client != null && client.IsConnected && text != null)
            {
                // Resolution
                var resolution = webCamInput.GetResolution();
                var resX = (int)resolution.x;
                var resY = (int)resolution.y;
                byte[] resolutionData = Helper.ConvertHex(new List<int> { resX, resY });
                int resolutionLength = resolutionData.Length;

                // Landmarks
                var landmarks = poseVisuallizer3D.GetLandmarks(true);
                var serializedLandmarks = JsonConvert.SerializeObject(landmarks, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                byte[] landmarksData = new UTF8Encoding().GetBytes(serializedLandmarks);
                int landmarksLength = landmarksData.Length;

                // Image
                Texture2D imageTexture = TextureToTexture2D(text);
                byte[] imageData = imageTexture.EncodeToJPG(100);
                int imageLength = imageData.Length;

                byte[] dataLengths = Helper.ConvertHex(new List<int> { 3, resolutionLength, landmarksLength, imageLength });

                // Concat the datas
                byte[] concatBytes = Helper.ConcatBytes(dataLengths, resolutionData, landmarksData, imageData);

                Destroy(imageTexture);

                SendData(concatBytes);
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
        landmarkClient.SocketSend(message);
    }

    void SendData(byte[] data)
    {
        client.SocketSend(data);
    }

}
