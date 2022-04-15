public class PoseSequenceBrowserItem : BrowserItemBase
{
    public override void SetChosenPose()
    {
        poseEditor.SetChosenPoseSequence(itemName);
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
