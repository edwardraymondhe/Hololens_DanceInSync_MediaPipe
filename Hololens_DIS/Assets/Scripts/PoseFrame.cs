using System.Collections.Generic;
using UnityEngine;
using static Helper.Pose;

[CreateAssetMenu(menuName = "Pose/New PoseFrame")]
public class PoseFrame : ScriptableBase
{
    /// <summary>
    /// Data structure to save angles, angular speeds corresponding to combinations of bones.
    /// </summary>
    public List<BonePair> bonePairs = new List<BonePair>();
    public float duration = 0.5f;
    public bool blank = false;

    public float widthFactor;
    public float heightFactor;
    public float depthFactor;

    #region Initialization

    public PoseFrame() { }

    /// <summary>
    /// Initialize and save a pose at one frame.
    /// </summary>
    /// <param name="lastPoseFrame">Pose data from last frame, to calculate angular acceleration</param>
    /// <param name="duration">Time between current frame and last frame, to calculate angular acceleration</param>
    /// <param name="landmarks">Pose data from current frame</param>
    /// <param name="widthFactor"></param>
    /// <param name="heightFactor"></param>
    /// <param name="depthFactor"></param>
    public static PoseFrame CreateInstance(PoseFrame lastPoseFrame, float duration, Landmarks landmarks, float widthFactor, float heightFactor, float depthFactor)
    {
        var data = CreateInstance<PoseFrame>();
        data.Init(lastPoseFrame, duration, landmarks, widthFactor, heightFactor, depthFactor);
        return data;
    }

    public static PoseFrame CreateInstance()
    {
        var data = CreateInstance<PoseFrame>();
        return data;
    }

    private void Init(PoseFrame lastPoseFrame, float duration, Landmarks landmarks, float widthFactor, float heightFactor, float depthFactor)
    {
        bonePairs = GetInitBonePairs();
        this.widthFactor = widthFactor;
        this.heightFactor = heightFactor;
        this.depthFactor = depthFactor;

        SetParams(lastPoseFrame, duration, landmarks);
    }

    public void SetParams(PoseFrame lastPoseFrame, float duration, Landmarks landmarks)
    {
        foreach (var bonePair in bonePairs)
        {
            List<Vector3> positions = bonePair.bonePairLink.GetImagePositions(landmarks, widthFactor, heightFactor, depthFactor);
            float angle = BonePairLink.GetAngle(positions[0], positions[1], positions[2]);
            bonePair.bonePairStatus.angle = angle;
            var lastFrameBonePair = lastPoseFrame.bonePairs.Find(e => e.bonePairLink.Equals(bonePair.bonePairLink));
            bonePair.bonePairStatus.velocity = (angle - (lastFrameBonePair == null ? angle : lastFrameBonePair.bonePairStatus.angle)) / (duration);
        }
        this.duration = duration;
    }

    public void UpdateParams(PoseFrame lastPoseFrame, float duration)
    {
        foreach (var bonePair in bonePairs)
        {
            var lastFrameBonePair = lastPoseFrame.bonePairs.Find(e => e.bonePairLink.Equals(bonePair.bonePairLink));
            bonePair.bonePairStatus.velocity = (bonePair.bonePairStatus.angle - (lastFrameBonePair == null ? bonePair.bonePairStatus.angle : lastFrameBonePair.bonePairStatus.angle)) / (duration);
        }

        this.duration = duration;
    }

    private static List<BonePair> GetInitBonePairs()
    {
        return new List<BonePair> { 
        // Upper body - Connected
        new BonePair(LeftUpperArm_LeftLowerArm_bonePair, new BonePairStatus()),
        new BonePair(RightUpperArm_RightLowerArm_bonePair, new BonePairStatus()),
        new BonePair(LeftShoulder_LeftUpperArm_bonePair, new BonePairStatus()),
        new BonePair(RightShoulder_RightUpperArm_bonePair, new BonePairStatus()),
                
        // Lower body - Connected
        new BonePair(Hips_LeftUpperLeg_bonePair, new BonePairStatus()),
        new BonePair(Hips_RightUpperLeg_bonePair, new BonePairStatus()),
        new BonePair(LeftUpperLeg_LeftLowerLeg_bonePair, new BonePairStatus()),
        new BonePair(RightUpperLeg_RightLowerLeg_bonePair, new BonePairStatus()),
                
        // Spine - Connected
        new BonePair(Spine_LeftUpperArm_bonePair, new BonePairStatus()),
        new BonePair(Spine_LeftUpperLeg_bonePair, new BonePairStatus()),

        // Spine - Unconnected
        new BonePair(Spine_RightUpperArm_bonePair, new BonePairStatus()),
        new BonePair(Spine_RightUpperLeg_bonePair, new BonePairStatus()),
        };
    }

    #endregion

    #region Featured Functions

    /// <summary>
    /// Compares the pose of current frame with target pose.
    /// </summary>
    /// <param name="lastPoseFrame">Pose data from last frame, to calculate angular acceleration</param>
    /// <param name="duration">Time between current frame and last frame, to calculate angular acceleration</param>
    /// <param name="landmarks">Pose data from current frame</param>
    /// <param name="widthFactor"></param>
    /// <param name="heightFactor"></param>
    /// <param name="depthFactor"></param>
    public void GetScore(PoseFrame lastPoseFrame, float duration, Landmarks landmarks, float widthFactor, float heightFactor, float depthFactor)
    {
        Debug.Log("----------- BEGIN ------------");
        foreach (var bonePair in bonePairs)
        {
            List<Vector3> positions = bonePair.bonePairLink.GetImagePositions(landmarks, widthFactor, heightFactor, depthFactor);
            float angle = BonePairLink.GetAngle(positions[0], positions[1], positions[2]);
            var lastFrameBonePair = lastPoseFrame.bonePairs.Find(e => e.bonePairLink.Equals(bonePair.bonePairLink));
            float velocity = (angle - (lastFrameBonePair == null ? angle : lastFrameBonePair.bonePairStatus.angle)) / (duration);
            // TODO: Compare the scores here
            // Debug.Log(string.Format("{0}\nBase - {1} <-> Std - {2}, Score: {3}", bonePair.Key.GetBoneString(), (int)angle, bonePair.Value.angle, ""));
        }
        Debug.Log("----------- END ------------");
    }

    #endregion
}