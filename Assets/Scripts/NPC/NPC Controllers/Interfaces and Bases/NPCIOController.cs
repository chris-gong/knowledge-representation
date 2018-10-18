using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public enum IO_CONTROLLER_TYPE {
        ISOMETRIC,
        THIRD_PESON,
        FIRST_PERSON,
        FREE
    }

    public abstract class NPCIOController : MonoBehaviour {

        protected bool g_Enabled = true;
        protected NPCController g_Target;
        protected NPCControlManager g_ControlManager;

        public abstract void HandleKeyboard();
        public abstract void HandlePointer();
        public abstract void HandleAxis();

        public abstract IO_CONTROLLER_TYPE GetIOControllerType();

        public abstract void Initialize();

        public void UpdateIO() {
            if (g_Enabled) {
                HandleKeyboard();
                HandlePointer();
                HandleAxis();
            }
        }

        public void SetEnabled(bool enabled) {
            g_Enabled = enabled;
        }

        public void ClearTarget() {
            if (g_Target != null) {
                g_Target.SetSelected(false);
            }
        }

        public void SetTarget(GameObject o) {
            g_Target = o.GetComponent<NPCController>();
            if (g_Target != null)
                g_Target.SetSelected(true);
        }


    }
}
