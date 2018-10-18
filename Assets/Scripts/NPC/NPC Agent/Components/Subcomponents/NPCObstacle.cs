using UnityEngine;
using System.Collections;
using System;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public class NPCObstacle : MonoBehaviour, INPCPerceivable {

        public Vector3 Dimensions;
        public Vector3 Location;
        public OBSTACLE_TYPE ObstacleType = OBSTACLE_TYPE.BLOCKER;
        public float Weight;

        void Reset() {
            Weight = ObstacleType == OBSTACLE_TYPE.BLOCKER ?
                (float)OBSTACLE_TYPE.BLOCKER : (float)OBSTACLE_TYPE.HARDENER;
            Location = transform.position;
            Dimensions = transform.localScale;
        }

        public void SetCurrentContext(string s) { }

        public string GetCurrentContext() {
            return null;
        }

        public Vector3 CalculateAgentRepulsionForce(INPCPerceivable p) {
            throw new NotImplementedException();
        }

        public float GetCurrentSpeed() {
            return 0f;
        }

        public bool HasTag(string s) {
            return false;
        }

        public Vector3 CalculateAgentSlidingForce(INPCPerceivable p) {
            throw new NotImplementedException();
        }

        public Vector3 CalculateRepulsionForce(INPCPerceivable p) {
            throw new NotImplementedException();
        }

        public Vector3 CalculateSlidingForce(INPCPerceivable p) {
            throw new NotImplementedException();
        }

        public float GetAgentRadius() {
            return 0f;
        }

        public Vector3 GetCurrentVelocity() {
            return new Vector3(0f,0f,0f);
        }

        public Vector3 GetForwardDirection() {
            return transform.forward;
        }

        public Transform GetMainLookAtPoint() {
            return transform;
        }

        public PERCEIVEABLE_TYPE GetNPCEntityType() {
            return PERCEIVEABLE_TYPE.OBJECT;
        }

        public PERCEIVE_WEIGHT GetPerceptionWeightType() {
            Rigidbody rb = GetComponent<Rigidbody>();
            return PERCEIVE_WEIGHT.TOTAL;
        }

        public Vector3 GetPosition() {
            return transform.position;
        }

        public Transform GetTransform() {
            return transform;
        }

        public float GetPerceptionWeight() {
            return 1f;
        }

        public Transform GetMainInteractionPoint() {
            return transform;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

    }

}