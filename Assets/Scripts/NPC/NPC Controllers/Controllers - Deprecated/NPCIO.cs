using UnityEngine;
using System.Collections;
using System;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public class NPCIO : MonoBehaviour {
        
        NPCController g_NPCController;
        private NPCCamController g_Camera;
        private bool gIOTargetting = false;
        NPCControlManager g_NPCControlManager;
        bool g_SelectDragging = false;

        private float gMouseDragSmootFactor = 2.0f;

        public void SetTarget(NPCController target) {
            g_NPCController = target;
            gIOTargetting = (g_NPCController != null);
        }

        void Start() {
            g_Camera = FindObjectOfType<NPCCamController>();
            g_NPCControlManager = FindObjectOfType<NPCControlManager>();
            if (g_NPCControlManager == null)
                Debug.Log("NPCIO --> Using NPCIO withou a NPCControlManager");
        }
    
        public enum INPUT_KEY {
            FORWARD = KeyCode.W,
            MODIFIER = KeyCode.LeftShift,
            MODIFIER_SEC = KeyCode.LeftControl,
            BACKWARD = KeyCode.S,
            LEFT = KeyCode.A,
            RIGHT = KeyCode.D,
            DUCK = KeyCode.LeftControl,
            JUMP = KeyCode.Space,
            RUN = KeyCode.LeftShift,
            INVENTORY = KeyCode.I,
            MAP = KeyCode.M,
            FOCUS_LOOK = KeyCode.Mouse1, // secondary mouse button
            CONTEXT_ACTION = KeyCode.Mouse1,
            SELECT_AGENT = KeyCode.Mouse0,
            FREE_CAM = KeyCode.F1,
            THIRD_CAM = KeyCode.F2,
            FIRST_CAM = KeyCode.F3
        }
	
	    // Update is called once per frame
	    public void UpdateIO () {
            // Only applicatble if currently targetting NPC
            gIOTargetting = g_NPCController != null;
            UpdateKeys();
            
        }

        private void UpdateKeys() {
            // Only if targetting agent
            if (gIOTargetting && 
                (g_Camera.CurrentMode == NPCCamController.CAMERA_MODE.THIRD_PERSON
                || g_Camera.CurrentMode == NPCCamController.CAMERA_MODE.FIRST_PERSON)) {
                // modifiers
                if (Input.GetKey((KeyCode)INPUT_KEY.RUN)) {
                    g_NPCController.Body.Move(LOCO_STATE.RUN);
                } else if (Input.GetKey((KeyCode)INPUT_KEY.DUCK)) {
                    g_NPCController.Body.Move(LOCO_STATE.DUCK);
                } else g_NPCController.Body.Move(LOCO_STATE.WALK);

                // forward motion
                if (Input.GetKey((KeyCode)INPUT_KEY.FORWARD)) {
                    g_NPCController.Body.Move(LOCO_STATE.FORWARD);
                } else if (Input.GetKey((KeyCode)INPUT_KEY.BACKWARD)) {
                    g_NPCController.Body.Move(LOCO_STATE.BACKWARDS);
                } else g_NPCController.Body.Move(LOCO_STATE.IDLE);

                // turning
                if (Input.GetKey((KeyCode)INPUT_KEY.LEFT)) {
                    g_NPCController.Body.Move(LOCO_STATE.LEFT);
                } else if (Input.GetKey((KeyCode)INPUT_KEY.RIGHT)) {
                    g_NPCController.Body.Move(LOCO_STATE.RIGHT);
                } else g_NPCController.Body.Move(LOCO_STATE.FRONT);

                // jumping
                if (Input.GetKeyDown((KeyCode)INPUT_KEY.JUMP)) { // Note we only read jump once
                    g_NPCController.Body.Move(LOCO_STATE.JUMP);
                }

                // focusing
                if (Input.GetKey((KeyCode)INPUT_KEY.FOCUS_LOOK)) {
                    if (g_Camera != null && g_NPCController != null && !g_Camera.CloseUp) {
                        g_Camera.CloseUp = true;
                        g_NPCController.Body.LookAround(true);
                    } else {
                        float xDelta = Input.GetAxis("Mouse X");
                        float yDelta = Input.GetAxis("Mouse Y");
                        if (Mathf.Abs(xDelta) > 0.75f) {
                            Vector3 pos = g_NPCController.Body.TargetObject.position + (xDelta > 0 ? 1 : -1) * g_NPCController.Body.TargetObject.right;
                            g_NPCController.Body.TargetObject.position = Vector3.Lerp(g_NPCController.Body.TargetObject.position, pos, Time.deltaTime * gMouseDragSmootFactor);
                        }
                        if (Mathf.Abs(yDelta) > 0.75f) {
                            Vector3 pos = g_NPCController.Body.TargetObject.position + (yDelta > 0 ? 1 : -1) * g_NPCController.Body.TargetObject.up;
                            g_NPCController.Body.TargetObject.position = Vector3.Lerp(g_NPCController.Body.TargetObject.position, pos, Time.deltaTime * gMouseDragSmootFactor);
                        }
                    }
                } else if (Input.GetKeyUp((KeyCode)INPUT_KEY.FOCUS_LOOK)) {
                    g_Camera.CloseUp = false;
                    g_NPCController.Body.LookAround(false);
                    g_Camera.ResetView();
                }   
            } else {

                // select agent
                if (Input.GetKey((KeyCode)INPUT_KEY.SELECT_AGENT)) {
                    // not dragging
                    if(g_SelectDragging) {
                        // TODO
                    } else {
                        RaycastHit hitInfo = new RaycastHit();
                        bool clickedOn = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                        if (clickedOn) {
                            // handle single selection
                            NPCController npc = hitInfo.transform.gameObject.GetComponent<NPCController>();
                            if (npc != null) {
                                if (g_NPCController != null) g_NPCController.SetSelected(false);
                                g_NPCController = npc;
                                npc.SetSelected(true);
                            } else {
                                // deselect
                                if (g_NPCController != null) {
                                    g_NPCController.SetSelected(false);
                                    g_NPCController = null;
                                }
                            }
                        }
                    }
                }

                if (Input.GetKeyDown((KeyCode)INPUT_KEY.CONTEXT_ACTION)) {
                    if(g_NPCController != null) {
                        if(g_Camera.CurrentMode == NPCCamController.CAMERA_MODE.ISOMETRIC) {
                            RaycastHit hitInfo = new RaycastHit();
                            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
                                if (Input.GetKey((KeyCode)INPUT_KEY.MODIFIER)) {
                                    g_NPCController.Body.OrientTowards(hitInfo.point);
                                } else if(Input.GetKey((KeyCode)INPUT_KEY.MODIFIER_SEC)) {
                                    g_NPCController.Body.RunTo(hitInfo.point);
                                } else
                                    g_NPCController.Body.GoTo(hitInfo.point);
                            }
                        }
                    }
                }
            }
        }
    }

}
