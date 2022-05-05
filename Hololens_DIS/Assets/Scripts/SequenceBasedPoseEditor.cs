using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Helper.Pose;

/// <summary>
/// Pose Editor & Browser Component
/// 1. Change frame positions
/// 2. Change duration
/// TODO: 3. Re-calculates angular speed when apply changes
/// TODO: 4. Moving on the canvas shows a frame following
/// </summary>
public class SequenceBasedPoseEditor: PoseEditor
{
    [Header("Current Pose Data")]
    public List<PoseSequence> edittingPoseSequences;
    public PoseSequence currentPoseSequence;
    public PoseEditorSequenceItem edittingEditorSequenceitem;
    public List<PoseEditorSequenceItem> selectedPoseEditorSequenceItems = new List<PoseEditorSequenceItem>();
    public InputField cycleDurationInputField;

    [Header("Sub Windows")]
    public InputSubWindowController inputSubWindow;
    public GameObject inputSubWindowPrefab;

    private void Awake()
    {
        RefreshPoseBrowserContent();
        CreatePoseEditorTimeline();

        targetPosition = new Vector2(cursor.transform.position.x, poseEditorTimelineContent.transform.position.y);
        scrollBar = poseEditorCanvas.GetComponent<ScrollRect>().horizontalScrollbar;

        cycleDurationInputField.placeholder.GetComponent<Text>().text = string.Format("默认输入时长: {0}", GlobalController.Instance.setting.editor.cycleDuration);
        cycleDurationInputField.onEndEdit.AddListener(SetCycleDuration);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
                poseEditorWidthPerSecond += scrollBarScaleSpeed * Input.GetAxis("Mouse ScrollWheel");
                UpdatePoseEditorTimelineScaleAndPosition();
            }
        }

        // Drag();

        if (isEditorPlay)
        {
            currentEditorTime += Time.deltaTime;
            maxEditorTime = currentEditorTime;
            UpdateFocusElementWithTime(scrollBar.size * scrollBarSkipFactor);
        }

        playButton.GetComponent<Image>().sprite = isEditorPlay ? pauseImage : playImage;

        cursor.transform.position = targetPosition;
        cursor.transform.localPosition = new Vector3(cursor.transform.localPosition.x, 0.0f, cursor.transform.localPosition.z);
    }

    public override void CreatePoseEditorTimeline()
    {
        if (chosenPoseSequence == null)
            return;

        // TODO: Check anything has changed before clearing
        ClearPoseEditorTimelineContent();

        // Adds a deep copy of a new sequence
        edittingPoseSequences.Add(Helper.DeepCopy(chosenPoseSequence));

        // Create a new timeline UI
        GameObject lastItem = null;
        var idx = 0;
        foreach (var poseSequence in edittingPoseSequences)
        {
            var item = Instantiate(poseEditorTimelineItemPrefab, poseEditorTimelineContent.transform);
            string fileName = (poseSequence.fileName == "") ? idx.ToString() : poseSequence.fileName;
            poseSequence.fileName = fileName;

            item.GetComponent<PoseEditorSequenceItem>().InitParams(this, poseEditorWidthPerSecond, poseSequence);

            if (lastItem != null)
            {
                item.GetComponent<PoseEditorSequenceItem>().prevEditorItem = lastItem;
                lastItem.GetComponent<PoseEditorSequenceItem>().nextEditorItem = item;
            }

            lastItem = item;
            idx++;
        }

        UpdateCycleDuration();
    }

    public void SetCycleDuration(string value)
    {
        float f;
        if (!float.TryParse(value, out f))
        {
            Debug.LogError("Input correct duration for an eight-beat.");
            return;
        }

        if (f > 0.0f)
            GlobalController.Instance.setting.editor.cycleDuration = f;
        else
        {
            GlobalController.Instance.setting.editor.cycleDuration = 4.5f;
            cycleDurationInputField.text = "";
        }

        UpdateCycleDuration();
    }

    private void UpdateCycleDuration()
    {
        foreach (var sequence in edittingPoseSequences)
        {
            float newDuration = sequence.curCycles * GlobalController.Instance.setting.editor.cycleDuration;
            sequence.SetDuration(newDuration);
        }

        UpdatePoseEditorTimelineScaleAndPosition();
    }

    protected override void UpdateTimeWithPosition(bool useMouse = true)
    {
        Vector3 leftBorderPosition = GetUIPoints(leftBorder)[1];
        Vector3 rightBorderPosition = GetUIPoints(rightBorder)[1];

        var totalTime = 0.0f;
        GameObject currentItem = null;
        bool currentSequenceFound = false;

        float inputPosition = 0.0f;

        if (useMouse)
            inputPosition = Input.mousePosition.x;
        else
            inputPosition = GetUIPoints(cursor)[1].x;


        // 1. Foreach and get the current frame
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            GameObject item = poseEditorTimelineContent.transform.GetChild(i).gameObject;
            var comp = item.GetComponent<PoseEditorSequenceItem>();
            comp.highlightUI.SetActive(false);

            if (currentSequenceFound == false)
            {
                var points = GetUIPoints(item);
                if (inputPosition <= points[2].x && inputPosition >= points[0].x)
                {
                    currentItem = item;
                    currentSequenceFound = true;
                    var percentage = (inputPosition - points[0].x) / (points[2].x - points[0].x);
                    totalTime += (comp.poseSequence.GetDuration() * percentage);
                }
                else
                {
                    if (i == 0 && inputPosition <= points[0].x)
                        break;

                    totalTime += comp.poseSequence.GetDuration();
                }
            }
        }

        currentEditorTime = totalTime;
        UpdateFocusElementWithTime(manualScrollBarSpeed);
        base.UpdateTimeWithPosition(useMouse);
    }

    protected override void UpdateFocusElementWithTime(float scrollBarSpeed)
    {
        Vector3 leftBorderPosition = GetUIPoints(leftBorder)[1];
        Vector3 rightBorderPosition = GetUIPoints(rightBorder)[1];

        var totalTime = 0.0f;
        GameObject currentItem = null;
        GameObject lastItem = null;
        bool currentItemFound = false;

        // 1. Foreach and get the current frame
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            GameObject item = poseEditorTimelineContent.transform.GetChild(i).gameObject;
            var comp = item.GetComponent<PoseEditorSequenceItem>();
            comp.highlightUI.SetActive(false);

            if (currentItemFound == false)
            {
                totalTime += comp.poseSequence.GetDuration();
                if (totalTime < currentEditorTime)
                    lastItem = item;
                else // Iterating frame is the next frame
                {
                    currentItem = item;
                    currentItemFound = true;
                }
            }
        }

        if (currentItemFound == false)
            currentEditorTime = totalTime;

        // 2. If the frame isn't within the screen

        if (currentItem != null)
        {
            var lastItemPoints = GetUIPoints(currentItem);
            Vector3 lastItemCenter = lastItemPoints[1];
            
            var currentEditorSequenceitem = currentItem.GetComponent<PoseEditorSequenceItem>();
            var currentDuration = currentEditorSequenceitem.poseSequence.GetDuration();

            currentEditorSequenceitem.highlightUI.SetActive(true);

            currentPoseSequence = currentEditorSequenceitem.poseSequence;

            // TODO: Control Humanoid by time within the sequence
            // editHumanoidController.UpdateByFrame(currentPoseSequence);

            float itemLeftBorder = lastItemPoints[0].x;
            float itemRightBorder = lastItemPoints[2].x;

            var percentage = (currentDuration - (totalTime - currentEditorTime)) / currentDuration;
            var x = itemLeftBorder + percentage * (itemRightBorder - itemLeftBorder);
            if ((scrollBar.value == 0.0f || x > leftBorderPosition.x + scrollBarEndOffset) && (x < rightBorderPosition.x - scrollBarEndOffset || scrollBar.value == 1.0f))
            {
                targetPosition = new Vector2(x, poseEditorTimelineContent.transform.position.y);
            }
            else
            {
                if (x > rightBorderPosition.x)
                {
                    // 3. Scroll the scroll bar step by its current size
                    scrollBar.value += scrollBarSpeed;
                    scrollBar.value = Mathf.Clamp(scrollBar.value, 0.0f, 1.0f);
                }
                else if (x < leftBorderPosition.x)
                {
                    // 3. Scroll the scroll bar step by its current size
                    scrollBar.value -= scrollBarSpeed;
                    scrollBar.value = Mathf.Clamp(scrollBar.value, 0.0f, 1.0f);
                }
            }
        }
        else
            isEditorPlay = false;
    }

    public override void UpdatePoseEditorTimelineScaleAndPosition()
    {
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            var child = poseEditorTimelineContent.transform.GetChild(i);
            child.GetComponent<PoseEditorSequenceItem>().UpdateParams(poseEditorWidthPerSecond);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0.0f);
        }

        UpdateTimeWithPosition(false);
    }
    public override void SetChosenPoseSequence(string poseSequenceName)
    {
        base.SetChosenPoseSequence(poseSequenceName);

        // Create the pose editor timeline
        CreatePoseEditorTimeline();
    }

    public override void SetChosenPoseFrame(string poseFrameName)
    {
        base.SetChosenPoseFrame(poseFrameName);

        LoadPoseFrameToHumanoid(chosenPoseFrameName);
        isEditorPlay = false;
    }

    public MouseStatus cursorStatus = new MouseStatus(true, false);
    public MouseStatus velocityStatus = new MouseStatus(true, false);
    public MouseStatus angleStatus = new MouseStatus(true, false);


    /*

    public void PrevSecond()
    {
        currentEditorTime -= 1.0f;
        // TODO: Give upper limit on this
        currentEditorTime = Mathf.Max(0.0f, currentEditorTime);

        UpdateCursorWithTime(scrollBar.size * scrollBarSkipFactor);
    }

    public void NextSecond()
    {
        currentEditorTime += 1.0f;
        currentEditorTime = Mathf.Max(0.0f, currentEditorTime);

        UpdateCursorWithTime(scrollBar.size * scrollBarSkipFactor);
    }
    */

    public void InsertItem(GameObject insertSequenceitem, PoseSequence insertPoseSequence, int insertIdx)
    {
        // Initializes the transform info
        insertSequenceitem.transform.SetParent(poseEditorTimelineContent.transform);
        insertSequenceitem.transform.SetSiblingIndex(insertIdx);
        insertSequenceitem.transform.localScale = Vector3.one;

        // Insert synced data
        if (0 <= insertIdx && insertIdx < edittingPoseSequences.Count)
            edittingPoseSequences.Insert(insertIdx, insertPoseSequence);
        else
            edittingPoseSequences.Add(insertPoseSequence);

        // Re-link all the UIs
        GameObject lastItem = null;
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            GameObject item = poseEditorTimelineContent.transform.GetChild(i).gameObject;
            if (lastItem != null)
            {
                item.GetComponent<PoseEditorSequenceItem>().prevEditorItem = lastItem;
                lastItem.GetComponent<PoseEditorSequenceItem>().nextEditorItem = item;
            }
            lastItem = item;
        }

        // Updates UI scale and browser content
        RefreshPoseBrowserContent();
        UpdatePoseEditorTimelineScaleAndPosition();
    }

    public override void InsertNewItem(int insertIdx, string insertItemName)
    {
        if (edittingPoseSequences == null)
            edittingPoseSequences = new List<PoseSequence>();

        // Loads pose frame data, create ui correspondingly
        var insertPoseSequence = LoadInstance<PoseSequence>(insertItemName);
        
        GameObject insertItem = Instantiate(poseEditorTimelineItemPrefab);
        insertItem.GetComponent<PoseEditorSequenceItem>().InitParams(this, poseEditorWidthPerSecond, insertPoseSequence);

        InsertItem(insertItem, insertPoseSequence, insertIdx);
        // TODO: Update the angular velocities between all the frames
    }

    public void ToggleSequenceItemMenu(PoseEditorSequenceItem poseEditorSequenceItem)
    {
        if (edittingEditorSequenceitem != null)
        {
            if (edittingEditorSequenceitem == poseEditorSequenceItem)
            {
                edittingEditorSequenceitem.ToggleMenu();
                return;
            }
            else
                edittingEditorSequenceitem.CloseMenu();
        }

        edittingEditorSequenceitem = poseEditorSequenceItem;
        edittingEditorSequenceitem.OpenMenu();
    }

    public void CloseSequenceitemMenu()
    {
        if (edittingEditorSequenceitem != null)
            edittingEditorSequenceitem.CloseMenu();
    }

    public void ResetChosenPoseSequence()
    {
        chosenPoseSequence = LoadInstance<PoseSequence>(chosenPoseSequenceName);

        // Create the pose editor timeline
        if (gameObject.activeInHierarchy)
            CreatePoseEditorTimeline();
    }

    public override void SaveEdit()
    {
        if (edittingPoseSequences.Count > 1)
            SaveInstance(PoseSequence.Concat(edittingPoseSequences));
        else if (edittingPoseSequences.Count == 1)
            SaveInstance(edittingPoseSequences[0]);

        base.SaveEdit();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        CloseSequenceitemMenu();
        isEditorPlay = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }
}