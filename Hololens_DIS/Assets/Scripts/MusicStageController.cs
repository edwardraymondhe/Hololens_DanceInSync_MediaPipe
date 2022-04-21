using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using RhythmTool.Examples;
using RhythmTool;
using System.Threading.Tasks;

public class MusicStageController : BasePrepareStageController
{
    private void Start()
    {
        SpawnCollection();
        SetMusic(0);
    }

    private void Update()
    {
        // TODO: Loop for 8 * 8Beat
    }
    protected override void SpawnCollection()
    {
        base.SpawnCollection();

        var index = 0;
        foreach (var song in audioClipSelector.songs)
        {
            var musicPressableButton = Instantiate(objectPrefab, objectCollection.transform);
            musicPressableButton.GetComponent<MusicPickerPressableButton>().Init(this, index, song.audioClip.name);
            index++;
        }

        objectCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    #region Music Mode
    
    public AudioClipSelector audioClipSelector;
    public float chosenEightBeatDuration = 0.0f;
    public int chosenStartEightBeat = 4;
    public float chosenStartTime = 0.0f; 
    
    public async void SetMusic(int index)
    {
        await audioClipSelector.PlaySong(index);
        GetEightBeat();
    }

    /// <summary>
    /// 1. Get the duration for a whole eight-beats
    /// 2. Get the start time for 
    /// </summary>
    public void GetEightBeat()
    {
        var rhythmData = audioClipSelector.player.rhythmData;
        List<Beat> beats = new List<Beat>();
        float startTime = audioClipSelector.songs.Find(e => e.audioClip.name == audioClipSelector.player.audioClip.name).startTime;
        float endTime = startTime + 0.01f;
        bool isEndOfMusic = false;

        // Loop through the audio
        while (true)
        {
            // Store the features within [startTime, endTime] -> beats
            rhythmData.GetFeatures(beats, startTime, endTime);

            // Get total of x beats (x % 8 == 0)
            int totalEightBeatCount = 2;
            int chosenEightBeatIndex = 2;
            float currentEightBeatDuration = 0.0f;

            // |-1|---(0)---1---2---3---4---5---6---|7|---(8)---9---10---11---12---13---14---|15|---(16)
            if (beats.Count == (totalEightBeatCount * 8 + 1))
            {
                Debug.Log("-----8-Beat-----");
                Beat first_1 = beats[(chosenEightBeatIndex - 1) * 8 + 0];
                Beat second_1 = beats[(chosenEightBeatIndex - 1) * 8 + 8];
                currentEightBeatDuration = second_1.timestamp - first_1.timestamp;

                // Check if the 8-beat have balanced intervals
                var avgDuration = currentEightBeatDuration / 8f;
                Beat prevBeat = null;
                Beat nextBeat;
                bool isAvgDurationValid = true;

                // Copy the chosen beats
                Beat[] pickedBeats = new Beat[9];
                beats.CopyTo((chosenEightBeatIndex - 1) * 8, pickedBeats, 0, pickedBeats.Length);

                foreach (var beat in pickedBeats)
                {
                    nextBeat = beat;

                    if (prevBeat != null)
                    {
                        var duration = nextBeat.timestamp - prevBeat.timestamp;

                        // The error between current and average duration must be smaller thant 5%
                        var error = Mathf.Abs((duration - avgDuration) / avgDuration);
                        Debug.Log(string.Format("Duration: {0}\nAvg: {1}\nError: {2}", duration, avgDuration, error));

                        if (error >= 0.05f)
                            isAvgDurationValid = false;
                    }

                    prevBeat = nextBeat;
                }

                if (isAvgDurationValid == true)
                {
                    Debug.Log("The average duration is valid.");
                    var str = string.Format("Interval: {0} - {1}\nDuration: {2}\nAvg Duration: {3}", first_1.timestamp, second_1.timestamp, currentEightBeatDuration, avgDuration);

                    chosenEightBeatDuration = currentEightBeatDuration;

                    // 2nd 8-beat 的 第一拍 + (4th - 2nd) * 八拍时长
                    Debug.Log("Second_1: " + second_1.timestamp);
                    Debug.Log("Current -> ChosenStart: " + chosenEightBeatIndex + " -> " + chosenStartEightBeat);
                    chosenStartTime = (second_1.timestamp - avgDuration) + (chosenStartEightBeat - chosenEightBeatIndex) * chosenEightBeatDuration;
                    break;
                }
                else
                {
                    Debug.Log("The average duration is invalid, need to change the \"start time\" manually.");
                }
            }

            beats.Clear();
            endTime += 0.01f;

            if (endTime > audioClipSelector.GetSong().length)
            {
                isEndOfMusic = true;
                break;
            }
        }

        if (isEndOfMusic)
            Debug.Log("The audio isn't long enough to calculate a valid 8-beat.");
    }
 
    #endregion
}
