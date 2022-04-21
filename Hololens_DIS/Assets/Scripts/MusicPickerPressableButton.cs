using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicPickerPressableButton : BasePickerPressableButton
{
    private MusicStageController stageController;
    public void Init(MusicStageController stageController, int idx, string name)
    {
        var finalName = (name == "" ? "Song" : name);
        this.buttonName = finalName;
        text.text = finalName;

        this.stageController = stageController;
        this.buttonIndex = idx;
    }

    public void SetMusic()
    {
        stageController.SetMusic(buttonIndex);
    }
}
