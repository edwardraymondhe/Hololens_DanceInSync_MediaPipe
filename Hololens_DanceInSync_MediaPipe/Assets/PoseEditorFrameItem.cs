using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class MouseStatus
{
    public bool isAvailable;
    public bool isEnabled;
    public MouseStatus() { }
    public MouseStatus(bool avail, bool enable) { this.isAvailable = avail; this.isEnabled = enable; }
    public void Set(bool avail, bool enable)
    {
        this.isAvailable = avail;
        this.isEnabled = enable;
    }
}

/// <summary>
/// An UI item in the Pose Editor
/// 1. Simple Drag & Drop, Change position
/// 2. Long Hold & Drag, Change scale
/// TODO: 3. Hover, Preview Pose
/// TODO: 4. Right Click, menu for Delete & Rename
/// </summary>
public class PoseEditorFrameItem : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerClickHandler
{
    public GameObject prevEditorItemMask;
    public GameObject nextEditorItemMask;
    public GameObject prevEditorItem;
    public GameObject nextEditorItem;
    public GameObject poseEditorMenu;

    public Toggle angleToggle;
    public Toggle velocityToggle;

    public PoseEditor poseEditor;
    public float frameItemExpandSpeed = 1.0f;

    private float originalWidth;
    private float widthPerSecond;
    private float originalDuration;
    
    private string itemName;
    private RectTransform rectTransform;
    public PoseFrame poseFrame;
    private Vector3 localPosition;
    private Transform frameItemParent;
    private int prevFrameItemIdx;

    public MouseStatus expandStatus = new MouseStatus(false, false);
    public MouseStatus dragStatus = new MouseStatus(true, true);
    private float focusTimer = 0.0f;
    private bool isFocused = false;

    public GameObject highlightUI;

    private void Update()
    {
        if (isFocused && (expandStatus.isEnabled == false && dragStatus.isEnabled == false))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                focusTimer += Time.deltaTime;
                if (focusTimer > 0.25f)
                {
                    expandStatus.Set(true, false);
                    dragStatus.Set(false, false);
                }
            }
        }
        else
            focusTimer = 0.0f;
    }

    public void InitParams(PoseEditor poseEditor, float widthPerSecond, PoseFrame poseFrame, bool angle, bool velocity)
    {
        rectTransform = GetComponent<RectTransform>();

        angleToggle.isOn = angle;
        velocityToggle.isOn = velocity;

        this.poseEditor = poseEditor;
        this.poseFrame = poseFrame;
        this.originalDuration = poseFrame.duration;
        this.widthPerSecond = widthPerSecond;
        GetComponentInChildren<Text>().text = poseFrame.fileName;

        // Sets all params
        Vector2 targetDelta = new Vector2(this.widthPerSecond * this.originalDuration, rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = targetDelta;

        originalWidth = targetDelta.x;
    }

    public void UpdateParams(float widthPerSecond)
    {
        this.widthPerSecond = widthPerSecond;

        // Sets all params
        Vector2 targetDelta = new Vector2(this.widthPerSecond * poseFrame.duration, rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = targetDelta;

        originalWidth = this.widthPerSecond * originalDuration;
    }

    public void DeletePoseFrame()
    {
        poseEditor.edittingPoseSequence.Remove(poseFrame);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO: Show Preview of the frame
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                poseEditor.ToggleFrameItemMenu(this);
                break;
        }
    }

    public void ToggleMenu()
    {
        poseEditorMenu.SetActive(!poseEditorMenu.activeInHierarchy);
    }

    public void CloseMenu()
    {
        poseEditorMenu.SetActive(false);
    }

    public void OpenMenu()
    {
        poseEditorMenu.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            localPosition = rectTransform.localPosition;
            frameItemParent = transform.parent;
            prevFrameItemIdx = transform.GetSiblingIndex();
            isFocused = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (expandStatus.isEnabled)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;

                float targetWidth = sizeDelta.x + Input.GetAxis("Mouse X") * frameItemExpandSpeed;
                Vector2 targetDelta = new Vector2(targetWidth, sizeDelta.y);
                float deltaPercentage = targetWidth / originalWidth;

                rectTransform.sizeDelta = targetDelta;
                poseFrame.duration = originalDuration * deltaPercentage;
            }
            else if (dragStatus.isEnabled)
            {
                rectTransform.position = Input.mousePosition;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (expandStatus.isAvailable)
                expandStatus.isEnabled = true;
            else if (dragStatus.isAvailable)
            {
                dragStatus.isEnabled = true;
                transform.SetParent(GameObject.Find("Overlay Canvas").transform);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // If dragging the item
            if (dragStatus.isEnabled)
            {
                var hoverObjects = Helper.GetOverUI(poseEditor.poseEditorCanvas);
                var idx = PoseFrameBrowserItem.GetInsertIndex(e => e.GetComponent<PoseEditorFrameItem>() != null && e.GetComponent<PoseEditorFrameItem>() != this, hoverObjects);

                Debug.Log("Idx: " + idx);
                if (idx == -1)
                {
                    // rectTransform.parent = frameItemParent;
                    // rectTransform.localPosition = localPosition;
                    poseEditor.InsertPoseEditorTimelineFrame(gameObject, poseFrame, angleToggle.isOn, velocityToggle.isOn, prevFrameItemIdx);
                }
                else
                {
                    poseEditor.edittingPoseSequence.Remove(poseFrame);
                    poseEditor.InsertPoseEditorTimelineFrame(gameObject, poseFrame, angleToggle.isOn, velocityToggle.isOn, idx);
                }
            }
            else // If not dragging, only expanding
            {
                rectTransform.localPosition = localPosition;
            }

            // Reset the status
            dragStatus.Set(true, false);
            expandStatus.Set(false, false);
            isFocused = false;
        }
    }
}