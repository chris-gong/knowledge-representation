using System;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    
    [Serializable]
    [AttributeUsage(AttributeTargets.All)]
    public class NPCAnimation : System.Attribute {
        public string Name;
        public int AnimationHash;
#if UNITY_EDITOR
        public AnimatorState AnimatorState;
        public AnimationClip AnimationClip;
        public AnimatorControllerLayer AnimatorLayer;
#endif
        public ANIMATION_PARAM_TYPE ParamType;
        public ANIMATION_LAYER Layer;
        public int RuntimeAnimatorLayer;

        readonly public float FazeInEnd;
        readonly public float FazeOutStart;

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        readonly public float Duration;

        public bool Timed;
        public NPCAnimation(string name, ANIMATION_PARAM_TYPE paramType, ANIMATION_LAYER layer) {
            this.Name = name;
            this.ParamType = paramType;
            this.Layer = layer;
        }
        public NPCAnimation(string name, ANIMATION_PARAM_TYPE paramType, ANIMATION_LAYER layer, float duration) : this(name,paramType,layer) {
            Duration = duration * 1000; // store duration in milliseconds
            Timed = true;
        }

        public NPCAnimation(string name, ANIMATION_PARAM_TYPE paramType, ANIMATION_LAYER layer, float duration, float fazeInEnd, float fazeOutStart) : this(name, paramType, layer, duration) {
            FazeInEnd = fazeInEnd * 1000;
            FazeOutStart = fazeOutStart * 1000;
            Timed = true;
        }

    }
}