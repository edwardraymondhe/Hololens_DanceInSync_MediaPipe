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

    public List<BasePrepareStageController> basePrepareControllers = new List<BasePrepareStageController>();
    public List<BaseTrainStageController> baseTrainControllers = new List<BaseTrainStageController>();

    public GameObject screenObjectCollection;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("MRTK"))
            SetStage(0);
    }

    public T GetPrepareStage<T>() where T : BasePrepareStageController
    {
        BasePrepareStageController foundStageController = null;
        foreach (var stageController in basePrepareControllers)
        {
            if (stageController is T)
                return stageController as T;
        }

        return foundStageController as T;
    }

    private T SetPrepareStage<T>() where T : BasePrepareStageController
    {
        BasePrepareStageController foundStageController = null;
        foreach (var stageController in basePrepareControllers)
        {
            if (stageController is T)
            {
                foundStageController = stageController;
                stageController.gameObject.SetActive(true);
            }
            else
                stageController.gameObject.SetActive(false);
        }

        CloseTrainStage();

        return foundStageController as T;
    }

    public T GetTrainStage<T>() where T : BaseTrainStageController
    {
        BaseTrainStageController foundStageController = null;
        foreach (var stageController in baseTrainControllers)
        {
            if (stageController is T)
                return stageController as T;
        }

        return foundStageController as T;
    }

    private T SetTrainStage<T>() where T : BaseTrainStageController
    {
        BaseTrainStageController foundStageController = null;
        foreach (var stageController in baseTrainControllers)
        {
            if (stageController is T)
            {
                foundStageController = stageController;
                stageController.gameObject.SetActive(true);
            }
            else
                stageController.gameObject.SetActive(false);
        }

        ClosePrepareStage();

        return foundStageController as T;
    }

    private void CloseTrainStage()
    {
        foreach (var stageController in baseTrainControllers)
            stageController.gameObject.SetActive(false);
    }

    private void ClosePrepareStage()
    {
        foreach (var stageController in basePrepareControllers)
            stageController.gameObject.SetActive(false);
    }

    public void SetStage(int idx)
    {
        stage = idx;

        Debug.Log(stage);
        switch (stage)
        {
            case 0:
                SetPrepareStage<MusicStageController>();
                break;
            case 1:
                SetPrepareStage<PoseStageController>();
                break;
            case 2:
                var modeStage = SetPrepareStage<ModeStageController>();
                modeStage.Init();
                break;
            case 3:
                var mode = GetPrepareStage<ModeStageController>();
                var pose = GetPrepareStage<PoseStageController>();
                var tweakStage = SetPrepareStage<TweakStageController>();
                tweakStage.Init(mode, pose);
                break;
            case 4:
                Debug.Log("Setting training mode");

                var fin_mode = GetPrepareStage<ModeStageController>();
                Debug.Log("Getting mode stage");
                if (fin_mode.isRhythmMode)
                {
                    Debug.Log("Setting rhythm mode");
                    SetTrainStage<RhythmStageController>();
                }
                else
                {
                }
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
        if (stage > basePrepareControllers.Count + basePrepareControllers.Count - 1)
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
