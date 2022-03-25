using UnityEngine;

public class WebCamInput : MonoBehaviour
{
    public WebCamDevice webCamDevice;
    public int webCamDeviceIndex = 0;
    public bool useWebCam = true;

    [SerializeField] Vector2 webCamResolution = new Vector2(1920, 1080);
    [SerializeField] Texture staticInput;

    // Provide input image Texture.
    public Texture InputImageTexture
    {
        get
        {
            if (!useWebCam && staticInput != null) return staticInput;
            return inputRT;
        }
    }

    WebCamTexture webCamTexture;
    RenderTexture inputRT;

    void Start()
    {
        // PlayDevice(0);
    }

    void Update()
    {
        if (!useWebCam && staticInput != null) return;
        if (InputImageTexture == null || !webCamTexture.didUpdateThisFrame) return;

        var aspect1 = (float)webCamTexture.width / webCamTexture.height;
        var aspect2 = (float)inputRT.width / inputRT.height;
        var aspectGap = aspect2 / aspect1;

        var vMirrored = webCamTexture.videoVerticallyMirrored;
        var scale = new Vector2(aspectGap, vMirrored ? -1 : 1);
        var offset = new Vector2((1 - aspectGap) / 2, vMirrored ? 1 : 0);

        Graphics.Blit(webCamTexture, inputRT, scale, offset);
    }

    public void PlayDevice(int idx)
    {
        useWebCam = true;
        webCamDevice = WebCamTexture.devices[idx];
        if (webCamDevice.name == "")
        {
            Debug.Log("Unreadable web cam: changing to 0");
            webCamDevice = WebCamTexture.devices[0];
        }
        Debug.Log(webCamDevice.name);
        Play();
    }

    public void PlayVideo()
    {
        useWebCam = false;
        Play();
    }

    private void Play()
    {
        ClearTexture();

        var name = "";
        if (useWebCam)
            name = webCamDevice.name;
        else
            name = "";

        webCamTexture = new WebCamTexture(name, (int)webCamResolution.x, (int)webCamResolution.y);
        webCamTexture.Play();

        inputRT = new RenderTexture((int)webCamResolution.x, (int)webCamResolution.y, 0);
    }

    public void ClearTexture()
    {
        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying) webCamTexture.Stop();

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
