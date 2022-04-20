using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicTweakSettingItem : BaseTweakSettingItem
{
    private void Update()
    {
        buttonText.text = value.ToString();
    }

    public override void Init(TweakStageController stageController, PoseSequence poseSequence)
    {
        var finalName = (poseSequence.fileName == "" ? "Pose" : poseSequence.fileName);
        this.itemName = finalName;
        itemText.text = finalName;

        this.stageController = stageController;
        this.poseSequence = poseSequence;

        this.stageController.SetSequenceBeats(poseSequence, value);

        offset = 0.5f;
    }

    public override void IncrBeats()
    {
        value += offset;
        value = Mathf.Min(value, 8);

        stageController.SetSequenceBeats(poseSequence, value);
    }

    public override void DecrBeats()
    {
        value -= offset;
        value = Mathf.Max(offset, value);

        stageController.SetSequenceBeats(poseSequence, value);
    }
}
