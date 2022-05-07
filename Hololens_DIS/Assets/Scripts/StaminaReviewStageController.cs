using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaReviewStageController : BaseReviewStageController
{
    private StaminaTrainStageController staminaStageController;
    private void Start()
    {

    }

    private void Update()
    {

    }
    public override void InitStage<T>(T trainStageController)
    {
        staminaStageController = trainStageController as StaminaTrainStageController;
    }

    public override void SaveRecord()
    {

    }
}
