using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class StaminaTweakSettingItem : BaseTweakSettingItem
{
    public float timer = 30.0f;
    public float timerOffset = 10.0f;
    
    public float counter = 2.0f;
    public float counterOffset = 1.0f;
    public bool isCounterMode = true;

    public ButtonConfigHelper buttonConfigHelper;


    protected override void Update()
    {
        base.Update();

        buttonText.text = value.ToString();
        buttonConfigHelper.MainLabelText = isCounterMode ? "组" : "秒";
    }

    public void Init(TweakStageController stageController, PoseSequence poseSequence, bool isCounterMode)
    {
        var finalName = (poseSequence.fileName == "" ? "Pose" : poseSequence.fileName);
        this.itemName = finalName;
        itemText.text = finalName;

        this.stageController = stageController;
        this.poseSequence = poseSequence;

        this.stageController.SetSequenceBeats(poseSequence);
        
        // Sets the opposite value, so that default values could be saved
        value = isCounterMode ? timer : counter;
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

            unitText.text = "Group";

            stageController.SetCounterValue(poseSequence, value);
        }
        else
        {
            counter = value;

            value = timer;
            offset = timerOffset;

            unitText.text = "Sec";

            stageController.SetTimerValue(poseSequence, value);
        }

        this.isCounterMode = isCounterMode;
    }

    public override void IncrBeats()
    {
        float targetValue = value + offset;
        Debug.Log(string.Format("Value: {0} -> {1}", value, targetValue));
        value = targetValue;
        SetModeValue();
    }

    public override void DecrBeats()
    {
        float targetValue = value - offset;
        targetValue = Mathf.Max(offset, targetValue);

        Debug.Log(string.Format("Value: {0} -> {1}", value, targetValue));
        value = targetValue;

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

