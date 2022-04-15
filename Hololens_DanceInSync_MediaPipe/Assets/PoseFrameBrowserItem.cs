using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// An UI item in the browser: 
/// 1. Click, Preview Pose
/// 2. Hover, Preview Pose within the UI
/// 3. Drag, Drag&Drop Pose to Editor
/// </summary>
public class PoseFrameBrowserItem : BrowserItemBase, IDragHandler, IEndDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // TODO: Instantiate a new mock item, destroys when finished drag, follows mouse when dragging
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Gets the objects being hovered by cursor
        var hoverObjects = Helper.GetOverUI(poseEditor.poseEditorCanvas);

        // Calculate the exact index to be inserted to
        var idx = GetInsertIndex(e => e.GetComponent<PoseEditorFrameItem>() != null, hoverObjects);
        poseEditor.InsertNewPoseEditorTimelineFrame(idx, itemName);

        // TODO: Update the angular velocities between all the frames
    }

    public static int GetInsertIndex(System.Predicate<GameObject> match, List<GameObject> objs)
    {
        var hoverFrame = objs.Find(match);
        if (hoverFrame != null)
        {
            var hoverFrameComp = hoverFrame.GetComponent<PoseEditorFrameItem>();
            var hoverFrameMask = objs.Find(e => e.name.ToLower().Contains("mask") && e.transform.parent.parent.gameObject == hoverFrame);

            if (hoverFrameMask != null)
            {
                // Gets the sibling index of the hovered frame
                int neighbourIdx = hoverFrame.transform.GetSiblingIndex();

                // If hovering a previous mask, give the new item the same idx as the hoverred frame, and move all the frames + 1 index
                if (hoverFrameMask == hoverFrameComp.prevEditorItemMask)
                    return neighbourIdx;
                else // If not, go for the index after that neighbour
                    return neighbourIdx + 1;
            }
        }
        
        // TODO: If index is invalid, do something
        return -1;
    }

    public override void SetChosenPose()
    {
        poseEditor.SetChosenPoseFrame(itemName);
    }

    #region IPointerClickHandler implementation
    /*
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // TODO: show a poseFrame applied on to the humanoid
    }
    */

    #endregion
}
