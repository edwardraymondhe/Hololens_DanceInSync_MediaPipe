
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

    [Header("Sub Windows")]
    public InputSubWindowController inputSubWindow;
    public GameObject inputSubWindowPrefab;

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
                inputSubWindow = Instantiate(inputSubWindowPrefab, GameObject.Find("Sub Window Canvas").transform).GetComponent<InputSubWindowController>();
                inputSubWindow.Init("设置", "请设置以下参数（整数）：");
                inputSubWindow.AddInput("最小八拍");
                inputSubWindow.AddInput("当前八拍");
                inputSubWindow.AddInput("正常八拍");
                inputSubWindow.AddSubmit(CheckAndSaveContinuousSequence);
            }

            isContinuousRecording = !isContinuousRecording;

            isContinuousRecordToggled = false;
        }

        // If currently recording, add current frame
        if (isContinuousRecording)
            poseRecorder.AddContinuousFrame(server.currentPoseFrame);
    }

    public void CheckAndSaveContinuousSequence()
    {
        foreach (var inputField in inputSubWindow.inputFields)
        {
            float f;
            if (!float.TryParse(inputField.Value.text, out f))
            {
                inputSubWindow.GetComponent<InputSubWindowController>().ChangeContent("请输入合法数据。\n请设置以下参数（整数）：");
                return;
            }

            if (f % 1.0f == 0.0f && f > 0.0f)
            {
                switch (inputField.Key)
                {
                    case "最小八拍":
                        poseRecorder.continuousPoseSequence.minCycles = f;
                        break;
                    case "当前八拍":
                        poseRecorder.continuousPoseSequence.curCycles = f;
                        break;
                    case "正常八拍":
                        poseRecorder.continuousPoseSequence.SetRawCycles(f);
                        break;
                    default:
                        break;
                }
            }
        }

        if (!poseRecorder.continuousPoseSequence.CheckCyclePowerValid())
        {
            inputSubWindow.GetComponent<InputSubWindowController>().ChangeContent("请输入合法数据。\n请设置以下参数（整数）：");
            return;
        }

        inputSubWindow.Close();

        poseRecorder.SaveContinuousSequence();
        poseEditor.RefreshPoseBrowserContent();
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
