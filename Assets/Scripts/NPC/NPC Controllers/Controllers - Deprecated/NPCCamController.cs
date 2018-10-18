using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {


    public class NPCCamController : MonoBehaviour {

        #region Enums
        public enum CAMERA_CONTROLS {
            FORWARD = KeyCode.W,
            BACKWARD = KeyCode.S,
            LEFT = KeyCode.A,
            RIGHT = KeyCode.D,
            ROTATE_LOCAL_LEFT = KeyCode.Q,
            ROTATE_LOCAL_RIGHT = KeyCode.E,
            SPEED_UP = KeyCode.LeftShift,
            SLOW_DOWN = KeyCode.LeftControl,
            LOOK_AROUND_DRAG = KeyCode.Mouse1,
            ACTION_ONE = KeyCode.Mouse0,
            ACTION_TWO = KeyCode.Mouse1
        }
        #endregion

        #region Properties
        public bool Targetting {
            get {
                return CurrentMode == CAMERA_MODE.THIRD_PERSON ||
                    CurrentMode == CAMERA_MODE.FIRST_PERSON || 
                    CurrentMode == CAMERA_MODE.ISOMETRIC_FOLLOW;
            }
        }

        public Vector3 MinLimits;

        public bool CloseUp {
            get {
                return gCloseUp;
            }
            set {
                gCloseUp = value;
                gPanning = true;
            }
        }
        
        public CAMERA_MODE CurrentMode;

        public enum CAMERA_MODE {
            FREE,
            THIRD_PERSON,
            FIRST_PERSON,
            ISOMETRIC,
            ISOMETRIC_FOLLOW
        }

        public float Speed = 1.0f;
        public float RotationSpeed = 20f;
        public float IsometricHeight = 4.0f;
        public float ZoomSpeed = 2f;
        public float ModMultiplier = 1.5f;
        public float IsometricAngle = 35f;
        public Vector3 ThirdPersonDistances = new Vector3(0.2f, 0.8f, -0.6f);
        public Vector3 CloseUpDistances = new Vector3(0.2f, 0.5f, -0.2f);
        #endregion
        
        #region Members
        NPCControlManager g_NPCControlManager;
        float PanSmoothness = 0.1f;
        public float Isometric_Y_Angle = 90f;
        NPCController Target = null;
        bool gPanning = false;
        bool gCloseUp = false;
        Vector3 g_LastMousePosition;
        #endregion

        #region Unity_Functions
        void Update() {
            g_LastMousePosition = Input.mousePosition;
        }

        void Start() {
            g_NPCControlManager = FindObjectOfType<NPCControlManager>();
            if (Target != null) {
                SetThirdPersonView();
                CurrentMode = CAMERA_MODE.THIRD_PERSON;
            } else {
                switch(CurrentMode) {
                    case CAMERA_MODE.ISOMETRIC:
                        SetIsometricView();
                        break;
                    case CAMERA_MODE.FIRST_PERSON:
                        SetFirstPersonView();
                        break;
                    case CAMERA_MODE.THIRD_PERSON:
                        SetThirdPersonView();
                        break;
                }
            }
            if(g_NPCControlManager == null) {
                Debug.Log("NPCCamController --> No NPCControlManager with the NPCCamController enabled");
            }
        }
        #endregion

        #region Public_Functions
        public void SetTarget(NPCController t) {
            Target = t;
        }

        public void UpdateCamera() {
            
            bool failed = false;
            switch (CurrentMode) {
                case CAMERA_MODE.FREE:
                    HandleFreeCamera();
                    break;
                case CAMERA_MODE.FIRST_PERSON:
                    if (Target == null) failed = true;
                    else {
                        if (!Target.Body.IsIdle)
                            SetFirstPersonView();
                    }
                    break;
                case CAMERA_MODE.THIRD_PERSON:
                    if (Target == null) failed = true;
                    else {
                        if (!Target.Body.IsIdle || CloseUp)
                            SetThirdPersonView();
                    }
                    break;
                case CAMERA_MODE.ISOMETRIC:
                    HandleIsometricCamera();
                    break;
            }
            if(failed) {
                Debug.Log("NPCCamController --> Can't set this mode without an NPC target");
                CurrentMode = CAMERA_MODE.FREE;
            }
        }
     
        public void UpdateCameraMode(CAMERA_MODE mode) {
            bool noTarget = false;
            CurrentMode = mode;
            switch (CurrentMode) {
                case CAMERA_MODE.FREE:
                    if (Target != null) {
                        Target.Body.Navigation = NAV_STATE.DISABLED;
                        SetThirdPersonView();
                    }
                    //g_NPCControlManager.SetIOTarget(null);
                    break;
                case CAMERA_MODE.FIRST_PERSON:
                    if (Target != null) {
                        Target.Body.Navigation = NAV_STATE.DISABLED;
                        SetFirstPersonView();
                        //g_NPCControlManager.SetIOTarget(Target);
                    } else noTarget = true;
                    break;
                case CAMERA_MODE.THIRD_PERSON:
                    if (Target != null) {
                        Target.Body.Navigation = NAV_STATE.DISABLED;
                        SetThirdPersonView();
                    }
                    else noTarget = true;
                    break;
                case CAMERA_MODE.ISOMETRIC:
                    if (Target != null) Target.Body.Navigation = NAV_STATE.STEERING_NAV;
                    SetIsometricView();
                    break;
            }
            if(noTarget) {
                //g_NPCControlManager.SetIOTarget(null);
                CurrentMode = CAMERA_MODE.FREE;
                Debug.Log("NPCCamControlelr --> No target agent set, camera stays in FREE mode.");
            }
        }

        public void ResetView() {
            UpdateCameraMode(CurrentMode);
        }
        #endregion

        #region Private_Functions
        private void HandleFreeCamera() {

            float speedModifier = Input.GetKey(KeyCode.LeftShift) ? ModMultiplier : 1f;

            if (Input.GetKey((KeyCode) CAMERA_CONTROLS.LOOK_AROUND_DRAG)) {
                Vector3 diff = Input.mousePosition - g_LastMousePosition;
                if (diff != Vector3.zero) {
                    transform.Rotate(transform.right, -1 * diff.y * Time.deltaTime * RotationSpeed, Space.World);
                    transform.Rotate(Vector3.up, diff.x * Time.deltaTime * RotationSpeed, Space.World);
                }
            }

            if (Input.GetKey((KeyCode)CAMERA_CONTROLS.FORWARD)) {
                transform.position += transform.forward * (Time.deltaTime * speedModifier) * Speed;
            } else if (Input.GetKey((KeyCode)CAMERA_CONTROLS.BACKWARD)) {
                transform.position -= transform.forward * (Time.deltaTime * speedModifier) * Speed;
            }

            if (Input.GetKey((KeyCode)CAMERA_CONTROLS.RIGHT)) {
                transform.position += transform.right * (Time.deltaTime * speedModifier) * Speed;
            } else if (Input.GetKey((KeyCode)CAMERA_CONTROLS.LEFT)) {
                transform.position -= transform.right * (Time.deltaTime * speedModifier) * Speed;
            }


        }

        private void HandleIsometricCamera() {
            float speedModifier = Input.GetKey(KeyCode.LeftShift) ? Speed * ModMultiplier : Speed * 1f;
            if (Input.GetKey(KeyCode.W)) {
                transform.position += Vector3.forward * (Time.deltaTime * speedModifier);
            } else if (Input.GetKey(KeyCode.S)) {
                if (transform.position.x > MinLimits.x)
                    transform.position -= Vector3.forward * (Time.deltaTime * speedModifier);
            }
            if (Input.GetKey(KeyCode.A)) {
                transform.position += Vector3.left * (Time.deltaTime * speedModifier);
            } else if (Input.GetKey(KeyCode.D)) {
                if(transform.position.z > MinLimits.z)
                    transform.position -= Vector3.left * (Time.deltaTime * speedModifier);
            }

            if(Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
                if (transform.position.y > MinLimits.y)
                    transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.up, Time.deltaTime * ZoomSpeed * speedModifier);
            } else if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
                transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up, Time.deltaTime * ZoomSpeed * speedModifier);
            }
        }

        private void SetThirdPersonView() {
            Vector3 pos;
            if (CloseUp) {
                pos = Target.Body.Head.position;
                pos += Target.transform.forward * CloseUpDistances.z;
                pos += Target.transform.right * CloseUpDistances.x;
                pos += Target.transform.up * CloseUpDistances.y;
                if (gPanning) {
                    float delta = Time.deltaTime * PanSmoothness;
                    transform.position = Vector3.Lerp(transform.position, pos, delta);
                    gPanning = delta > 1f;
                } else {
                    gPanning = false;
                    transform.position = pos;
                }
                transform.LookAt(Target.Body.TargetObject);
            } else {
                transform.rotation = Target.transform.rotation;
                pos = Target.transform.position;
                pos += transform.up * ThirdPersonDistances.y;
                pos += transform.forward * ThirdPersonDistances.z;
                pos += transform.right * ThirdPersonDistances.x;
                transform.position = pos;
                transform.RotateAround(transform.position, transform.right, 15f);
            }
        }

        private void SetFirstPersonView() {
            transform.position = Target.transform.position;
            transform.rotation = Target.transform.rotation;
            transform.position += Target.transform.forward * ThirdPersonDistances.z;
            transform.position += Target.transform.up * ThirdPersonDistances.y;
        }

        private void SetIsometricView() {
            Vector3 curPos = transform.position;
            transform.rotation = Quaternion.identity;
            transform.Rotate(Vector3.up, Isometric_Y_Angle);
            transform.Rotate(Vector3.right, IsometricAngle);
            transform.position = new Vector3(curPos.x, IsometricHeight, curPos.z);
            transform.position -= (Vector3.right * 0.5f);
        }
        #endregion
    }

}
