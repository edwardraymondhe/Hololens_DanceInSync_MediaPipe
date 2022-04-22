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
    [SerializeField]
    protected bool initialized = false;

    protected virtual void Update()
    {
        UpdateTrainProgress();
    }

    private void UpdateTrainProgress()
    {
        time = audioClipSelector.player.time;
    }

    public virtual void StartStage()
    {
        musicStage = GlobalController.Instance.GetPrepareStage<MusicStageController>();
        poseStage = GlobalController.Instance.GetPrepareStage<PoseStageController>();
        modeStage = GlobalController.Instance.GetPrepareStage<ModeStageController>();
        tweakStage = GlobalController.Instance.GetPrepareStage<TweakStageController>();

        audioClipSelector = musicStage.audioClipSelector;

        initialized = true;
    }

    public virtual void InitializeStage()
    {
        // TODO: Reset all the data
        initialized = false;
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
