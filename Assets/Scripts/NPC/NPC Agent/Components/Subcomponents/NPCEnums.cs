using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    #region Enums

    public enum LOG_TYPE {
        INFO,
        WARNING,
        ERROR
    }

    // NPC Behavior

    public enum BEHAVIOR_STATUS {
        INACTIVE,
        PENDING,
        STOPPED,
        INTERRUPTED,
        STARTED,
        RUNNING,
        SUCCESS,
        FAILURE
    }

    // NPC Object

    /// <summary>
    /// The type of interaction an agent can perform on an object.
    /// </summary>
    public enum INTERACTION_TYPE {
        GRAB,
        OPERATE,
        SIT,
        CONSUME
    }

    // Locomotion

    /// <summary>
    /// Describes the locomotion state of the agent. FORWARD and BACKWARDS alter's an
    /// agent Speed and Direction parameters, while others, such as RUN, SPRINT, TOOGLE_WALK
    /// and FALL modify their FORWARD and BACKWARDS behaviors.
    /// </summary>
    public enum LOCO_STATE {
        IDLE,
        FRONT,
        FORWARD,
        BACKWARDS,
        LEFT,
        RIGHT,
		SPRINT,
        RUN,
        WALK,
		TOGGLE_WALK,
        TOGGLE_ACTION,
        DUCK,
        GROUND,
        JUMP,
        FALL
    }

    /// <summary>
    /// AGENT_FORWARD allows for Direction to be independent from Camera.forward direction.
    /// CAMERA_FORWARD will make the agent Direction to be relative for the Camera.forward
    /// vector.
    /// </summary>
    public enum LOCO_CONTROL {
        AGENT_RELATIVE,
        CAMERA_RELATIVE
    }

    // Gestures

    /// <summary>
    /// To add gestures, just add it to the animator controller then define it here with the NPCAnimation System.Attribute
    /// </summary>
    public enum GESTURE_CODE {
        [NPCAnimation("Climb_Low", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 1f)]
        CLIMB_LOW,
        [NPCAnimation("Climb_High", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 1f)]
        CLIMB_HIGH,
        [NPCAnimation("Flag_Female", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FLAG, 0f)]
        FEMALE_FLAG,
        [NPCAnimation("Flag_Action", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FLAG, 0f)]
        ACTION_FLAG,
        [NPCAnimation("Flag_Drunk", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FLAG, 0f)]
        DRUNK_FLAG,
        [NPCAnimation("Idle_C", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FULL_BODY, 1.4f)]
        IDLE_LEAN_B,
        [NPCAnimation("Gest_Acknowledge", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.7f)]//1.4
        ACKNOWLEDGE,
        [NPCAnimation("Gest_Angry", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 3f)]
        ANGRY,
        [NPCAnimation("Gest_Why", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.633333f)]//1.4
        WHY,
        [NPCAnimation("Gest_Short_Wave", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 4.733334f)]//4.4f
        WAVE_HELLO,
        [NPCAnimation("Gest_Negate", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.4f)]
        NEGATE,
        [NPCAnimation("Body_Die", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 1.2f)]
        DIE,
        [NPCAnimation("Gest_Anger_Posture", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 19f)]
        ANGER_POSTURE,
        [NPCAnimation("Gest_Dissapointment", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 3.11f)]
        DISSAPOINTMENT,
        [NPCAnimation("Gest_Hurray", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2.05f)]
        HURRAY,
        [NPCAnimation("Gest_Grab_Front", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 3.75f)]//2f
        GRAB_FRONT,
        [NPCAnimation("Gest_Talk_Long", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 4.5f)]//4.1f
        TALK_LONG,
        [NPCAnimation("Gest_Talk_Short", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.5f)]//1.12f
        TALK_SHORT,
        [NPCAnimation("Gest_Think", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2.5f)]//2.15f
        THINK,
        [NPCAnimation("Gest_Greet_At_Distance", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2f)]
        GREET_AT_DISTANCE,
        [NPCAnimation("Body_Sit", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 2.75f)]
        SIT,
        [NPCAnimation("Body_Sitting", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FULL_BODY, 2.5f)]
        SITTING,
        [NPCAnimation("Body_Sitting_Floor", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FULL_BODY, 3.5f)]
        SITTING_FLOOR,
        [NPCAnimation("Gest_Look_Around", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 5.866669f)]//5.3f
        LOOK_AROUND,
        [NPCAnimation("Gest_Reject", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2f)]
        REJECT,
        [NPCAnimation("Gest_Explanation", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.5f)]
        EXPLANATION,
        [NPCAnimation("Body_Idle_Small_Steps", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 7f)]
        IDLE_SMALL_STEPS,
        [NPCAnimation("Gest_Writing", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 4.3f)]
        DESK_WRITING,
        [NPCAnimation("Body_On_The_Phone", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FULL_BODY, 15f)]
        STAND_PHONE_CALL,
        [NPCAnimation("Gest_Warning", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 3.1f)]
        WARNING,
        [NPCAnimation("Gest_Yawn", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2.2f)]
        YAWN,
        [NPCAnimation("Body_Bored_Idle", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY, 12.4f)]
        BORED_IDLE,
        [NPCAnimation("Gest_Drinking", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2.4f)]
        DRINK,
        [NPCAnimation("Gest_Texting", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 15f)]
        TEXTING,
        [NPCAnimation("Gest_Desperation", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 0.9000015f)]//1.5f
        DESPERATION,
        [NPCAnimation("Gest_Singing", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 5.5f)]
        SING,
        [NPCAnimation("Gest_Clap", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 1.166667f)]//1.05f
        CLAP,
        [NPCAnimation("Gest_Headbang", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.GESTURE, 3.5f)]
        MUSIC_HEADBANG,
        [NPCAnimation("Gest_Drunk", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 9.5f)]
        DRUNK,
        [NPCAnimation("Gest_Blow_Kiss", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 2.2f)]
        BLOW_KISS,
        [NPCAnimation("Gest_Point_To", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.GESTURE, 1.2f)]
        POINT_TO,
        [NPCAnimation("Gest_Dance_HipHop", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY_GESTURE, 12.3f)]
        DANCE_HIPHOP,
        [NPCAnimation("Gest_Military_Salute", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY_GESTURE, 1.25f)]
        MILITARY_SALUTE,
        [NPCAnimation("Gest_Dance_HipHop_2", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY_GESTURE, 18.1f)]
        DANCE_HIPHOP_2,
        [NPCAnimation("Gest_Dance_House", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FULL_BODY_GESTURE, 25.3f)]
        DANCE_HOUSE,
        [NPCAnimation("Gest_Push_Button", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.INTERACTION, 2.1f)]
        PUSH_BUTTON,
        [NPCAnimation("Gest_Pick_Up", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.INTERACTION, 1.1f)]
        PICK_UP,
        [NPCAnimation("Gest_Activate_Object", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.INTERACTION, 1.1f)]
        ACTIVATE_OBJECT,
        [NPCAnimation("Gest_Free_Walk_A", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.FULL_BODY_GESTURE, 10f)]
        FREE_WALK_A,
        [NPCAnimation("Facial_Blink", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_GESTURE, 0.1f)]
        BLINK,
        [NPCAnimation("Facial_Slight_Smile", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.INTERACTION, 0.625f)]  //0.1f
        SLIGHT_SMILE,
        [NPCAnimation("Facial_Phoneme_A", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_PHONEME, 0.5f)]
        PHONEME_A,
        [NPCAnimation("Facial_Phoneme_E", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_PHONEME, 0.5f)]
        PHONEME_E,
        [NPCAnimation("Facial_Phoneme_P", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_PHONEME, 0.5f)]
        PHONEME_P,
        [NPCAnimation("Facial_Phoneme_O", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_PHONEME, 0.5f)]
        PHONEME_O,
        [NPCAnimation("Facial_Phoneme_U", ANIMATION_PARAM_TYPE.TRIGGER, ANIMATION_LAYER.FACIAL_PHONEME, 0.5f)]
        PHONEME_U,
        [NPCAnimation("Hands_Right_Grab", ANIMATION_PARAM_TYPE.BOOLEAN, ANIMATION_LAYER.HANDS, 0.1f)]
        HAND_GRAB_RIGHT
    }

    // Navigation

    public enum NAV_STATE {
        DISABLED = 0,
        STEERING_NAV,
        NAVMESH_NAV
    }

    public enum OBSTACLE_TYPE {
        BLOCKER = 100,
        HARDENER = 10
    }

    public enum GRID_SCALE {
        ONE = 1,
        HALF = 2,
        QUARTER = 4
    }

    public enum GRID_DIRECTION {
        CURRENT,
        NORTH,
        SOUTH,
        EAST,
        WEST,
        NORTH_EAST,
        NORTH_WEST,
        SOUTH_EAST,
        SOUTH_WEST
    }

    // NPC Animation
    
    public enum ANIMATION_PARAM_TYPE {
        BOOLEAN,
        FLOAT,
        TRIGGER
    }

    public enum ANIMATION_LAYER {
        FLAG,
        GESTURE,
        FULL_BODY,
        FULL_BODY_GESTURE,
        INTERACTION,
        FACIAL_GESTURE,
        FACIAL_PHONEME,
        HANDS
    }

    // NPC Goals

    public enum NPCGOAL_TYPE {
        BODY,
        TRAIT,
        STATE
    }

    public enum NPCGOAL_STATUS {
        PENDING,
        SATISFIED,
        INTERRUPTED,
        FAILED
    }

    // INPCModule

    public enum NPC_MODULE_TARGET {
        BODY,
        PERCEPTION,
        AI,
        CONTROLS
    }

    public enum NPC_MODULE_TYPE {
        AUDIO_LISTENER,
        PATHFINDER,
        BEHAVIOR,
        EXPLORATION,
        AUDIO_CONTROL,
        IO_CONTROL,
        NATURAL_LANGUAGE
    }

    // INPCPerceivable

    public enum PERCEIVEABLE_TYPE {
        NPC,
        OBJECT
    }

    public enum PERCEIVE_WEIGHT {
        NONE,
        WEIGHTED,
        TOTAL
    }

    #endregion

}