using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// 
    /// The NPCControlManager is the main set of [IO, UI and Camera] controllers for the scene's NPCs.
    /// 
    /// SETUP: Create an empty game object and add the NPCControllerManager
    /// 
    /// CAMERAS: every camera will implement the INPCCameraController - types and other values are defined there.
    /// 
    /// IN WAY TO DEPRECTAION --
    /// 
    /// To set up the controller:
    /// 1) Before anything:
    ///     Ensure there exist a Camera with tag "MainCamera" (default)
    /// 2) Create an empty GameObject in the scene - for example "NPC Controllers"
    /// 3) Add the NPCControlManager to the "NPC Controllers" object.
    ///     - The camera will be set as a child of the "NPC Controllers" game object.
    /// 4) If you want to enable the UI, add a Canvas as a child of the "NPC Controllers"
    ///    and add it to the NPCControlManager Canvas public member from the inspector.
    ///    You can also create a secondary Canvas as the context menu one. Set its camera to be
    ///    the main one.
    /// 
    /// </summary>
    
    public class NPCControlManager : MonoBehaviour {

        #region Enums

        #endregion

        #region Members

        [SerializeField]
        public bool DebugMode = true;

        [SerializeField]
        public bool EnableIOController = false;

        [SerializeField]
        public bool EnableCameraController = false;

        [SerializeField]
        public bool EnableUIController = false;

        public CAMERA_TYPE CameraType;
        public IO_CONTROLLER_TYPE IOType;

        public NPCController NPCControllerTarget = null;
        public Canvas MainCanvas;
        public Canvas ContextCanvas;
        private static string mNPCUI = "NPC UI";
        
        private Dictionary<CAMERA_TYPE, NPCCameraController> g_AvailableCameras;
        private Dictionary<IO_CONTROLLER_TYPE, NPCIOController> g_AvailableIO;

        #endregion Members
        
        NPCCameraController g_NPCCamera;
        NPCIOController g_NPCIO;
        NPCUIController g_UI;
        
        public void SetTarget(NPCController c) {
            NPCControllerTarget = c;
        }

        #region Unity_Functions

        void Reset() {

            if (FindObjectsOfType<NPCControlManager>().Length > 1)
                throw new System.Exception("NPCControlManager --> ERROR - a NPCControlManager has already been added!");

            // CAMERA - adjustment
            
            Transform cam = Camera.main.transform;
            Camera.main.nearClipPlane = 0.001f;
            
            // UI

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) {
                GameObject io = null;
                if (GetComponent<NPCUIController>() == null) {
                    io = new GameObject();
                    io.AddComponent<NPCUIController>();
                }
                io.transform.parent = this.transform;
                canvas.transform.SetParent(io.transform);
                io.name = mNPCUI;
            }

        }

        void Awake () {

            try {

                if(NPCControllerTarget == null)
                    FindMainNPC();

                InitializeCamera();
                InitializeIO();
                
                // g_UI = GetComponentInChildren<NPCUIController>();

                //if (g_UI != null) {
                //    // g_UI.NPCCamera = g_NPCCamera;
                //} else EnableUIController = false;
                //if (NPCController != null) {
                //    g_NPCIO.SetTarget(NPCController);
                //}

            } catch(System.Exception e) {
                Debug("NPCControlManager --> Components missing from the controller, please add them. Disabling controller: " + e.Message);
                this.enabled = false;
            }
	    }

        void Update() {
            
            if (EnableIOController) {
                g_NPCIO.UpdateIO();
            }
            if (EnableCameraController) {
                g_NPCCamera.UpdateCamera();
            }
            if (EnableUIController) {
                g_UI.UpdateUI();
            }
        }
        #endregion

        void FindMainNPC() {
            foreach (NPCController npc in FindObjectsOfType<NPCController>()) {
                if (npc.MainAgent) {
                    if (NPCControllerTarget == null) {
                        NPCControllerTarget = npc;
                    } else if (npc != NPCControllerTarget) {
                        NPCControllerTarget = null;
                        Debug("NPCControlManager --> Many NPCs marked as MainAgents, Target defaults to empty");
                    }
                }
            }
        }

        private void InitializeCamera() {

            g_AvailableCameras = new Dictionary<CAMERA_TYPE, NPCCameraController>();
            
            foreach(NPCCameraController c in GetComponents<NPCCameraController>()) {
                g_AvailableCameras.Add(c.GetCameraType(), c);
            }

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.BaseType == typeof(NPCCameraController)) {
                    NPCCameraController c = (NPCCameraController)Activator.CreateInstance(t);
                    if(!g_AvailableCameras.ContainsKey(c.GetCameraType()))
                        g_AvailableCameras.Add(c.GetCameraType(), c);
                }
            }

            if(CameraType == CAMERA_TYPE.NONE) {
                EnableCameraController = false;
            } else if (g_AvailableCameras.ContainsKey(CameraType)) {
                g_NPCCamera = g_AvailableCameras[CameraType];
                g_NPCCamera.Initialize();
            } else if (EnableCameraController) {
                EnableCameraController = false;
                Debug("No camera was initialized - controller is not enabled");
            } 
        }

        private void InitializeIO() {

            g_AvailableIO = new Dictionary<IO_CONTROLLER_TYPE, NPCIOController>();

            foreach (NPCIOController c in GetComponents<NPCIOController>()) {
                g_AvailableIO.Add(c.GetIOControllerType(), c);
            }

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.BaseType == typeof(NPCIOController)) {
                    NPCIOController c = (NPCIOController) Activator.CreateInstance(t);
                    if (!g_AvailableIO.ContainsKey(c.GetIOControllerType()))
                        g_AvailableIO.Add(c.GetIOControllerType(), c);
                }
            }

            if (g_AvailableIO.ContainsKey(IOType)) {
                g_NPCIO = g_AvailableIO[IOType];
                g_NPCIO.Initialize();
            } else if (EnableIOController) {
                Debug("No IO controller was initialized - non has been enabled");
            }
        }
        
        public void Debug(string msg) {
            if(DebugMode) {
                UnityEngine.Debug.Log(this + ": " + msg);
            }
        }
    }

}
