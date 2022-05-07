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
    
    [SerializeField]
    protected float value = 1;
    [SerializeField]
    protected float offset = 0.5f;
    protected TweakStageController stageController;

    public HumanoidController humanoid;
    protected float humanoidTimer = 0.0f;

    protected virtual void Update()
    {
        Helper.UpdateHumanoidBySequence(ref humanoidTimer, ref poseSequence, ref humanoid);
    }

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
