using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public enum CAMERA_TYPE_KEYS {
        FREE_FLY = KeyCode.F1,
        THIRD_PERSON = KeyCode.F2,
        FIRST_PERSON = KeyCode.F3,
        ISOMETRIC = KeyCode.F4

    }

    public enum CAMERA_TYPE {
        NONE,
        FIRST_PERSON,
        THIRD_PERSON,
        ISOMETRIC,
        TOP_DOWN
    }

    /// <summary>
    /// Base class for any type of camera that the NPCControlManager can recognize.
    /// This class must be extended and overrode if necessary.
    /// </summary>

    public abstract class NPCCameraController : MonoBehaviour {

        [SerializeField]
        public bool EnableIO = true;

        [SerializeField]
        [Range(0f, 2f)]
        public float CameraSpeed = 1f;

        [SerializeField]
        [Range(0f, 50f)]
        public float ZoomSpeed = 5f;

        [SerializeField]
        [Range(1, 20)]
        public int ZoomOut = 5;

        [SerializeField]
        public string MainCameraTag = "MainCamera";

        protected static string MOUSE_WHEEL_AXIS = "Mouse ScrollWheel";

        protected Camera g_Camera;
        protected NPCControlManager g_ControlManager;
        private Transform g_Target;
        private bool g_Primary = true;

        public abstract void Initialize();
        public abstract void SetHeight(float height);

        protected abstract void HandleKeyboard();
        protected abstract void HandlePointer();
        protected abstract void HandleAxix();
        protected abstract void UpdatePosition();

        public abstract CAMERA_TYPE GetCameraType();
        public abstract CAMERA_TYPE_KEYS GetCameraActivationKey();

        public void UpdateCamera() {
            if (EnableIO) {
                HandleKeyboard();
                HandlePointer();
                HandleAxix();
                UpdatePosition();
            }
        }

        public void ClearTarget() {
            g_Target = null;
        }

        public bool IsEnabled() {
            return g_Camera.gameObject.activeSelf;
        }

        public bool IsMain() {
            return g_Primary;
        }

        public bool IsTargetting() {
            return g_Target != null;
        }

        public void SetCamera(Camera camera) {
            g_Camera = camera;
        }

        public void SetEnabled(bool enabled) {
            g_Camera.gameObject.SetActive(enabled);
        }

        public void SetPrimary(bool primary) {
            g_Primary = primary;
        }

        public void SetTarget(Transform target) {
            g_Target = target;
        }

    }
}
