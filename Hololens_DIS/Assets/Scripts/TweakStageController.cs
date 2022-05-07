using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;

public class TweakStageController : BasePrepareStageController
{
    public GameObject staminaSettingPrefab;
    public GameObject musicSettingPrefab;

    public List<RhythmSequenceData> rhythmModeData = new List<RhythmSequenceData>();
    // public Dictionary<PoseSequence, float> rhythmModeData = new Dictionary<PoseSequence, float>();

    public Dictionary<PoseSequence, bool> staminaModeData = new Dictionary<PoseSequence, bool>();
    public Dictionary<PoseSequence, float> counterModeData = new Dictionary<PoseSequence, float>();
    public Dictionary<PoseSequence, float> timerModeData = new Dictionary<PoseSequence, float>();
    
    PoseStageController poseStageController;
    ModeStageController modeStageController;

    public void InitStage(bool acrossStage, ModeStageController modeStageController, PoseStageController poseStageController)
    {
        this.poseStageController = poseStageController;
        this.modeStageController = modeStageController;
        SpawnCollection();
    }

    public void SetSequenceBeats(PoseSequence poseSequence)
    {
        SetRhythmModeData(poseSequence);
    }

    public void SetCounterValue(PoseSequence poseSequence, float value)
    {
        Debug.Log("Counter count: " + counterModeData.Count);
        SetStaminaModeData(counterModeData, poseSequence, value);
        SetStaminaModeData(staminaModeData, poseSequence, true);
    }

    public void SetTimerValue(PoseSequence poseSequence, float value)
    {
        Debug.Log("Timer count: " + timerModeData.Count);
        SetStaminaModeData(timerModeData, poseSequence, value);
        SetStaminaModeData(staminaModeData, poseSequence, false);
    }

    public void SetRhythmModeData(PoseSequence poseSequence)
    {
        bool found = false;
        int index = 0;
        foreach (var item in rhythmModeData)
        {
            if (item.poseSequence == poseSequence)
            {
                found = true;
                break;
            }
            index++;
        }

        if (found)
            rhythmModeData[index] = new RhythmSequenceData(poseSequence);
        else
            rhythmModeData.Add(new RhythmSequenceData(poseSequence));
    }

    public void SetStaminaModeData<T>(Dictionary<PoseSequence, T> dict, PoseSequence poseSequence, T value)
    {
        if (dict.ContainsKey(poseSequence))
            dict[poseSequence] = value;
        else
            dict.Add(poseSequence, value);
    }


    protected override void SpawnCollection()
    {
        base.SpawnCollection();

        var index = 0;

        foreach (var poseSequence in poseStageController.poseSequences)
        {
            if (modeStageController.isRhythmMode)
            {
                var tweakMusicSettingItem = Instantiate(musicSettingPrefab, objectCollection.transform);
                tweakMusicSettingItem.GetComponent<MusicTweakSettingItem>().Init(this, poseSequence);
            }
            else
            {
                var tweakMusicSettingItem = Instantiate(staminaSettingPrefab, objectCollection.transform);
                tweakMusicSettingItem.GetComponent<StaminaTweakSettingItem>().Init(this, poseSequence, true);
            }
            index++;
        }
     
        StartCoroutine(RefreshCollection(objectCollection));
    }
}
