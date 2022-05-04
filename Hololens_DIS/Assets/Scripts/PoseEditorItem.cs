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
public class PoseEditorItem : MonoBehaviour, IPointerClickHandler,IPointerUpHandler,IPointerEnterHandler,IPointerDownHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public GameObject prevEditorItemMask;
    public GameObject nextEditorItemMask;
    public GameObject prevEditorItem;
    public GameObject nextEditorItem;
    public GameObject poseEditorMenu;

    protected float originalWidth;

    protected float widthPerSecond;
    protected float originalDuration;

    protected string itemName;
    protected RectTransform rectTransform;
    protected Vector3 localPosition;
    protected Transform itemParent;
    protected int prevItemIdx;

    public MouseStatus expandStatus = new MouseStatus(true, false);
    public MouseStatus dragStatus = new MouseStatus(true, false);

    protected float pressedTimer = 0.0f;
    protected bool isPressed = false;
    protected bool isFocused = false;

    public GameObject highlightUI;


    public virtual void UpdateParams(float widthPerSecond)
    {
    }

    public virtual void DeleteItem()
    {
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isFocused = true;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    public virtual void ToggleMenu()
    {
        poseEditorMenu.SetActive(!poseEditorMenu.activeInHierarchy);
    }

    public virtual void CloseMenu()
    {
        poseEditorMenu.SetActive(false);
    }

    public virtual void OpenMenu()
    {
        poseEditorMenu.SetActive(true);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            localPosition = rectTransform.localPosition;
            itemParent = transform.parent;
            prevItemIdx = transform.GetSiblingIndex();
            isPressed = true;
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (expandStatus.isAvailable)
                expandStatus.isEnabled = true;
            else if (dragStatus.isAvailable)
            {
                dragStatus.isEnabled = true;
                transform.SetParent(GameObject.Find("Overlay Canvas").transform);
            }
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isFocused = false;
    }
}