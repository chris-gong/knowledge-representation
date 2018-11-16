using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine.AI;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [System.Serializable]
    public class NPCBody : MonoBehaviour {

        #region Members

        [SerializeField]
        private NavMeshAgent gNavMeshAgent;

        [SerializeField]
        private Animator g_Animator;

        [SerializeField]
        private NPCIKController gIKController;

        private Rigidbody gRigidBody;
        private CapsuleCollider gCapsuleCollider;
        private bool g_LookingAround = false;
        private Vector3 g_TargetLocation;
        private Vector3 g_LastpdatedPosition;

        // Timed gestures / IK controller
        private NPCTimer g_GestureTimer;

        private static string g_AnimParamSpeed = "Speed";
        private static string g_AnimParamDirection = "Direction";
        private static string g_AnimParamStrafe = "Strafe";
        private static string g_AnimParamJump = "Body_Jump";
        private static string g_AnimParamJumping = "Flag_Jumping";
        private static string g_AnimParamFalling = "Flag_Falling";
        private static string g_AnimParamActionFlag = "Flag_Action";
        private static string g_AnimParamColliderFactor = "Collider_Factor";
        private static string g_AnimParamLocomotion = "Body_Locomotion";

        private static int   SPEED_MOD = 2;
        private static float MAX_WALK_SPEED = 1.00f;
        private static float MAX_BACKWARDS_SPEED = -1f;
        private static float MAX_RUN_SPEED = 1.00f * SPEED_MOD;
		private static float MAX_SPRINT_SPEED = 1.5f * SPEED_MOD;
        private static float MIN_WALK_SPEED = -1 * MAX_WALK_SPEED;
        private static float MIN_RUN_SPEED = -1 * MAX_WALK_SPEED;

        private static float MAX_TURNING_ANGLE = 180f;

        private static Dictionary<GESTURE_CODE, NPCAnimation> m_Gestures;
        private GESTURE_CODE LastGesture;
        private static Dictionary<NPCAffordance, string> m_Affordances;
        
        private LOCO_STATE g_CurrentStateFwd = LOCO_STATE.IDLE;
        private LOCO_STATE g_CurrentStateGnd = LOCO_STATE.GROUND;
        private LOCO_STATE g_CurrentStateDir = LOCO_STATE.FRONT;
        private LOCO_STATE g_CurrentStateMod = LOCO_STATE.WALK;

        // This correlate with the parameters from the Animator
        private bool g_AnimatorLocked = false;
        private bool  g_Blocked = false;
        private bool g_FrontBlocked = false, g_RearBlocked = false;
        private Collider g_FrontBlocker = null;
        private bool g_SkipUpdate = false;
        private bool g_ToggleWalk = false;
        private float g_CurrentSpeedPenalty = 0f;
        private float g_CurrentSpeed = 0.0f;
        private float g_CurrentStrafe = 0.0f;
        private float g_CurrentVelocity = 0.05f;
        private float g_CurrentOrientation = 0.0f;
        private float g_TurningVelocity;
        private float g_OrientationAngle = 0f;
        private float g_TopFowardSpeed = 1f;

        // for feet correction
        private float g_ColliderHeight;
        private bool g_TargetLocationReached = false;
        private static int gHashJump = Animator.StringToHash("JumpLoco");
        private static int gHashIdle = Animator.StringToHash("Idle");
        private Vector3 g_TargetOrientation;                                // Wheres the NPC currently looking at
        private float g_DynamicFriction;
        private float g_StaticFriction;
        
        // Animations
        private Dictionary<string,AnimationClip> g_AnimationStates;
        private Dictionary<string, NPCAnimatedAudio> g_AnimatedAudioClips;
        private float g_NextBlink;

        // Navigation
        private List<Vector3> g_NavQueue;
        private bool OverrideMaxSpeedValue = false;
        private NPCController g_NPCController;
        [System.ComponentModel.DefaultValue(1f)]
        private float MaxWalkSpeed { get; set; }
        [System.ComponentModel.DefaultValue(2f)]
        private float MaxRunSpeed { get; set; }
        private float g_DistanceToTarget;

        #endregion

        #region Properties

        public float TurningAngleThreshold = 5; // Orientation threshold
        public float JumpHeight = 0.8f;
        
        public Animator Animator {
            get {
                return g_Animator;
            }
        }
        
        public List<Vector3> NavigationPath {
            get {
                return g_NavQueue;
            }
        }

        /* STEERING / WALKING PARAMS */

        public Vector3 TargetLocation {
            get {
                // return g_TargetLocation;
                return Navigating ?
                    g_NavQueue[g_NavQueue.Count - 1] : g_TargetLocation;
            }
            set {
                g_TargetLocation = value;
            }
        }

        public Vector3 TargetOrientation {
            get {
                return g_TargetOrientation;
            }
        }
        
        [SerializeField] public float LocomotionBlendDelay = 0.15f;
        [SerializeField] public float TurningBlendDelay = 0.15f;
        [SerializeField] public bool  ObstaclesDetectionEnabled = true;
        [SerializeField] public float StepHeight = 0.3f;
        [SerializeField] public float MaxStepHeight = 0.6f;
        [SerializeField] public float TurningAngle = 55f;
        [SerializeField] public float MinTurningAngle = 40f;
        [SerializeField] public float MaxTurningAngle = 90f;
        [SerializeField] public float StepColliderHeightCorrection = 0.1f;
        [SerializeField] public float ColliderRadiusCorrection = 0.1f;
        [SerializeField] public float ColliderTestDistance = 0.1f;
        [SerializeField] public float ColliderTestAngle = 0.1f;
        [SerializeField] public float HeightAdjustInterpolation = 5f;
        [SerializeField] public float MaxArmsReach = 0.7f;


        /* SOCIAL FORCES */
        [SerializeField] public bool EnableSocialForces;
        [SerializeField] public bool EnableVelocityModifier;
        [SerializeField] public float VelocityModifierScale = 1f;
        [SerializeField] public float AgentAttractionForce = 0f;
        [SerializeField] public float AgentRepulsionWeight = 0.3f;
        [SerializeField] public float DistanceTolerance = 0.8f;
        [SerializeField] public float ProximityScaleMultiplier = 3f;
        [SerializeField] public float FollowAgentsFlow = 0f;

        /* PHYSICS */

        /// <summary>
        /// Enabled when a single collider is used for the entire agents body.
        /// Otherwise, it is assumed a Ragdoll or other colliders, manually controlled are
        /// used instead.
        /// </summary>
        [SerializeField]
        public bool UseSingleCapsuleCollider = false;
        
        public float Mass {
            get {
                return gRigidBody.mass;
            }
        }

        public float AgentHeight {
            get {
                return g_ColliderHeight;
            }
        }

        public float AgentRadius {
            get {
                return gCapsuleCollider.radius;
            }
        }

        public Vector3 Velocity {
            get {
                return (transform.position - g_LastpdatedPosition) * Time.deltaTime;
            }
        }

        public float Speed {
            get {
                return g_Animator.GetFloat(g_AnimParamSpeed);
            }
        }

        public float Orientation {
            get {
                return g_CurrentOrientation;
            }
        }

        public bool Blocked {
            get {
                return g_Blocked;
            }
        }
        
        public bool Navigating = false;

        public bool Oriented {
            get {
                g_OrientationAngle = Vector3.Angle(g_TargetOrientation,
                    new Vector3(transform.forward.x, 0, transform.forward.z));
                // Reduce or increment the accuracy based on the distance to final destination
                return g_OrientationAngle <= TurningAngleThreshold;
            }
        }
        
        [SerializeField]
        public NAV_STATE Navigation;

        [SerializeField]
        public bool UseCurvesAndParams = true;

        [SerializeField]
        public bool EnableBlinking;

        [SerializeField]
        public float BlinkingSpeed = 2f;

        /* IK */
        [SerializeField] public bool IKEnabled;
        [SerializeField] public bool IK_FEET_Enabled;
        [SerializeField] public float IK_FEET_HEIGHT_CORRECTION;
        [SerializeField] public float IK_FEET_FORWARD_CORRECTION;
        [SerializeField] public float IK_FEET_HEIGHT_EFFECTOR_CORRECTOR;
        [SerializeField] public bool IK_USE_HINTS = true;
        [SerializeField] public float IK_START_LOOK_AT_ADJUST = 1f;
        [SerializeField] public float IK_LOOK_AT_SMOOTH;
        [SerializeField] public float IK_HANDS_SMOOTH;
        [SerializeField] public bool UseAnimatorController;
        [SerializeField] public float NavDistanceThreshold = 0.3f;

        public bool LookingAround {
            get {
                return g_LookingAround;
            }
        }

        public Transform TargetObject {
            get {
                return gIKController.LOOK_AT;
            }
        }

        public Transform Head {
            get {
                return gIKController.Head;
            }
        }

        public bool IsTimedGesturePlaying(GESTURE_CODE gest) {
            // TODO - finish this implementation
            return !g_GestureTimer.Finished;
        }

        public bool IsAtTargetLocation(Vector3 targetLoc) {
            return g_TargetLocationReached &&
                Vector3.Distance(targetLoc, transform.position) <= NavDistanceThreshold * 2f // to account for floating point error
                * (g_NPCController.PerceivedAgents.Count == 0 ? 1 : g_NPCController.PerceivedAgents.Count);
        }

        public bool IsIdle {
            get {
                return
                    // TODO - this should test for navigation and state, not animation state
                    // We always need to test for a state and a possible active transition
                    g_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == gHashIdle
                    && g_Animator.GetAnimatorTransitionInfo(0).fullPathHash == 0;
            }
        }

        // Function to tell whether or not the player is on ground.
        public bool Grounded {
            get {
                Vector3 extents = gCapsuleCollider.bounds.extents;
                Vector3 fromPoint = transform.position + Vector3.up * 2 * extents.x;
                Ray ray = new Ray(fromPoint, Vector3.down);
                return Physics.SphereCast(ray, extents.x, extents.x + 0.2f);
            }
        }

        public bool Jumping;
        public bool Colliding;
        public float StairsClimbingForceFactor = 1f;

        public LOCO_CONTROL LocoControlMode = LOCO_CONTROL.AGENT_RELATIVE;
        
        /// <summary>
        /// Set the value of action mode to ensure it is on or off
        /// </summary>
        public bool SetActionMode {
            set {
                g_Animator.SetBool(g_AnimParamActionFlag, value);
            }
        }

        #endregion

        #region Unity_Methods

        private void OnCollisionEnter(Collision collision) {
            if (collision.collider == g_FrontBlocker)
                g_FrontBlocker = null;
        }

        // Collision detection.
        private void OnCollisionStay(Collision collision) {
            Colliding = true;
            Color c = Color.blue;
            RaycastHit hit;
            Vector3 startRayPosition = transform.position + Vector3.up;
            Physics.Raycast(transform.position + Vector3.up * StepHeight, transform.forward, out hit, 0.5f);
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (hit.collider == collision.collider && (rb == null || rb.isKinematic)) {
                float dir = NPCUtils.Direction(collision.contacts[0].normal, transform);
                g_FrontBlocked = true;
                g_FrontBlocker = collision.collider;
                g_NPCController.Debug("Front blocked");
                c = Color.red;
            } else {
                g_FrontBlocked = false;
            }
            g_NPCController.DebugRay(startRayPosition, transform.forward, c);
        }

        private void OnCollisionExit(Collision collision) {
            Colliding = false;
            g_FrontBlocked = false;
        }

        void Reset() {
            g_NPCController = GetComponent<NPCController>();
            g_NPCController.Debug("Initializing NPCBody ... ");
            gNavMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            gRigidBody = gameObject.GetComponent<Rigidbody>();
            g_Animator = gameObject.GetComponent<Animator>();
            gIKController = gameObject.GetComponent<NPCIKController>();
            if (gIKController == null) gIKController = gameObject.AddComponent<NPCIKController>();
            gIKController.hideFlags = HideFlags.HideInInspector;
            if (gNavMeshAgent == null) {
                gNavMeshAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                gNavMeshAgent.autoBraking = true;
                gNavMeshAgent.enabled = false;
                g_NPCController.Debug("NPCBody requires a NavMeshAgent if navigation is on, adding a default one.");
            }
            if (g_Animator == null || g_Animator.runtimeAnimatorController == null) {
                g_NPCController.Debug("NPCBody --> Agent requires an Animator Controller!!! - consider adding the NPCDefaultAnimatorController");
            } else UseAnimatorController = true;
            if (gRigidBody == null) {
                gRigidBody = gameObject.AddComponent<Rigidbody>();
                gRigidBody.useGravity = true;
                gRigidBody.mass = 3;
                gRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
                gRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            
            // Initialize main collider if none exists
            gCapsuleCollider = gameObject.GetComponentInChildren<CapsuleCollider>();
            if (gCapsuleCollider == null) {
                UseSingleCapsuleCollider = true;
                gCapsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                gCapsuleCollider.radius = 0.2f;
                gCapsuleCollider.height = 1.8f;
                gCapsuleCollider.center = new Vector3(0.0f, gCapsuleCollider.height / 2f, 0.0f);
                gCapsuleCollider.material = Resources.Load<PhysicMaterial>("Basic_Agent_Material");
            }
            
            if (gIKController == null) {
                gIKController = gameObject.AddComponent<NPCIKController>();
            }

            g_NPCController.EntityType = PERCEIVEABLE_TYPE.NPC;
        }

        #endregion

        #region Public_Funtions

        public void InitializeBody() {

            g_NPCController = GetComponent<NPCController>();

            // For handling height shifts - i.e. stairs
            if (UseSingleCapsuleCollider) {
                if (GetComponentInChildren<CharacterJoint>() != null) {
                    UseSingleCapsuleCollider = false;
                    g_NPCController.Debug("Ragdoll joints detected, disabling Use Single Capsule Collider");
                } else {
                    gCapsuleCollider = gameObject.GetComponent<CapsuleCollider>();
                    g_ColliderHeight = GetComponent<CapsuleCollider>().height;
                }
                // Always use the friction of the agent on collision
                gCapsuleCollider.material.frictionCombine = PhysicMaterialCombine.Minimum;
                g_DynamicFriction = gCapsuleCollider.material.dynamicFriction;
                g_StaticFriction = gCapsuleCollider.material.staticFriction;
            }

            // Initialize static members for all NPC - this assumes all Agents use the same animator for its meta data
            if (NPCBody.m_Gestures == null) {
                InitializeGestures();
            }
            if (NPCBody.m_Affordances == null) {
                InitializeAffordances();
            }

            g_Animator = gameObject.GetComponent<Animator>();
            gIKController = gameObject.GetComponent<NPCIKController>();
            gNavMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            gRigidBody = GetComponent<Rigidbody>();
            if (g_Animator == null) UseAnimatorController = false;
            if (gIKController == null) IKEnabled = false;
            g_NavQueue = new List<Vector3>();
            if (g_NPCController.TestTargetLocation != null) {
                GoTo(new List<Vector3>() { g_NPCController.TestTargetLocation.position });
            }
            g_TargetLocation = transform.position;
            g_TargetOrientation = transform.forward;
            g_GestureTimer = new NPCTimer();

            if (NPCBody.m_Gestures == null) {
                InitializeGestures();
            }

            // Index Animated Audio Clips
            g_AnimatedAudioClips = new Dictionary<string, NPCAnimatedAudio>();
            foreach (NPCAnimatedAudio a in g_NPCController.AnimatedAudioClips) {
                if (string.IsNullOrEmpty(a.Name)) {
                    g_NPCController.Debug("An animated clip hasn't had a name assigned to it.");
                } else {
                    a.BakeAnimatedAudioClip(m_Gestures);
                    g_AnimatedAudioClips.Add(a.Name, a);
                }
            }

            if (EnableBlinking) {
                g_NextBlink = Time.time + (UnityEngine.Random.value * BlinkingSpeed);
            }

            g_TurningVelocity = TurningAngleThreshold / MAX_TURNING_ANGLE;
        }

        public void OverrideMaxSpeedValues(bool activate, float walk = 1f, float run = 2f) {
            OverrideMaxSpeedValue = activate;
            MaxRunSpeed = OverrideMaxSpeedValue  ? run  : MAX_RUN_SPEED;
            MaxWalkSpeed = OverrideMaxSpeedValue ? walk : MAX_WALK_SPEED;
        }

        public NPCAnimation Animation(GESTURE_CODE g) {
            return m_Gestures[g];
        }

        /// <summary>
        /// Main Body component update routine
        /// </summary>
        internal void UpdateBody() {

            // Update gestures timer
            if (!g_GestureTimer.Finished)
                g_GestureTimer.UpdateTimer();


            if (IKEnabled) {
                gIKController.UpdateIK();
            }

            if(EnableBlinking) {
                if(Time.time > g_NextBlink) {
                    DoGesture(GESTURE_CODE.BLINK);
                    g_NextBlink = Time.time + (BlinkingSpeed * UnityEngine.Random.value);
                }
            }


            if (g_SkipUpdate) {
                g_NPCController.Debug("Skipping body update");
                return;
            }

            /*
             *  Every agent, main agent (main character) or not, will constantly update their
             *  steering and orientation if needed. This is necessary due to the fact that
             *  we need to allow controllers to interface via the NPCController body accessors
             *  in different ways.
             */

            UpdateNavigation();
            UpdateOrientation();
            if(ObstaclesDetectionEnabled)
                UpdateObstaclesDetection();
            
            // Apply curves if enabled
            if (UseCurvesAndParams) {
                if (UseSingleCapsuleCollider)
                    gCapsuleCollider.height = g_ColliderHeight * g_Animator.GetFloat(g_AnimParamColliderFactor);
            }

            g_LastpdatedPosition = transform.position;

            if (UseAnimatorController) {

                bool locomotion = false;
                
                bool actionMode = g_Animator.GetBool(g_AnimParamActionFlag);

                // If accidentally checked on set up
                if (g_Animator == null) {
                    g_NPCController.Debug("NPCBody --> No Animator in agent, disabling UseAnimatorController");
                    UseAnimatorController = false;
                    return;
                }

                // set base forward speed
                if (LocoControlMode == LOCO_CONTROL.AGENT_RELATIVE) {
                    if (g_CurrentStateFwd == LOCO_STATE.FORWARD) {
                        if (g_ToggleWalk)
                            g_TopFowardSpeed = MAX_WALK_SPEED;
                        else
                            g_TopFowardSpeed = MAX_RUN_SPEED;
                    } else if (g_CurrentStateFwd == LOCO_STATE.BACKWARDS) {
                        g_TopFowardSpeed = MAX_BACKWARDS_SPEED;
                    }
                } else if (LocoControlMode == LOCO_CONTROL.CAMERA_RELATIVE) {
                    if (g_CurrentStateFwd != LOCO_STATE.IDLE 
                        || g_CurrentStateDir != LOCO_STATE.FRONT) {
                        if (g_ToggleWalk)
                            g_TopFowardSpeed = MAX_WALK_SPEED;
                        else
                            g_TopFowardSpeed = MAX_RUN_SPEED;
                    }
                }

				// modify base forward speed
				if(g_CurrentStateMod == LOCO_STATE.RUN) {
					if (g_ToggleWalk)
						g_TopFowardSpeed = MAX_RUN_SPEED;
					else
						g_TopFowardSpeed = MAX_SPRINT_SPEED;
				}

                // handle mod
                float orient = g_CurrentStateDir == LOCO_STATE.RIGHT ? 1.0f : -1.0f;
                bool  duck = (g_CurrentStateMod == LOCO_STATE.DUCK);
                bool  jump = g_CurrentStateGnd == LOCO_STATE.JUMP;

                if (OverrideMaxSpeedValue) {
                    g_TopFowardSpeed = g_CurrentStateMod == LOCO_STATE.RUN
                        ? MaxRunSpeed : MaxWalkSpeed;
                    g_NPCController.Debug("Max Speed Overwritten: " + g_TopFowardSpeed + " Max Walk: " + MaxWalkSpeed);
                }

                if(actionMode) {
                    
                    // update forward
                    float deltaVal = g_CurrentSpeed;
                    if (g_CurrentStateFwd != LOCO_STATE.IDLE) {
                        locomotion = true;
                        g_CurrentSpeed = g_CurrentStateFwd == LOCO_STATE.BACKWARDS ? -1 * g_TopFowardSpeed : g_TopFowardSpeed;
                    } else g_CurrentSpeed = 0f;

                    // update strafing
                    if (g_CurrentStateDir != LOCO_STATE.FRONT) {
                        locomotion = true;
                        g_CurrentStrafe = g_TopFowardSpeed * orient;
                    } else g_CurrentStrafe = 0f;

                    if (LocoControlMode == LOCO_CONTROL.CAMERA_RELATIVE) {
                        if (locomotion) {
                            Vector3 agentFwd = new Vector3(transform.forward.x, 0, transform.forward.z);
                            Vector3 camFwd = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                            transform.forward = Vector3.Lerp(agentFwd, camFwd, Time.deltaTime * 2f);
                        }
                    }

                } else if (LocoControlMode == LOCO_CONTROL.AGENT_RELATIVE) {

                    // update forward
                    if (g_CurrentStateFwd != LOCO_STATE.IDLE) {
                        locomotion = true;
                        g_CurrentSpeed = g_TopFowardSpeed;
                    }

                    if (g_CurrentStateFwd == LOCO_STATE.IDLE || (g_FrontBlocked && g_CurrentSpeed > 0)) {
                        g_CurrentSpeed = 0f;
                    }

                    // update direction
                    if (g_CurrentStateDir != LOCO_STATE.FRONT) {
                        locomotion = true;
                        if(Navigating)
                            g_CurrentOrientation = orient * (g_OrientationAngle / 180f);
                        else
                            g_CurrentOrientation = orient;
                    } else {
                        g_CurrentOrientation = 0f;
                    }

                } else if (LocoControlMode == LOCO_CONTROL.CAMERA_RELATIVE) {

                    // Correct orientation only if agent is not turning by command
                    Vector3 agentFwd = new Vector3(transform.forward.x, 0, transform.forward.z);
                    Transform camTransform = Camera.main.transform;
                    Vector3 camFwd = camTransform.TransformDirection(Vector3.forward);
                    camFwd.y = 0f; camFwd = camFwd.normalized;
                    float dir = 0f;
                    if (g_CurrentStateDir == LOCO_STATE.FRONT
                            && g_CurrentStateFwd != LOCO_STATE.IDLE) {
                        float angleCam = Vector3.Angle(camFwd, agentFwd);
                        if (angleCam > TurningAngleThreshold) {
                            dir = NPCUtils.Direction(camFwd, transform);
                            g_CurrentOrientation = (angleCam / 90f) * dir;
                        }
                    }

                    if (g_CurrentStateDir != LOCO_STATE.FRONT ||
                        g_CurrentStateFwd != LOCO_STATE.IDLE) {
                        g_TargetOrientation = Vector3.zero;
                        if(g_CurrentStateFwd == LOCO_STATE.FORWARD) {
                            g_TargetOrientation = camFwd;
                        } else if (g_CurrentStateFwd == LOCO_STATE.BACKWARDS) {
                            g_TargetOrientation = -camFwd;
                        }
                        if(g_CurrentStateDir == LOCO_STATE.LEFT) {
                            g_TargetOrientation -= camTransform.right;
                            Move(LOCO_STATE.FORWARD);
                        } else if (g_CurrentStateDir == LOCO_STATE.RIGHT) {
                            g_TargetOrientation += camTransform.right;
                            Move(LOCO_STATE.FORWARD);
                        }
                    }

                    float angle = Vector3.Angle(g_TargetOrientation, agentFwd);
                    dir = NPCUtils.Direction(g_TargetOrientation, transform);
                    g_CurrentOrientation = (angle / 180f) * dir;
                    Quaternion targetRotation = Quaternion.LookRotation(g_TargetOrientation);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * g_CurrentSpeed);

                    if (angle > TurningAngleThreshold) {
                        locomotion = true;
                    }
                    
                    if (angle > TurningAngle && g_CurrentSpeed == 0)
                        g_TopFowardSpeed = 0;

                    // update forward
                    float deltaVal = g_CurrentSpeed;
                    if (g_CurrentStateFwd != LOCO_STATE.IDLE || g_CurrentStateDir != LOCO_STATE.FRONT) {
                        locomotion = true;
                        g_CurrentSpeed = g_TopFowardSpeed;
                    }

                    if (g_CurrentStateFwd == LOCO_STATE.IDLE || (g_FrontBlocked && g_CurrentSpeed > 0)) {
                        g_CurrentSpeed = 0f;
                    }

                }

                // update ground

                // Start a new jump.
                
                Jumping = g_Animator.GetBool(g_AnimParamJumping);
                if (jump && !Jumping && Grounded) {
                    if (g_FrontBlocked) {
                        float rootDistance = (g_FrontBlocker.transform.position.y + g_FrontBlocker.bounds.extents.y) - g_NPCController.GetPosition().y;
                        float agentHeight = gCapsuleCollider.bounds.size.y;
                        if (rootDistance < agentHeight) {
                            StartCoroutine(ClimbJump(GESTURE_CODE.CLIMB_LOW, rootDistance));
                        } else if (rootDistance < agentHeight + JumpHeight) {
                            StartCoroutine(ClimbJump(GESTURE_CODE.CLIMB_HIGH, rootDistance));
                        } else {
                            g_NPCController.Debug("Can't climb, too high @ " + rootDistance);
                        }
                    } else {
                        StartCoroutine(Jump());
                    }
                }
                else if (gRigidBody.velocity.y < -4 && !Grounded) {
                    SetAnimatorParam(g_AnimParamFalling, true);
                } else {
                    SetAnimatorParam(g_AnimParamFalling, false);
                }
                
                // set animator
                SetAnimatorParam(g_AnimParamLocomotion, locomotion);
                SetAnimatorParam(g_AnimParamSpeed, g_CurrentSpeed * (1f - g_CurrentSpeedPenalty), LocomotionBlendDelay);
                SetAnimatorParam(g_AnimParamDirection, g_CurrentOrientation, TurningBlendDelay);
                SetAnimatorParam(g_AnimParamStrafe, g_CurrentStrafe, LocomotionBlendDelay);

                // reset all states until updated again
                SetIdle();
            }
        }

        /// <summary>
        /// Commands the agent to move with LOCO_STATE enum options.
        /// </summary>
        /// <param name="state"></param>
        public void Move(LOCO_STATE state) {
            switch (state) {
            case LOCO_STATE.RUN:
			case LOCO_STATE.SPRINT:
            case LOCO_STATE.DUCK:
            case LOCO_STATE.WALK:
                g_CurrentStateMod = state;
                break;
            case LOCO_STATE.FORWARD:
            case LOCO_STATE.BACKWARDS:
            case LOCO_STATE.IDLE:
                g_CurrentStateFwd = state;
                break;
            case LOCO_STATE.RIGHT:
            case LOCO_STATE.LEFT:
            case LOCO_STATE.FRONT:
                g_CurrentStateDir = state;
                break;
            case LOCO_STATE.JUMP:
                g_CurrentStateGnd = state;
                break;
			case LOCO_STATE.TOGGLE_WALK:
				g_ToggleWalk = !g_ToggleWalk;
				break;
            default:
                g_NPCController.Debug("NPCBody --> Invalid direction especified for ModifyMotion");
                break;
            }
        }

        #region Affordances

        /// <summary>
        /// No path finding involved.
        /// </summary>
        /// <param name="location"></param>
        [NPCAffordance("WalkTowards")]
        public void WalkTowards(Vector3 location) {
            g_NavQueue.Clear();
            g_NavQueue.Add(location);
        }

        /// <summary>
        /// Run towards location point
        /// </summary>
        /// <param name="t"></param>
        [NPCAffordance("RunTo")]
        public void RunTo(Vector3 t) {
            float distance = Vector3.Distance(t, TargetLocation);
            if (distance
                >= NavDistanceThreshold * 2) {
                List<Vector3> path = g_NPCController.AI.FindPath(t);
                if (path.Count < 1) {
                    throw new Exception("NPCController --> No path found to target location");
                } else {
                    RunTo(path);
                }
            }
        }

        /// <summary>
        /// The queue will we checked and followed every UpdateNavigation call
        /// </summary>
        /// <param name="List of locations to follow"></param>
        [NPCAffordance("GoTo")]
        public void GoTo(Vector3 t) {
            float distance = Vector3.Distance(t, TargetLocation);
            if (distance
                >= NavDistanceThreshold * 2) {
                List<Vector3> path = g_NPCController.AI.FindPath(t);
                if (path.Count < 1) {
                    throw new Exception("NPCController --> No path found to target location");
                } else {
                    GoTo(path);
                }
            }
        }

        /// <summary>
        /// Stop following all targets
        /// </summary>
        [NPCAffordance("StopFollow")]
        public void StopFollow() {
            g_NPCController.FollowTarget = null;
        }

        /// <summary>
        /// Follow a specific target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="run"></param>
        [NPCAffordance("Follow")]
        public void Follow(Transform target, bool run) {
            g_NPCController.FollowTarget = target;
            List<Vector3> path = g_NPCController.AI.FindPath(target.position);
            if(run) {
                RunTo(path);
            } else {
                GoTo(path);
            }
        }

        /// <summary>
        /// Requires an orientation vector from the agent's position
        /// target - current position
        /// </summary>
        /// <param name="target"></param>
        [NPCAffordance("OrientTowards")]
        public void OrientTowards(Vector3 target) {
            g_TargetOrientation = target;
        }

        /// <summary>
        /// Start looking a specific target
        /// </summary>
        /// <param name="t"></param>
        [NPCAffordance("StartLookAt")]
        public void StartLookAt(Transform t) {
            INPCPerceivable p = t.GetComponent<INPCPerceivable>();
            if (p != null) {
                gIKController.LOOK_AT = p.GetMainLookAtPoint();
            } else
                gIKController.LOOK_AT = t;
        }
        
        /// <summary>
        /// Stop looking at all targets
        /// </summary>
        [NPCAffordance("StopLookAt")]
        public void StopLookAt() {
            gIKController.LOOK_AT = null;
        }

        /// <summary>
        /// The caller might specify an optional parameter depeding on the type of animation.
        /// The optional parameter could be a float or a boolean. Triggers do not require
        /// parameters.
        /// </summary>
        /// <param name="gesture"></param>
        /// <param name="o"></param>
        [NPCAffordance("DoGesture")]
        public void DoGesture(GESTURE_CODE gesture, System.Object o = null, bool timed = false) {
            o = (o == null || string.IsNullOrEmpty(o.ToString())) ? null : o;
            NPCAnimation anim = m_Gestures[gesture];
            switch (anim.ParamType) {
                case ANIMATION_PARAM_TYPE.TRIGGER:
                    SetAnimatorParam(anim.Name);
                    break;
                case ANIMATION_PARAM_TYPE.BOOLEAN:
                    bool b = (o == null ? !g_Animator.GetBool(anim.Name) : Convert.ToBoolean(o));
                    SetAnimatorParam(anim.Name, b);
                    break;
                case ANIMATION_PARAM_TYPE.FLOAT:
                    float f = Convert.ToSingle(o);
                    SetAnimatorParam(anim.Name, f);
                    break;
            }
            LastGesture = gesture;
            if (timed)
                g_GestureTimer.StartTimer(m_Gestures[gesture].Duration);
        }

        /// <summary>
        /// Starts a coroutine which hanles synchronized anims and audio
        /// </summary>
        /// <param name="animAudioName"></param>
        [NPCAffordance("DoAudioAnimGesture")]
        public void DoAudioAnimGesture(string animAudioName) {
            if (g_AnimatedAudioClips.ContainsKey(animAudioName)) {
                StartCoroutine(PlayAnimatedAudioClip(g_AnimatedAudioClips[animAudioName]));
            } else {
                g_NPCController.Debug("Animated Audio Clip is not mapped for key: " + animAudioName);
            }
        }

        [NPCAffordance("StopNavigation")]
        public void StopNavigation() {
            g_NPCController.FollowTarget = null;
            SetIdle();
            g_TargetOrientation = transform.forward;
            g_NavQueue.Clear();
            Navigating = false;
            g_TargetLocation = transform.position;
            g_ToggleWalk = false;
            g_TargetLocationReached = true;
        }

        [NPCAffordance("StopAllMotion")]
        public void StopAllMotion() {
            StopNavigation();
            g_GestureTimer.StopTimer();
        }

        [NPCAffordance("GrabRightHand")]
        public void GrabRightHand(NPCObject t, bool grab = true, GESTURE_CODE grabGest = GESTURE_CODE.GRAB_FRONT) {
            float dist = Vector3.Distance(t.GetPosition(), transform.position);
            if (grab && dist <= MaxArmsReach) {
                OrientTowards(t.GetPosition() - transform.position);
                gIKController.GRAB_RIGHT = t;
                if(dist < MaxArmsReach * 0.5f)
                    DoGesture(GESTURE_CODE.PICK_UP, true, true);
                else
                    DoGesture(grabGest, true, true);
            } else {
                gIKController.GRAB_RIGHT = null;
            }
            DoGesture(GESTURE_CODE.HAND_GRAB_RIGHT, grab);
        }

        /// <summary>
        /// Used to start and stop looking around
        /// </summary>
        /// <param name="startLooking"></param>
        public void LookAround(bool startLooking) {
            GameObject go;
            if (startLooking && !g_LookingAround) {
                go = new GameObject();
                go.name = "TmpLookAtTarget";
                Func<Vector3, Vector3> pos = np => (np + (1.50f * transform.forward));
                go.transform.position = pos(transform.position);
                go.transform.rotation = transform.rotation;
                go.transform.SetParent(transform);
                StartLookAt(go.transform);
                g_LookingAround = true;
            } else if (g_LookingAround) {
                go = gIKController.LOOK_AT.gameObject;
                StopLookAt();
                DestroyImmediate(go);
                g_LookingAround = false;
            }
        }

        #endregion

        /// <summary>
        /// Turns action mode on and off
        /// </summary>
        public void ToggleAction() {
            bool val = !g_Animator.GetBool(g_AnimParamActionFlag);
            g_Animator.SetBool(g_AnimParamActionFlag, val);
        }

        #endregion

        #region Private_Functions

        private void RunTo(List<Vector3> location) {
            GoTo(location);
            g_ToggleWalk = false;
        }

        private void GoTo(List<Vector3> location) {
            g_ToggleWalk = true;
            Navigating = true;
            g_NavQueue.Clear();
            g_NavQueue = location;
            g_TargetLocationReached = false;
        }

        private void UpdateNavigation() {
            if (Navigation != NAV_STATE.DISABLED) {
                if (Navigation == NAV_STATE.STEERING_NAV) {
                    if (g_NavQueue.Count > 0 || g_NPCController.Following) {
                        // if following a target and no path is available, recalculate path
                        if (g_NPCController.AI.PathUpdateable) {
                            if (g_NPCController.Following) {
                                if (g_NavQueue.Count == 0)
                                    g_NavQueue = g_NPCController.AI.FindPath(g_NPCController.FollowTarget.position);
                                else if (Vector3.Distance(
                                    g_NPCController.FollowTarget.position, g_NavQueue[g_NavQueue.Count - 1]) > NavDistanceThreshold) {
                                    g_NavQueue = g_NPCController.AI.FindPath(g_NPCController.FollowTarget.position);
                                }
                            }
                        }
                        Navigating = g_NavQueue.Count > 0;
                        g_TargetLocationReached = false;
                        HandleSteering();
                    }
                } else {
                    g_TargetLocation = g_NavQueue[0];
                    g_NavQueue.Clear();
                    HandleNavAgent();
                }
            } else {
                Navigating = false;
            }
        }

        private void HandleNavAgent() {
            if (gNavMeshAgent != null) {
                if (!gNavMeshAgent.enabled)
                    gNavMeshAgent.enabled = true;
                if (g_NavQueue.Count > 0)
                    gNavMeshAgent.SetDestination(g_TargetLocation);
            }
        }

        private void HandleSteering() {
            if (!Navigating) return;
            g_TargetLocation = g_NavQueue[0];
            g_DistanceToTarget = Vector3.Distance(TargetLocation, transform.position);
            float distanceToNextPoint = Vector3.Distance(transform.position, g_TargetLocation);
            g_TargetOrientation = g_TargetLocation - transform.position;
            g_NPCController.DebugLine(Head.position, g_TargetLocation, Color.yellow);
            float threshold = NavDistanceThreshold * (g_NavQueue.Count > 1 ? 2f : 1f);
            // TODO - there's gotta be a smarter way to control stopping, yet this one's working for now in volumes.
            int agentsCount = 1 + g_NPCController.PerceivedAgents.Count;
            if ((g_DistanceToTarget <= threshold * agentsCount)) {
                RaycastHit h;
                if (Physics.SphereCast(Head.position, AgentRadius * agentsCount, g_TargetOrientation, out h,
                    distanceToNextPoint)) {
                    goto NextPoint;
                }
            }

            if (EnableSocialForces) {
                ComputeSocialForces(ref g_TargetOrientation);
                if (g_NPCController.DebugMode)
                    g_NPCController.DebugRay(transform.position + Vector3.up, g_TargetOrientation, Color.magenta);
            }

            Move(LOCO_STATE.FORWARD);
            
            if (distanceToNextPoint >= threshold) {
                return;
            }

            NextPoint:
            if (Navigating)
                g_NavQueue.RemoveAt(0);
            Navigating = g_NavQueue.Count > 0;
            if(!Navigating) StopNavigation();
        }
        
        private void SetIdle() {
            g_CurrentStateFwd = LOCO_STATE.IDLE;
            g_CurrentStateGnd = LOCO_STATE.GROUND;
            g_CurrentStateDir = LOCO_STATE.FRONT;
            g_CurrentStateMod = LOCO_STATE.WALK;
            g_CurrentSpeedPenalty = 0f;
        }

        private void ComputeSocialForces(ref Vector3 currentTarget) {
            float dist = Vector3.Distance(transform.position, TargetLocation);
            float currentSpeed = g_CurrentSpeed;
            Vector3 preferredForce = currentTarget.normalized;
            Vector3 followAgents = ComputeCohesionForce();
            Vector3 repulsionForce = ComputeAgentsRepulsionForce();
            if(EnableVelocityModifier) {
                g_CurrentSpeed = Mathf.Lerp(g_CurrentSpeed, ComputeVelocity(g_CurrentSpeed), Time.deltaTime * VelocityModifierScale);
                OverrideMaxSpeedValues(true, g_CurrentSpeed);
            }
            if(currentSpeed == g_CurrentSpeed) OverrideMaxSpeedValues(false);
            currentTarget = preferredForce + (repulsionForce + followAgents) * g_CurrentSpeed * Mathf.Min(1f,dist);
        }

        private Vector3 ComputeCohesionForce() {
            Vector3 totalForces = Vector3.zero;
            float count = 0;
            foreach (INPCPerceivable p in g_NPCController.Perception.PerceivedAgents) {
                totalForces += (p.GetPosition() - transform.position) * p.GetPerceptionWeight();
                count += p.GetPerceptionWeight();
            }
            if (count > 0) {
                totalForces /= g_NPCController.Perception.PerceivedAgents.Count;
            }
            return g_NPCController.PerceivedAgents.Count > 0 ?
                totalForces.normalized * AgentAttractionForce : Vector3.zero;
        }

        private float ComputeVelocity(float currentSpeed) {
            if (g_NPCController.PerceivedAgents.Count == 0)
                return currentSpeed;
            else {
                float speed = 0;
                int agents = 0;
                bool modSpeed = false;
                foreach (INPCPerceivable p in g_NPCController.PerceivedAgents) {
                    float distance = Vector3.Distance(transform.position, p.GetPosition());
                    float angle = Vector3.Angle(transform.forward, p.GetForwardDirection());
                    if(angle <= 45) {
                        speed += p.GetCurrentSpeed();
                        modSpeed = true;
                    }
                    agents++;
                }
                return modSpeed ? (speed / agents) : currentSpeed;
            }
        }

        private Vector3 ComputeAgentsRepulsionForce() {
            Vector3 repulsionForce = Vector3.zero,
                    proximityForce = Vector3.zero,
                    avgDirection = Vector3.zero,
                    flowForce = Vector3.zero;

            int agents = g_NPCController.Perception.PerceivedAgents.Count;
            foreach (INPCPerceivable p in g_NPCController.PerceivedAgents) {

                Vector3 normal = (transform.position - p.GetPosition());
                Vector3 direction = NPCUtils.Direction(p.GetPosition() - transform.position, transform) < 0f ? transform.right : -transform.right;
                float radii = AgentRadius + p.GetAgentRadius();
                float distance = Vector3.Distance(transform.position, p.GetPosition());
                
                /* Compute Proximity */
                float scale = 0f;

                if (p.GetNPCEntityType() == PERCEIVEABLE_TYPE.NPC) {
                    scale = ProximityScaleMultiplier * Mathf.Exp(radii - (distance / DistanceTolerance));
                } else {
                    // TODO: add the weights for objects - constant B
                    scale = ProximityScaleMultiplier * Mathf.Exp(radii - (distance / DistanceTolerance));
                }

                /* Obtain avg direction */
                proximityForce += (normal * scale) / distance;

                /* Compute Agents Flow */
                if (p.GetCurrentSpeed() > 0.1f) {
                    flowForce += p.GetForwardDirection() * FollowAgentsFlow;
                }
            }
            
            return agents > 0 ?
                (flowForce + repulsionForce + proximityForce)
                    : Vector3.zero;
        }

        private void UpdateOrientation() {
            if (!(Navigation == NAV_STATE.DISABLED)) {
                g_TargetOrientation = new Vector3(g_TargetOrientation.x, 0f, g_TargetOrientation.z);
                //g_OrientationAngle = Vector3.Angle(g_TargetOrientation, 
                //    new Vector3(transform.forward.x,0,transform.forward.z));
                //// Reduce or increment the accuracy based on the distance to final destination
                //Oriented = g_OrientationAngle <= TurningAngleThreshold;
                if (!Oriented) {
                    g_NPCController.DebugRay(transform.position, g_TargetOrientation, Color.cyan);
                    LOCO_STATE d = NPCUtils.Direction(g_TargetOrientation, transform) < 1.0f ? LOCO_STATE.LEFT : LOCO_STATE.RIGHT;
                    if (g_OrientationAngle > TurningAngle)
                        Move(LOCO_STATE.IDLE);
                    Move(d);
                    Quaternion targetRotation = Quaternion.LookRotation(g_TargetOrientation);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * Mathf.Max(g_CurrentSpeed,g_CurrentOrientation));
                }
            }
        }

        private void UpdateObstaclesDetection() {
            if (ObstaclesDetectionEnabled && g_CurrentStateFwd == LOCO_STATE.FORWARD && Grounded) {
                Vector3 heightCorrection = (Vector3.up * StepColliderHeightCorrection);
                RaycastHit rayHit;
                Ray ray = new Ray(transform.position + heightCorrection + (transform.forward * ColliderRadiusCorrection),
                    transform.forward + Vector3.down * ColliderTestAngle);
                if (Physics.Raycast(ray, out rayHit, ColliderTestDistance)) {
                    Transform t = rayHit.collider.transform;
					float currColliderFactor = g_Animator.GetFloat (g_AnimParamColliderFactor);
                    if (rayHit.collider.bounds.size.y <= MaxStepHeight) {
                        float height = rayHit.collider.bounds.size.y;
                        SetAnimatorParam(g_AnimParamColliderFactor, 0.9f, HeightAdjustInterpolation);
						gRigidBody.AddForce(Vector3.up * ( 1 + height) * StairsClimbingForceFactor, ForceMode.VelocityChange);
                        g_CurrentSpeedPenalty += 0.5f;
                    }
                } else {
                    SetAnimatorParam(g_AnimParamColliderFactor, 1f);
                }
            }
        }

        /// <summary>
        /// Initialize all defined enum gestures by using reflection
        /// </summary>
        private void InitializeGestures() {
            Array a = Enum.GetValues(typeof(GESTURE_CODE));
            m_Gestures = new Dictionary<GESTURE_CODE, NPCAnimation>();
            Dictionary<string, NPCAnimation> npcAnimsStates = new Dictionary<string, NPCAnimation>();
            foreach (var t in a) {
                Type type = t.GetType();
                var name = Enum.GetName(type, t);
                var att = type.GetField(name).GetCustomAttributes(typeof(NPCAnimation), false);
                NPCAnimation anim = (NPCAnimation)att[0];
                anim.AnimationHash = Animator.StringToHash(anim.Name);
                m_Gestures.Add((GESTURE_CODE)t, anim);
                npcAnimsStates.Add(anim.Name, anim);
            }

            // Fill up animation meta data in case needed in the future
#if UNITY_EDITOR
            g_AnimationStates = new Dictionary<string, AnimationClip>();
            if (g_Animator != null) {
                AnimatorController controller = g_Animator.runtimeAnimatorController as AnimatorController;
                if (controller != null) {
                    foreach (AnimatorControllerLayer layer in controller.layers) {
                        foreach (ChildAnimatorState state in layer.stateMachine.states) {
                            AnimationClip clip = state.state.motion as AnimationClip;
                            string stateName = state.state.name;
                            if (clip != null) {
                                g_AnimationStates.Add(stateName, clip);
                            }
                            if (npcAnimsStates.ContainsKey(stateName)) {
                                npcAnimsStates[stateName].AnimationClip = clip;
                                npcAnimsStates[stateName].AnimatorLayer = layer;
                                npcAnimsStates[stateName].RuntimeAnimatorLayer = Animator.GetLayerIndex(layer.name);
                                npcAnimsStates[stateName].AnimatorState = state.state;
                            }
                        }
                    }
                } else {
                    g_NPCController.Debug("ERROR: No animator controller assigned!");
                }
            }
#endif
        }

        /// <summary>
        /// Initialize all existing affordances
        /// </summary>
        private void InitializeAffordances() {
            Array a = typeof(NPCBody).GetMethods();
            m_Affordances = new Dictionary<NPCAffordance, string>();
            foreach (MethodInfo m in a) {
                object[] att = m.GetCustomAttributes(typeof(NPCAffordance), false);
                if (att.Length == 1) {
                    NPCAffordance aff = (NPCAffordance)att[0];
                    m_Affordances.Add(aff, aff.Name);
                }
            }
            g_NPCController.Debug("modular NPC AFFORDANCES successfully initialized: " + m_Affordances.Count);
        }

        private IEnumerator PlayAnimatedAudioClip(NPCAnimatedAudio clip) {

            if (clip.RandomizeAnimations) {
                clip.ShuffleAnimations();
            }

            Queue<NPCAnimatedAudio.AnimationStamp> anims = clip.AnimationsQueue();
            Queue<NPCAnimatedAudio.AudioClipStamp> clips = clip.AudioClipsQueue();

            float startTime = Time.time;
            float runLength = startTime + clip.Length();

            while (Time.time <= runLength) {
                if (clips.Count > 0 && (clips.Peek().ExecutionTime() + startTime) <= Time.time) {
                    g_NPCController.PlayAudioClip(clips.Dequeue().Clip);
                }
                if (anims.Count > 0 && (anims.Peek().ExecutionTime() + startTime) <= Time.time) {
                    DoGesture(anims.Dequeue().Gesture);
                }
                yield return null;
            }

            // clean up
            foreach(NPCAnimatedAudio.AnimationStamp s in clip.Animations) {
                if(m_Gestures[s.Gesture].ParamType == ANIMATION_PARAM_TYPE.BOOLEAN) {
                    DoGesture(s.Gesture, false);
                }
            }
            
        }

        private void SetAnimatorParam(string param, object value = null, object dampTime = null) {
            if(UseAnimatorController && !g_AnimatorLocked) {
                if(value == null) {
                    g_Animator.SetTrigger(param);
                } else {
                    if (value is bool) {
                        g_Animator.SetBool(param, (bool) value);
                    } else {
                        float val = (float) value;
                        if (dampTime == null) {
                            g_Animator.SetFloat(param, val);
                        } else {
                            float damp = (float) dampTime;
                            g_Animator.SetFloat(param, val, damp, Time.deltaTime);
                        }
                    }
                }
            }
        }

        private IEnumerator Jump() {
            // Set jump related parameters.
            SetAnimatorParam(g_AnimParamJump);
            SetAnimatorParam(g_AnimParamJumping, true);
            Jumping = true;
            g_SkipUpdate = true;
            g_AnimatorLocked = true;
            if (g_CurrentSpeed > 0.1f) { // == LOCO_STATE.FORWARD) {
                // Temporarily change player friction to pass through obstacles.
                gCapsuleCollider.material.dynamicFriction = 0f;
                gCapsuleCollider.material.staticFriction = 0f;
                // Set jump vertical impulse velocity.
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * JumpHeight;
                velocity = Mathf.Sqrt(velocity);
                gRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
            yield return null;
            while (Jumping) {
                // Keep forward movement while in the air.
                gRigidBody.AddForce(transform.forward * 10f * Physics.gravity.magnitude *
                    g_Animator.GetFloat(g_AnimParamSpeed), ForceMode.Acceleration);
                // Has landed?
                if (gRigidBody.velocity.y <= 0 && Grounded) {
                    Jumping = false;
                    g_AnimatorLocked = false;
                    g_SkipUpdate = false;
                    SetAnimatorParam(g_AnimParamJumping, false);
                    // Change back player friction to default.
                    gCapsuleCollider.material.dynamicFriction = g_DynamicFriction;
                    gCapsuleCollider.material.staticFriction = g_StaticFriction;
                }
                yield return null;
            }
        }

        private IEnumerator ClimbJump(GESTURE_CODE gest, float rootDistance) {
            g_SkipUpdate = true;
            DoGesture(gest, null, true);
            g_AnimatorLocked = true;
            GameObject rightHandTarget = new GameObject();
            rightHandTarget.transform.position = transform.position
                + new Vector3(0, rootDistance * 1.2f, 0)
                + (transform.right * 0.2f)
                + (transform.forward * 0.2f);
            yield return null;
            while (IsTimedGesturePlaying(gest)) {
                if(!g_Animator.IsInTransition(0)) {
                    gIKController.Climb(rightHandTarget.transform);
                }
                yield return null;
            }
            GameObject.DestroyImmediate(rightHandTarget);
            g_AnimatorLocked = false;
            g_SkipUpdate = false;
        }

        #endregion
    }

}
