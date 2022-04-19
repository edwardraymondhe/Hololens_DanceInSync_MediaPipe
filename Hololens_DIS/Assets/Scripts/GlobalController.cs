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

    #region Music Mode
    
    public void SetMusic()
    {

    }

    #endregion

    #region Pose Mode

    public void AddPose()
    {

    }

    public void RemovePose()
    {

    }

    public void ClearPose()
    {

    }

    #endregion

    public class PoseSequenceRuntimeData
    {
        public float startTime;
        public float duration;
        public PoseSequenceRuntimeData() { }
        public PoseSequenceRuntimeData(float startTime, float duration) { this.startTime = startTime; this.duration = duration; }
    }

    public Dictionary<PoseSequence, PoseSequenceRuntimeData> runtimeData = new Dictionary<PoseSequence, PoseSequenceRuntimeData>(); 
}
