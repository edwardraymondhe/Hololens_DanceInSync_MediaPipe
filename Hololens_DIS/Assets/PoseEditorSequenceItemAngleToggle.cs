using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseEditorSequenceItemAngleToggle : PoseEditorSequenceItemToggleBase
{
    public override void Select()
    {
        Debug.Log("Select Angle");
        if (true)
        {
            /*
             * TODO:
             * 1. See what index are we
             * 2. Map the index to the original
             * 3. Original toggles on/off the neighbours
             */

            float siblingIndex = (float)transform.GetSiblingIndex();
            
            float rate = (poseEditorSequenceItem.poseSequence.curCycles / poseEditorSequenceItem.poseSequence.rawCycles);
            if (rate > 1)
            {
                // xxxxxxxx|yyyyyyyy|zzzzzzzz (original)
                // xxxxxxxx|Xxxxxxxx|yyyyyyyy|yyyyyyyy|zzzzzzzz|zzzzzzzz (curr)

                // 9 / 2 = 4.5f => 5

                int originalIndex = Mathf.FloorToInt(siblingIndex / rate);
                Debug.Log(string.Format("Sib -> Raw: {0} -> {1}", siblingIndex, originalIndex));
                int siblingMin = (int)((originalIndex) * rate);
                int siblingMax = (int)((originalIndex + 1) * rate - 1);
                Debug.Log(string.Format("Sib Range: {0} - {1}", siblingMin, siblingMax));

                bool allOn = true;
                for (int i = siblingMin; i <= siblingMax; i++)
                {
                    var item = transform.parent.GetChild(i);
                    if (item.GetComponent<Toggle>().isOn == false)
                        allOn = false;
                }

                for (int i = siblingMin; i <= siblingMax; i++)
                {
                    var item = transform.parent.GetChild(i);
                    item.GetComponent<Toggle>().isOn = !allOn;
                }

                poseEditorSequenceItem.poseSequence.angles[originalIndex] = !allOn;
            }
            else
            {
                // xxxxXxxx|yyyyyyyy|zzzzzzzz (curr)
                // xxxxxxxx|XXxxxxxx|yyyyyyyy|yyyyyyyy|zzzzzzzz|zzzzzzzz (original)

                // rate = 24 / 48 = 0.5f

                // [(5-1) / 0.5f, 5 * 0.5f]

                int originalMin = (int)((siblingIndex) / rate);
                int originalMax = (int)((siblingIndex + 1) / rate - 1);

                toggle.isOn = !toggle.isOn;
                for (int i = originalMin; i <= originalMax; i++)
                    poseEditorSequenceItem.poseSequence.angles[i] = toggle.isOn;
            }

        }
    }
}
