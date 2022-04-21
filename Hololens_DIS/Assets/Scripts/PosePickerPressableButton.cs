using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class BasePickerPressableButton : MonoBehaviour
{
    public int buttonIndex = 0;
    public string buttonName = "";
    public TMP_Text text;
}

public class PosePickerPressableButton : BasePickerPressableButton
{
    public PoseSequence poseSequence;
    private PoseStageController stageController;

    public void Init(PoseStageController stageController, int index, PoseSequence poseSequence, string name)
    {
        var finalName = (name == "" ? "Pose" : name);
        this.buttonName = finalName;
        text.text = finalName;

        this.stageController = stageController;
        this.buttonIndex = index;
        this.poseSequence = poseSequence;
    }

    public void TogglePose()
    {
        if (stageController.ContainsPose(poseSequence))
            RemovePose();
        else
            SetPose();
    }

    public void SetPose()
    {
        stageController.AddPose(poseSequence);
    }

    public void RemovePose()
    {
        stageController.RemovePose(poseSequence);
    }
}
