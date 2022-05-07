using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaTrainStageController : BaseTrainStageController
{
    public List<StaminaSequenceData> staminaSequenceDatas = new List<StaminaSequenceData>();
    [SerializeField]
    StaminaSequenceData lastSequence, currSequence;
    [SerializeField]
    PoseFrame lastFrame, currFrame;

    public override void StartStage()
    {
        /*
         * TODO:
         * 1. Create a new deep copy dictionary for sequence that holds all the infomation
         */
        Debug.Log("Prepare adding mode data");
        Debug.Log("Counter: " + tweakStage.counterModeData.Count);
        Debug.Log("Timer: " + tweakStage.timerModeData.Count);
        foreach (var keyValuePair in tweakStage.counterModeData)
        {
            Debug.Log("Adding counter mode data");
            var poseSequenceDeepCopy = Helper.DeepCopy(keyValuePair.Key);
            var staminaSequenceData = new StaminaSequenceData(poseSequenceDeepCopy);
            staminaSequenceData.Init(true, false, musicStage.chosenEightBeatDuration, keyValuePair.Value);
            staminaSequenceDatas.Add(staminaSequenceData);
        }

        foreach (var keyValuePair in tweakStage.timerModeData)
        {
            Debug.Log("Adding timer mode data");
            var poseSequenceDeepCopy = Helper.DeepCopy(keyValuePair.Key);
            var staminaSequenceData = new StaminaSequenceData(poseSequenceDeepCopy);
            staminaSequenceData.Init(true, true, musicStage.chosenEightBeatDuration, keyValuePair.Value);
            staminaSequenceDatas.Add(staminaSequenceData);
        }

        Debug.Log("Finish adding mode data");

        // Manually start the music stage
        musicStage.audioClipSelector.Play();

        ChooseSequence(staminaSequenceDatas[0]);

        base.StartStage();
    }

    public override void InitStage()
    {
        base.InitStage();

        lastSequence = null;
        currSequence = null;
        lastFrame = null;
        currFrame = null;
        isStageOver = false;
        staminaSequenceDatas.Clear();
    }

    protected override void Update()
    {
        if (!initialized)
            return;

        if (isStageOver)
        {
            // TODO: Music fade out a little bit
            foreach (var rhythmSequenceLength in staminaSequenceDatas)
                rhythmSequenceLength.CalculateScore();

            GlobalController.Instance.NextStage();

            initialized = false;
            return;
        }

        base.Update();
        if (!audioClipSelector.player.audioSource.isPlaying)
            audioClipSelector.Play();

        /*
         * TODO:
         * 1. Check if currSeq is timer or counter
         * 2. If timer
         *      a. countdown
         *      b. check curr == next-milestone
         *      c. if good, proceed
         *      d. if bad, exit
         * 3. else
         *      a. check curr == next-milestone
         *      b. if good, proceed
         *      c. if bad, exit
         */

        


        if (!currSequence.isOver)
        {
            Debug.Log("Update - Count: " + currSequence.stageFrames.Count);
            currSequence.Process(GlobalController.Instance.server.currentPoseFrame);
        }
        else
        {
            var proceedSequence = staminaSequenceDatas.Find(e => e.isOver == false);
            if (proceedSequence != null)
            {
                ChooseSequence(proceedSequence);
            }
            else
            {
                isStageOver = true;
            }
        }
    }

    public void ChooseSequence(string seqName)
    {
        StaminaSequenceData chosenSequence = staminaSequenceDatas.Find(e => e.poseSequence.fileName == seqName);
        ChooseSequence(chosenSequence);
    }
    public void ChooseSequence(StaminaSequenceData chosenSequence)
    {
        lastSequence = currSequence;
        currSequence = chosenSequence;
    }

}