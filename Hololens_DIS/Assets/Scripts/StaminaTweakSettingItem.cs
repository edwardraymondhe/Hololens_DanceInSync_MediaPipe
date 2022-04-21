using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class StaminaTweakSettingItem : BaseTweakSettingItem
{
    public float timer = 10.0f;
    public float timerOffset = 10.0f;
    
    public float counter = 2.0f;
    public float counterOffset = 0.5f;
    public bool isCounterMode = true;

    public ButtonConfigHelper buttonConfigHelper;
    
    private void Update()
    {
        buttonText.text = value.ToString();
        buttonConfigHelper.MainLabelText = isCounterMode ? "Counter" : "Timer";
    }

    public void Init(TweakStageController stageController, PoseSequence poseSequence, bool isCounterMode)
    {
        var finalName = (poseSequence.fileName == "" ? "Pose" : poseSequence.fileName);
        this.itemName = finalName;
        itemText.text = finalName;

        this.stageController = stageController;
        this.poseSequence = poseSequence;

        this.stageController.SetSequenceBeats(poseSequence, value);
        
        SetMode(isCounterMode);
    }

    public void ToggleMode()
    {
        SetMode(!isCounterMode);
    }

    public void SetMode(bool isCounterMode)
    {
        if (isCounterMode)
        {
            timer = value;

            value = counter;
            offset = counterOffset;

            unitText.text = "Sec";
        }
        else
        {
            counter = value;

            value = timer;
            offset = timerOffset;

            unitText.text = "Group";
        }
        
        this.isCounterMode = isCounterMode;
    }

    public override void IncrBeats()
    {
        value += offset;

        SetModeValue();
    }

    public override void DecrBeats()
    {
        value -= offset;
        value = Mathf.Max(offset, value);

        SetModeValue();
    }

    private void SetModeValue()
    {
        if (isCounterMode)
            stageController.SetCounterValue(poseSequence, value);
        else
            stageController.SetTimerValue(poseSequence, value);
    }
}

