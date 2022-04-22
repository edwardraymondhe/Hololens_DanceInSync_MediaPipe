using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class ModeStageController : BasePrepareStageController
{
    public GameObject chosenObjectPrefab;
    public GameObject chosenObjectGridCollection;
    public List<PoseSequence> poseSequences = new List<PoseSequence>();
    public bool isRhythmMode = true;
    PoseStageController poseStageController;

    private void Start()
    {
        poseStageController = GlobalController.Instance.GetPrepareStage<PoseStageController>();
    }

    public void SetStage(bool isMusicMode)
    {
        this.isRhythmMode = isMusicMode;
        NextStage();
    }

    public override void Init()
    {
        ClearPose();

        this.chosenObjectPrefab = poseStageController.chosenObjectPrefab;
        foreach (var poseSequence in poseStageController.poseSequences)
            AddPose(poseSequence);
    }

    void AddPose(PoseSequence poseSequence)
    {
        poseSequences.Add(poseSequence);
        var obj = Instantiate(chosenObjectPrefab, chosenObjectGridCollection.transform);
        obj.GetComponent<PoseStageChosenItem>().Init(poseSequence);

        UpdateChosenCollection();
    }

    void UpdateChosenCollection()
    {
        for (int i = chosenObjectGridCollection.transform.childCount - 1; i >= 0; i--)
            chosenObjectGridCollection.transform.GetChild(i).GetComponent<PoseStageChosenItem>().UpdateIndex();

        chosenObjectGridCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    void ClearPose()
    {
        poseSequences.Clear();

        for (int i = chosenObjectGridCollection.transform.childCount - 1; i >= 0; i--)
            Destroy(chosenObjectGridCollection.transform.GetChild(i).gameObject);

        UpdateChosenCollection();
    }
}
