using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoseEditorSequenceItemToggleBase : MonoBehaviour, IPointerEnterHandler
{
    public PoseEditorSequenceItem poseEditorSequenceItem;
    public Toggle toggle;
    private void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }

    public virtual void Select()
    {
    }
}
