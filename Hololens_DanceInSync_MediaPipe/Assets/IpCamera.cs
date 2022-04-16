using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

/*
    Õý & Êú£º
    ÊúÆÁ & ¾µÏñ

    ÄæÊ±Õë & ºá£º
    ºáÆÁ & ÎÞ¾µÏñ

    Ë³Ê±Õë & ºá£º
    ºáÆÁ & ·­×ª

    ·´ & Êú£º
    ÊúÆÁ & ·­×ª
*/

public class IpCamera : MonoBehaviour
{
    // private string sourceURL = "http://server/axis-cgi/mjpg/video.cgi";
    public RawImage streamImage;
    public string sourceURL = "http://192.168.0.118:8081/video";
    public Button streamButton;
    public bool isStreamOn = true;
    public float reloadMaxTime = 3.0f;
    public float reloadTimer = 0.0f;
    public GameObject streamToggleIcon;
    private Texture2D texture;
    private Stream stream;
    public bool isStartReload = false;

    private void Start()
    {
        StartStream();
    }

    private void Update()
    {
        if (isStartReload)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer > reloadMaxTime)
            {
                Debug.Log("Reload");
                StartStream();
                isStartReload = false;
            }
        }
    }

    public void ToggleStream()
    {
        isStreamOn = !isStreamOn;
    }


    public void Rotate(bool clockWise)
    {
        streamImage.gameObject.transform.Rotate(new Vector3(0, 0, 1), (clockWise ? -1 : 1) * 90);
    }

    public void Flip()
    {
        var scale = streamImage.gameObject.transform.localScale;
        streamImage.gameObject.transform.localScale = new Vector3(scale.x, -1.0f * scale.y, scale.z);
    }

    private static List<string> frontPortrait = new List<string>
    {
        "ffc?set=off",
        "mirror_flip?set=none",
        "orientation?set=landscape"
    };

    private static List<string> backPortrait = new List<string>
    {
        "ffc?set=on",
        "mirror_flip?set=mirror",
        "orientation?set=landscape"
    };

    private void StartStream()
    {
        if (streamImage.texture != null)
            Destroy(streamImage.texture);

        texture = new Texture2D(0, 0);
        streamImage.texture = texture;
        // create HTTP request
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
        //Optional (if authorization is Digest)
        req.Credentials = new NetworkCredential("", "");
        // get response
        WebResponse resp = req.GetResponse();
        // get response stream
        stream = resp.GetResponseStream();

        foreach (var item in (frontCamera?frontPortrait:backPortrait))
            Request(item);

        StartCoroutine(GetFrame());
    }

    public bool frontCamera = true;

    public void SwichCamera()
    {
        frontCamera = !frontCamera;

        foreach (var item in (frontCamera ? frontPortrait : backPortrait))
            Request(item);
    }

    private void Request(string url)
    {
        // create HTTP request
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://192.168.0.118:8081/settings/" + url);
        //Optional (if authorization is Digest)
        req.Credentials = new NetworkCredential("", "");
        // get response
        WebResponse resp = req.GetResponse();
    }

    private IEnumerator GetFrame()
    {
        Byte[] JpegData = new Byte[1000000];

        while (true)
        {
            int bytesToRead = FindLength(stream);
            // print(bytesToRead);
            if (bytesToRead == -1)
            {
                print("End of stream");
                isStartReload = true;
                streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                break;
            }

            int leftToRead = bytesToRead;

            while (leftToRead > 0)
            {
                leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }

            MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

            texture.LoadImage(ms.GetBuffer());

            if (isStreamOn)
                streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            else
                streamImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

            streamToggleIcon.SetActive(isStreamOn);

            // mediaPipeServer.HandleStream(ms.GetBuffer());

            stream.ReadByte(); // CR after bytes
            stream.ReadByte(); // LF after bytes
        }
    }

    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;
    }
}
