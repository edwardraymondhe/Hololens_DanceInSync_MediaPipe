using UnityEngine;
using UnityEngine.Video;

public class WebCamInput : MonoBehaviour
{
    public WebCamDevice webCamDevice;
    public int webCamDeviceIndex = 0;
    public bool useWebCam = true;

    public Vector2 webCamResolution = new Vector2(1920, 1080);
    [SerializeField] Texture staticInput;

    // Provide input image Texture.
    public Texture InputImageTexture
    {
        get
        {
            if (!useWebCam && staticInput != null)
            {
                return staticInput;
            }

            return inputRT;
        }
    }

    WebCamTexture webCamTexture;
    RenderTexture inputRT;

    void Start()
    {
        PlayVideo();
    }

    void Update()
    {
        if (!useWebCam && staticInput != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                GetComponent<VideoPlayer>().Stop();

            return;
        }

        if (InputImageTexture == null || !webCamTexture.didUpdateThisFrame) return;



        var aspect1 = (float)webCamTexture.width / webCamTexture.height;
        var aspect2 = (float)inputRT.width / inputRT.height;
        var aspectGap = aspect2 / aspect1;

        var vMirrored = webCamTexture.videoVerticallyMirrored;
        var scale = new Vector2(aspectGap, vMirrored ? -1 : 1);
        var offset = new Vector2((1 - aspectGap) / 2, vMirrored ? 1 : 0);

        Graphics.Blit(webCamTexture, inputRT, scale, offset);
    }

    public void PlayDevice()
    {
        GetComponent<VideoPlayer>().Stop();

        useWebCam = true;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                webCamDeviceIndex = 1 - webCamDeviceIndex;
                break;
            default:
                webCamDeviceIndex = 0;
                break;
        }

        webCamDevice = WebCamTexture.devices[webCamDeviceIndex];
        
        ClearTexture();

        var name = webCamDevice.name;

        var resolution = GetResolution();

        inputRT = new RenderTexture((int)resolution.x, (int)resolution.y, 0);
        webCamTexture = new WebCamTexture(name, (int)resolution.x, (int)resolution.y);
        webCamTexture.Play();
    }

    public void PlayVideo()
    {
        useWebCam = false;
        ClearTexture();

        var name = "";

        var videoPlayer = GetComponent<VideoPlayer>();
        var resolution = GetResolution();

        staticInput = new RenderTexture((int)resolution.x, (int)resolution.y, 0);
        videoPlayer.targetTexture = (RenderTexture)staticInput;
        webCamTexture = new WebCamTexture(name, (int)resolution.x, (int)resolution.y);
        webCamTexture.Play();
    }

    public Vector2 GetResolution()
    {
        if (useWebCam)
            return webCamResolution;
        else
        {
            var clip = GetComponent<VideoPlayer>().clip;
            return new Vector2(clip.width, clip.height);
        }
    }

    private void ClearTexture()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Pause();
            webCamTexture.Stop();
            Destroy(webCamTexture);
        }
        if (inputRT != null)
            Destroy(inputRT);

    }

    void OnDestroy()
    {
        ClearTexture();
    }
}
