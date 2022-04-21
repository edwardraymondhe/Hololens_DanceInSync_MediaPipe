using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhythmTool.Examples;

public abstract class BaseTrainStageController : MonoBehaviour
{
    [SerializeField]
    protected AudioClipSelector audioClipSelector;
    [SerializeField]
    protected MusicStageController musicStage;
    [SerializeField]
    protected PoseStageController poseStage;
    [SerializeField]
    protected ModeStageController modeStage;
    [SerializeField]
    protected TweakStageController tweakStage;

    protected float time = 0.0f;

    protected virtual void Awake()
    {
        GetPrepareStageControllers();
    }

    protected virtual void Update()
    {
        UpdateTrainProgress();
    }

    private void UpdateTrainProgress()
    {
        time = audioClipSelector.player.time;
    }

    private void GetPrepareStageControllers()
    {
        musicStage = GlobalController.Instance.GetStage<MusicStageController>();
        poseStage = GlobalController.Instance.GetStage<PoseStageController>();
        modeStage = GlobalController.Instance.GetStage<ModeStageController>();
        tweakStage = GlobalController.Instance.GetStage<TweakStageController>();

        audioClipSelector = musicStage.audioClipSelector;
    }
}
