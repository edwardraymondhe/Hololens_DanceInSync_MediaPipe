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
public class PoseEditor: MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("Scroll View Contents")]
    public GameObject poseEditorCanvas;

    public GameObject poseFrameBrowserContent;
    public GameObject poseSequenceBrowserContent;
    public GameObject poseEditorTimelineContent;

    [Header("Scroll View Item Prefabs")]
    public GameObject poseFrameBrowserItemPrefab;
    public GameObject poseSequenceBrowserItemPrefab;
    public GameObject poseEditorTimelineFrameItemPrefab;

    [Header("Current Pose Data")]
    public string chosenPoseSequenceName;
    public string chosenPoseFrameName;
    public PoseSequence chosenPoseSequence;
    public PoseSequence edittingPoseSequence;
    public PoseFrame chosenPoseFrame;

    [Range(150f, 300f)]
    public float poseEditorWidthPerSecond = 200.0f;
    public float scrollBarScaleSpeed = 20f;

    [Header("Cursor")]
    public bool isEditorPlay = false;
    public bool isEditorControl = false;
    public float currentEditorTime = 0.0f;
    private float maxEditorTime = 0.0f;
    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject cursor;
    public float cursorSpeedFactor = 15.0f;
    public float scrollBarSkipFactor = 1.5f;
    private Vector3 targetPosition;
    private Scrollbar scrollBar;
    public float scrollBarEndOffset = 0.0f;
    public float manualScrollBarSpeed = 0.1f;

    [Header("Control")]
    public Sprite playImage;
    public Sprite pauseImage;
    public Button playButton;

    private void Awake()
    {
        RefreshPoseBrowserContent();
        CreatePoseEditorTimeline();

        targetPosition = new Vector2(cursor.transform.position.x, poseEditorTimelineContent.transform.position.y);
        scrollBar = poseEditorCanvas.GetComponent<ScrollRect>().horizontalScrollbar;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
                poseEditorWidthPerSecond += scrollBarScaleSpeed * Input.GetAxis("Mouse ScrollWheel");
                UpdatePoseEditorTimelineScaleAndPosition();
            }
        }

        Drag();

        if (isEditorPlay)
        {
            currentEditorTime += Time.deltaTime;
            maxEditorTime = currentEditorTime;
            UpdateCursorWithTime(scrollBar.size * scrollBarSkipFactor);
        }

        playButton.GetComponent<Image>().sprite = isEditorPlay ? pauseImage : playImage;

        cursor.transform.position = targetPosition;
        cursor.transform.localPosition = new Vector3(cursor.transform.localPosition.x, 0.0f, cursor.transform.localPosition.z);
    }

    public void Drag()
    {
        #region Start Select Items
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2))
        {
            selectedPoseEditorFrameItems.Clear();

            var raycastedObjects = Helper.GetOverUI(poseEditorCanvas);

            // TODO: See if the pointer locates at
            // 1. Cursor area
            // 2. Angle area
            // 3. Velocity area
            if (raycastedObjects == null)
                return;

            foreach (var obj in raycastedObjects)
            {
                Debug.Log(obj.name);
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (obj.name.ToLower() == "cursor")
                    {
                        cursorStatus.isEnabled = true;
                        angleStatus.isEnabled = false;
                        velocityStatus.isEnabled = false;
                        break;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Mouse2))
                {
                    Debug.Log("Middle");
                    if (obj.name.ToLower().Contains("angle"))
                    {
                        cursorStatus.isEnabled = false;
                        angleStatus.isEnabled = true;
                        velocityStatus.isEnabled = false;

                        obj.GetComponent<PoseEditorFrameItemToggleBase>().Select();

                        break;
                    }
                    else if (obj.name.ToLower().Contains("velocity"))
                    {
                        cursorStatus.isEnabled = false;
                        angleStatus.isEnabled = false;
                        velocityStatus.isEnabled = true;

                        obj.GetComponent<PoseEditorFrameItemToggleBase>().Select();

                        break;
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.Mouse2))
        {
            if (angleStatus.isEnabled)
            {
                var falseItem = selectedPoseEditorFrameItems.Find(e => e.angleToggle.isOn == false);
                // Change all the toggles
                // Sync datas
                var value = falseItem != null;

                foreach (var item in selectedPoseEditorFrameItems)
                {
                    item.angleToggle.isOn = value;
                    edittingPoseSequence.SetAngle(item.poseFrame, value);
                }
            }
            else if (velocityStatus.isEnabled)
            {
                var falseItem = selectedPoseEditorFrameItems.Find(e => e.velocityToggle.isOn == false);
                // Change all the toggles
                // Sync datas
                var value = falseItem != null;

                foreach (var item in selectedPoseEditorFrameItems)
                {
                    item.velocityToggle.isOn = value;
                    edittingPoseSequence.SetVelocity(item.poseFrame, value);
                }
            }

            cursorStatus.Set(true, false);
            velocityStatus.Set(true, false);
            angleStatus.Set(true, false);
        }
        #endregion
    }

    public List<PoseEditorFrameItem> selectedPoseEditorFrameItems = new List<PoseEditorFrameItem>();

    public void TogglePlay()
    {
        isEditorPlay = !isEditorPlay;
    }

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

    private List<Vector3> GetUIPoints(GameObject item)
    {
        var rectTransform = item.GetComponent<RectTransform>();
        var center = rectTransform.rect.center;

        var worldPos = rectTransform.TransformPoint(new Vector2(center.x, center.y));
        var leftPos = rectTransform.TransformPoint(new Vector2(center.x - rectTransform.rect.width / 2.0f, center.y));
        var rightPos = rectTransform.TransformPoint(new Vector2(center.x + rectTransform.rect.width / 2.0f, center.y));

        return new List<Vector3> { leftPos, worldPos, rightPos };
    }

    private void UpdateCursorWithPosition(bool useMouse = true)
    {
        Vector3 leftBorderPosition = GetUIPoints(leftBorder)[1];
        Vector3 rightBorderPosition = GetUIPoints(rightBorder)[1];

        var totalTime = 0.0f;
        GameObject currentItem = null;
        bool currentItemFound = false;

        float inputPosition = 0.0f;

        if (useMouse)
            inputPosition = Input.mousePosition.x;
        else
            inputPosition = GetUIPoints(cursor)[1].x;


        // 1. Foreach and get the current frame
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            GameObject item = poseEditorTimelineContent.transform.GetChild(i).gameObject;
            var comp = item.GetComponent<PoseEditorFrameItem>();
            comp.highlightUI.SetActive(false);

            if (currentItemFound == false)
            {
                var points = GetUIPoints(item);
                if (inputPosition <= points[2].x && inputPosition >= points[0].x)
                {
                    currentItem = item;
                    currentItemFound = true;
                    var percentage = (inputPosition - points[0].x) / (points[2].x - points[0].x);
                    totalTime += (comp.poseFrame.duration * percentage);
                }
                else
                {
                    if (i == 0 && inputPosition <= points[0].x)
                        break;

                    totalTime += comp.poseFrame.duration;
                }
            }
        }

        currentEditorTime = totalTime;
        UpdateCursorWithTime(manualScrollBarSpeed);
    }

    private void UpdateCursorWithTime(float scrollBarSpeed)
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
            var comp = item.GetComponent<PoseEditorFrameItem>();
            comp.highlightUI.SetActive(false);

            if (currentItemFound == false)
            {
                totalTime += comp.poseFrame.duration;
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
            
            var currentEditorFrameItem = currentItem.GetComponent<PoseEditorFrameItem>();
            var currentDuration = currentEditorFrameItem.poseFrame.duration;

            currentEditorFrameItem.highlightUI.SetActive(true);


            float itemLeftBorder = lastItemPoints[0].x;
            float itemRightBorder = lastItemPoints[2].x;

            // if (itemLeftBorder > leftBorderPosition.x && itemRightBorder < rightBorderPosition.x)
            var percentage = (currentDuration - (totalTime - currentEditorTime)) / currentDuration;
            // Debug.Log("Percentage: " + percentage);
            var x = itemLeftBorder + percentage * (itemRightBorder - itemLeftBorder);
            // Debug.Log(string.Format("X: {0}, RightBorder: {1}", x, itemRightBorder));
            if ((scrollBar.value == 0.0f || x > leftBorderPosition.x + scrollBarEndOffset) && (x < rightBorderPosition.x - scrollBarEndOffset || scrollBar.value == 1.0f))
            {
                // Debug.Log(GetUIPoints(cursor)[1].y);
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

    public void InsertPoseEditorTimelineFrame(GameObject insertFrameItem, PoseFrame insertPoseFrame, bool angle, bool velocity, int insertIdx)
    {
        // Initializes the transform info
        insertFrameItem.transform.SetParent(poseEditorTimelineContent.transform);
        insertFrameItem.transform.SetSiblingIndex(insertIdx);
        insertFrameItem.transform.localScale = Vector3.one;

        // Insert synced data
        if (0 <= insertIdx && insertIdx < edittingPoseSequence.poseFrames.Count)
            edittingPoseSequence.Insert(insertIdx, insertPoseFrame, angle, velocity);
        else
            edittingPoseSequence.Add(insertPoseFrame);

        // Re-link all the UIs
        GameObject lastItem = null;
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            GameObject item = poseEditorTimelineContent.transform.GetChild(i).gameObject;
            if (lastItem != null)
            {
                item.GetComponent<PoseEditorFrameItem>().prevEditorItem = lastItem;
                lastItem.GetComponent<PoseEditorFrameItem>().nextEditorItem = item;
            }
            lastItem = item;
        }

        // Updates UI scale and browser content
        RefreshPoseBrowserContent();
        UpdatePoseEditorTimelineScaleAndPosition();
    }

    public void InsertNewPoseEditorTimelineFrame(int insertIdx, string insertItemName)
    {
        if (edittingPoseSequence == null)
            return;

        // Loads pose frame data, create ui correspondingly
        var insertPoseFrame = LoadInstance<PoseFrame>(insertItemName);
        if (insertPoseFrame.duration == 0)
            insertPoseFrame.duration = 0.25f;

        GameObject insertItem = Instantiate(poseEditorTimelineFrameItemPrefab);
        insertItem.GetComponent<PoseEditorFrameItem>().InitParams(this, poseEditorWidthPerSecond, insertPoseFrame, false, false);

        InsertPoseEditorTimelineFrame(insertItem, insertPoseFrame, false, false, insertIdx);
        // TODO: Update the angular velocities between all the frames
    }

    public void CreatePoseEditorTimeline()
    {
        if (chosenPoseSequence == null)
            return;

        // TODO: Check anything has changed before clearing
        ClearPoseEditorTimelineContent();

        // Create a new "Editting Pose Sequence"
        edittingPoseSequence = new PoseSequence();
        edittingPoseSequence.Set(chosenPoseSequence);

        // Create a new timeline UI
        GameObject lastItem = null;
        var idx = 0;
        foreach (var poseFrame in edittingPoseSequence.poseFrames)
        {
            var item = Instantiate(poseEditorTimelineFrameItemPrefab, poseEditorTimelineContent.transform);
            string fileName = "";
            if (poseFrame.blank == true)
                fileName = "";
            else
                fileName = ((poseFrame.fileName == "") ? idx.ToString() : poseFrame.fileName);

            poseFrame.fileName = fileName;
            item.GetComponent<PoseEditorFrameItem>().InitParams(this, poseEditorWidthPerSecond, poseFrame, edittingPoseSequence.angles[idx], edittingPoseSequence.velocities[idx] );
            if (lastItem != null)
            {
                item.GetComponent<PoseEditorFrameItem>().prevEditorItem = lastItem;
                lastItem.GetComponent<PoseEditorFrameItem>().nextEditorItem = item;
            }

            lastItem = item;
            idx++;
        }

        UpdatePoseEditorTimelineScaleAndPosition();
    }

    public void ToggleFrameItemMenu(PoseEditorFrameItem poseEditorFrameItem)
    {
        if (edittingEditorFrameItem != null)
        {
            if (edittingEditorFrameItem == poseEditorFrameItem)
            {
                edittingEditorFrameItem.ToggleMenu();
                return;
            }
            else
                edittingEditorFrameItem.CloseMenu();
        }

        edittingEditorFrameItem = poseEditorFrameItem;
        edittingEditorFrameItem.OpenMenu();
    }

    public void CloseFrameItemMenu()
    {
        if (edittingEditorFrameItem != null)
            edittingEditorFrameItem.CloseMenu();
    }

    public void UpdatePoseEditorTimelineScaleAndPosition()
    {
        for (int i = 0; i < poseEditorTimelineContent.transform.childCount; i++)
        {
            var child = poseEditorTimelineContent.transform.GetChild(i);
            child.GetComponent<PoseEditorFrameItem>().UpdateParams(poseEditorWidthPerSecond);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0.0f);
        }

        UpdateCursorWithPosition(false);
    }

    /// <summary>
    /// Refreshes the UI contents
    /// </summary>
    public void RefreshPoseBrowserContent()
    {
        // TODO: 
        // 0. Clear everything from before
        ClearPoseBrowserContent();

        // 1. Reads everything from local folder
        var poseFrameNames = LoadInstanceNames<PoseFrame>();
        var poseSequenceNames = LoadInstanceNames<PoseSequence>();

        // 2. Update the scroll items
        foreach (var poseFrameName in poseFrameNames)
        {
            var poseFrameScrollItem = Instantiate(poseFrameBrowserItemPrefab, poseFrameBrowserContent.transform);
            poseFrameScrollItem.GetComponent<PoseFrameBrowserItem>().Init(this, poseFrameName);
        }

        foreach (var poseSequenceName in poseSequenceNames)
        {
            var poseSequenceScrollItem = Instantiate(poseSequenceBrowserItemPrefab, poseSequenceBrowserContent.transform);
            poseSequenceScrollItem.GetComponent<PoseSequenceBrowserItem>().Init(this, poseSequenceName);
        }
    }

    public void ResetChosenPoseSequence()
    {
        chosenPoseSequence = LoadInstance<PoseSequence>(chosenPoseSequenceName);

        // Create the pose editor timeline
        if (gameObject.activeInHierarchy)
            CreatePoseEditorTimeline();
    }

    public void SetChosenPoseSequence(string poseSequenceName)
    {
        // Load the pose sequence
        chosenPoseSequenceName = poseSequenceName;
        chosenPoseSequence = LoadInstance<PoseSequence>(chosenPoseSequenceName);

        // Create the pose editor timeline
        if (gameObject.activeInHierarchy)
            CreatePoseEditorTimeline();
    }

    public void SetChosenPoseFrame(string poseFrame)
    {
        // Load the pose frame
        chosenPoseFrameName = poseFrame;
        chosenPoseFrame = LoadInstance<PoseFrame>(chosenPoseFrameName);
    }

    public PoseEditorFrameItem edittingEditorFrameItem;

    public void SaveEdittingPoseSequence()
    {
        SaveInstance(edittingPoseSequence);
    }

    private void ClearPoseBrowserContent()
    {
        var count = poseFrameBrowserContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseFrameBrowserContent.transform.GetChild(i).gameObject);

        count = poseSequenceBrowserContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseSequenceBrowserContent.transform.GetChild(i).gameObject);
    }

    private void ClearPoseEditorTimelineContent()
    {
        var count = poseEditorTimelineContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseEditorTimelineContent.transform.GetChild(i).gameObject);
    }



    public MouseStatus cursorStatus = new MouseStatus(true, false);
    public MouseStatus velocityStatus = new MouseStatus(true, false);
    public MouseStatus angleStatus = new MouseStatus(true, false);

    public void OnDrag(PointerEventData eventData)
    {
        UpdateCursorWithPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CloseFrameItemMenu();
        isEditorPlay = false;
    }
}