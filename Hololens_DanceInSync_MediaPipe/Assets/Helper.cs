using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Helper
{
    public static class Socket
    {
        public static int port = 9001;

        private static string ip = "127.0.0.1";

        public static string Ip {
            get
            {
#if UNITY_EDITOR
                return "127.0.0.1";
#else
                return "192.168.0.243";
#endif
            }
        }
    }

    public static class Pose
    {
        #region Raw Landmarks Data
        [System.Serializable]
        public class Landmark
        {
            public float score;
            public float x;
            public float y;
            public float z;

            public Landmark() { }
            public Landmark(float x, float y, float z, float score)
            {
                this.score = score;
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public Landmark( Vector4 landmark)
            {
                x = landmark.x;
                y = landmark.y;
                z = landmark.z;
                score = landmark.w;
            }


            public string GetString()
            {
                return string.Format("Score: {0}, KeyPoint: [{1}, {2}, {3}]", score, x, y, z);
            }
        }

        [System.Serializable]
        public class Landmarks
        {
            public Dictionary<int, Landmark> landmarks = new Dictionary<int, Landmark>();

            public Landmarks() { }
            public Landmarks(Dictionary<int, Landmark> landmarks) { this.landmarks = landmarks; }

            public string GetString()
            {
                string str = "";
                foreach (var landmark in landmarks)
                    str += (landmark.Value.GetString() + "\n");

                return str;
            }
        }
        #endregion

        #region Reference Bone Pairs
        // Upper body - Connected
        // 11, 13, 15
        public static BonePairLink LeftUpperArm_LeftLowerArm_bonePair = new BonePairLink(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm);
        // 12, 14, 16
        public static BonePairLink RightUpperArm_RightLowerArm_bonePair = new BonePairLink(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm);
        // 12, 11, 13
        public static BonePairLink LeftShoulder_LeftUpperArm_bonePair = new BonePairLink(HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm);
        // 11, 12, 14
        public static BonePairLink RightShoulder_RightUpperArm_bonePair = new BonePairLink(HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm);

        // Lower body - Connected
        // 24, 23, 25
        public static BonePairLink Hips_LeftUpperLeg_bonePair = new BonePairLink(HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg);
        // 23, 24, 26
        public static BonePairLink Hips_RightUpperLeg_bonePair = new BonePairLink(HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg);
        // 23, 25, 27
        public static BonePairLink LeftUpperLeg_LeftLowerLeg_bonePair = new BonePairLink(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg);
        // 24, 26, 28
        public static BonePairLink RightUpperLeg_RightLowerLeg_bonePair = new BonePairLink(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg);

        // Spine - Connected
        // 13, 11, 23
        public static BonePairLink Spine_LeftUpperArm_bonePair = new BonePairLink(HumanBodyBones.Spine, HumanBodyBones.LeftUpperArm);
        // 11, 23, 25
        public static BonePairLink Spine_LeftUpperLeg_bonePair = new BonePairLink(HumanBodyBones.Spine, HumanBodyBones.LeftUpperLeg);

        // Spine - Unconnected
        // 14, 12, 11, 23
        public static BonePairLink Spine_RightUpperArm_bonePair = new BonePairLink(HumanBodyBones.Spine, HumanBodyBones.RightUpperArm);
        // 11, 23, 24, 26
        public static BonePairLink Spine_RightUpperLeg_bonePair = new BonePairLink(HumanBodyBones.Spine, HumanBodyBones.RightUpperLeg);


        // Bone -> Start&End Index
        public static Dictionary<HumanBodyBones, Vector2Int> linePair = new Dictionary<HumanBodyBones, Vector2Int>
        {
            { HumanBodyBones.LeftShoulder, new Vector2Int(11, 12) },
            { HumanBodyBones.RightShoulder, new Vector2Int(11, 12) },

            { HumanBodyBones.LeftUpperArm, new Vector2Int(11, 13) },
            { HumanBodyBones.LeftLowerArm, new Vector2Int(13, 15) },
            { HumanBodyBones.RightUpperArm, new Vector2Int(12, 14) },
            { HumanBodyBones.RightLowerArm, new Vector2Int(14, 16) },

            { HumanBodyBones.Hips, new Vector2Int(23, 24) },
            { HumanBodyBones.Spine, new Vector2Int(11, 23) },

            { HumanBodyBones.LeftUpperLeg, new Vector2Int(23, 25) },
            { HumanBodyBones.LeftLowerLeg, new Vector2Int(25, 27) },
            { HumanBodyBones.RightUpperLeg, new Vector2Int(24, 26) },
            { HumanBodyBones.RightLowerLeg, new Vector2Int(26, 28) }
        };

        public static string poseFrameFolder = "data_poseFrame";

        #endregion

        #region File Load & Save

        public static void SaveInstance<T>(T t) where T: ScriptableBase
        {
            Debug.Log(Application.persistentDataPath);

            var directoryPath = GetTypeDirectory<T>();
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // Format to binary
            BinaryFormatter formatter = new BinaryFormatter();

            if (t.fileName == "")
                t.fileName = GetTempFileName<T>();

            var filePath = string.Format("{0}/{1}.txt", directoryPath, t.fileName);
            FileStream file;

            if (File.Exists(filePath))
                file = File.Open(filePath, FileMode.Truncate);
            else
                file = File.Create(filePath);

            var json = JsonConvert.SerializeObject(t);
            formatter.Serialize(file, json);
            file.Close();
        }

        public static string GetTypeDirectory<T>() where T: ScriptableBase
        {
            return string.Format("{0}/data_{1}", Application.persistentDataPath, typeof(T).ToString());
        }

        public static List<string> LoadInstanceNames<T>() where T: ScriptableBase
        {
            BinaryFormatter bf = new BinaryFormatter();
            var directoryPath = GetTypeDirectory<T>();
            var files = Directory.GetFiles(directoryPath, "*.txt");
            List<string> parsedFiles = new List<string>();
            foreach (var file in files)
            {
                var startIdx = file.LastIndexOf(@"\") + 1;
                var parsedFile = file.Substring(startIdx);
                var endIdx = parsedFile.LastIndexOf(".");
                parsedFile = parsedFile.Remove(endIdx);
                parsedFiles.Add(parsedFile);
            }
            return parsedFiles;
        }

        public static T LoadInstance<T>(string fileName) where T: ScriptableBase
        {
            BinaryFormatter bf = new BinaryFormatter();
            var directoryPath = GetTypeDirectory<T>();
            var filePath = string.Format("{0}/{1}.txt", directoryPath, fileName);

            T t = default;
            if (File.Exists(filePath))
            {
                FileStream file = File.Open(filePath, FileMode.Open);
                t = JsonConvert.DeserializeObject<T>((string)bf.Deserialize(file));
                file.Close();
            }

            t.fileName = fileName;
            return t;
        }

        private static string GetTempFilePath<T>() where T: ScriptableBase
        {
            var directoryPath = Application.persistentDataPath + "/data_" + typeof(T).ToString();
            var fileName = GetTempFileName<T>();
            var filePath = directoryPath + "/" + fileName + ".txt";
            return filePath;
        }

        private static string GetTempFileName<T>() where T: ScriptableBase
        {
            int idx = 0;

            var directoryPath = Application.persistentDataPath + "/data_" + typeof(T).ToString();
            while (true)
            {
                var fileName = "Temp_" + idx.ToString();
                var filePath = directoryPath + "/" + fileName + ".txt";
                if (!File.Exists(filePath))
                    return fileName;
                else
                    idx++;
            }
        }
        #endregion
    }

    [System.Serializable]
    public class Timer
    {
        public Text showText;
        public float showTime = 1f;
        public StatFile<StatStruct.Fps> statFile;
        private int count = 0;
        private float deltaTime = 0f;

        public Timer() { }
        public Timer(float showTime)
        {
            this.showTime = showTime;
        }
        public Timer(float showTime, string saveFileName)
        {
            this.showTime = showTime;
            statFile  = new StatFile<StatStruct.Fps>(saveFileName);
        }

        public void CalculateFPS()
        {
            count++;
            deltaTime += Time.deltaTime;
            if (deltaTime >= showTime)
            {
                float fps = count / deltaTime;
                float milliSecond = deltaTime * 1000 / count;
                count = 0;
                deltaTime = 0f;

                var str = string.Format("��ǰÿִ֡�м����{0:0.0} ms ({1:0.} ֡ÿ��)", milliSecond, fps);

                if (showText != null)
                    showText.text = str;
                if (statFile != null)
                    statFile.AddBuffer(new StatStruct.Fps(fps));

            }
        }

        public void EndTimer()
        {
            if (statFile != null)
                statFile.BeginWrite();
        }
    }

    public static List<int> ConvertInt(byte[] bytes)
    {
        int length = bytes.Length / 4;
        List<int> ints = new List<int>();
        for (int i = 0; i < length; i++)
        {
            int count = System.BitConverter.ToInt32(bytes, i * 4);
            ints.Add(count);
        }

        return ints;
    }

    public static List<int> ConvertIntWithCount(byte[] bytes)
    {
        int countLength = System.BitConverter.ToInt32(bytes, 0);
        // Debug.Log("Length: " + countLength);
        List<int> countList = new List<int>();
        for (int i = 0; i < countLength; i++)
        {
            int count = System.BitConverter.ToInt32(bytes, (i + 1) * 4);
            // Debug.Log(count);
            countList.Add(count);
        }

        return countList;
    }

    public static byte[] ConvertHex(List<int> vals)
    {
        int count = 0;
        byte[] hex = new byte[4 * vals.Count];
        foreach (var val in vals)
        {
            int velocity = val;
            int offset = count * 4;
            hex[0 + offset] = (byte)((velocity >> (8 * (0 + offset))) & 0xff);
            hex[1 + offset] = (byte)((velocity >> (8 * (1 + offset))) & 0xff);   //�������������
            hex[2 + offset] = (byte)((velocity >> (8 * (2 + offset))) & 0xff);
            hex[3 + offset] = (byte)((velocity >> (8 * (3 + offset))) & 0xff);
            count++;
        }

        return hex;
    }

    /// <summary>
    /// �ϲ�byte����
    /// </summary>
    /// <param name="sourceBytesArray">Ҫ�ϲ������鼯��</param>
    /// <returns>�ϲ����byte����</returns>
    public static byte[] ConcatBytes(params byte[][] sourceBytesArray)
    {
        int allLength = sourceBytesArray.Sum(o => o.Length);
        byte[] resultBytes = new byte[allLength];
        for (int i = 0; i < sourceBytesArray.Length; i++)
        {
            int copyToIndex = GetCopyToIndex(sourceBytesArray, i);
            sourceBytesArray[i].CopyTo(resultBytes, copyToIndex);
        }
        return resultBytes;
    }

    /// <summary>
    /// ��ȡ���ƿ�ʼ��������
    /// </summary>
    /// <param name="sourceBytesArray">byte[]����������</param>
    /// <param name="index">byte[]���������������</param>
    /// <returns>���ƿ�ʼ��������</returns>
    private static int GetCopyToIndex(byte[][] sourceBytesArray, int index)
    {
        if (index == 0)
        {
            return 0;
        }
        return sourceBytesArray[index - 1].Length + GetCopyToIndex(sourceBytesArray, index - 1);
    }

    /// <summary>
    /// ��ȡ���ͣ����UI
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static List<GameObject> GetOverUI(GameObject canvas)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerEventData, results);
        if (results.Count != 0)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            foreach (var item in results)
                gameObjects.Add(item.gameObject);

            return gameObjects;
        }
        return null;
    }

    public static bool IsUIinView(RectTransform rect)
    {
        bool isInView = false;
        Vector3 worldPos = rect.transform.position;
        Debug.Log(worldPos);
        float leftX = worldPos.x - rect.sizeDelta.x / 2;
        float rightX = worldPos.x + rect.sizeDelta.x / 2;
        if (leftX >= 0 && rightX <= Screen.width)
            isInView = true;

        return isInView;
    }

    public static class StatStruct
    {
        public class ParseData
        {
            public int ProcessFrame;
            public int ReadFrame;
            public int QueueLen;
            public long ParseSpeed;
            public ParseData() { }
            public ParseData(int processFrame, int readFrame, int queue, long speed)
            {
                ProcessFrame = processFrame;
                ReadFrame = readFrame;
                QueueLen = queue;
                ParseSpeed = speed;
            }
        }

        [System.Serializable]
        public class Fps
        {
            public float framePerSecond;
            public Fps() { }
            public Fps(float fps) { framePerSecond = fps; }
        }
    }

    public class StatFile<T> {

        public StatFile() { }
        public StatFile(string name, List<T> contents)
        {
            this.baseFileName = name;
            this.contents = contents;
        }

        public StatFile(string name)
        {
            this.baseFileName = name;
            this.contents = new List<T>();
        }

        public T content;
        public List<T> contents;
        public string baseFileName;

        public void AddBuffer(T content)
        {
            this.contents.Add(content);
        }

        public void BeginWrite()
        {
            int idx = 0;

            while (true)
            {
                var tmpPath = Application.dataPath + "\\Data\\" + baseFileName + " - " + idx.ToString() +".txt";
                if (WriteToFile(tmpPath))
                    return;
                else
                    idx++;
            }
        }

        //д������
        private bool WriteToFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //�ı������ڴ����ı�
                FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.WriteLine("[");
                int idx = 0;
                foreach (var item in contents)
                {
                    idx++;
                    sw.WriteLine(JsonConvert.SerializeObject(item) + (idx == contents.Count ? "" : ","));
                }

                sw.WriteLine("]");

                sw.Close();
                fileStream.Close();
                return true;
            }
            else
                return false;
        }
    }
}
