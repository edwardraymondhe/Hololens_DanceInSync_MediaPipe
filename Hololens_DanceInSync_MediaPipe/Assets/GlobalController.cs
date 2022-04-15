using UnityEngine;

public class GlobalController : GlobalSingleTon<GlobalController>
{
    public Canvas poseEditorView;
    public Canvas poseBrowserView;
    public Canvas controlPanelView;

    [SerializeField] bool poseSequenceEditMode;
    [SerializeField] bool poseBrowserMode;
    
    // Update is called once per frame
    void Update()
    {
        poseBrowserView.gameObject.SetActive(poseBrowserMode);
        poseEditorView.gameObject.SetActive(poseSequenceEditMode);
    }        

    public void TogglePoseSequenceEdit()
    {
        poseSequenceEditMode = !poseSequenceEditMode;
    }

    public void TogglePoseBrowser()
    {
        poseBrowserMode = !poseBrowserMode;
    }
}
