using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoseSequenceBrowserItem : BrowserItemBase
{
    public override void SetChosenPose()
    {
        if (poseEditor is SequenceBasedPoseEditor seqEditor)
            seqEditor.SetChosenPoseSequence(itemName);
        else if (poseEditor is FrameBasedPoseEditor frmEditor)
            frmEditor.SetChosenPoseSequence(itemName);
    }

    #region IPointerClickHandler implementation
    public override void OnPointerExit(PointerEventData eventData)
    {
        poseEditor.isSequenceLoaded = false;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        poseEditor.BeginPreviewPoseSequenceToHumanoid(itemName);
    }


    public override void OnPointerClick(PointerEventData eventData)
    {
        poseEditor.isSequenceLoaded = true;
    }
    #endregion
}
