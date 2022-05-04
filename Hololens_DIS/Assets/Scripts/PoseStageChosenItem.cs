using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoseStageChosenItem : MonoBehaviour
{
    public int itemIndex = 0;
    public string itemName = "";
    public TMP_Text text;
    public PoseSequence poseSequence;
    public HumanoidController humanoid;
    public float humanoidSpeed = 0.1f;
    private float currentTimer = 0.0f;
    public void Init(PoseSequence poseSequence)
    {
        this.itemName = poseSequence.fileName;
        this.poseSequence = poseSequence;
    }

    public void UpdateIndex()
    {
        this.text.text = (transform.GetSiblingIndex()+1).ToString() + " " + itemName;
    }


    private void Update()
    {
        Helper.UpdateHumanoidBySequence(ref currentTimer, ref poseSequence, ref humanoid);
    }

}
