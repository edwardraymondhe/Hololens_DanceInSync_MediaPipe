using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmTrainStageController : BaseTrainStageController
{
    public List<TrainModeData> rhythmModeData = new List<TrainModeData>();
    [SerializeField]
    private float processedTimer = 0.0f;
    [SerializeField]
    private float eightBeatTimer = 0.0f;
    [SerializeField]
    private float trainedTimer = 0.0f;
    [SerializeField]
    TrainModeData currSequence, nextSequence;
    [SerializeField]
    PoseFrame currFrame, nextFrame;

    [SerializeField]
    private bool isEightBeatCountDown = false;
    [SerializeField]
    private bool isStageOver = false;

    public HumanoidController humanoid;

    public override void StartStage()
    {
        /*
         * TODO:
         * 1. Create a new deep copy dictionary for sequence that holds all the infomation
         */

        foreach (var keyValuePair in tweakStage.rhythmModeData)
        {
            var poseSequenceDeepCopy = Helper.DeepCopy(keyValuePair.poseSequence);
            var poseSequenceLength = musicStage.chosenEightBeatDuration * keyValuePair.value;
            poseSequenceDeepCopy.SetDuration(poseSequenceLength);

            rhythmModeData.Add(new TrainModeData(poseSequenceDeepCopy, keyValuePair.value));
        }

        // Eight beat timer gives the blank-out when an 8Beat is not fully used
        // this.eightBeatTimer = (Mathf.Ceil(currSequence.value) - currSequence.value) * musicStage.chosenEightBeatDuration;

        // Sets the first poseSequence
        // currSequence = rhythmModeData[0];
        // currFrame = currSequence.poseSequence.poseFrames[0];

        // Manually start the music stage
        musicStage.audioClipSelector.Play();

        base.StartStage();
    }

    public override void InitStage()
    {
        base.InitStage();

        processedTimer = 0.0f;
        eightBeatTimer = 0.0f;
        trainedTimer = 0.0f;
        currSequence = null;
        nextSequence= null;
        currFrame = null;
        nextFrame = null;
        isEightBeatCountDown = false;
        isStageOver = false;
        rhythmModeData.Clear();
    }

    protected override void Update()
    {
        if (!initialized)
            return;

        if (isStageOver)
        {
            // TODO: Music fade out a little bit
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
        Debug.Log("TrainedTimer: " + trainedTimer);
        
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
        bool isLastSequence = false;
        float trainedTimerTmp = trainedTimer;
        int seqIndex = -1;
        foreach (var keyValuePair in rhythmModeData)
        {
            seqIndex++;
            if (seqIndex == rhythmModeData.Count - 1)
                isLastSequence = true;

            Debug.Log("Seq: " + seqIndex);
            /*
            // TODO: Get next pose sequence
            if (isSequenceFound)
            {
                nextSequence = keyValuePair;
                break;
            }
            */

            // Decrease the timer by seq's duration if the seq is done, then go for the next seq; else found the in-progress seq, and find frames
            // var tmp = trainedTimerTmp - keyValuePair.value * Mathf.Ceil(musicStage.chosenEightBeatDuration);
            var tmp = trainedTimerTmp - keyValuePair.value * musicStage.chosenEightBeatDuration;
            if (tmp > 0)
            {
                trainedTimerTmp = tmp;
                Debug.Log("Over this seq.");
                continue;
            }

            Debug.Log("In this seq.");
            // If it's a new sequence, setup timer for eightbeat if it's not 8Beat, else don't setup timer
            if (currSequence != keyValuePair)
            {
                if (currSequence != null && currSequence.poseSequence != null)
                {
                    if (currSequence.value % 1.0f != 0.0f)
                    {
                        isEightBeatCountDown = true;
                        eightBeatTimer = musicStage.chosenEightBeatDuration - ((currSequence.value % 1.0f) * musicStage.chosenEightBeatDuration);
                    }
                    var first = string.Format("Seq {0}, {1} 8-Beats, {2}", currSequence.poseSequence.fileName, currSequence.value, isEightBeatCountDown ? ("Bad 8-Beat! 8-Beat-Timer start: " + eightBeatTimer + " secs") : "All clear");
                    var middle = string.Format("\nat {0} ->\n", trainedTimer);
                    var second = string.Format("Seq {0}, {1} 8-Beats", keyValuePair.poseSequence.fileName, keyValuePair.value);
                    Debug.Log(first + middle + second);
                }else
                    Debug.Log(string.Format("Seq {0}, {1} 8-Beats", keyValuePair.poseSequence.fileName, keyValuePair.value));

                currSequence = keyValuePair;
                if (seqIndex < rhythmModeData.Count - 1)
                    nextSequence = rhythmModeData[seqIndex + 1];
                else
                    nextSequence = null;

            }

            // TODO: Set current pose sequence
            bool isLastFrame = false;
            int frameIndex = -1;
            foreach (var poseFrame in keyValuePair.poseSequence.poseFrames)
            {
                frameIndex++;
                if (frameIndex == keyValuePair.poseSequence.poseFrames.Count - 1)
                    isLastFrame = true;

                // Decrease the timer by frames's duration if the frame is done, then go for the next frame; else found the in-progress frame
                var leftTrainedTime = trainedTimerTmp - poseFrame.duration;
                if (leftTrainedTime > 0)
                {
                    trainedTimerTmp = leftTrainedTime;
                    Debug.Log("Over this frame.");

                    continue;
                }
                Debug.Log("In this frame.");

                isFrameFound = true;
                // TODO: Set current pose frame
                if (currFrame != poseFrame)
                {
                    Debug.Log("Frame " + (currFrame == null ? "Null" : currFrame.duration.ToString()) + " -> Frame " + poseFrame.duration);
                    currFrame = poseFrame;
                    if (frameIndex < keyValuePair.poseSequence.poseFrames.Count - 1)
                        nextFrame = keyValuePair.poseSequence.poseFrames[frameIndex + 1];
                    else
                        nextFrame = null;
                }
                
                break;
            }

            // TODO: If sequence just changed, need to increase lerp
            humanoid.UpdateByFrame(currFrame);

            if (isFrameFound)
            {
                Debug.Log("Frame found");
                break;
            }

        }

        if (!isFrameFound)
            isStageOver = true;
    }
}