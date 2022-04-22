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
    public List<BaseReviewStageController> baseReviewControllers = new List<BaseReviewStageController>();

    public GameObject screenObjectCollection;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("MRTK"))
            SetStage(0);
    }

    #region General Stage Control
    public void SetStage(int idx)
    {
        stage = idx;

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
                var mode_tweak = GetPrepareStage<ModeStageController>();
                var pose_tweak = GetPrepareStage<PoseStageController>();
                var tweakStage = SetPrepareStage<TweakStageController>();
                tweakStage.Init(mode_tweak, pose_tweak);
                break;
            case 4:
                var mode_train = GetPrepareStage<ModeStageController>();
                if (mode_train.isRhythmMode)
                {
                    var trainStage = SetTrainStage<RhythmTrainStageController>();
                    trainStage.StartStage();
                }
                else
                {
                    // TODO: Stamina Mode
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
                    reviewStage.Init(review_train);
                }
                else
                {
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
        stage--;
        if (stage < 0)
            return;

        SetStage(stage);
    }
    public void NextStage()
    {
        stage++;
        if (stage > 5)
            return;

        SetStage(stage);
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
        InitializeTrain();
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
            trainStage.InitializeStage();
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
