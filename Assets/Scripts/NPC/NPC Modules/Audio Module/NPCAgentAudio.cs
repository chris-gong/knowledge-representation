using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPC;
using System;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

public class NPCAgentAudio : MonoBehaviour, INPCModule {

    public enum COMPONENT_TYPE {
        CONTROL,
        RIGHT_FOOT,
        LEFT_FOOT
    }

    #region Members

    public GameObject RightFoot;
    public GameObject LeftFoot;
    public GameObject RightHand;
    public GameObject LeftHand;
    public AudioClip[] FootSteps;

    private NPCController g_NPCController;
    private Dictionary<COMPONENT_TYPE, GameObject> g_Components;
    private int LastFootstepAssigned = 0;

    #endregion

    #region Properties

    [SerializeField]
    public bool FootstepsAudioEnabled = false;

    [SerializeField]
    public bool Enabled = true;
    
    public COMPONENT_TYPE Type;

    #endregion

    #region Unity_Functions
    
    private void OnCollisionEnter(Collision collision) {
        if(Enabled) {

        }
    }

    #endregion

    #region INPCModule

    public void CleanupModule() {

    }

    public void InitializeModule() {
        g_NPCController = GetComponent<NPCController>();
        Type = COMPONENT_TYPE.CONTROL;
        g_Components = new Dictionary<COMPONENT_TYPE, GameObject>();
        if (FootSteps.Length > 0) {
            if (RightFoot != null) {
                InitializeComponent(RightFoot, COMPONENT_TYPE.RIGHT_FOOT);
            }
            if (LeftFoot != null) {
                InitializeComponent(LeftFoot, COMPONENT_TYPE.LEFT_FOOT);
            }
        }

    }

    public bool IsEnabled() {
        return Enabled;
    }

    public bool IsUpdateable() {
        return true;
    }

    public string NPCModuleName() {
        return "NPC Agent Audio Handler";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.CONTROLS;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.AUDIO_CONTROL;
    }

    public void RemoveNPCModule() {

    }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    public void TickModule() {
        // TODO - here we will, every once in a while, shuffle a8nd update
        // current sounds
    }

    #endregion

    #region Private_Functions

    private void InitializeComponent(GameObject go, COMPONENT_TYPE type) {
        if (go != null) {
            NPCAgentAudio rf = go.AddComponent<NPCAgentAudio>();
            AudioSource aSource = go.AddComponent<AudioSource>();
            rf.Type = type;
            if (type == COMPONENT_TYPE.LEFT_FOOT || type == COMPONENT_TYPE.RIGHT_FOOT) {
                FootstepsAudioEnabled = true;
                aSource.clip = LastFootstepAssigned < FootSteps.Length ? FootSteps[LastFootstepAssigned] : FootSteps[0];
                LastFootstepAssigned++;
            }
            g_Components.Add(type, go);
        } else {
            g_NPCController.Debug(NPCModuleName() + " - Attempted to initialize an audio component with empty game object");
        }
    }

    #endregion
}