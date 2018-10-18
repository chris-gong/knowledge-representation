using UnityEngine;
using System.Collections.Generic;
using System;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public class NPCObject : MonoBehaviour, INPCPerceivable {

        #region Members

        [SerializeField]
        public String[] Tags;

        private string g_CurrentContext;
        private Vector3 g_Offsets;
        private Collider g_Collider;
        private Rigidbody g_RigidBody;
        private HashSet<string> g_NPCTags;

        #endregion

        #region Properties

        [SerializeField]
        public Transform MainInteractionPoint;

        [SerializeField]
        public PERCEIVE_WEIGHT PerceptionWeightType;

        [SerializeField]
        public string Name;

        [SerializeField]
        public INTERACTION_TYPE[] Interactions;

        #endregion

        #region Unity_Methods
        
        void Awake() {
            g_NPCTags = new HashSet<string>();
            foreach (string tag in Tags)
                g_NPCTags.Add(tag);
        }

        void Reset() {
            MainInteractionPoint = transform;
            PerceptionWeightType = PERCEIVE_WEIGHT.TOTAL;
        }

        private void Start() {
            g_Offsets = Vector3.zero;
            if(MainInteractionPoint != null && MainInteractionPoint != transform) {
                    g_Offsets = transform.position - MainInteractionPoint.position;
            }
            g_Collider = GetComponent<Collider>();
            g_RigidBody = GetComponent<Rigidbody>();
        }

        #endregion

        #region Public_Functions

        #endregion

        #region INPCPerceivable

        public bool HasTag(string tag) {
            return g_NPCTags.Contains(tag);
        }

        public void SetCurrentContext(string context) {
            g_CurrentContext = context;
        }

        public string GetCurrentContext() {
            return g_CurrentContext;
        }

        public float GetPerceptionWeight() {
            return 1f;
        }
        public Transform GetMainLookAtPoint() {
            return transform;
        }
        public PERCEIVEABLE_TYPE GetNPCEntityType() {
            return PERCEIVEABLE_TYPE.OBJECT;
        }

        public PERCEIVE_WEIGHT GetPerceptionWeightType() {
            return PerceptionWeightType;
        }

        public Transform GetTransform() {
            return transform;
        }

        public Vector3 GetCurrentVelocity() {
            return Vector3.zero;
        }

        public Vector3 GetPosition() {
            return transform.position;
        }

        public Vector3 GetForwardDirection() {
            return transform.forward;
        }
        
        public float GetAgentRadius() {
            return GetComponent<Collider>().bounds.size.x;
        }
        
        public GameObject GetGameObject() {
            return gameObject;
        }

        #endregion

        #region Utilities

        public override string ToString() {
            return Name;
        }

        public float GetCurrentSpeed() {
            throw new NotImplementedException();
        }

        public void SetHeld(bool held = true) {
            if (held) {
                if (g_Collider != null) g_Collider.enabled = false;
                if (g_RigidBody != null) Destroy(gameObject.GetComponent<Rigidbody>());
            } else {
                if (g_Collider != null) g_Collider.enabled = true;
                if (g_RigidBody == null) gameObject.AddComponent<Rigidbody>();
            }
        }

        #endregion
    }

}