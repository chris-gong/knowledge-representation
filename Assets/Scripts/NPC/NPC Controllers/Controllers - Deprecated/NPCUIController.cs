using UnityEngine;
using UnityEngine.UI;
using System.Collections;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public class NPCUIController : MonoBehaviour {

        public Text gTxtLabel;
        public NPCCamController NPCCamera;
        public bool Debug = true;

        // Update is called once per frame
        public void UpdateUI() {
            try {
                if (Debug) {
                    if (NPCCamera != null) {
                        CameraInfo();
                    }
                }
            } catch (System.Exception e) {
                Text t = GetComponentInChildren<Text>();
                if(t != null) {
                    gTxtLabel = t;
                } else {
                    UnityEngine.Debug.Log("NPCUIController --> Failed at updating text field: " + e.Message + " - disabling component");
                    this.enabled = false;
                }
            }
        }

        public void SetText(Text t) {
            gTxtLabel = t;
        }

        /// <summary>
        /// Displays Camera Data
        /// </summary>
        void CameraInfo() {
            gTxtLabel.text = "Camera Mode: ";
            switch (NPCCamera.CurrentMode) {
                case NPCCamController.CAMERA_MODE.FIRST_PERSON:
                    gTxtLabel.text += "First Person";
                    break;
                case NPCCamController.CAMERA_MODE.THIRD_PERSON:
                    gTxtLabel.text += "Third Person";
                    break;
                case NPCCamController.CAMERA_MODE.FREE:
                    gTxtLabel.text += "Free Flight";
                    break;
            }
        }
    }
}
