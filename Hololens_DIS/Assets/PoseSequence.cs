using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pose/New PoseSequence")]
public class PoseSequence : ScriptableBase
{
    /// <summary>
    /// Data structures to store multiple frames of poses.
    /// </summary>
    public List<PoseFrame> poseFrames = new List<PoseFrame>();

    public List<bool> angles = new List<bool>();
    public List<bool> velocities = new List<bool>();

    public void Set(PoseSequence poseSequence)
    {
        this.poseFrames = poseSequence.poseFrames;
        this.fileName = poseSequence.fileName;
        for (int i = 0; i < poseFrames.Count; i++)
        {
            angles.Add(poseSequence.angles[i]);
            velocities.Add(poseSequence.velocities[i]);
        }
    }

    public bool GetAngle(PoseFrame poseFrame)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        return angles[idx];
    }

    public bool GetVelocity(PoseFrame poseFrame)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        return velocities[idx];
    }

    public void SetAngle(PoseFrame poseFrame, bool value)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        angles[idx] = value;
    }

    public void SetVelocity(PoseFrame poseFrame, bool value)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        velocities[idx] = value;
    }

    public void Add(PoseFrame poseFrame)
    {
        poseFrames.Add(poseFrame);
        angles.Add(false);
        velocities.Add(false);
    }

    public void Insert(int idx, PoseFrame poseFrame, bool angle, bool velocity)
    {
        poseFrames.Insert(idx, poseFrame);
        angles.Insert(idx, angle);
        velocities.Insert(idx, velocity);
    }

    public void Remove(PoseFrame poseFrame)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        poseFrames.RemoveAt(idx);
        angles.RemoveAt(idx);
        velocities.RemoveAt(idx);
    }
}
