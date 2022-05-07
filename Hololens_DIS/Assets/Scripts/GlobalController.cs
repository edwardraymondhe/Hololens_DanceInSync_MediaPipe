using RhythmTool.Examples;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.SceneManagement;


#region Setting
[System.Serializable]
public class Editor
{
    public float expandSpeed = 0.1f;
    public float expandTimer = 0.5f;
    public float humanoidPlaySpeed = 0.1f;
    public float cycleDuration = 4.5f;
}
[System.Serializable]
public class Setting
{
    public int port = 9002;
    private string ip = "127.0.0.1";
    public string Ip
    {
        get
        {
#if UNITY_EDITOR
                return "127.0.0.1";
#else
            return "192.168.0.243";
#endif
        }
    }

    public float processFPS = 30.0f;
    public bool mockMocap = false;
    public Vector3 mockMocapOffset = new Vector3(3, 3, 3);
    public float mocapScore = 10.0f;

    public Editor editor = new Editor();
}
#endregion

public class GlobalController : GlobalSingleTon<GlobalController>
{
    #region Stage
    /// <summary>
    /// 0: Music
    /// 1: Pose
    /// 2: Mode
    /// 3: Tweak
    /// </summary>
    public int stage = -1;

    public List<BasePrepareStageController> basePrepareControllers = new List<BasePrepareStageController>();
    public List<BaseTrainStageController> baseTrainControllers = new List<BaseTrainStageController>();
    public List<BaseReviewStageController> baseReviewControllers = new List<BaseReviewStageController>();

    public GameObject screenObjectCollection;
    #endregion

    public Setting setting = new Setting();

    public MediaPipeServer server;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("MRTK"))
            SetStage(0);
    }

    #region General Stage Control
    public void SetStage(int idx)
    {
        bool acrossStage = false;
        if ((stage < 0) || (stage >= 0 && stage <= 3 && idx > 3) || (idx >= 0 && idx <= 3 && stage > 3))
            acrossStage = true;

        Debug.Log(string.Format("Setting stage: {0} -> {1}", stage, idx));
        stage = idx;

        switch (stage)
        {
            case 0:
                var musicStage = SetPrepareStage<MusicStageController>();
                musicStage.InitStage(acrossStage);
                break;

            case 1:
                var poseStage = SetPrepareStage<PoseStageController>();
                poseStage.InitStage(acrossStage);
                break;

            case 2:
                var modeStage = SetPrepareStage<ModeStageController>();
                modeStage.InitStage(acrossStage);
                break;

            case 3:
                var mode_tweak = GetPrepareStage<ModeStageController>();
                var pose_tweak = GetPrepareStage<PoseStageController>();
                var tweakStage = SetPrepareStage<TweakStageController>();
                tweakStage.InitStage(acrossStage, mode_tweak, pose_tweak);
                break;

            case 4:
                var mode_train = GetPrepareStage<ModeStageController>();
                if (mode_train.isRhythmMode)
                {
                    var trainStage = SetTrainStage<RhythmTrainStageController>();
                    trainStage.InitStage();
                    trainStage.StartStage();
                }
                else
                {
                    var trainStage = SetTrainStage<StaminaTrainStageController>();
                    trainStage.InitStage();
                    trainStage.StartStage();
                }

                // TODO: Sets corresponding near menu buttons
                // TODO: Sets corresponding hand menu buttons

                break;

            case 5:
                var mode_review = GetPrepareStage<ModeStageController>();

                if (mode_review.isRhythmMode)
                {
                    var review_train = GetTrainStage<RhythmTrainStageController>();
                    var reviewStage = SetReviewStage<RhythmReviewStageController>();
                    reviewStage.InitStage(review_train);
                }
                else
                {
                    var review_train = GetTrainStage<StaminaTrainStageController>();
                    var reviewStage = SetReviewStage<StaminaReviewStageController>();
                    reviewStage.InitStage(review_train);
                    // TODO: Stamina Mode
                }

                // TODO: Sets corresponding near menu buttons
                // TODO: Sets corresponding hand menu buttons

                break;

            default:
                break;
        }

        screenObjectCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }
    public void PrevStage()
    {
        int targetStage = stage - 1;
        if (targetStage < 0)
            return;

        SetStage(targetStage);
    }
    public void NextStage()
    {
        int targetStage = stage + 1;
        if (targetStage > 5)
            return;

        SetStage(targetStage);
    }
    #endregion

    #region Prepare Stage Control
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
        CloseReviewStage();

        return foundStageController as T;
    }
    private void ClosePrepareStage()
    {
        foreach (var stageController in basePrepareControllers)
            stageController.gameObject.SetActive(false);
    }
    #endregion

    #region Train Stage Control
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
        CloseReviewStage();

        return foundStageController as T;
    }
    private void CloseTrainStage()
    {
        foreach (var stageController in baseTrainControllers)
            stageController.gameObject.SetActive(false);
    }
    public void RestartTrain()
    {
        // InitializeTrain();
        SetStage(4);
    }
    public void FromTrainToMusic()
    {
        InitializeTrain();
        SetStage(0);
    }
    private void InitializeTrain()
    {
        var modeStage = GetPrepareStage<ModeStageController>();
        if (modeStage.isRhythmMode)
        {
            var trainStage = SetTrainStage<RhythmTrainStageController>();
            trainStage.InitStage();
        }
        else
        {
            // TODO: Stamina Mode
        }
    }
    #endregion

    #region Review Stage Control
    public T GetReviewStage<T>() where T : BaseReviewStageController
    {
        BaseReviewStageController foundStageController = null;
        foreach (var stageController in baseReviewControllers)
        {
            if (stageController is T)
                return stageController as T;
        }

        return foundStageController as T;
    }
    private T SetReviewStage<T>() where T : BaseReviewStageController
    {
        BaseReviewStageController foundStageController = null;
        foreach (var stageController in baseReviewControllers)
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
        CloseTrainStage();

        return foundStageController as T;
    }
    private void CloseReviewStage()
    {
        foreach (var stageController in baseReviewControllers)
            stageController.gameObject.SetActive(false);
    }
    #endregion


}
