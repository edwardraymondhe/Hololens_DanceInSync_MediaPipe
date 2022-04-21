using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using static Helper.Pose;

public class PoseStageController : BasePrepareStageController
{
    public GameObject chosenObjectPrefab;
    public GameObject chosenObjectGridCollection;
    public List<PoseSequence> poseSequences = new List<PoseSequence>();

    private void Start()
    {
        SpawnCollection();
    }

    protected override void SpawnCollection()
    {
        base.SpawnCollection();

        var index = 0;

        var poseSequenceNames = LoadInstanceNames<PoseSequence>();
        foreach (var poseSequenceName in poseSequenceNames)
        {
            var posePressableButton = Instantiate(objectPrefab, objectCollection.transform);
            posePressableButton.GetComponent<PosePickerPressableButton>().Init(this, index, LoadInstance<PoseSequence>(poseSequenceName), poseSequenceName);
            index++;
        }

        objectCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    public void AddPose(PoseSequence poseSequence)
    {
        poseSequences.Add(poseSequence);
        var obj = Instantiate(chosenObjectPrefab, chosenObjectGridCollection.transform);
        obj.GetComponent<PoseStageChosenItem>().Init(poseSequence);

        UpdateChosenCollection();
    }

    public void UpdateChosenCollection()
    {
        for (int i = chosenObjectGridCollection.transform.childCount - 1; i >= 0; i--)
            chosenObjectGridCollection.transform.GetChild(i).GetComponent<PoseStageChosenItem>().UpdateIndex();

        chosenObjectGridCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    public void RemovePoseAtIndex(int idx)
    {
        Destroy(chosenObjectGridCollection.transform.GetChild(idx).gameObject);
        poseSequences.RemoveAt(idx);

        UpdateChosenCollection();
    }

    public void RemovePose(PoseSequence poseSequence)
    {
        var idx = poseSequences.FindIndex(e => e == poseSequence);
        Destroy(chosenObjectGridCollection.transform.GetChild(idx).gameObject);
        poseSequences.Remove(poseSequence);

        UpdateChosenCollection();
    }

    public void ClearPose()
    {
        poseSequences.Clear();

        for (int i = chosenObjectGridCollection.transform.childCount - 1; i >= 0; i--)
            Destroy(chosenObjectGridCollection.transform.GetChild(i).gameObject);

        UpdateChosenCollection();
    }

    public bool ContainsPose(PoseSequence poseSequence)
    {
        return poseSequences.Contains(poseSequence);
    }
}
