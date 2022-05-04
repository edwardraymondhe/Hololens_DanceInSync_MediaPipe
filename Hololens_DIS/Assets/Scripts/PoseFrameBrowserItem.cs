using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// An UI item in the browser: 
/// 1. Click, Preview Pose
/// 2. Hover, Preview Pose within the UI
/// 3. Drag, Drag&Drop Pose to Editor
/// </summary>
public class PoseFrameBrowserItem : BrowserItemBase, IPointerEnterHandler, IPointerExitHandler
{
    public override void SetChosenPose()
    {
        poseEditor.SetChosenPoseFrame(itemName);
    }
    
    #region IPointerClickHandler implementation

    public override void OnPointerExit(PointerEventData eventData)
    {
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        poseEditor.BeginPreviewPoseFrameToHumanoid(itemName);
        poseEditor.isSequenceLoaded = false;
    }


    public override void OnPointerClick(PointerEventData eventData)
    {
        // TODO: show a poseFrame applied on to the humanoid
    }

    #endregion
}
