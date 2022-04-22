using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class BaseReviewStageController : MonoBehaviour
{
    public TMP_Text scoreText;

    public virtual void Init<T>(T trainStageController) where T : BaseTrainStageController
    {
    }

    public void ShowScore(string text)
    {
        scoreText.text = text;
    }

    public virtual void SaveRecord(){ }
}
