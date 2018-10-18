using UnityEngine;
using System.Collections;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [RequireComponent(typeof(Animator))]
    [System.Serializable]
    public class NPCIKController : MonoBehaviour {

        #region Members

        /* Animator g_Animator */
        Animator g_Animator;
        NPCBody g_NPCBody;
        NPCController g_NPCController;

        /* Targets */
        private Vector3 RIGHT_HAND_POINT, LEFT_HAND_POINT;
        private Quaternion RIGHT_HAND_ROTATION, LEFT_HAND_ROTATION;
        private Transform LOOK_AT_TARGET;
        private Transform GRAB_RIGHT_TARGET;
        private Transform GRAB_LEFT_TARGET;

        Vector3 g_RightFootPosition,
            g_LeftFootPosition;

        Quaternion g_RightFootRotation,
            g_LeftFootRotation;

        /* Hints */
        [SerializeField]
        Transform HINT_LEFT_KNEE;
        [SerializeField]
        Transform HINT_RIGHT_KNEE;

        /* Weights*/
        public float IK_WEIGHT;
        public float IK_RIGHT_FOOT_WEIGHT = 0f;
        public float IK_LEFT_FOOT_WEIGHT = 0f;
        public float MAX_LOOK_WEIGHT = 1f;

        private float g_CurrentLookWeight = 0.0f;
        private float g_CurrentRightHandWeight = 0.0f;
        private float g_CurrentLeftHandWeight = 0.0f;
        private float g_CurrentRightFootWeight = 0.0f;
        private float g_CurrentLeftFootWeight = 0.0f;
        private bool g_FeetIK = false;

        RaycastHit g_RayHit;
        private static string m_AnimatorRightFootParam = "IK_Right_Foot";
        private static string m_AnimatorLeftFootParam = "IK_Left_Foot";
        private static string m_AnimatorRightHandParam = "IK_Right_Hand";
        private static string m_AnimatorLeftHandParam = "IK_Left_Hand";


        /* Enable disabled IK and COmponents during runtime */
        public bool IK_ACTIVE;
        public float REACH_DISTANCE = 0.5f;

        /* Bones */
        [SerializeField]
        Transform HEAD;
        [SerializeField]
        Transform RIGHT_HAND;
        [SerializeField]
        Transform LEFT_HAND;
        [SerializeField]
        Transform RIGHT_FOOT;
        [SerializeField]
        Transform LEFT_FOOT;
        [SerializeField]
        Transform LEFT_KNEE;
        [SerializeField]
        Transform RIGHT_KNEE;

        /* States */
        private bool g_RightHandHolding = false;
        #endregion

        #region Properties

        public Transform Head {
            get {
                return HEAD;
            }
        }

        public NPCObject GRAB_RIGHT {
            get {
                if (GRAB_RIGHT_TARGET != null) {
                    if (GRAB_RIGHT_TARGET.parent != null) {
                        return GRAB_RIGHT_TARGET.parent.GetComponent<NPCObject>();
                    } else
                        return GRAB_RIGHT_TARGET.GetComponent<NPCObject>();
                } else return null;
            }
            set {
                if (value == null) {
                    g_RightHandHolding = false;
                    if (GRAB_RIGHT_TARGET != null) {
                        if(GRAB_RIGHT_TARGET.parent != null)
                            GRAB_RIGHT_TARGET.parent.parent = null;
                        else
                            GRAB_RIGHT_TARGET.parent = null;
                        GRAB_RIGHT.SetHeld(false);
                        GRAB_RIGHT_TARGET = null;
                    }
                } else {
                    GRAB_RIGHT_TARGET = value.MainInteractionPoint;
                }
            }
        }

        public Transform LOOK_AT {
            get {
                return LOOK_AT_TARGET;
            }
            set {
                LOOK_AT_TARGET = value;
            }
        }

        #endregion   

        #region Unity_Functions

        public void Reset() {
            g_Animator = gameObject.GetComponent<Animator>();
            if (g_Animator == null) {
                Debug.Log("NPCIKController --> An animator controller is needed for IK");
                this.enabled = false;
            } else {
                g_Animator.applyRootMotion = true;
            }

            // Initialize Bones
            RIGHT_HAND = g_Animator.GetBoneTransform(HumanBodyBones.RightHand);
            LEFT_HAND = g_Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            RIGHT_FOOT = g_Animator.GetBoneTransform(HumanBodyBones.RightFoot);
            LEFT_FOOT = g_Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            HEAD = g_Animator.GetBoneTransform(HumanBodyBones.Head);
            LEFT_KNEE = g_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            RIGHT_KNEE = g_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

            // Initialize Hints
            HINT_LEFT_KNEE = new GameObject().transform;
            HINT_RIGHT_KNEE = new GameObject().transform;
            HINT_LEFT_KNEE.gameObject.name = "IK_HINT_Left_Knee";
            HINT_RIGHT_KNEE.gameObject.name = "IK_HINT_Right_Knee";
            HINT_LEFT_KNEE.parent = g_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            HINT_RIGHT_KNEE.parent = g_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            HINT_LEFT_KNEE.localRotation = Quaternion.identity;
            HINT_RIGHT_KNEE.localRotation = Quaternion.identity;
            HINT_LEFT_KNEE.localPosition = Vector3.zero;
            HINT_RIGHT_KNEE.localPosition = Vector3.zero;
        }

        // Use this for initialization
        void Start() {

            g_NPCController = GetComponent<NPCController>();
            g_NPCBody = g_NPCController.Body;
            g_Animator = GetComponent<Animator>();
            

            if (g_Animator == null) {
                g_NPCController.Debug("NPCIKController --> An animator controller is needed for IK, disabling component during runtime");
                this.enabled = false;
            }

            // default weight
            IK_WEIGHT   = IK_WEIGHT < 0.1f ? 1f : IK_WEIGHT;
            
            g_RightFootPosition = RIGHT_FOOT.position;
            g_LeftFootPosition = LEFT_FOOT.position;

        }

        // Unity's main IK method called every frame
        void OnAnimatorIK(int layerIndex) {

            if(layerIndex == 0 && g_NPCBody.IKEnabled) {
                
                /* Feet */
                if(g_FeetIK)
                    DoFeetIK();

                /* Look At */
                DoLookIK();


                /* Do Hands */
                DoHandsIK();
            }
        }

        #endregion

        #region Private_Functions

        private void DoHandsIK() {

            // Check for rigid body collisions on the right and left diagonals
            Transform rightShoulder = g_Animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                      leftShoulder = g_Animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Vector3 fromPointRight = rightShoulder.position,
                    fromPointLeft = leftShoulder.position,
                    toRight = rightShoulder.forward + transform.right,
                    toLeft = leftShoulder.forward - transform.right;

            g_NPCController.DebugRay(fromPointRight, toRight, Color.magenta);
            g_NPCController.DebugRay(fromPointLeft, toLeft, Color.magenta);

            // Ignore those targets with no rigidbody
            RaycastHit hitR, hitL;
            bool hitRight = Physics.Raycast(fromPointRight, toRight, out hitR, g_NPCBody.MaxArmsReach);
            bool hitLeft = Physics.Raycast(fromPointLeft, toLeft, out hitL, g_NPCBody.MaxArmsReach);
            hitRight = hitRight && hitR.transform.GetComponent<Rigidbody>() != null && hitR.transform.GetComponent<NPCController>() == null;
            hitLeft = hitLeft && hitL.transform.GetComponent<Rigidbody>() != null && hitL.transform.GetComponent<NPCController>() == null;

            // Enable animator curves to use animator-controlled values
            if (g_NPCBody.UseAnimatorController) {
                if(g_Animator.GetFloat(m_AnimatorRightHandParam) != 0)
                    g_CurrentRightHandWeight = Mathf.Lerp(g_CurrentRightHandWeight, g_Animator.GetFloat(m_AnimatorRightHandParam), Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH);
                if(g_Animator.GetFloat(m_AnimatorLeftHandParam) != 0)
                    g_CurrentLeftHandWeight = Mathf.Lerp(g_CurrentLeftHandWeight, g_Animator.GetFloat(m_AnimatorLeftHandParam), Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH);
            }

            if (GRAB_RIGHT_TARGET != null && g_CurrentRightHandWeight > 0f) {
                if (g_CurrentRightHandWeight > 0.9f && Vector3.Distance(RIGHT_HAND.position, GRAB_RIGHT_TARGET.position) <= 0.05f) {
                    if (GRAB_RIGHT_TARGET.parent != null) {
                        GRAB_RIGHT_TARGET.parent.parent = RIGHT_HAND;
                    } else GRAB_RIGHT_TARGET.parent = RIGHT_HAND;
                    GRAB_RIGHT.SetHeld(true);
                    g_RightHandHolding = true;
                } else if(!g_RightHandHolding) {
                    if (GRAB_RIGHT_TARGET.parent != null) { // only correct MainInteractionPoint if it is not the transform of the main object
                        float angle = Vector3.Angle(transform.forward, GRAB_RIGHT_TARGET.forward);
                        if (angle > 1f) {
                            GRAB_RIGHT_TARGET.RotateAround(GRAB_RIGHT_TARGET.parent.position, GRAB_RIGHT_TARGET.parent.up, angle);
                        }
                    }
                }
                g_Animator.SetIKPosition(AvatarIKGoal.RightHand, GRAB_RIGHT_TARGET.position);
                g_Animator.SetIKRotation(AvatarIKGoal.RightHand, GRAB_RIGHT_TARGET.rotation);
            } else {
                Vector3 handRotation = transform.right;
                Quaternion handsRotation = Quaternion.LookRotation(transform.up, -transform.forward);
                if (hitRight) {
                    RIGHT_HAND_POINT = hitR.point;
                    RIGHT_HAND_ROTATION = handsRotation;
                    g_CurrentRightHandWeight = Mathf.Lerp(g_CurrentRightHandWeight, 1f, Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH);
                } else {
                    g_CurrentRightHandWeight = Mathf.Lerp(g_CurrentRightHandWeight, 0f, Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH * 2f);
                }
                if(hitLeft) {
                    LEFT_HAND_POINT = hitL.point;
                    LEFT_HAND_ROTATION = handsRotation;
                    g_CurrentLeftHandWeight = Mathf.Lerp(g_CurrentLeftHandWeight, 1f, Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH);
                } else {
                    g_CurrentLeftHandWeight = Mathf.Lerp(g_CurrentLeftHandWeight, 0f, Time.deltaTime * g_NPCBody.IK_HANDS_SMOOTH * 2f);
                }
                g_Animator.SetIKPosition(AvatarIKGoal.RightHand, RIGHT_HAND_POINT);
                g_Animator.SetIKPosition(AvatarIKGoal.LeftHand, LEFT_HAND_POINT);
                g_Animator.SetIKRotation(AvatarIKGoal.RightHand, RIGHT_HAND_ROTATION);
                g_Animator.SetIKRotation(AvatarIKGoal.LeftHand, LEFT_HAND_ROTATION);
            }
            g_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, g_CurrentRightHandWeight);
            g_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, g_CurrentRightHandWeight);

            g_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, g_CurrentLeftHandWeight);
            g_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, g_CurrentLeftHandWeight);

        }

        private void DoLookIK() {
            if (LOOK_AT != null && g_NPCController.Perception.IsEntityPerceived(LOOK_AT)) {
                g_Animator.SetLookAtPosition(LOOK_AT.position);
                g_CurrentLookWeight = Mathf.Lerp(g_CurrentLookWeight, 1.0f, Time.deltaTime * g_NPCBody.IK_LOOK_AT_SMOOTH * g_NPCBody.IK_START_LOOK_AT_ADJUST);
            } else {
                g_CurrentLookWeight = Mathf.Lerp(g_CurrentLookWeight, 0.0f, Time.deltaTime * g_NPCBody.IK_LOOK_AT_SMOOTH);
            }
            g_Animator.SetLookAtWeight(g_CurrentLookWeight);
        }

        private void DoFeetIK() {
            
            // Using animation curves - walk and idle
            if(g_NPCBody.Speed == 0 && g_NPCBody.Orientation == 0) {
                IK_RIGHT_FOOT_WEIGHT = IK_LEFT_FOOT_WEIGHT = 0.5f;
            } else {
                IK_RIGHT_FOOT_WEIGHT = g_Animator.GetFloat(m_AnimatorRightFootParam);
                IK_LEFT_FOOT_WEIGHT = g_Animator.GetFloat(m_AnimatorLeftFootParam);
            }

            // Adjust Hints
            if (g_NPCBody.IK_USE_HINTS) {
                g_Animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0.5f);
                g_Animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0.5f);
                g_Animator.SetIKHintPosition(AvatarIKHint.RightKnee, HINT_RIGHT_KNEE.position);
                g_Animator.SetIKHintPosition(AvatarIKHint.LeftKnee, HINT_LEFT_KNEE.position);
            }

            // IK Feet Position Weight
            g_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, IK_RIGHT_FOOT_WEIGHT);
            g_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IK_LEFT_FOOT_WEIGHT);

            // IK Feet Rotation Weight
            g_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, IK_RIGHT_FOOT_WEIGHT);
            g_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, IK_LEFT_FOOT_WEIGHT);

            // Feet Position
            g_Animator.SetIKPosition(AvatarIKGoal.RightFoot, g_RightFootPosition);
            g_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, g_LeftFootPosition);

            // Feet Rotation
            g_Animator.SetIKRotation(AvatarIKGoal.RightFoot, g_RightFootRotation);
            g_Animator.SetIKRotation(AvatarIKGoal.LeftFoot, g_LeftFootRotation);
        }

        #endregion

        #region Public_Functions

        /// <summary>
        /// Body calls UpdateIK when UpdateBody is called from the controller
        /// </summary>
        public void UpdateIK() {

            Vector3 heightCorrection = (Vector3.up * g_NPCBody.IK_FEET_HEIGHT_EFFECTOR_CORRECTOR);

            // Update feet
            if (g_NPCBody.IK_FEET_Enabled) {
                g_FeetIK = Physics.Raycast(RIGHT_FOOT.position + heightCorrection, Vector3.down, out g_RayHit);
                g_RightFootPosition = g_RayHit.point + (Vector3.up * g_NPCBody.IK_FEET_HEIGHT_CORRECTION) + (transform.forward * g_NPCBody.IK_FEET_FORWARD_CORRECTION);
                g_RightFootRotation = Quaternion.FromToRotation(Vector3.up, g_RayHit.normal) * transform.rotation;

                g_FeetIK = Physics.Raycast(LEFT_FOOT.position + heightCorrection, Vector3.down, out g_RayHit);
                g_LeftFootPosition = g_RayHit.point + (Vector3.up * g_NPCBody.IK_FEET_HEIGHT_CORRECTION) + (transform.forward * g_NPCBody.IK_FEET_FORWARD_CORRECTION);
                g_LeftFootRotation = Quaternion.FromToRotation(Vector3.up, g_RayHit.normal) * transform.rotation;

                if(g_NPCController.DebugMode) {
                    Debug.DrawRay(LEFT_FOOT.position + heightCorrection, Vector3.down, Color.red);
                    Debug.DrawRay(RIGHT_FOOT.position + heightCorrection, Vector3.down, Color.red);
                }

            } else g_FeetIK = false;


        }


        public bool CanBeReached(INPCPerceivable per) {
            return (Vector3.Distance(per.GetTransform().position, RIGHT_HAND.position) <= REACH_DISTANCE);
        } 

        public bool ReachFor(INPCPerceivable per) {
            return false;
        }
        
        public void Climb(Transform target) {
            float matchStart = g_Animator.GetFloat("Match_Start");
            float matchEnd = g_Animator.GetFloat("Match_End");
            g_Animator.MatchTarget(
                target.position,
                target.rotation,
                AvatarTarget.RightHand,
                new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 0f),
                matchStart,
                matchEnd);
        }
        
        #endregion


    }

}
