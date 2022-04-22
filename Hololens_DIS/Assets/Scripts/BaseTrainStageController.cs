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
    [SerializeField]
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
        musicStage = GlobalController.Instance.GetPrepareStage<MusicStageController>();
        poseStage = GlobalController.Instance.GetPrepareStage<PoseStageController>();
        modeStage = GlobalController.Instance.GetPrepareStage<ModeStageController>();
        tweakStage = GlobalController.Instance.GetPrepareStage<TweakStageController>();

        audioClipSelector = musicStage.audioClipSelector;
    }

    public void Pause()
    {
        audioClipSelector.PauseSong();
    }

    public void UnPause()
    {
        audioClipSelector.UnPauseSong();
    }

    public void TogglePause()
    {
        if (audioClipSelector.IsPlaying())
            audioClipSelector.PauseSong();
        else
            audioClipSelector.UnPauseSong();
    }
}
