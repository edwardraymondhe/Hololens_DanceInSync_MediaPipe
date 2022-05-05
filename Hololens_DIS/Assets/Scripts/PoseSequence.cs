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
        float curCycles = 0.0f;
        float minCyclesRate = 64.0f;
        PoseSequence finalSequence = CreateInstance<PoseSequence>();
        foreach (var sequence in poseSequences)
        {
            finalSequence.fileName += sequence.fileName + " - ";
            finalSequence.poseFrames.AddRange(sequence.poseFrames);
            finalSequence.angles.AddRange(sequence.angles);
            finalSequence.velocities.AddRange(sequence.velocities);
            curCycles += sequence.curCycles;
            rawCycles += sequence.rawCycles;
            minCyclesRate = Mathf.Min(minCyclesRate, sequence.curCycles / sequence.minCycles);
        }
        finalSequence.rawCycles = rawCycles;
        finalSequence.curCycles = curCycles;
        finalSequence.minCycles = rawCycles / minCyclesRate;
        finalSequence.fileName = finalSequence.fileName.Remove(finalSequence.fileName.Length - 3, 3);
        return finalSequence;
    }

}



[System.Serializable]
public class RhythmSequenceBeatData
{
    // section
    public int index;
    public List<RhythmFrameData> data = new List<RhythmFrameData>();
    public RhythmSequenceBeatData(int index)
    {
        this.index = index;
    }
}

[System.Serializable]
public class RhythmSequenceCycleData
{
    // section
    public int index;
    public List<RhythmSequenceBeatData> data = new List<RhythmSequenceBeatData>();
    public RhythmSequenceCycleData(int index)
    {
        this.index = index;
    }
}

[System.Serializable]
public class RhythmSequenceData
{
    public PoseSequence poseSequence;
    public List<RhythmFrameData> recordedFrames = new List<RhythmFrameData>();
    public List<RhythmBeatScore> angleScores = new List<RhythmBeatScore>();
    public List<RhythmBeatScore> velocityScores = new List<RhythmBeatScore>();
    public RhythmSequenceData() { }
    public RhythmSequenceData(PoseSequence poseSequence) { 
        this.poseSequence = poseSequence;
        // TODO: Initialize section -> beats -> score
    }

    public void InitScore(float eightBeatDuration)
    {
        /* TODO: Angle
         * 1. Get all the continuous chosen beats -->>  List<List<beats>>
         * 2. Foreach List<...>, get seqbeat-start, seqbeat-end, get the frames within
         *      a. 1 score -> 0.5 beat -> half the frames
         *      b. get the List<score> for this seqbeat
         */
        float oneBeatDuration = eightBeatDuration / 8.0f;
        bool foundSeqBeats = false;
        List<PoseFrame> curreSeqFrames = new List<PoseFrame>();
        int startBeat = 0;
        int endBeat = 0;
        float startTime = 0.0f;
        float endTime = 0.0f;
        for (int i = 0; i < poseSequence.angles.Count; i++)
        {
            if (poseSequence.angles[i] == true && foundSeqBeats == false)
            {
                foundSeqBeats = true;
                curreSeqFrames = new List<PoseFrame>();
                startBeat = i;
            }

            // TODO: End of curr seq beats
            if ((poseSequence.angles[i] == false || i == poseSequence.angles.Count - 1 ) && foundSeqBeats == true)
            {
                foundSeqBeats = false;
                endBeat = i;

                startTime = oneBeatDuration * startBeat;
                endTime = oneBeatDuration * (endBeat + 1);
                Debug.Log(string.Format("One Seq-Beats: {0} ~ {1}", startTime, endTime));

                float tmpTimer = 0.0f;
                foreach (var poseFrame in poseSequence.poseFrames)
                {
                    // Found the current poseFrame
                    tmpTimer += poseFrame.duration;
                    if (tmpTimer > startTime)
                    {
                        if (tmpTimer < endTime)
                            curreSeqFrames.Add(poseFrame);
                        else
                            break;
                    }
                }

                // TODO: Add this complete seqbeatScore 
                angleScores.Add(new RhythmBeatScore(curreSeqFrames));
                curreSeqFrames.Clear();
            }
        }

        /* TODO: Velocity
         * 1. Get all the continuous chosen beats -->>  List<List<beats>>
         * 2. Foreach List<...>, get seqbeat-start, seqbeat-end, get the frames within
         *      a. 1 score -> 0.5 beat -> half the frames
         *      b. get the List<velocity> for this seqbeat
         */

        foundSeqBeats = false;
        curreSeqFrames = new List<PoseFrame>();
        startBeat = 0;
        endBeat = 0;
        startTime = 0.0f;
        endTime = 0.0f;
        for (int i = 0; i < poseSequence.velocities.Count; i++)
        {
            if (poseSequence.velocities[i] == true && foundSeqBeats == false)
            {
                foundSeqBeats = true;
                curreSeqFrames = new List<PoseFrame>();
                startBeat = i;
            }

            // TODO: End of curr seq beats
            if (foundSeqBeats == true || i == poseSequence.velocities.Count - 1)
            {
                foundSeqBeats = false;
                endBeat = i;

                startTime = eightBeatDuration * startBeat;
                endTime = eightBeatDuration * (endBeat + 1);
                Debug.Log(string.Format("One Seq-Beats: {0} ~ {1}", startTime, endTime));

                float tmpTimer = 0.0f;
                foreach (var poseFrame in poseSequence.poseFrames)
                {
                    // Found the current poseFrame
                    tmpTimer += poseFrame.duration;
                    if (tmpTimer > startTime)
                    {
                        if (tmpTimer < endTime)
                            curreSeqFrames.Add(poseFrame);
                        else
                            break;
                    }
                }

                // TODO: Add this complete seqbeatScore 
                velocityScores.Add(new RhythmBeatScore(curreSeqFrames));
                curreSeqFrames.Clear();
            }
        }
    }

    public void CalculateScore()
    {
        // The beats for calculation
        foreach (var angleScore in angleScores)
        {
            // The std-frames within each beat
            float beatScoreSum = 0.0f;
            int calculatedCount = 0;
            foreach (var containedFrame in angleScore.containedFrames)
            {
                // The recorded frames when training with std
                RhythmFrameData recordedFrames = this.recordedFrames.Find(e => e.referenceFrame == containedFrame);
                if (recordedFrames != null)
                {
                    beatScoreSum += recordedFrames.CalculateAngleScore();
                    calculatedCount++;
                }
                else
                {
                    // TODO: There were no mocap data when training with std
                }
            }

            // TODO: Set the final score for this beat
            angleScore.beatScore = beatScoreSum / calculatedCount;
        }

        return;
    }
}

[System.Serializable]
public class RhythmBeatScore
{
    public List<PoseFrame> containedFrames = new List<PoseFrame>();
    public float beatScore;
    public RhythmBeatScore(List<PoseFrame> frames)
    {
        containedFrames = new List<PoseFrame>(frames);
    }
}

[System.Serializable]
public class RhythmFrameData
{
    public PoseFrame referenceFrame;
    public List<PoseFrame> actualFrames = new List<PoseFrame>();
    public RhythmFrameData(PoseFrame poseFrame)
    {
        this.referenceFrame = poseFrame;
    }

    public float CalculateAngleScore()
    {

        List<Vector3> sumEuler = new List<Vector3>();
        for (int i = 0; i < 19; i++)
            sumEuler.Add(Vector3.zero);

        // Get average from the recorded poses
        foreach (var recordedFrame in actualFrames)
        {
            for (int i = 0; i < recordedFrame.boneQuaternions.Count; i++)
                sumEuler[i] += recordedFrame.boneQuaternions[i].eulerAngles;
        }

        Debug.Log("SumEuler[0]: " + sumEuler[0]);

        float score = 0.0f;
        for (int i = 0; i < sumEuler.Count; i++)
        {
            if (i == 8 || i == 11 || i == 12)
                continue;
            sumEuler[i] = sumEuler[i] / actualFrames.Count;
            Vector3 refEuler = referenceFrame.boneQuaternions[i].eulerAngles;
            score += (1 - ((sumEuler[i] - refEuler).magnitude / refEuler.magnitude));
            Debug.Log(score);
        }

        score /= (19.0f - 3.0f);
        Debug.Log("CalculteAngleScore: " + score);
        return score;
    }

    public float CalculateVelocityScore()
    {
        float score = 0.0f;
        foreach (var recordedFrame in actualFrames)
        {
            // TODO: Do something with the frame
        }

        return score;
    }
}