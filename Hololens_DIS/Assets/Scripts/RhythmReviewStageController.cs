using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmReviewStageController : BaseReviewStageController
{
    private RhythmTrainStageController rhythmStageController;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public override void InitStage<T>(T trainStageController)
    {
        rhythmStageController = trainStageController as RhythmTrainStageController;
    }

    public override void SaveRecord()
    {

    }
}
