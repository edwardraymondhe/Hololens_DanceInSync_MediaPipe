using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrowserItemBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler 
{
    public Text itemDisplayText;
    public string itemName;
    public PoseEditor poseEditor;

    public bool isFocuesd = false;

    public GameObject mockItemPrefab;
    public GameObject mockItem;

    public virtual void Init(PoseEditor poseEditor, string name)
    {
        itemName = name;
        itemDisplayText.text = name;

        this.poseEditor = poseEditor;
    }

    public virtual void SetChosenPose() { }

    public static int GetInsertIndex(System.Predicate<GameObject> match, List<GameObject> objs, PoseEditor poseEditor)
    {
        var hoverItem = objs.Find(match);
        if (hoverItem != null)
        {
            if (poseEditor is SequenceBasedPoseEditor seqEditor)
            {
                var hoverItemComp = hoverItem.GetComponent<PoseEditorSequenceItem>();
                var hoverItemMask = objs.Find(e => e.name.ToLower().Contains("mask") && e.transform.parent.parent.gameObject == hoverItem);

                if (hoverItemMask != null)
                {
                    // Gets the sibling index of the hovered frame
                    int neighbourIdx = hoverItem.transform.GetSiblingIndex();

                    // If hovering a previous mask, give the new item the same idx as the hoverred frame, and move all the frames + 1 index
                    if (hoverItemMask == hoverItemComp.prevEditorItemMask)
                        return neighbourIdx;
                    else // If not, go for the index after that neighbour
                        return neighbourIdx + 1;
                }
            }
            else
            {
                var hoverItemComp = hoverItem.GetComponent<PoseEditorFrameItem>();
                var hoverItemMask = objs.Find(e => e.name.ToLower().Contains("mask") && e.transform.parent.parent.gameObject == hoverItem);

                if (hoverItemMask != null)
                {
                    // Gets the sibling index of the hovered frame
                    int neighbourIdx = hoverItem.transform.GetSiblingIndex();

                    // If hovering a previous mask, give the new item the same idx as the hoverred frame, and move all the frames + 1 index
                    if (hoverItemMask == hoverItemComp.prevEditorItemMask)
                        return neighbourIdx;
                    else // If not, go for the index after that neighbour
                        return neighbourIdx + 1;
                }
            }
        }

        // TODO: If index is invalid, do something
        return -1;
    }

    #region IPointerClickHandler implementation

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        mockItem = Instantiate(mockItemPrefab);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        // TODO: Instantiate a new mock item, destroys when finished drag, follows mouse when dragging
        mockItem.transform.position = eventData.position;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Destroy(mockItem);

        // Gets the objects being hovered by cursor
        var hoverObjects = Helper.GetOverUI(poseEditor.poseEditorCanvas);

        // Calculate the exact index to be inserted to
        int index = 0;
        
        if (poseEditor is SequenceBasedPoseEditor)
            index = GetInsertIndex(e => e.GetComponent<PoseEditorSequenceItem>() != null, hoverObjects, poseEditor);
        else
            index = GetInsertIndex(e => e.GetComponent<PoseEditorFrameItem>() != null, hoverObjects, poseEditor);
        
        poseEditor.InsertNewItem(index, itemName);

        // TODO: Update the angular velocities between all the frames
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
    }


    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    #endregion
}
