using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoseEditorFrameItemToggleBase : MonoBehaviour, IPointerEnterHandler
{
    public PoseEditorFrameItem poseEditorFrameItem;
    Toggle toggle;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }

    public virtual void Select()
    {
    }
}
