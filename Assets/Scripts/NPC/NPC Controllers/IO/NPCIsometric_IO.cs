using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    enum IO_CONTROLS {
        MOUSE_RIGHT = KeyCode.Mouse0,
        MOUSE_LEFT = KeyCode.Mouse1,
        MOUSE_MIDDLE = KeyCode.Mouse2
    }

    public class NPCIsometric_IO : NPCIOController {

        #region Membres

        [SerializeField]
        public bool FocusedTarget = false;

        [SerializeField]
        [Range(0.1f,0.5f)] public float DoubleClickThreshold = 0.05f;

        private float g_LastClick;
        private bool g_DoubleClick = false;
        private GameObject g_CurrentHoverTarget;
        private Dictionary<GameObject, Material> g_ObjectsMaterials;
        private Material g_Outliner;

        #endregion

        #region NPCIOCONTROLLER

        public override void Initialize() {
            g_Outliner = Resources.Load<Material>("Materials/Outliner");
            g_ObjectsMaterials = new Dictionary<GameObject, Material>();
            g_ControlManager = GetComponent<NPCControlManager>();
            if(FocusedTarget) {
                if (g_ControlManager.NPCControllerTarget == null) {
                    g_ControlManager.Debug("No main character was defined, defaulting to free isometric");
                    FocusedTarget = false;
                } else {
                    g_Target = g_ControlManager.NPCControllerTarget;
                }
            }
            g_LastClick = Time.realtimeSinceStartup;
        }

        public override void HandleKeyboard() { }

        public override void HandlePointer() {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            bool hit = Physics.Raycast(ray, out rayHit);
            GameObject go = hit ? rayHit.collider.gameObject : null;

            if (Input.anyKeyDown) {

                float currentClick = Time.realtimeSinceStartup,
                    deltaTime = Time.realtimeSinceStartup - g_LastClick;

                if (deltaTime < DoubleClickThreshold) {
                    g_DoubleClick = true;
                } else g_DoubleClick = false;
                
                g_LastClick = currentClick;

                if (hit) {

                    if (Input.GetKeyDown((KeyCode) IO_CONTROLS.MOUSE_RIGHT)) {
                        if (!FocusedTarget) {
                            NPCController c = go.GetComponent<NPCController>();
                            if (c != null) {
                                g_Target = c;
                                g_Target.SetSelected(true);
                            } else {
                                if (g_Target != null) {
                                    g_Target.SetSelected(false);
                                    g_Target = null;
                                }
                            }
                        }
                    }

                    if (Input.GetKeyDown((KeyCode) IO_CONTROLS.MOUSE_LEFT)) {

                        if (g_Target != null) {
                            if (Input.GetKey(KeyCode.LeftShift))
                                g_Target.Body.OrientTowards(rayHit.point);
                            else {
                                if(g_DoubleClick)
                                    g_Target.Body.RunTo(rayHit.point);
                                else
                                    g_Target.Body.GoTo(rayHit.point);
                            }
                        }
                    }

                    if (Input.GetKeyDown((KeyCode)IO_CONTROLS.MOUSE_MIDDLE)) {
                        Debug.Log("Mouse 3 clicked");
                    }

                } else if (hit && go.GetComponent<INPCPerceivable>() != null) {
                    if (g_CurrentHoverTarget != go) {
                        Renderer renderer = go.GetComponent<Renderer>();
                        if (!g_ObjectsMaterials.ContainsKey(go)) {
                            g_ObjectsMaterials.Add(go, renderer.material);
                        }
                        renderer.sharedMaterial = g_Outliner;
                        g_CurrentHoverTarget = go;
                    }
                } else {
                    if(g_CurrentHoverTarget != null && g_ObjectsMaterials.ContainsKey(go)) {
                        g_CurrentHoverTarget.GetComponent<Renderer>().sharedMaterial = g_ObjectsMaterials[go];
                    }
                    g_CurrentHoverTarget = null;
                }

            } 
        }

        public override void HandleAxis() { }

        public override IO_CONTROLLER_TYPE GetIOControllerType() {
            return IO_CONTROLLER_TYPE.ISOMETRIC;
        }
        #endregion
    }
}
