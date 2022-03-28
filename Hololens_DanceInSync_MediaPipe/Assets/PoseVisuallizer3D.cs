using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.BlazePose;

public class PoseVisuallizer3D : MonoBehaviour
{
    public float showTime = 1f;
    public Text tvFpsInfo;

    private int count = 0;
    private float deltaTime = 0f;
    

    [SerializeField] Camera mainCamera;
    [SerializeField] WebCamInput webCamInput;
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
        new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10), new Vector4(11, 12), 
        new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15), 
        new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20), 
        new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24), 
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27), 
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
    };

    void Start(){
        material = new Material(shader);
        detector = new BlazePoseDetecter();
    }

    void Update(){
        mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, 0.1f);
    }

    public Texture GetTexture()
    {
        return webCamInput.InputImageTexture;
    }

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

    public Helper.Pose.Landmarks GetLandmarks(bool world)
    {
        List<Helper.Pose.Landmark> landmarks = new List<Helper.Pose.Landmark>();

        if (world)
            for (int i = 0; i < detector.vertexCount + 1; i++)
                landmarks.Add(new Helper.Pose.Landmark(detector.GetPoseWorldLandmark(i)));
        else
            for (int i = 0; i < detector.vertexCount + 1; i++)
                landmarks.Add(new Helper.Pose.Landmark(detector.GetPoseLandmark(i)));

        Helper.Pose.Landmarks PoseLandmarks = new Helper.Pose.Landmarks(landmarks);

        return PoseLandmarks;
    }

    void LateUpdate(){
        if (webCamInput.InputImageTexture != null)
            ProcessImage(webCamInput.InputImageTexture);
    }

    void OnRenderObject(){
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

    void OnApplicationQuit(){
        // Must call Dispose method when no longer in use.
        detector.Dispose();
    }

    void CalculateFPS()
    {
        count++;
        deltaTime += Time.deltaTime;
        if (deltaTime >= showTime)
        {
            float fps = count / deltaTime;
            float milliSecond = deltaTime * 1000 / count;
            string strFpsInfo = string.Format(" 当前每帧渲染间隔：{0:0.0} ms ({1:0.} 帧每秒)", milliSecond, fps);
            count = 0;
            deltaTime = 0f;
        }
    }

    public void ProcessImage(Texture inputTexture)
    {
        inputImageUI.texture = inputTexture;
        // Predict pose by neural network model.
        detector.ProcessImage(inputTexture);
    }

}
