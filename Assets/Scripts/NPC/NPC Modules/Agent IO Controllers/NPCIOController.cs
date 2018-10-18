using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NPC;
using UnityEngine.UI;
using UnityEngine.EventSystems;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

[RequireComponent(typeof(NPCController))]
[ExecuteInEditMode]
public class NPCIOController : MonoBehaviour, INPCModule {

    #region Enums

    public enum IO_TYPE {
        THIRD_PERSON,
        ISOMETRIC
    }

    private enum ISOMETRIC_KEYBOARD_INPUT {
        MOVE_CAM_LEFT= KeyCode.A,
        MOVE_CAM_RIGHT = KeyCode.D,
        MOVE_CAM_UP = KeyCode.W,
        MOVE_CAM_DOWN = KeyCode.S,
        ROT_CAM_LEFT = KeyCode.E,
        ROT_CAM_RIGHT = KeyCode.Q
    }

    private enum ISOMETRIC_MOUSE_INPUT {
        INTERACT = KeyCode.Mouse0,
        WALK_TO = KeyCode.Mouse1
    }

    private enum THIRD_PERSON_KEYBOARD_INPUT {
        TOGGLE_NAVIGATION = KeyCode.F1,
        FORWARD = KeyCode.W,
        BACKWARDS = KeyCode.S,
        CAM_ROT_LEFT = KeyCode.Q,
        CAM_ROT_RIGHT = KeyCode.E,
        TURN_LEFT = KeyCode.A,
        TURN_RIGHT = KeyCode.D,
        RUN = KeyCode.LeftShift,
		TOGGLE_WALK = KeyCode.Tab,
        TOGGLE_ACTION = KeyCode.LeftAlt,
        JUMP = KeyCode.Space,
        CROUCH = KeyCode.LeftControl
    }

    private enum THIRD_PERSON_MOUSE_INPUT {
        INTERACT = KeyCode.Mouse0,
        FLOAT_CAMERA = KeyCode.Mouse2,
        WALK_TO = KeyCode.Mouse1
    }

    #endregion

    #region Members

    [SerializeField]
    public bool Enabled = true;

    [SerializeField]
    public IO_TYPE IOMode;

    [SerializeField]
    public bool EnableAgentCamera = true;

    [SerializeField]
    public bool EnableMouseCameraRotation = true;

    [SerializeField]
    public bool CaptureCurrentCameraOffset = false;

    [SerializeField]
    public float UpdateSmoothing = 2f;

    [SerializeField]
    public float IsometricSpeedModifier = 1f;

    [SerializeField]
    public float CameraDistanceTolerance = 0.05f;

    [SerializeField]
    public Vector3 CameraOffset;
    
    [SerializeField]
    public float CameraRotationThreshold = 1f;

    [SerializeField]
    public float CameraRotationSpeed;

    [SerializeField]
    public float CameraTargetHeight = 1.2f;

    [SerializeField]
    public InteractionMapping[] InteractionsMap;

    [SerializeField]
    private NPCController g_NPCController;

    [SerializeField]
    public GameObject InteractionsPanel;


    private Dictionary<GameObject, InteractionMapping> g_InteractionsMap;
    private bool g_Initialized = false;
    private Vector3 g_MouseLastPosition;
    private Transform g_Camera;
    private NPCObject g_FocusedObject;
    private bool g_ObjectFocused = false;
    private NPCBehavior g_BehaviorAgent;

    private const float MIN_ISO_CAM_HEIGHT = 1f;
    private const float MAX_ISO_CAM_HEIGHT = 7f;
    private const string Interaction_Grab = "Interaction_Grab";
    private const string Interaction_Sit = "Interaction_Sit";
    private const string Interaction_Operate = "Interaction_Operate";
    private const string Interaction_Consume = "Interaction_Consume";

    #endregion

    #region Unity_Methods

    // Update is called once per frame
    void Update() {
        if (Application.isPlaying) {
            if (Enabled && g_Initialized) {
                UpdateCamera();
                UpdateInput();
            }
        } else {
            if(CaptureCurrentCameraOffset) {
                switch (IOMode) {
                    case IO_TYPE.THIRD_PERSON:
                        CameraOffset = Camera.main.transform.position - g_NPCController.GetTransform().position;
                        break;
                }
                CaptureCurrentCameraOffset = false;
            }
        }
    }

    public void Reset() {
        g_NPCController = GetComponent<NPCController>();
    }

    #endregion

    #region INPCModule

    public void CleanupModule() {
        StopCoroutine(MenuListener());
    }

    public void InitializeModule() {
        g_BehaviorAgent = GetComponent<NPCBehavior>();
        g_Camera = Camera.main.transform;
        g_MouseLastPosition = Input.mousePosition;
        g_Camera.forward = Vector3.Normalize(
            g_NPCController.GetTransform().position - g_Camera.position + new Vector3(0, CameraTargetHeight, 0));
        CameraOffset = Camera.main.transform.position - g_NPCController.GetTransform().position;
        StartCoroutine(MenuListener());
        if (InteractionsPanel != null) {
            foreach (Image go in InteractionsPanel.GetComponents<Image>()) {
                Color c = go.color;
                c.a -= 0.05f;
                go.color = c;
            }
            InteractionsPanel.SetActive(false);
            g_InteractionsMap = new Dictionary<GameObject, InteractionMapping>();
            foreach (InteractionMapping im in InteractionsMap) {
                g_InteractionsMap.Add(im.InteractionIcon,im);
            }
        }
        g_Initialized = true;
    }

    public bool IsEnabled() {
        return Enabled;
    }

    public bool IsUpdateable() {
        return false;
    }

    public string NPCModuleName() {
        return "NPC IO Controller";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.CONTROLS;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.IO_CONTROL;
    }

    public void RemoveNPCModule() {
        CleanupModule();
    }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    public void TickModule() { }

    #endregion

    #region Private_Function

    protected IEnumerator MenuListener() {
        while(true) {
            if(g_ObjectFocused) {
                if(!Input.GetKey((KeyCode) THIRD_PERSON_MOUSE_INPUT.INTERACT)) {
                    StartCoroutine(FadeContextMenu(false));
                    g_ObjectFocused = false;
                    g_NPCController.Body.StopLookAt();
                }
            }
            yield return null;
        }
    }

    private void UpdateCamera() {
        switch(IOMode) {
            case IO_TYPE.THIRD_PERSON:
                UpdateThirdPersonCamera();
                break;
            case IO_TYPE.ISOMETRIC:
                UpdateIsometricCamera();
                break;
        }
    }

    private void UpdateIsometricCamera() {
        // TODO - Fading logic here?
        if (EnableAgentCamera) {
            
        }
    }

    private void UpdateThirdPersonCamera() {
        if (EnableAgentCamera) {
            Vector3 camFwd = new Vector3(g_Camera.forward.x, 0, g_Camera.forward.z);
            float angle = Vector3.Angle(camFwd, g_NPCController.GetTransform().forward);
            Vector3 agentPosition = g_NPCController.GetTransform().position;
            float distance = Vector3.Distance(agentPosition, Camera.main.transform.position);
            g_Camera.position = g_NPCController.GetTransform().position + CameraOffset;
            g_Camera.forward = (g_NPCController.GetPosition() + new Vector3(0, CameraTargetHeight, 0) + (g_NPCController.GetTransform().forward * 0.5f)) - g_Camera.position;
        }
    }

    private void UpdateInput() {
        switch(IOMode) {
            case IO_TYPE.THIRD_PERSON:
                UpdateThirdPersonInput();
                break;
            case IO_TYPE.ISOMETRIC:
                UpdateIsometricInput();
                break;
        }
    }

    private void UpdateIsometricInput() {
        foreach(ISOMETRIC_KEYBOARD_INPUT ki in Enum.GetValues(typeof(ISOMETRIC_KEYBOARD_INPUT))) {
            if(Input.GetKey((KeyCode)ki)) {
                switch(ki) {
                    case ISOMETRIC_KEYBOARD_INPUT.MOVE_CAM_LEFT:
                        Vector3 left = g_Camera.transform.right;
                        left.y = 0;
                        g_Camera.transform.position -= (left * IsometricSpeedModifier);
                        break;
                    case ISOMETRIC_KEYBOARD_INPUT.MOVE_CAM_RIGHT:
                        Vector3 right = g_Camera.transform.right;
                        right.y = 0;
                        g_Camera.transform.position += (right * IsometricSpeedModifier);
                        break;
                    case ISOMETRIC_KEYBOARD_INPUT.MOVE_CAM_UP:
                        Vector3 forth = g_Camera.transform.forward;
                        forth.y = 0;
                        g_Camera.transform.position += (forth * IsometricSpeedModifier);
                        break;
                    case ISOMETRIC_KEYBOARD_INPUT.MOVE_CAM_DOWN:
                        Vector3 back = g_Camera.transform.forward;
                        back.y = 0;
                        g_Camera.transform.position -= (back * IsometricSpeedModifier);
                        break;
                    case ISOMETRIC_KEYBOARD_INPUT.ROT_CAM_LEFT:
                        break;
                    case ISOMETRIC_KEYBOARD_INPUT.ROT_CAM_RIGHT:
                        break;
                }
            }
        }

        foreach (ISOMETRIC_MOUSE_INPUT mi in Enum.GetValues(typeof(ISOMETRIC_MOUSE_INPUT))) {
            if(Input.GetKeyUp((KeyCode) mi )) {
                switch(mi) {
                    case ISOMETRIC_MOUSE_INPUT.WALK_TO:
                        g_NPCController.Debug("Go To clicked");
                        RaycastHit locationHit;
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out locationHit, 100.0f)) {
                            g_NPCController.Body.GoTo(locationHit.point);
                        }
                        break;
                }
            }
        }

        // Mouse Wheel
        float mouseWheelDelta = Input.GetAxis("Mouse ScrollWheel");
        if(mouseWheelDelta > 0 || mouseWheelDelta < 0) {
            int mod = mouseWheelDelta > 0 ? -1 : 1;
            float size = Mathf.Clamp(g_Camera.GetComponent<Camera>().orthographicSize + mod, MIN_ISO_CAM_HEIGHT, MAX_ISO_CAM_HEIGHT);
            g_Camera.GetComponent<Camera>().orthographicSize = size;
        }
    }

    private void UpdateThirdPersonInput() {
        foreach (THIRD_PERSON_MOUSE_INPUT mi in Enum.GetValues(typeof(THIRD_PERSON_MOUSE_INPUT))) {

            if (Input.GetKeyUp((KeyCode)mi)) {
                switch (mi) {
                    case THIRD_PERSON_MOUSE_INPUT.INTERACT:
                        RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector3.zero);
                        if (hit.collider != null) {
                            if (g_InteractionsMap.ContainsKey(hit.collider.gameObject)) {
                                string tag = g_InteractionsMap[hit.collider.gameObject].Tag;
                                switch (tag) {
                                    case Interaction_Grab:
                                        g_BehaviorAgent.StartEvent(g_BehaviorAgent.NPCBehavior_Grab(g_FocusedObject), true);
                                        break;
                                    case Interaction_Consume:
                                        break;
                                    case Interaction_Sit:
                                        g_BehaviorAgent.StartEvent(g_BehaviorAgent.NPCBehavior_TakeSit(g_FocusedObject), true);
                                        break;
                                    default:
                                        Debug.Log("No Interaction Tag: " + hit.transform.gameObject + " for object: " + g_FocusedObject);
                                        break;
                                }
                            }

                        }
                        break;
                }
            } else if (Input.GetKey((KeyCode)mi)) {
                switch (mi) {
                    case THIRD_PERSON_MOUSE_INPUT.WALK_TO:
                        RaycastHit locationHit;
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out locationHit, 100.0f)) {
                            g_NPCController.Body.GoTo(locationHit.point);
                        }
                        break;
                    case THIRD_PERSON_MOUSE_INPUT.FLOAT_CAMERA:
                        if (EnableMouseCameraRotation) {
                            float horChange = Input.mousePosition.x - g_MouseLastPosition.x;
                            float verChange = Input.mousePosition.y - g_MouseLastPosition.y;
                            bool correct = false;
                            if (Mathf.Abs(horChange) > CameraRotationThreshold) {
                                int mod = horChange > 0 ? 1 : -1;
                                g_Camera.RotateAround(g_NPCController.GetPosition(), mod * Vector3.up, CameraRotationSpeed * Time.deltaTime);
                                correct = true;
                            }
                            if (Mathf.Abs(verChange) > CameraRotationThreshold) {
                                int mod = verChange > 0 ? -1 : 1;
                                Vector3 rotationAxis =
                                    Vector3.Cross(
                                        g_Camera.forward,
                                        Vector3.up);
                                g_Camera.RotateAround(g_NPCController.GetPosition(), rotationAxis, mod * CameraRotationSpeed * Time.deltaTime);
                                correct = true;
                            }
                            if (correct) {
                                CameraOffset = Camera.main.transform.position - g_NPCController.GetTransform().position;
                            }
                        }
                        break;
                    case THIRD_PERSON_MOUSE_INPUT.INTERACT:
                        if (!g_ObjectFocused) {
                            RaycastHit hit;
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out hit, 100.0f)) {
                                NPCObject npcObject = hit.transform.GetComponent<NPCObject>();
                                if (npcObject != null) {
                                    g_FocusedObject = npcObject;
                                    g_ObjectFocused = true;
                                    g_NPCController.Body.StartLookAt(npcObject.GetTransform());
                                    StartCoroutine(FadeContextMenu(true, g_FocusedObject));
                                }
                            }
                        }
                        break;
                }
                g_MouseLastPosition = Input.mousePosition;
            }
        }

        foreach (THIRD_PERSON_KEYBOARD_INPUT key in Enum.GetValues(typeof(THIRD_PERSON_KEYBOARD_INPUT))) {
            if (Input.GetKey((KeyCode)key)) {
                switch (key) {
                    case THIRD_PERSON_KEYBOARD_INPUT.FORWARD:
                        g_NPCController.Body.Move(LOCO_STATE.FORWARD);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.BACKWARDS:
                        g_NPCController.Body.Move(LOCO_STATE.BACKWARDS);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.CAM_ROT_LEFT:
                        RotateCameraLeft();
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.CAM_ROT_RIGHT:
                        RotateCameraRight();
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.TURN_LEFT:
                        g_NPCController.Body.Move(LOCO_STATE.LEFT);
                        RotateCameraRight(0.5f);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.TURN_RIGHT:
                        g_NPCController.Body.Move(LOCO_STATE.RIGHT);
                        RotateCameraLeft(0.5f);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.RUN:
                        g_NPCController.Body.Move(LOCO_STATE.RUN);
                        break;
                }
            } else if (Input.GetKeyUp((KeyCode)key)) {
                switch (key) {
                    case THIRD_PERSON_KEYBOARD_INPUT.JUMP:
                        g_NPCController.Body.Move(LOCO_STATE.JUMP);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.TOGGLE_WALK:
                        g_NPCController.Body.Move(LOCO_STATE.TOGGLE_WALK);
                        break;
                    case THIRD_PERSON_KEYBOARD_INPUT.TOGGLE_ACTION:
                        g_NPCController.Body.ToggleAction();
                        break;
                }
            }
        }
    }

    private IEnumerator FadeContextMenu(bool fadeIn, NPCObject obj = null) {
        InteractionsPanel.SetActive(fadeIn);
        float alpha = fadeIn ? 0f : 1f;
        if(fadeIn) {
            SetContextMenuEnabled(true);
            while(alpha < 0.9f) {
                InteractionsPanel.transform.position = Camera.main.WorldToScreenPoint(g_FocusedObject.GetPosition());
                foreach (Image go in InteractionsPanel.GetComponents<Image>()) {
                    Color c = go.color;
                    c.a += 0.05f;
                    alpha = c.a;
                    go.color = c;
                }
                yield return null;
            }
            
        } else {
            while (alpha > 0f) {
                foreach (Image go in InteractionsPanel.GetComponents<Image>()) {
                    Color c = go.color;
                    c.a -= 0.05f;
                    alpha = c.a;
                    go.color = c;
                }
                yield return null;
            }
            SetContextMenuEnabled(false);
        }
    }

    private void SetContextMenuEnabled(bool b) {
        foreach (Image go in InteractionsPanel.GetComponents<Image>()) {
            go.gameObject.SetActive(b);
        }
    }

    public void OnMouseUp() {
        Debug.Log("Mouse up");
    }

    [Serializable]
    public class InteractionMapping {
        public GameObject InteractionIcon;
        public string Tag;
        public INTERACTION_TYPE Interaction;
    }
    #endregion

    #region Private

    private void RotateCameraLeft(float mod = 1f) {
        g_Camera.RotateAround(g_NPCController.GetPosition(), Vector3.up, mod * CameraRotationSpeed * Time.deltaTime);
        CameraOffset = Camera.main.transform.position - g_NPCController.GetTransform().position;
    }

    private void RotateCameraRight(float mod = 1f) {
        g_Camera.RotateAround(g_NPCController.GetPosition(), -1 * Vector3.up, mod * CameraRotationSpeed * Time.deltaTime);
        CameraOffset = Camera.main.transform.position - g_NPCController.GetTransform().position;
    }

    #endregion

}
