using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public enum ANIMATED_AUDIO_TYPE {
        SPEECH,
        ACTION
    }

    /// <summary>
    /// TODO - Under construction
    /// Links an audio clip with a series of animations to be
    /// executed and specified times.
    /// </summary>
    [Serializable]
    public class NPCAnimatedAudio : ScriptableObject {

        #region Members
        private float g_AnimationTime;
        private float g_AudioTime;
        private float g_Duration;
        private List<AnimationStamp> g_AnimationsList;
        private List<AudioClipStamp> g_AudioClipsList;
        #endregion

        #region Properties

        [SerializeField]
        public string Name;

        [SerializeField]
        public AudioClipStamp[] Clips;

        [SerializeField]
        public AnimationStamp[] Animations;

        [SerializeField]
        public bool RandomizeAnimations;

        [SerializeField]
        public ANIMATED_AUDIO_TYPE Type;

        #endregion

        #region Utility_Classes
        [Serializable]
        public class AnimationStamp {

            [SerializeField]
            public GESTURE_CODE Gesture;

            [SerializeField]
            [Range(0,100f)]
            public float Time;

            private float g_ExecutionTime;

            public void SetExecutionTime(float time) {
                g_ExecutionTime = time;
            }

            public float ExecutionTime() {
                return g_ExecutionTime;
            }
        }

        [Serializable]
        public class AudioClipStamp {

            [SerializeField]
            public AudioClip Clip;

            [SerializeField]
            [Range(0, 100f)]
            public float Time;

            private float g_ExecutionTime;

            public void SetExecutionTime(float time) {
                g_ExecutionTime = time;
            }

            public float ExecutionTime() {
                return g_ExecutionTime;
            }
        }

        #endregion

        #region Public_Functions

        public float Length() {
            return g_Duration;
        }

        public Queue<AnimationStamp> AnimationsQueue() {
            return new Queue<AnimationStamp>(g_AnimationsList);
        }

        public Queue<AudioClipStamp> AudioClipsQueue() {
            return new Queue<AudioClipStamp>(g_AudioClipsList);
        }

        public void ShuffleAnimations() {

        }

        /// <summary>
        /// Only to be called on initialization during runtime
        /// </summary>
        public void BakeAnimatedAudioClip(Dictionary<GESTURE_CODE,NPCAnimation> anims) {
            if(Application.isPlaying) {

                foreach(AudioClipStamp c in Clips) {
                    g_AudioTime += c.Clip.length;
                }
                foreach(AnimationStamp a in Animations) {
                    g_AnimationTime += anims[a.Gesture].Duration / 1000f;
                }

                g_Duration = Mathf.Max(g_AudioTime, g_AnimationTime);

                foreach(AnimationStamp a in Animations) {
                    a.SetExecutionTime(g_Duration * (a.Time / 100.0f));
                }

                foreach (AudioClipStamp c in Clips) {
                    c.SetExecutionTime(g_Duration * (c.Time / 100.0f));
                }

                g_AnimationsList = new List<AnimationStamp>(Animations);
                g_AudioClipsList = new List<AudioClipStamp>(Clips);

                if (RandomizeAnimations) {
                    // TODO - shuffle anims here
                } else
                    g_AnimationsList.Sort((emp1, emp2) => emp1.Time.CompareTo(emp2.Time));

                g_AnimationsList.Sort((emp1, emp2) => emp1.Time.CompareTo(emp2.Time));

            }
        }
        
        #endregion
    }
}