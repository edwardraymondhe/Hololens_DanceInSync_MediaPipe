using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class Helper
{
    public static class Socket
    {
        public static int port = 9001;
        public static string ip = "127.0.0.1";
    }

    public static class Pose
    {

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

            public Landmark(Vector4 landmark)
            {
                this.score = landmark.w;
                x = landmark.x;
                y = landmark.y;
                z = landmark.z;
            }

            public string GetString()
            {
                return string.Format("Score: {0}, KeyPoint: [{1}, {2}, {3}]", score, x, y, z);
            }
        }

        [System.Serializable]
        public class Landmarks
        {
            public List<Landmark> landmarks;

            public Landmarks() { }
            public Landmarks(List<Landmark> landmarks) { this.landmarks = landmarks; }

            public string GetString()
            {
                string str = "";
                foreach (var landmark in landmarks)
                    str += (landmark.GetString() + "\n");

                return str;
            }
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
            hex[1 + offset] = (byte)((velocity >> (8 * (1 + offset))) & 0xff);   //先右移再与操作
            hex[2 + offset] = (byte)((velocity >> (8 * (2 + offset))) & 0xff);
            hex[3 + offset] = (byte)((velocity >> (8 * (3 + offset))) & 0xff);
            count++;
        }

        return hex;
    }

    /// <summary>
    /// 合并byte数组
    /// </summary>
    /// <param name="sourceBytesArray">要合并的数组集合</param>
    /// <returns>合并后的byte数组</returns>
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
    /// 获取复制开始处的索引
    /// </summary>
    /// <param name="sourceBytesArray">byte[]的所在数组</param>
    /// <param name="index">byte[]的所在数组的索引</param>
    /// <returns>复制开始处的索引</returns>
    private static int GetCopyToIndex(byte[][] sourceBytesArray, int index)
    {
        if (index == 0)
        {
            return 0;
        }
        return sourceBytesArray[index - 1].Length + GetCopyToIndex(sourceBytesArray, index - 1);
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
                var tmpPath = Application.dataPath + "\\Data\\" + baseFileName + idx.ToString() +".txt";
                if (WriteToFile(tmpPath))
                    return;
                else
                    idx++;
            }
        }

        //写入数据
        private bool WriteToFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //文本不存在创建文本
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
