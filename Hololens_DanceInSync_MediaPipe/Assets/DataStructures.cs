using System.Collections.Generic;
using UnityEngine;

public static class DataStructures
{
    [System.Serializable]
    public class PoseLandmark
    {
        public float score;
        public float x;
        public float y;
        public float z;

        public PoseLandmark() { }
        public PoseLandmark(float x, float y, float z, float score)
        {
            this.score = score;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public PoseLandmark(Vector4 landmark)
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
    public class PoseLandmarks
    {
        public List<PoseLandmark> landmarks;

        public PoseLandmarks() { }
        public PoseLandmarks(List<PoseLandmark> landmarks) { this.landmarks = landmarks; }

        public string GetString()
        {
            string str = "";
            foreach (var landmark in landmarks)
                str += (landmark.GetString() + "\n");

            return str;
        }
    }
}
