using RhythmTool.Examples;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.SceneManagement;

public class GlobalController : GlobalSingleTon<GlobalController>
{
    /// <summary>
    /// 0: Music
    /// 1: Pose
    /// 2: Mode
    /// 3: Tweak
    /// </summary>
    public int stage = 0;

    public List<BasePrepareStageController> baseStageControllers = new List<BasePrepareStageController>();

    public GameObject screenObjectCollection;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MRTK")
            SetStage(0);
    }

    public T GetStage<T>() where T : BasePrepareStageController
    {
        BasePrepareStageController foundStageController = null;
        foreach (var stageController in baseStageControllers)
        {
            if (stageController is T)
                return stageController as T;
        }

        return foundStageController as T;
    }

    private T SetStage<T>() where T : BasePrepareStageController
    {
        BasePrepareStageController foundStageController = null;
        foreach (var stageController in baseStageControllers)
        {
            if (stageController is T)
            {
                foundStageController = stageController;
                stageController.gameObject.SetActive(true);
            }
            else
                stageController.gameObject.SetActive(false);
        }

        return foundStageController as T;
    }

    public void SetStage(int idx)
    {
        stage = idx;

        switch (stage)
        {
            case 0:
                SetStage<MusicStageController>();
                break;
            case 1:
                SetStage<PoseStageController>();
                break;
            case 2:
                var modeStage = SetStage<ModeStageController>();
                modeStage.Init();
                break;
            case 3:
                var mode = GetStage<ModeStageController>();
                var pose = GetStage<PoseStageController>();
                var tweakStage = SetStage<TweakStageController>();
                tweakStage.Init(mode, pose);
                break;
            default:
                break;
        }

        screenObjectCollection.GetComponent<GridObjectCollection>().UpdateCollection();
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
