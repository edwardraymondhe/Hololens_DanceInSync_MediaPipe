using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoseSequenceStatus
{
    public float start;
    public float end;
    public bool angle;
    public bool velocity;
}

[CreateAssetMenu(menuName = "Pose/New PoseSequence")]
public class PoseSequence : ScriptableBase
{
    /// <summary>
    /// Data structures to store multiple frames of poses.
    /// </summary>
    public List<PoseFrame> poseFrames = new List<PoseFrame>();
    public float rawCycles = 1.0f;
    public float curCycles = 1.0f;
    public float minCycles = 1.0f;

    // public List<PoseSequenceStatus> poseSequenceStatuses = new List<PoseSequenceStatus>();

    public List<bool> angles = new List<bool>();
    public List<bool> velocities = new List<bool>();

    public void SetRawCycles(float f)
    {
        rawCycles = f;
        for (int i = 0; i < (int)(f*8); i++)
        {
            angles.Add(false);
            velocities.Add(false);
        }
    }

    public bool CheckCyclePowerValid()
    {
        return ((curCycles / minCycles) % 2.0f == 0.0f) && ((rawCycles / minCycles) % 2.0f == 0.0f) && (minCycles <= curCycles) && (minCycles <= rawCycles);
    }

    public void SetCyclePower(int value)
    {
        float oldDuration = GetDuration();
        float oldCycles = curCycles;
        float newCycles = rawCycles;
        if (value >= 1)
        {
            for (int i = 0; i < value; i++)
                newCycles *= 2;
        }else if (value < 0)
        {
            for (int i = 0; i > value; i--)
                newCycles /= 2;
        }

        newCycles = Mathf.Max(newCycles, minCycles);

        float newDuration = oldDuration / oldCycles * newCycles;
        curCycles = newCycles;




        Debug.Log("Duration: " + newDuration);
        Debug.Log("Cycles: " + newCycles);

        SetDuration(newDuration);
    }

    public float GetDuration()
    {
        float duration = 0.0f;
        foreach (var item in poseFrames)
            duration += item.duration;
        return duration;
    }

    public void SetDuration(float newTotalDuration)
    {
        float oldTotalDuration = 0.0f;
        foreach (var poseFrame in poseFrames)
            oldTotalDuration += poseFrame.duration;

        float factor = newTotalDuration / oldTotalDuration;
        foreach (var poseFrame in poseFrames)
            poseFrame.duration = poseFrame.duration * factor;

        PoseFrame currentPoseFrame;
        PoseFrame lastPoseFrame = null;

        foreach (var poseFrame in poseFrames)
        {
            currentPoseFrame = poseFrame;
            currentPoseFrame.UpdateParams(lastPoseFrame, currentPoseFrame.duration);

            lastPoseFrame = currentPoseFrame;
        }
    }

    public void Set(PoseSequence poseSequence)
    {
        this.poseFrames = poseSequence.poseFrames;
        this.fileName = poseSequence.fileName;

        /*
        for (int i = 0; i < poseFrames.Count; i++)
        {
            angles.Add(poseSequence.angles[i]);
            velocities.Add(poseSequence.velocities[i]);
        }
        */
    }

    /*
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
    */


    public void Add(PoseFrame poseFrame, bool edit = false)
    {
        poseFrames.Add(poseFrame);
        // angles.Add(false);
        // velocities.Add(false);

        // TODO: Recalculate velocities
        if (edit)
            CalculateVelocities();
    }

    public void Insert(int idx, PoseFrame poseFrame, bool angle, bool velocity)
    {
        poseFrames.Insert(idx, poseFrame);
        // angles.Insert(idx, angle);
        // velocities.Insert(idx, velocity);

        // TODO: Recalculate velocities
        CalculateVelocities();
    }

    public void Remove(PoseFrame poseFrame)
    {
        int idx = poseFrames.FindIndex(e => e == poseFrame);
        poseFrames.RemoveAt(idx);
        // angles.RemoveAt(idx);
        // velocities.RemoveAt(idx);

        // TODO: Recalculate velocities
        CalculateVelocities();
    }

    private void CalculateVelocities()
    {
        PoseFrame currentPoseFrame;
        PoseFrame lastPoseFrame = null;
        
        foreach (var poseFrame in poseFrames)
        {
            currentPoseFrame = poseFrame;
            currentPoseFrame.UpdateParams(lastPoseFrame, currentPoseFrame.duration);

            lastPoseFrame = currentPoseFrame;
        }
    }

    public static PoseSequence Concat(List<PoseSequence> poseSequences)
    {
        // TODO: Sets the cycles and min cycles
        float rawCycles = 0.0f;
        float minCyclesRate = 64.0f;
        PoseSequence finalSequence = CreateInstance<PoseSequence>();
        foreach (var sequence in poseSequences)
        {
            finalSequence.fileName += sequence.fileName + " - ";
            finalSequence.poseFrames.AddRange(sequence.poseFrames);

            rawCycles += sequence.curCycles;
            minCyclesRate = Mathf.Min(minCyclesRate, sequence.curCycles / sequence.minCycles);
        }

        finalSequence.SetRawCycles(rawCycles);
        finalSequence.curCycles = rawCycles;
        finalSequence.minCycles = rawCycles / minCyclesRate;
        finalSequence.fileName = finalSequence.fileName.Remove(finalSequence.fileName.Length - 3, 3);
        return finalSequence;
    }

}

[System.Serializable]
public class TrainModeData
{
    public PoseSequence poseSequence;
    public float value;

    public TrainModeData() { }
    public TrainModeData(PoseSequence poseSequence, float value) { this.poseSequence = poseSequence; this.value = value; }
}