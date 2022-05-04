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
public class PoseEditorFrameItem : PoseEditorItem
{
    public Toggle angleToggle;
    public Toggle velocityToggle;
    public FrameBasedPoseEditor poseEditor;
    public PoseFrame poseFrame;
    
    private void Update()
    {
        if (isPressed && (expandStatus.isEnabled == false && dragStatus.isEnabled == false))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                pressedTimer += Time.deltaTime;
                if (pressedTimer > 0.25f)
                {
                    expandStatus.Set(true, false);
                    dragStatus.Set(false, false);
                }
            }
        }
        else
            pressedTimer = 0.0f;
    }

    public void InitParams(FrameBasedPoseEditor poseEditor, float widthPerSecond, PoseFrame poseFrame)
    {
        rectTransform = GetComponent<RectTransform>();

        // angleToggle.isOn = angle;
        // velocityToggle.isOn = velocity;

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

    public override void UpdateParams(float widthPerSecond)
    {
        this.widthPerSecond = widthPerSecond;

        // Sets all params
        Vector2 targetDelta = new Vector2(this.widthPerSecond * poseFrame.duration, rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = targetDelta;

        originalWidth = this.widthPerSecond * originalDuration;
    }

    public override void DeleteItem()
    {
        poseEditor.edittingPoseSequence.Remove(poseFrame);
        Destroy(gameObject);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                poseEditor.ToggleFrameItemMenu(this);
                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (expandStatus.isEnabled)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;

                float targetWidth = sizeDelta.x + Input.GetAxis("Mouse X") * GlobalController.Instance.setting.editor.expandSpeed;
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

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // If dragging the item
            if (dragStatus.isEnabled)
            {
                var hoverObjects = Helper.GetOverUI(poseEditor.poseEditorCanvas);
                var idx = BrowserItemBase.GetInsertIndex(e => e.GetComponent<PoseEditorFrameItem>() != null && e.GetComponent<PoseEditorFrameItem>() != this, hoverObjects, poseEditor);

                Debug.Log("Idx: " + idx);
                if (idx == -1)
                {
                    // rectTransform.parent = frameItemParent;
                    // rectTransform.localPosition = localPosition;
                    poseEditor.InsertItem(gameObject, poseFrame, angleToggle.isOn, velocityToggle.isOn, prevItemIdx);
                }
                else
                {
                    poseEditor.edittingPoseSequence.Remove(poseFrame);
                    poseEditor.InsertItem(gameObject, poseFrame, angleToggle.isOn, velocityToggle.isOn, idx);
                }
            }
            else // If not dragging, only expanding
            {
                rectTransform.localPosition = localPosition;
            }

            // Reset the status
            dragStatus.Set(true, false);
            expandStatus.Set(false, false);
            isPressed = false;
        }
    }
}