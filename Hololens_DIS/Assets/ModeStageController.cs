using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeStageController : PoseStageController
{
    PoseStageController poseStageController;
    private void Start()
    {
        poseStageController = GlobalController.Instance.poseStage;
    }

    public void Init()
    {
        ClearPose();

        this.chosenObjectPrefab = poseStageController.chosenObjectPrefab;
        foreach (var poseSequence in poseStageController.poseSequences)
            AddPose(poseSequence);
    }
}
