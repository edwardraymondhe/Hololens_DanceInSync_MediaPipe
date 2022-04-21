using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseTweakSettingItem : MonoBehaviour
{
    public int itemIndex = 0;
    public string itemName = "";
    public PoseSequence poseSequence;
    public TMP_Text itemText;

    public TMP_Text buttonText;
    public TMP_Text unitText;

    protected float value = 1;
    protected float offset = 0.5f;
    protected TweakStageController stageController;

    public virtual void Init(TweakStageController stageController, PoseSequence poseSequence)
    {
    }

    public virtual void IncrBeats()
    {
    }

    public virtual void DecrBeats()
    {
    }
}
