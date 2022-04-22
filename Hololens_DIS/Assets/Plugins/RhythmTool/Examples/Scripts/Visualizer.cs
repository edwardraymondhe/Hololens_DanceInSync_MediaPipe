using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmTool.Examples
{
    public class Visualizer : MonoBehaviour
    {
        public RhythmAnalyzer analyzer;
        public RhythmPlayer player;
        public RhythmEventProvider eventProvider;

        public Text textBPM;

        public Line linePrefab;

        public GameObject symbolParent;

        public bool isMrtk = false;

        private List<Line> lines;

        private List<Chroma> chromaFeatures;

        private Note lastNote = Note.FSHARP;

        public Color beatColor = Color.white;
        public Color onSetColor = Color.yellow;
        public Color segmentColor = Color.green;

        void Awake()
        {           
            analyzer.Initialized += OnInitialized;
            player.Reset += OnReset;

            eventProvider.Register<Beat>(OnBeat);
            eventProvider.Register<Onset>(OnOnset);
            eventProvider.Register<Value>(OnSegment, "Segments");

            lines = new List<Line>();

            chromaFeatures = new List<Chroma>();
        }
        
        void Update()
        {
            if (!player.isPlaying)
                return;

            UpdateLines();
        }
        
        private void UpdateLines()
        {
            float time = player.time;

            //Destroy all lines with a timestamp less than the current playback time.
            List<Line> toRemove = new List<Line>();
            foreach (Line line in lines)
            {
                if (line.timestamp < time || line.timestamp > time + eventProvider.offset)
                {
                    Destroy(line.gameObject);
                    toRemove.Add(line);
                }
            }

            foreach (Line line in toRemove)
                lines.Remove(line);

            //Update all Line positions based on their timestamp and current playback time, 
            //so they will move as the song plays.
            foreach (Line line in lines)
            {
                Vector3 position;
                if (isMrtk)
                    position = line.transform.localPosition;
                else
                    position = line.transform.position;

                position.x = (line.timestamp - time) * xFactor;

                // line.transform.position = position;
                line.transform.localPosition = position;
            }
        }

        public float xFactor = 100f;
                
        private void OnInitialized(RhythmData rhythmData)
        {
            //Start playing the song.
            //player.Play();
        }

        private void OnReset()
        {
            //Destroy all lines when playback is reset.
            foreach (Line line in lines)
                Destroy(line.gameObject);

            lines.Clear();
        }

        private void OnBeat(Beat beat)
        {
            //Instantiate a line to represent the Beat.
            CreateLine(beat.timestamp, 0, 1, beatColor, 1, beatParent);

            //Update BPM text.
            float bpm = Mathf.Round(beat.bpm * 10) / 10;
            textBPM.text = "(" + bpm + " BPM)";
        }

        private void OnOnset(Onset onset)
        {
            //Clear any previous Chroma features.
            chromaFeatures.Clear();

            //Find Chroma features that intersect the Onset's timestamp.
            player.rhythmData.GetIntersectingFeatures(chromaFeatures, onset.timestamp, onset.timestamp);

            //Instantiate a line to represent the Onset and Chroma feature.
            foreach (Chroma chroma in chromaFeatures)
                CreateLine(onset.timestamp, -2 + (float)chroma.note * .1f, .2f, onSetColor, onset.strength / 10, onSetParent);

            if (chromaFeatures.Count > 0)
                lastNote = chromaFeatures[chromaFeatures.Count - 1].note;

            //If no Chroma Feature was found, use the last known Chroma feature's note.
            if (chromaFeatures.Count == 0)
                CreateLine(onset.timestamp, -2 + (float)lastNote * .1f, .2f, onSetColor, onset.strength / 10, onSetParent);
        }

        private void OnSegment(Value segment)
        {
            //Instantiate a line to represent the segment.
            CreateLine(segment.timestamp, -3, 1, segmentColor, segment.value / 10, segmentParent);
        }

        public Transform beatParent;
        public Transform onSetParent;
        public Transform segmentParent;
        public float x = 10f;
        public float z = 1f;
        public float factor = 0.01f;

        private void CreateLine(float timestamp, float position, float scale, Color color, float opacity, Transform parent)
        {
            Line line;
            
            if (isMrtk)
            {
                line = Instantiate(linePrefab, parent);
                line.transform.localPosition = new Vector3(0, position, 0);
                line.transform.localScale = new Vector3(x, scale * factor, z);
            }
            else
            {
                line = Instantiate(linePrefab);
                line.transform.position = new Vector3(0, position, 0);
                line.transform.localScale = new Vector3(.1f, scale, .01f);
            }



            line.Init(color, opacity, timestamp);

            lines.Add(line);
        }
    }
}