using RhythmTool.Examples;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : GlobalSingleTon<GlobalController>
{
    /// <summary>
    /// 0: Music
    /// 1: Pose
    /// 2: Mode
    /// 3: Tweak
    /// </summary>
    public int stage = 0;

    public MusicStageController musicStage;
    public PoseStageController poseStage;
    public ModeStageController modeStage;

    public void SetStage(int idx)
    {
        stage = idx;

        switch (stage)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                modeStage.Init();
                break;
            case 3:
                break;
            default:
                break;
        }
    }

    public void PrevStage()
    {
        stage--;
        if (stage < 0)
            return;

        SetStage(stage);
    }

    public void NextStage()
    {
        stage++;
        if (stage > 3)
            return;

        SetStage(stage);
    }

    public class PoseSequenceRuntimeData
    {
        public float startTime;
        public float duration;
        public PoseSequenceRuntimeData() { }
        public PoseSequenceRuntimeData(float startTime, float duration) { this.startTime = startTime; this.duration = duration; }
    }

    public Dictionary<PoseSequence, PoseSequenceRuntimeData> runtimeData = new Dictionary<PoseSequence, PoseSequenceRuntimeData>(); 
}
