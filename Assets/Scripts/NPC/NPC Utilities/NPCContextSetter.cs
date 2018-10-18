using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public class NPCContextSetter : MonoBehaviour {

        public void OnTriggerStay(Collider collider) {
            INPCPerceivable c = collider.gameObject.GetComponent<INPCPerceivable>();
            if (c != null)
                c.SetCurrentContext(gameObject.name);
        }

        public void OnTriggerExit(Collider collider) {
            INPCPerceivable c = collider.gameObject.GetComponent<INPCPerceivable>();
            if (c != null)
                c.SetCurrentContext(null);
        }

    }

}
