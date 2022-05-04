using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSubWindowController : SubWindowController
{
    public Dictionary<string, InputField> inputFields = new Dictionary<string, InputField>();
    public InputField inputFieldTemplate;
    public GameObject inputFieldParents;
    public Button submitButton;
    public void AddInput(string text)
    {
        InputField inputField = Instantiate(inputFieldTemplate, inputFieldParents.transform).GetComponent<InputField>();
        inputField.gameObject.SetActive(true);
        inputField.placeholder.GetComponent<Text>().text = text;
        inputFields.Add(text, inputField);
    }

    public void AddSubmit(UnityEngine.Events.UnityAction call)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        submitButton.onClick.AddListener(call);
    }
}
