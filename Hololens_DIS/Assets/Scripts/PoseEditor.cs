using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Helper.Pose;


public class PoseEditor : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("Current Pose Data")]
    public string chosenPoseSequenceName;
    public string chosenPoseFrameName;
    public PoseSequence chosenPoseSequence;
    public PoseFrame chosenPoseFrame;

    [Header("Scroll View Contents")]
    public GameObject poseEditorCanvas;

    public GameObject poseFrameBrowserContent;
    public GameObject poseSequenceBrowserContent;
    public GameObject poseEditorTimelineContent;

    [Header("Scroll View Item Prefabs")]
    public GameObject poseFrameBrowserItemPrefab;
    public GameObject poseSequenceBrowserItemPrefab;
    public GameObject poseEditorTimelineItemPrefab;

    [Range(150f, 300f)]
    public float poseEditorWidthPerSecond = 200.0f;
    public float scrollBarScaleSpeed = 20f;

    [Header("Cursor")]
    public bool isEditorPlay = false;
    public bool isEditorControl = false;
    public float currentEditorTime = 0.0f;
    protected float maxEditorTime = 0.0f;
    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject cursor;
    public float cursorSpeedFactor = 15.0f;
    public float scrollBarSkipFactor = 1.5f;
    protected Vector3 targetPosition;
    protected Scrollbar scrollBar;
    public float scrollBarEndOffset = 0.0f;
    public float manualScrollBarSpeed = 0.1f;

    [Header("Control")]
    public Sprite playImage;
    public Sprite pauseImage;
    public Button playButton;

    [Header("Humanoid")]
    public HumanoidController editHumanoidController; // 1. From video, 2. Focusing on the current cursor
    public HumanoidController previewHumanoidController; // 1. Left-click on frame


    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected virtual void Update()
    {
        if (isSequenceLoaded)
            Helper.UpdateHumanoidBySequence(ref humanoidPreviewTimer, ref previewPoseSequence, ref editHumanoidController);
        else
            humanoidPreviewTimer = 0.0f;
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

    protected void ClearPoseBrowserContent()
    {
        var count = poseFrameBrowserContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseFrameBrowserContent.transform.GetChild(i).gameObject);

        count = poseSequenceBrowserContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseSequenceBrowserContent.transform.GetChild(i).gameObject);
    }

    public void TogglePlay()
    {
        isEditorPlay = !isEditorPlay;
    }

    public PoseFrame lastFrame;
    public PoseSequence previewPoseSequence;
    public string lastFrameName = "";
    public float loadSequenceTimer = 0.0f;
    public bool isSequenceLoaded = false;

    /// <summary>
    /// Clears the elements within the content.
    /// </summary>
    protected void ClearPoseEditorTimelineContent()
    {
        var count = poseEditorTimelineContent.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
            Destroy(poseEditorTimelineContent.transform.GetChild(i).gameObject);
    }

    public void BeginPreviewPoseFrameToHumanoid(string poseFrameName)
    {
        LoadPoseFrameToHumanoid(poseFrameName);
    }
    
    public float humanoidPreviewTimer = 0.0f;

    public void BeginPreviewPoseSequenceToHumanoid(string poseSequenceName)
    {
        previewPoseSequence = LoadPoseSequenceToHumanoid(poseSequenceName);
        isSequenceLoaded = true;
    }

    public void EndPreviewPoseSequenceToHumanoid(string poseSequenceName)
    {
        // TODO: Finish view curr sequence
        // TODO: Load last frame
    }

    protected PoseFrame LoadPoseFrameToHumanoid(string poseFrameName)
    {
        var poseFrame = LoadInstance<PoseFrame>(poseFrameName);
        editHumanoidController.UpdateByFrame(poseFrame, true);
        return poseFrame;
    }

    protected PoseSequence LoadPoseSequenceToHumanoid(string poseSequenceName)
    {
        // Load the pose frame
        var poseSequence = LoadInstance<PoseSequence>(poseSequenceName);

        // TODO: Start loading
        return poseSequence;
    }

    protected List<Vector3> GetUIPoints(GameObject item)
    {
        var rectTransform = item.GetComponent<RectTransform>();
        var center = rectTransform.rect.center;

        var worldPos = rectTransform.TransformPoint(new Vector2(center.x, center.y));
        var leftPos = rectTransform.TransformPoint(new Vector2(center.x - rectTransform.rect.width / 2.0f, center.y));
        var rightPos = rectTransform.TransformPoint(new Vector2(center.x + rectTransform.rect.width / 2.0f, center.y));

        return new List<Vector3> { leftPos, worldPos, rightPos };
    }

    /// <summary>
    /// Updates the time with mouse position, or cursor position
    /// </summary>
    /// <param name="useMouse"></param>
    protected virtual void UpdateTimeWithPosition(bool useMouse = true)
    {
        isSequenceLoaded = false;
    }

    /// <summary>
    /// Updates the cursor, editor, element with correct time.
    /// </summary>
    /// <param name="scrollBarSpeed"></param>
    protected virtual void UpdateFocusElementWithTime(float scrollBarSpeed)
    {
    }

    /// <summary>
    /// Updates the scale for each element in the editor, and update the time, focus element afterwards.
    /// </summary>
    public virtual void UpdatePoseEditorTimelineScaleAndPosition()
    {
    }

    /// <summary>
    /// Creates a time line in the editor with corresponding elements.
    /// </summary>
    public virtual void CreatePoseEditorTimeline()
    {
    }

    public virtual void SetChosenPoseSequence(string poseSequenceName)
    {
        // Load the pose sequence
        chosenPoseSequenceName = poseSequenceName;
        chosenPoseSequence = LoadInstance<PoseSequence>(chosenPoseSequenceName);
    }

    public virtual void SetChosenPoseFrame(string poseFrameName)
    {
        // Load the pose frame
        chosenPoseFrameName = poseFrameName;
        chosenPoseFrame = LoadInstance<PoseFrame>(poseFrameName);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        UpdateTimeWithPosition();
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
    }

    public virtual void SaveEdit()
    {
        RefreshPoseBrowserContent();
    }

    public virtual void InsertNewItem(int insertIdx, string insertItemName)
    { }
}
