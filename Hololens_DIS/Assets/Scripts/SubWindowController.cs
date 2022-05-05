using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubWindowController : MonoBehaviour
{
    public Text titleText;
    public Text contentText;

    public virtual void Init(string title, string content)
    {
        titleText.text = title;
        contentText.text = content;

        transform.SetParent(GameObject.Find("Sub Window Canvas").transform);
        transform.localPosition = Vector2.zero;
    }

    public virtual void Close()
    {
        Destroy(gameObject);
    }

    public virtual void ChangeContent(string content)
    {
        contentText.text = content;
    }
}
