using UnityEngine.EventSystems;

public class PoseEditorFrameItemAngleToggle : PoseEditorFrameItemToggleBase
{
    public override void Select()
    {
        if (poseEditorFrameItem.poseEditor.angleStatus.isEnabled)
        {
            if (!poseEditorFrameItem.poseEditor.selectedPoseEditorFrameItems.Contains(poseEditorFrameItem))
                poseEditorFrameItem.poseEditor.selectedPoseEditorFrameItems.Add(poseEditorFrameItem);
        }
    }
}
