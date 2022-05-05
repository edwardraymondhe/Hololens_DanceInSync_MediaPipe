using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicTweakSettingItem : BaseTweakSettingItem
{
    protected override void Update()
    {
        base.Update();
        buttonText.text = poseSequence.curCycles.ToString();
    }

    public override void Init(TweakStageController stageController, PoseSequence poseSequence)
    {
        var finalName = (poseSequence.fileName == "" ? "Pose" : poseSequence.fileName);
        this.itemName = finalName;
        itemText.text = finalName;

        this.stageController = stageController;
        this.poseSequence = poseSequence;

        // curCycles = rawCycles * (2^value)
        value = Mathf.Log(poseSequence.curCycles / poseSequence.rawCycles, 2);

        this.stageController.SetSequenceBeats(poseSequence);

        offset = 1.0f;
    }

    public override void IncrBeats()
    {
        value += offset;
        poseSequence.SetCyclePower((int)value);
        value = Mathf.Log(poseSequence.curCycles / poseSequence.rawCycles, 2);

        stageController.SetSequenceBeats(poseSequence);
    }

    public override void DecrBeats()
    {
        value -= offset;
        poseSequence.SetCyclePower((int)value);
        value = Mathf.Log(poseSequence.curCycles / poseSequence.rawCycles, 2);

        stageController.SetSequenceBeats(poseSequence);
    }
}
