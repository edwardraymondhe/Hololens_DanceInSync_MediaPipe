using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweakStageController : BasePrepareStageController
{
    public GameObject staminaSettingPrefab;
    public GameObject musicSettingPrefab;

    public List<KeyValuePair<PoseSequence, float>> rhythmModeData = new List<KeyValuePair<PoseSequence, float>>();
    // public Dictionary<PoseSequence, float> rhythmModeData = new Dictionary<PoseSequence, float>();

    public Dictionary<PoseSequence, bool> staminaModeData = new Dictionary<PoseSequence, bool>();
    public Dictionary<PoseSequence, float> counterModeData = new Dictionary<PoseSequence, float>();
    public Dictionary<PoseSequence, float> timerModeData = new Dictionary<PoseSequence, float>();
    
    PoseStageController poseStageController;
    ModeStageController modeStageController;

    public void Init(ModeStageController modeStageController, PoseStageController poseStageController)
    {
        this.poseStageController = poseStageController;
        this.modeStageController = modeStageController;
        SpawnCollection();
    }

    public void SetSequenceBeats(PoseSequence poseSequence, float value)
    {
        SetModeData(poseSequence, value);
    }

    public void SetCounterValue(PoseSequence poseSequence, float value)
    {
        SetModeData(counterModeData, poseSequence, value);
        SetModeData(staminaModeData, poseSequence, true);
    }

    public void SetTimerValue(PoseSequence poseSequence, float value)
    {
        SetModeData(timerModeData, poseSequence, value);
        SetModeData(staminaModeData, poseSequence, false);
    }

    public void SetModeData(PoseSequence poseSequence, float value)
    {
        bool found = false;
        int index = 0;
        foreach (var item in rhythmModeData)
        {
            if (item.Key == poseSequence)
            {
                found = true;
                break;
            }
            index++;
        }

        if (found)
            rhythmModeData[index] = new KeyValuePair<PoseSequence, float>(poseSequence, value);
        else
            rhythmModeData.Add(new KeyValuePair<PoseSequence, float>(poseSequence, value));
    }

    public void SetModeData<T>(Dictionary<PoseSequence, T> dict, PoseSequence poseSequence, T value)
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
            if (modeStageController.isMusicMode)
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
    }
}
