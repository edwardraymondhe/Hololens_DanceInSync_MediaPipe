using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool.Examples
{
    [CreateAssetMenu(menuName = "Audio/New AudioClipData")]
    public class AudioClipData : ScriptableObject
    {
        public AudioClip audioClip;
        public float startTime;
    }
}