using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmReviewStageController : BaseReviewStageController
{
    private RhythmTrainStageController rhythmStageController;

    public override void Init<T>(T trainStageController)
    {
        rhythmStageController = trainStageController as RhythmTrainStageController;
    }

    public override void SaveRecord()
    {

    }
}
