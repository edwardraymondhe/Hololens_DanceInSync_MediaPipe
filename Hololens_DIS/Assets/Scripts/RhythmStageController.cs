using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmStageController : BaseTrainStageController
{
    public List<KeyValuePair<PoseSequence, float>> rhythmModeData = new List<KeyValuePair<PoseSequence, float>>();
    [SerializeField]
    private float processedTimer = 0.0f;
    [SerializeField]
    private float eightBeatTimer = 0.0f;
    [SerializeField]
    private float trainedTimer = 0.0f;
    [SerializeField]
    KeyValuePair<PoseSequence, float> currSequence, nextSequence;
    [SerializeField]
    PoseFrame currFrame, nextFrame;
    protected override void Awake()
    {
        base.Awake();

        /*
         * TODO:
         * 1. Create a new deep copy dictionary for sequence that holds all the infomation
         */

        foreach (var keyValuePair in tweakStage.rhythmModeData)
        {
            var poseSequenceDeepCopy = (PoseSequence)Helper.DeepCopy(keyValuePair.Key);
            var poseSequenceLength = musicStage.chosenEightBeatDuration * keyValuePair.Value;
            poseSequenceDeepCopy.FitTotalDuration(poseSequenceLength);

            rhythmModeData.Add(new KeyValuePair<PoseSequence, float>(poseSequenceDeepCopy, keyValuePair.Value));
        }

        // Eight beat timer gives the blank-out when an 8Beat is not fully used
        this.eightBeatTimer = (Mathf.Ceil(currSequence.Value) - currSequence.Value) * musicStage.chosenEightBeatDuration;

        // Sets the first poseSequence
        currSequence = rhythmModeData[0];

        // Manually start the music stage
        musicStage.audioClipSelector.Replay();
    }
    private bool isEightBeatCountDown = false;
    private bool isStageOver = false;

    protected override void Update()
    {
        if (isStageOver)
        {
            GlobalController.Instance.NextStage();
            return;
        }

        base.Update();

        // TODO: Prepare before song starts
        if (time <= musicStage.chosenStartTime)
        {
            // If music is starting over, and sequence isn't done...
            return;
        }

        // TODO: When song is over
        if (time > audioClipSelector.player.audioClip.length)
        {
            // If sequences isn't over, start over the music
            return;
        }

        // The actual time of train "process"
        processedTimer += Time.deltaTime;

        if (isEightBeatCountDown)
        {
            eightBeatTimer -= Time.deltaTime;
            Debug.Log("8-Beat-Timer count down: " + eightBeatTimer + " secs");
            if (eightBeatTimer <= 0)
            {
                Debug.Log("8-Beat-Timer count down finished.");
                isEightBeatCountDown = false;
            }

            return;
        }

        // The actual progress of the music since start
        float musicProgressTimer = time - musicStage.chosenStartTime;

        // The actual time of "training"
        trainedTimer += Time.deltaTime;
        
        
        /*
         * TODO:
         * 1. See if the song is between startTime, start once reached, continue once over, pause when not-reached
         * 
         * If inGame,
         * 1. Gets the current & next poseSequence ( song isn't over, loop again )
         * 2. If current PoseSequence doesn't end with 0.0f, wait for (8beat - currBeat) secs, until next poseSequence starts
         */

        // Flags to detect whether sequences and frames are found
        bool isSequenceFound = false;
        bool isFrameFound = false;
        bool isLastSequence = true;
        bool isLastFrame = true;
        float trainedTimerTmp = trainedTimer;
        foreach (var keyValuePair in rhythmModeData)
        {
            // TODO: Get next pose sequence
            if (isSequenceFound)
            {
                nextSequence = keyValuePair;
                break;
            }
            
            // Decrease the timer by seq's duration if the seq is done, then go for the next seq; else found the in-progress seq, and find frames
            var tmp = trainedTimerTmp - keyValuePair.Value * musicStage.chosenEightBeatDuration;
            if (tmp > 0)
            {
                trainedTimerTmp = tmp;
                continue;
            }

            // TODO: Set current pose sequence
            foreach (var poseFrame in keyValuePair.Key.poseFrames)
            {
                // TODO: Get next pose frame
                if (isFrameFound)
                {
                    nextFrame = poseFrame;
                    break;
                }

                // Decrease the timer by frames's duration if the frame is done, then go for the next frame; else found the in-progress frame
                var leftTrainedTime = trainedTimerTmp - poseFrame.duration;
                if (leftTrainedTime > 0)
                {
                    trainedTimerTmp = leftTrainedTime;
                    continue;
                }

                // TODO: Set current pose frame
                isFrameFound = true;
                if (currFrame != poseFrame)
                {
                    isLastFrame = false;
                    Debug.Log("Frame " + (currFrame == null ? "Null" : currFrame.duration.ToString()) + " -> Frame " + poseFrame.duration);
                    currFrame = poseFrame;
                }
            }

            isSequenceFound = true;
            // If it's a new sequence, setup timer for eightbeat if it's not 8Beat, else don't setup timer
            if (currSequence.Key != keyValuePair.Key)
            {
                isLastSequence = false;
                if (currSequence.Value % 1.0f != 0.0f)
                {
                    isEightBeatCountDown = true;
                    eightBeatTimer = musicStage.chosenEightBeatDuration - ((currSequence.Value % 1.0f) * musicStage.chosenEightBeatDuration);
                }
                var first = string.Format("Seq {0}, {1} 8-Beats, {2}", currSequence.Key.fileName, currSequence.Value, isEightBeatCountDown?("Bad 8-Beat! 8-Beat-Timer start: " + eightBeatTimer + " secs"):"All clear");
                var middle = string.Format("\nat {0} ->\n", trainedTimer);
                var second = string.Format("Seq {0}, {1} 8-Beats", keyValuePair.Key.fileName, keyValuePair.Value);
                Debug.Log(first + middle + second);
                currSequence = keyValuePair;
            }
        }

        if (isLastFrame)
            isStageOver = true;
    }
}