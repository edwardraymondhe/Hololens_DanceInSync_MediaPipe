using UnityEngine;
using UnityEngine.UI;

public class BrowserItemBase : MonoBehaviour
{
    public Text itemDisplayText;
    public string itemName;
    public PoseEditor poseEditor;

    public void Init(PoseEditor poseEditor, string name)
    {
        itemName = name;
        itemDisplayText.text = name;

        this.poseEditor = poseEditor;
    }

    public virtual void SetChosenPose() { }
}
