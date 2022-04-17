using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScriptableBase : ScriptableObject
{
    /// <summary>
    /// Name to display, and store as file. (e.g. Samurai, T-Pose, etc...)
    /// </summary>
    public string fileName = "";
}
