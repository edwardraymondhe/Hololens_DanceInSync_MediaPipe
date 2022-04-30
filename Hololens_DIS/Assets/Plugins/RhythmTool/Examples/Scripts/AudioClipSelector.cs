using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RhythmTool.Examples
{
    /// <summary>
    /// The SongSelector selects a song and starts analyzing and playing it.
    /// </summary>
    public abstract class SongSelector : MonoBehaviour
    {
        public RhythmAnalyzer analyzer;
        public RhythmPlayer player;
        public virtual void PauseSong()
        {
            player.Pause();
        }

        public virtual void UnPauseSong()
        {
            player.UnPause();
        }

        public virtual void NextSong()
        {
            //Stop playing.
            player.Stop();           
        }

        public virtual void PrevSong()
        {
            //Stop playing.
            player.Stop();
        }

        public virtual void StopSong()
        {
            player.Stop();
        }

        public virtual bool IsPlaying()
        {
            return player.isPlaying;
        }
    }

    /// <summary>
    /// The AudioClipSelector is a SongSelector that selects a song from a list of AudioClips.
    /// </summary>
    public class AudioClipSelector : SongSelector
    {
        public List<AudioClipData> songs;
        public float initialLength = 0.0f;

        private int currentSong = -1;

        void Start()
        {
            //Immediately go to the next song.
            // NextSong();
        }

        public override async void NextSong()
        {
            base.NextSong();

            //Clean up old resources.
            Destroy(player.rhythmData);

            //start analyzing the next song.
            currentSong++;

            if (currentSong >= songs.Count)
                currentSong = 0;

            await PlaySong(currentSong);
        }

        private RhythmData AnalyzeRhythmData()
        {
            AudioClip audioClip = songs[currentSong].audioClip;
            //return analyzer.Analyze(audioClip, audioClip.length / 2.0f);
            return analyzer.Analyze(audioClip, audioClip.length / 10.0f);
        }


        public override async void PrevSong()
        {
            base.PrevSong();

            //Clean up old resources.
            Destroy(player.rhythmData);

            //start analyzing the next song.
            currentSong--;

            if (currentSong < 0)
                currentSong = songs.Count - 1;

            await PlaySong(currentSong);
        }

        public async Task PlaySong(int idx)
        {
            currentSong = idx;

            //Give the RhythmData to the RhythmPlayer.
            player.rhythmData = AnalyzeRhythmData();

            while (!analyzer.isDone)
                await Task.Yield();

            Play();
        }

        public void Play()
        {
            player.time = 0.0f;
            player.Play();
        }

        public AudioClip GetSong()
        {
            return songs[currentSong].audioClip;
        }
    }
}