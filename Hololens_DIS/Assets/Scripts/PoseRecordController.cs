
using UnityEngine;
using UnityEngine.UI;

public class PoseRecordController : MonoBehaviour
{
    public float processTimer;
    public float currentTimer = 0.0f;
    public Button addScatterShotButton;
    public IpCamera ipCamera;
    public PoseEditor poseEditor;
    public MediaPipeServer server;
    public PoseRecorder poseRecorder;
    public bool isContinuousRecordToggled = false;
    public bool isContinuousRecording = false;
    private void Update()
    {
        processTimer = 1.0f / GlobalController.Instance.setting.processFPS;
        if (server.poseLandmarks.landmarks.Count > 0)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer > processTimer)
            {
                RecordContinuousSequence();
                currentTimer = 0.0f;
            }

            addScatterShotButton.interactable = isScatteredRecording;
        }
    }

    public void RecordInstantFrame()
    {
        if (poseRecorder == null)
            poseRecorder = new PoseRecorder();

        poseRecorder.SaveInstantFrame(server.currentPoseFrame);
    }

    /// <summary>
    /// Toggles on/off continous-record, called from UI
    /// </summary>
    public void ToggleContinuousRecord()
    {
        isContinuousRecordToggled = true;
    }

    /// <summary>
    /// Controls continuous-record flow
    /// </summary>
    public void RecordContinuousSequence()
    {
        // Pose frames flow control
        if (isContinuousRecordToggled)
        {
            if (!isContinuousRecording)
                // Initialize a new recorder
                poseRecorder = new PoseRecorder();
            else
            {
                // Save continous sequence to local
                poseRecorder.SaveContinuousSequence();
                poseEditor.RefreshPoseBrowserContent();
            }

            isContinuousRecording = !isContinuousRecording;

            isContinuousRecordToggled = false;
        }

        // If currently recording, add current frame
        if (isContinuousRecording)
            poseRecorder.AddContinuousFrame(server.currentPoseFrame);
    }

    // Controls scattered record
    public bool isScatteredRecording = false;

    /// <summary>
    /// Toggles on/off scattered-record, called from UI
    /// </summary>
    public void ToggleScatteredRecord()
    {
        if (poseRecorder == null)
        {
            poseRecorder = new PoseRecorder();
            isScatteredRecording = true;
        }
        else
            isScatteredRecording = !isScatteredRecording;
    }

    /// <summary>
    /// Adds the frame to current scattered sequence, called from UI
    /// </summary>
    public void AddScatteredFrame()
    {
        poseRecorder.AddScatteredFrame(server.currentPoseFrame);
    }

}

public class PoseRecorder
{
    public PoseSequence continuousPoseSequence = new PoseSequence();
    public PoseSequence scatteredPoseSequence = new PoseSequence();

    public PoseRecorder() { }

    public void SaveInstantFrame(PoseFrame poseFrame)
    {
        Helper.Pose.SaveInstance(poseFrame);
    }

    public void AddScatteredFrame(PoseFrame poseFrame)
    {
        scatteredPoseSequence.Add(poseFrame);
    }
    public void AddContinuousFrame(PoseFrame poseFrame)
    {
        continuousPoseSequence.Add(poseFrame);
    }

    public void SaveScatteredFrames()
    {
        foreach (var frame in continuousPoseSequence.poseFrames)
            Helper.Pose.SaveInstance(frame);
    }
    public void SaveScatteredSequence()
    {
        Helper.Pose.SaveInstance(scatteredPoseSequence);
    }


    public void SaveContinuousFrames()
    {
        foreach (var frame in continuousPoseSequence.poseFrames)
            Helper.Pose.SaveInstance(frame);
    }

    public void SaveContinuousSequence()
    {
        Helper.Pose.SaveInstance(continuousPoseSequence);
    }
}
