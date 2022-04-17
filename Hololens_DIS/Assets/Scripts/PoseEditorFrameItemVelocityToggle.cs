using UnityEngine.EventSystems;

public class PoseEditorFrameItemVelocityToggle : PoseEditorFrameItemToggleBase
{
    public override void Select()
    {
        if (poseEditorFrameItem.poseEditor.velocityStatus.isEnabled)
        {
            if (!poseEditorFrameItem.poseEditor.selectedPoseEditorFrameItems.Contains(poseEditorFrameItem))
                poseEditorFrameItem.poseEditor.selectedPoseEditorFrameItems.Add(poseEditorFrameItem);
        }
    }
}
