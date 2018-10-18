using System;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// Attach this object to the one which already contains the NPCControlManager
    /// for full Editor interface. Otherwise, use the getters/setters defined in the
    /// base abstract class NPCCameraController
    /// </summary>

    public class NPCIsometric_Camera : NPCCameraController {

        #region Members

        [SerializeField]
        public bool FocusTarget = false;

        [SerializeField]
        public bool DragFollow = false;

        [SerializeField]
        [Range(0f,100f)] public float FollowSpeed = 1f;

        [SerializeField]
        [Range(1f, 20f)]
        public float Height;

        [SerializeField]
        public bool ZoomEnabled = true;

        [SerializeField]
        [Range(10f, 45f)]
        public float IsometricXAngle = 30f;

        [SerializeField]
        [Range(10f, 90f)]
        public float IsometricYAngle = 45f;

        [SerializeField]
        [Range(1, 50)]
        public int DiagonalDistance = 4;

        [SerializeField]
        [Range(1, 500)]
        public int FarView = 10;
        
        private Vector3 g_TargetOffset = new Vector3(1f,0,1f);
        private Transform g_Follower;

        #endregion

        #region INPCCameraController

        public override void Initialize() {
            g_ControlManager = GetComponent<NPCControlManager>();
            g_Camera = GameObject.FindGameObjectWithTag(MainCameraTag).GetComponent<Camera>();
            if (g_ControlManager.NPCControllerTarget == null || !FocusTarget) {
                g_Camera.transform.position = new Vector3(-DiagonalDistance, Height, -DiagonalDistance);
                FocusTarget = false;
            } else {
                g_Follower = new GameObject().transform;
                g_Follower.parent = g_ControlManager.NPCControllerTarget.transform;
                g_Follower.gameObject.name = "Isometric_Follower";
                g_Follower.localPosition = Vector3.zero;
                g_Camera.transform.position = 
                    g_ControlManager.NPCControllerTarget.transform.position + g_TargetOffset;
            }
            g_Camera.transform.eulerAngles = new Vector3(IsometricXAngle, IsometricYAngle, 0f);
            g_Camera.orthographic = true;
            g_Camera.orthographicSize = ZoomOut;
            g_Camera.nearClipPlane = (-1f * (Height * 3f));
            g_Camera.farClipPlane = FarView;
        }

        protected override void UpdatePosition() {
            if(FocusTarget) {
                if (DragFollow) {
                    g_Camera.transform.position =
                        Vector3.Lerp(g_Camera.transform.position,
                        g_Follower.position + g_TargetOffset,
                        Time.deltaTime * FollowSpeed);
                } else {
                    g_Camera.transform.position = g_Follower.position;
                }
            }
        }

        public override CAMERA_TYPE GetCameraType() {
            return CAMERA_TYPE.ISOMETRIC;
        }

        protected override void HandleKeyboard() {
            if (!FocusTarget) {
                if (Input.GetKey(KeyCode.W)) {
                    g_Camera.transform.localPosition += new Vector3(1f * CameraSpeed, 0, 1f * CameraSpeed);
                }
                if (Input.GetKey(KeyCode.S)) {
                    g_Camera.transform.localPosition += new Vector3(-1f * CameraSpeed, 0, -1f * CameraSpeed);
                }
                if (Input.GetKey(KeyCode.A)) {
                    g_Camera.transform.localPosition += new Vector3(-1f * CameraSpeed, 0, 1f * CameraSpeed);
                }
                if (Input.GetKey(KeyCode.D)) {
                    g_Camera.transform.localPosition += new Vector3(1f * CameraSpeed, 0, -1f * CameraSpeed);
                }
            }
        }

        protected override void HandlePointer() { }

        protected override void HandleAxix() {
            if (ZoomEnabled) {
                float axis = -1 * Input.GetAxis(NPCCameraController.MOUSE_WHEEL_AXIS);
                if (axis != 0) {
                    g_Camera.orthographicSize = Mathf.Clamp(
                        Mathf.Lerp(g_Camera.orthographicSize,
                            g_Camera.orthographicSize + (axis * ZoomSpeed), CameraSpeed),
                        1f, 20f);
                }
            }
        }

        public override void SetHeight(float height) {
            Height = height;
        }

        public override CAMERA_TYPE_KEYS GetCameraActivationKey() {
            return CAMERA_TYPE_KEYS.ISOMETRIC;
        }

        #endregion

    }

}
