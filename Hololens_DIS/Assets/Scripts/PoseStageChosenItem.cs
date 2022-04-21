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

    public void Init(PoseSequence poseSequence)
    {
        this.itemName = poseSequence.fileName;
        this.poseSequence = poseSequence;
    }

    public void UpdateIndex()
    {
        this.text.text = (transform.GetSiblingIndex()+1).ToString() + " " + itemName;
    }
}
