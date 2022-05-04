using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// An UI item in the Pose Editor
/// 1. Simple Drag & Drop, Change position
/// 2. Long Hold & Drag, Change scale
/// TODO: 3. Hover, Preview Pose
/// TODO: 4. Right Click, menu for Delete & Rename
/// </summary>
public class PoseEditorSequenceItem : PoseEditorItem
{
    public SequenceBasedPoseEditor poseEditor;
    public PoseSequence poseSequence;
    public float draggedAmount = 0.0f;
    private float huamnoidTimer = 0.0f;

    [Header("Toggles")]
    public GameObject angleToggleParents;
    public GameObject velocityToggleParents;
    public GameObject angleTogglePrefab;
    public GameObject velocityTogglePrefab;


    private void Update()
    {
        
        GetComponentInChildren<Text>().text = poseSequence.fileName + "\n" + poseSequence.curCycles;

        if (isPressed && (expandStatus.isEnabled == false && dragStatus.isEnabled == false))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                pressedTimer += Time.deltaTime;
                if (pressedTimer > GlobalController.Instance.setting.editor.expandTimer)
                {
                    expandStatus.Set(true, false);
                    dragStatus.Set(false, false);
                }
            }
        }
        else
            pressedTimer = 0.0f;
    }

    public void InitParams(SequenceBasedPoseEditor poseEditor, float widthPerSecond, PoseSequence poseSequence)
    {
        rectTransform = GetComponent<RectTransform>();
        
        this.poseEditor = poseEditor;
        this.poseSequence = poseSequence;
        this.originalDuration = poseSequence.GetDuration();
        this.widthPerSecond = widthPerSecond;
        GetComponentInChildren<Text>().text = poseSequence.fileName + "\n" + poseSequence.curCycles;

        // Sets all params
        Vector2 targetDelta = new Vector2(this.widthPerSecond * this.originalDuration, rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = targetDelta;

        originalWidth = targetDelta.x;
        UpdateToggles();
    }

    private void UpdateToggles()
    {
        for (int i = angleToggleParents.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(angleToggleParents.transform.GetChild(i).gameObject);
            Destroy(velocityToggleParents.transform.GetChild(i).gameObject);
        }

        float rate = (poseSequence.curCycles / poseSequence.rawCycles);
        for (int siblingIndex = 0; siblingIndex < poseSequence.curCycles * 8.0f; siblingIndex++)
        {
            var angle = Instantiate(angleTogglePrefab, angleToggleParents.transform);
            var velocity = Instantiate(velocityTogglePrefab, velocityToggleParents.transform);

            angle.GetComponent<PoseEditorSequenceItemAngleToggle>().poseEditorSequenceItem = this;
            velocity.GetComponent<PoseEditorSequenceItemVelocityToggle>().poseEditorSequenceItem = this;

            if (rate > 1)
            {
                // xxxxxxxx|yyyyyyyy|zzzzzzzz (original)
                // xxxxxxxx|Xxxxxxxx|yyyyyyyy|yyyyyyyy|zzzzzzzz|zzzzzzzz (curr)

                // 9 / 2 = 4.5f => 5
                int originalIndex = Mathf.FloorToInt((float)siblingIndex / rate);
                angle.GetComponent<Toggle>().isOn = poseSequence.angles[originalIndex];
                velocity.GetComponent<Toggle>().isOn = poseSequence.velocities[originalIndex];
            }
            else
            {
                // xxxxXxxx|yyyyyyyy|zzzzzzzz (curr)
                // xxxxxxxx|XXxxxxxx|yyyyyyyy|yyyyyyyy|zzzzzzzz|zzzzzzzz (original)

                // rate = 24 / 48 = 0.5f

                // [(5-1) / 0.5f, 5 * 0.5f]

                int originalMin = (int)(((float)siblingIndex) / rate);
                int originalMax = (int)(((float)siblingIndex + 1) / rate - 1);

                bool velocityHasTrue = false;
                bool angleHasTrue = false;
                for (int i = originalMin; i <= originalMax; i++)
                {
                    if (poseSequence.angles[i] == true)
                        angleHasTrue = true;
                    if (poseSequence.velocities[i] == true)
                        velocityHasTrue = true;
                }

                if (angleHasTrue)
                    for (int i = originalMin; i <= originalMax; i++)
                        poseSequence.angles[i] = true;

                if (velocityHasTrue)
                    for (int i = originalMin; i <= originalMax; i++)
                        poseSequence.velocities[i] = true;

                velocity.GetComponent<Toggle>().isOn = velocityHasTrue;
                angle.GetComponent<Toggle>().isOn = angleHasTrue;
            }
        }
    }

    public override void UpdateParams(float widthPerSecond)
    {
        this.widthPerSecond = widthPerSecond;

        // Sets all params
        Vector2 targetDelta = new Vector2(this.widthPerSecond * poseSequence.GetDuration(), rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = targetDelta;

        originalWidth = this.widthPerSecond * originalDuration;

        LayoutRebuilder.ForceRebuildLayoutImmediate(angleToggleParents.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(velocityToggleParents.GetComponent<RectTransform>());
    }

    public override void DeleteItem()
    {
        poseEditor.edittingPoseSequences.Remove(poseSequence);
        Destroy(gameObject);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                poseEditor.ToggleSequenceItemMenu(this);
                break;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        huamnoidTimer = 0.0f;
    }


    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (expandStatus.isEnabled)
            {
                float newDraggedAmount = draggedAmount + Input.GetAxis("Mouse X") * GlobalController.Instance.setting.editor.expandSpeed;
                if (Mathf.FloorToInt(draggedAmount) != Mathf.FloorToInt(newDraggedAmount))
                {
                    poseSequence.SetCyclePower(Mathf.FloorToInt(newDraggedAmount));
                    UpdateToggles();
                }
                draggedAmount = newDraggedAmount;
                UpdateParams(this.widthPerSecond);
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
                var idx = BrowserItemBase.GetInsertIndex(e => e.GetComponent<PoseEditorSequenceItem>() != null && e.GetComponent<PoseEditorSequenceItem>() != this, hoverObjects, poseEditor);

                Debug.Log("Idx: " + idx);
                if (idx == -1)
                {
                    // rectTransform.parent = frameItemParent;
                    // rectTransform.localPosition = localPosition;
                    poseEditor.InsertItem(gameObject, poseSequence, prevItemIdx);
                }
                else
                {
                    poseEditor.edittingPoseSequences.Remove(poseSequence);
                    poseEditor.InsertItem(gameObject, poseSequence, idx);
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