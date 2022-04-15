using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.BlazePose;

public class PoseVisuallizer3D : MonoBehaviour
{
    [Header("Scale Factor")]
    public float xyFactor = 0.15f;
    public float zFactor = 25f;
    public Helper.Timer processImageTimer = new Helper.Timer(1);
    public bool worldCoord = false;
    public float widthFactor = 0.0f;
    public float heightFactor = 0.0f;
    public float depthFactor = 0.0f;


    [SerializeField] Camera mainCamera;
    [SerializeField] RawImage inputImageUI;
    [SerializeField] Shader shader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;
    Material material;
    BlazePoseDetecter detector;

    // Lines count of body's topology.
    const int BODY_LINE_NUM = 35;
    // Pairs of vertex indices of the lines that make up body's topology.
    // Defined by the figure in https://google.github.io/mediapipe/solutions/pose.
    readonly List<Vector4> linePair = new List<Vector4>{
        new Vector4(0, 1), new Vector4(1, 2), new Vector4(2, 3), new Vector4(3, 7), new Vector4(0, 4), 
        new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10),
        new Vector4(11, 12), new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15), 
        new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20), 
        new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24), 
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27), 
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
    };

    /*
        0~32 index datas are pose world landmark.
        Check below Mediapipe document about relation between index and landmark position.
        https://google.github.io/mediapipe/solutions/pose#pose-landmark-model-blazepose-ghum-3d
        Each data factors are
        x, y and z: Real-world 3D coordinates in meters with the origin at the center between hips.
        w: The score of whether the world landmark position is visible ([0, 1]).
        
        33 index data is the score whether human pose is visible ([0, 1]).
        This data is (score, 0, 0, 0).
    */
    Helper.Pose.Landmarks poseLandmarks = new Helper.Pose.Landmarks();

    public PoseEditor poseEditor;

    void Start()
    {
        material = new Material(shader);
        detector = new BlazePoseDetecter();
    }

    void Update(){
        if (inputImageUI.texture != null)
        {
            ProcessImage();
            SetLandmarks(worldCoord);
            ProcessPoseFrame();
            ProcessPoseRecord();
        }
    }

    public RawImage GetInputImage()
    {
        return inputImageUI;
    }

    public PoseFrame lastPoseFrame;
    public PoseFrame currentPoseFrame;

    public PoseRecorder poseRecorder;
    public bool isPoseStartRecording = false;
    public bool isPoseNowRecording = false;

    private void ProcessImage()
    {
        // Predict pose by neural network model.
        detector.ProcessImage(inputImageUI.texture);

        widthFactor = inputImageUI.texture.width * xyFactor;
        heightFactor = inputImageUI.texture.height * xyFactor;
        depthFactor = zFactor;

        processImageTimer.CalculateFPS();
    }
    private void SetLandmarks(bool world)
    {
        Dictionary<int, Helper.Pose.Landmark> landmarks = new Dictionary<int, Helper.Pose.Landmark>();

        if (world)
            for (int i = 0; i < detector.vertexCount + 1; i++)
                poseLandmarks.landmarks[i] = new Helper.Pose.Landmark(detector.GetPoseWorldLandmark(i));
        else
            for (int i = 0; i < detector.vertexCount + 1; i++)
                poseLandmarks.landmarks[i] = new Helper.Pose.Landmark(detector.GetPoseLandmark(i));
    }

    public void ProcessPoseFrame()
    {
        // Initialize and calculate pose frames
        if (currentPoseFrame == null)
            currentPoseFrame = PoseFrame.CreateInstance();

        lastPoseFrame = currentPoseFrame;
        currentPoseFrame = PoseFrame.CreateInstance(lastPoseFrame, Time.deltaTime, poseLandmarks, widthFactor, heightFactor, depthFactor);
    }

    public void ProcessPoseRecord()
    {
        // Pose frames flow control
        if (isPoseStartRecording)
        {
            if (!isPoseNowRecording)
                poseRecorder = new PoseRecorder();
            else
            {
                poseRecorder.SaveSequence();
                poseEditor.RefreshPoseBrowserContent();
            }

            isPoseNowRecording = !isPoseNowRecording;

            isPoseStartRecording = false;
        }

        if (isPoseNowRecording)
            poseRecorder.RecordFrame(currentPoseFrame);
    }

    public void TogglePoseRecording()
    {
        isPoseStartRecording = true;
    }


    [Header("Point Cloud")]
    public GameObject pointCloud;
    public Dictionary<int, GameObject> points;
    public float pointSize = 1.5f;

    #region Visualize Point Cloud

    public void ProcessPointCloud()
    {
        var landmarks = poseLandmarks;

        if (pointCloud == null)
        {
            pointCloud = new GameObject("PointCloud");
            pointCloud.transform.localPosition = new Vector3(0, 0, 0);
        }

        pointCloud.transform.position = new Vector3(-4.0f, -1f, 0.0f);
        pointCloud.transform.localScale = Vector3.one * 0.02f;

        if (points == null)
            points = new Dictionary<int, GameObject>();

        foreach (var landmark in landmarks.landmarks)
            ProcessPoint(landmark.Key, landmark.Value);
    }

    public void ProcessPoint(int idx, Helper.Pose.Landmark landmark)
    {
        if (!points.ContainsKey(idx))
        {
            points[idx] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            points[idx].name = string.Format("Point {0}", idx);
        }

        var point = points[idx];
        point.transform.localPosition = new Vector3(landmark.x * widthFactor, landmark.y * heightFactor, landmark.z * depthFactor);
        point.transform.SetParent(pointCloud.transform);
        point.transform.localScale = Vector3.one * pointSize;
    }

    #endregion

    public PoseFrame poseFrame;

    void OnRenderObject()
    {
        // Use predicted pose world landmark results on the ComputeBuffer (GPU) memory.
        material.SetBuffer("_worldVertices", detector.worldLandmarkBuffer);

        // Set pose landmark counts.
        material.SetInt("_keypointCount", detector.vertexCount);
        material.SetFloat("_humanExistThreshold", humanExistThreshold);
        material.SetVectorArray("_linePair", linePair);
        material.SetMatrix("_invViewMatrix", mainCamera.worldToCameraMatrix.inverse);

        // Draw 35 world body topology lines.
        material.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

        // Draw 33 world landmark points.
        material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detector.vertexCount);
    }

    void OnApplicationQuit()
    {
        // Must call Dispose method when no longer in use.
        processImageTimer.EndTimer();
        detector.Dispose();
    }
}
