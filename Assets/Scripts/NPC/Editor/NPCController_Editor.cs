using UnityEditor;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NPCController))]
    public class NPCController_Editor : Editor {

        NPCController gController;

        #region Constants
        private const string label_ViewAngle = "View Angle";
        private const string label_PerceptionRadius = "Perception Radius";
        private const string label_BodyNavigation = "Navigation";
        private const string label_IKEnabled = "Enable IK";
        private const string label_MainAgent = "Main Agent";
        private const string label_SelectHighlight = "Enable Selection Indicator";
        private const string label_AnimatorEnabled = "Use Animator";
        private const string label_UseAnimCurves = "Use Animation Curves and Params";
        private const string label_NavStoppingThresh = "Breaking Threshold";
        private const string label_AIPathfind = "Pathfinder";
        private const string label_NPCLoadedMods = "Loaded NPC Modules";
        private const string label_DebugPrint = "Debug Mode";
        private const string label_NavMeshAgentPathfinding = "Use NavMeshAgent";
        private const string label_UseSocialForces = "Use Social Forces";
        private const string label_AgentRepulsionWeight = "Agents Repulsion Weight";
        private const string label_AgentRepulsionDistanceTolerance = "Agents Distance Tolerance";
        private const string label_TestNPC = "Test NPC";
        private const string label_IKFeetHeight = "IK Feet Height Correction";
        private const string label_IKFeetForward = "IK Feet Forward Correction";
        private const string label_IKFeetEffectorCorrector = "IK Feet Effector Height";
        private const string label_IKUseHints = "Use IK Hints";
        private const string label_IKFeetStairsInt = "IK Stairs Interpolation";
        private const string label_IKFeetEnabled = "IK Feet Enabled";
        private const string label_IKLookStartAdjust = "IK Start Look Adjust";
        private const string label_IKLookSmoothness = "IK Look Smoothing";
        private const string label_StepObstacleDetection = "Enable Obstacles Detection";
        private const string label_StepHeight = "Step Height";
        private const string label_MaxStepHeight = "Max Step Height";
        private const string label_BodyTurningAngle = "Turning Angle";
        private const string label_BodyColliderRadiusCorrection = "Collider Radius Correction";
        private const string label_BodyStepHeightCorrection = "Step Height Correction";
        private const string label_BodyColliderTestDistance = "Step Obstacle Test Distance";
        private const string label_BodyColliderTestAngle = "Step Collider Test Angle";
        private const string label_BodyHeightAdjustInterpolation = "Height Interpolation Factor";
        private const string label_BodyEnableVelocityModifier = "Social Forces - Enable Velocity Modifier";
        private const string label_BodyVelocityModifierFactor = "Velocity Modifier Factor";
        private const string label_BodySFAgentsAttractionForce = "Agents Attraction Force";
        private const string label_BodyLocomotionBlendDelay = "Locomotion Blend Delay";
        private const string label_BodyTurningBlendDelay = "Turning Blend Delay";
        private const string label_PerceptionPercpWeight = "Perceived Weight";
        private const string label_BodySFMatchAgentsFlow = "Follow Agents Flow";
        private const string label_AgentProximityScaleMultiplier = "Proximity Scale Multiplier";
        private const string label_ControllerUpdateTime = "Update Controller Time";
        private const string label_AIPathRecalculationTime = "Path Recalculation Time";
        private const string label_BodyUseSingleCapsuleCollider = "Use Single Capsule Collider";
        private const string label_UseAduioClips = "Use Animated Audio Clips";
        private const string label_EnableAgentAudio = "Enable Agent Audio";
        private const string label_EnableBlinking = "Enable Blinking";
        private const string label_BlinkingSpeed = "Blinking Speed";
        private const string label_IKHandsSmoothing = "Hands IK Smooth";
        private const string label_IKLeftHandSmoothing = "Left hand IK Smooth";
        private const string label_IKMaxArmsReach = "Max IK Arms Reach";
        private const string label_NPCAgentTags = "Agent Tags";
        private const string label_BodyJumpHeight = "Jump Height";
        private const string label_BodyStairsClimbingForceFactor = "Stairs Force Factor";
        private const string label_BodyLocoControlMode = "Locomotion Control Mode";

        [SerializeField]
        int selectedPathfinder;
        
        #endregion

        #region Insperctor_GUI
        private bool gGeneralSettings = true;
        private bool gShowPerception = true;
        private bool gShowBody = true;
        private bool gShowAI = true;
        private bool gShowMods = true;
        private bool gShowAnimationsSection = true;

        public override void OnInspectorGUI() {
            // Update all serialized objects
            serializedObject.Update();
            gController = (NPCController) target;

            // Variables for multiple selected agents
            float agentAttractionForce = 0, agentRepulsionWeight = 0, distanceTolerance = 0, proximityScaleMultiplier = 0, followAgentsFlow = 0, velocityModifierScale = 0;
            float stairsClimbingForceFactor = 0, heightAdjustInterpolation = 0, stepHeight = 0, maxStepHeight = 0, colliderRadiusCorrection = 0, stepColliderHeightCorrection = 0,
                    colliderTestDistance = 0, colliderTestAngle = 0;
            bool enableVelocityModifier = false;
            
            EditorGUI.BeginChangeCheck();

            /* Load Modules */
            if (gController.GetComponent<INPCModule>() != null) {
                gShowMods = EditorGUILayout.Foldout(gShowMods, "NPC Modules");
                if (gShowMods) {
                    gController.LoadNPCModules();
                    INPCModule[] mods = gController.NPCModules;
                    GUILayoutOption[] ops = new GUILayoutOption[1];
                    foreach(INPCModule m in mods) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(m.NPCModuleName());
                        m.SetEnable(EditorGUILayout.Toggle( m.IsEnabled()));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            } else EditorGUILayout.LabelField("No NPC Modules Loaded");

            /* General Controller */
            gGeneralSettings = EditorGUILayout.Foldout(gGeneralSettings, "General Settings");
            if(gGeneralSettings) {
                gController.UpdateTime = EditorGUILayout.FloatField(label_ControllerUpdateTime, gController.UpdateTime);
                gController.MainAgent = EditorGUILayout.Toggle(label_MainAgent, gController.MainAgent);
                gController.DebugMode = EditorGUILayout.Toggle(label_DebugPrint, gController.DebugMode);
                gController.AgentType = (AGENT_TYPE) EditorGUILayout.EnumPopup("Agent Type", gController.AgentType);
                gController.TestNPC = EditorGUILayout.Toggle(label_TestNPC, gController.TestNPC);
                if (gController.TestNPC) {
                    gController.TestTargetLocation = (Transform)EditorGUILayout.ObjectField("Test Target Location", (Transform)gController.TestTargetLocation, typeof(Transform), true);
                } else gController.TestTargetLocation = null;
                gController.EnableAudio = EditorGUILayout.Toggle(label_EnableAgentAudio, gController.EnableAudio);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(label_NPCAgentTags);
                if(serializedObject.FindProperty("AgentTags") != null)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AgentTags").FindPropertyRelative("Array.size"), true);
                if (gController.AgentTags == null) gController.AgentTags = new string[0];
                else {
                    for (int at = 0; at < gController.AgentTags.Length; ++at) {
                        gController.AgentTags[at] = EditorGUILayout.TextField("Agent Tag " + (at + 1), gController.AgentTags[at]);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            /* Perception */
            gShowPerception = EditorGUILayout.Foldout(gShowPerception, "Perception") && gController.Perception != null;
            if(gShowPerception) {
                gController.Perception.ViewAngle = EditorGUILayout.IntSlider(label_ViewAngle, (int) gController.Perception.ViewAngle, 
                    (int) NPCPerception.MIN_VIEW_ANGLE, 
                    (int) NPCPerception.MAX_VIEW_ANGLE);
                gController.Perception.PerceptionRadius = EditorGUILayout.Slider(label_PerceptionRadius,  gController.Perception.PerceptionRadius, 
                    (int) NPCPerception.MIN_PERCEPTION_FIELD, 
                    (int) NPCPerception.MAX_PERCEPTION_FIELD);
                gController.Perception.PerceptionWeight = EditorGUILayout.FloatField(
                    label_PerceptionPercpWeight, 
                    gController.Perception.PerceptionWeight);
            }

            /* AI */
            gShowAI = EditorGUILayout.Foldout(gShowAI, "AI") && gController.AI != null;
            if(gShowAI) {
                if(gController.AI.Pathfinders != null) {
                    gController.AI.PathRecalculationTime = EditorGUILayout.FloatField(label_AIPathRecalculationTime, gController.AI.PathRecalculationTime);
                    string[] pfds = new string[gController.AI.Pathfinders.Count];
                    gController.AI.Pathfinders.Keys.CopyTo(pfds, 0);
                    selectedPathfinder = 0;
                    for (int i = 0; i < pfds.Length; ++i) { 
                        if (pfds[i] == gController.AI.SelectedPathfinder)
                            selectedPathfinder = i;
                    }
                    if (gController.AI.Pathfinders.ContainsKey(pfds[selectedPathfinder])) {
                        selectedPathfinder = EditorGUILayout.Popup("Pathfinders", selectedPathfinder, pfds);
                        gController.AI.SelectedPathfinder = pfds[selectedPathfinder];
                    } else {
                        gController.AI.SelectedPathfinder = pfds[0];
                    }
                    if (gController.Body.Navigation == NAV_STATE.STEERING_NAV) {
                        gController.AI.NavMeshAgentPathfinding = EditorGUILayout.Toggle(label_NavMeshAgentPathfinding, gController.AI.NavMeshAgentPathfinding);
                        if(gController.AI.NavMeshAgentPathfinding)
                            gController.AI.SelectedPathfinder = pfds[0];
                    } else {
                        gController.AI.NavMeshAgentPathfinding = false;
                    }
                }
                   
            }

            /* Body */
            gShowBody = EditorGUILayout.Foldout(gShowBody, "Body") && gController.Body != null;
            if (gShowBody) {

                // Navigation
                gController.Body.LocoControlMode = (LOCO_CONTROL) EditorGUILayout.EnumPopup(label_BodyLocoControlMode, gController.Body.LocoControlMode);
                gController.Body.LocomotionBlendDelay = EditorGUILayout.Slider(label_BodyLocomotionBlendDelay, gController.Body.LocomotionBlendDelay, 0f, 5f);
                gController.Body.TurningBlendDelay = EditorGUILayout.Slider(label_BodyTurningBlendDelay, gController.Body.TurningBlendDelay, 0f, 5f);
                gController.Body.JumpHeight = EditorGUILayout.Slider(label_BodyJumpHeight, gController.Body.JumpHeight, 0.8f, 100f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(label_BodyNavigation);
                if (gController.Body.Navigation != NAV_STATE.DISABLED) {
                    gController.Body.NavDistanceThreshold = EditorGUILayout.FloatField(label_NavStoppingThresh, gController.Body.NavDistanceThreshold);
                    gController.Body.NavDistanceThreshold = gController.Body.NavDistanceThreshold < 0.08f ? 0.1f : gController.Body.NavDistanceThreshold;
                }
                gController.Body.Navigation = (NAV_STATE)EditorGUILayout.EnumPopup((NAV_STATE)gController.Body.Navigation);
                gController.Body.TurningAngle = EditorGUILayout.Slider(label_BodyTurningAngle, gController.Body.TurningAngle,
                    gController.Body.MinTurningAngle,
                    gController.Body.MaxTurningAngle);
                EditorGUILayout.EndVertical();

                // Obstacles detection
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gController.Body.ObstaclesDetectionEnabled = EditorGUILayout.Toggle(label_StepObstacleDetection, gController.Body.ObstaclesDetectionEnabled);
                gController.Body.UseSingleCapsuleCollider = EditorGUILayout.Toggle(label_BodyUseSingleCapsuleCollider, gController.Body.UseSingleCapsuleCollider);
                if (gController.Body.ObstaclesDetectionEnabled) {
                    stairsClimbingForceFactor = gController.Body.StairsClimbingForceFactor = EditorGUILayout.Slider(label_BodyStairsClimbingForceFactor, gController.Body.StairsClimbingForceFactor, 0f, 2f);
                    heightAdjustInterpolation = gController.Body.HeightAdjustInterpolation = EditorGUILayout.FloatField(label_BodyHeightAdjustInterpolation, gController.Body.HeightAdjustInterpolation);
                    stepHeight = gController.Body.StepHeight = EditorGUILayout.FloatField(label_StepHeight, gController.Body.StepHeight);
                    maxStepHeight = gController.Body.MaxStepHeight = EditorGUILayout.FloatField(label_MaxStepHeight, gController.Body.MaxStepHeight);
                    colliderRadiusCorrection = gController.Body.ColliderRadiusCorrection = EditorGUILayout.Slider(label_BodyColliderRadiusCorrection,
                        gController.Body.ColliderRadiusCorrection,
                        0f,
                        1f);
                    stepColliderHeightCorrection = gController.Body.StepColliderHeightCorrection = EditorGUILayout.Slider(label_BodyStepHeightCorrection,
                        gController.Body.StepColliderHeightCorrection,
                        0f,
                        1f);
                    colliderTestDistance = gController.Body.ColliderTestDistance = EditorGUILayout.Slider(label_BodyColliderTestDistance,
                        gController.Body.ColliderTestDistance,
                        0f,
                        1f);
                    colliderTestAngle = gController.Body.ColliderTestAngle = EditorGUILayout.Slider(label_BodyColliderTestAngle,
                        gController.Body.ColliderTestAngle,
                        -2f,
                        2f);
                }
                EditorGUILayout.EndVertical();
                
                // IK
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gController.Body.IKEnabled = EditorGUILayout.Toggle(label_IKEnabled, gController.Body.IKEnabled);
                if (gController.Body.IKEnabled) {
                    gController.Body.IK_START_LOOK_AT_ADJUST = EditorGUILayout.Slider(label_IKLookStartAdjust, gController.Body.IK_START_LOOK_AT_ADJUST, 0.1f, 1f);
                    gController.Body.IK_LOOK_AT_SMOOTH = EditorGUILayout.Slider(label_IKLookSmoothness, gController.Body.IK_LOOK_AT_SMOOTH, 1f, 10f);
                    gController.Body.MaxArmsReach = EditorGUILayout.Slider(label_IKMaxArmsReach, gController.Body.MaxArmsReach, 0f, 10f);
                    gController.Body.IK_HANDS_SMOOTH = EditorGUILayout.Slider(label_IKHandsSmoothing, gController.Body.IK_HANDS_SMOOTH, 0.1f,3f);
                    gController.Body.IK_USE_HINTS = EditorGUILayout.Toggle(label_IKUseHints, gController.Body.IK_USE_HINTS);
                    gController.Body.IK_FEET_Enabled = EditorGUILayout.Toggle(label_IKFeetEnabled, gController.Body.IK_FEET_Enabled);
                    if (gController.Body.IK_FEET_Enabled) {
                        gController.Body.IK_FEET_HEIGHT_CORRECTION = EditorGUILayout.Slider(label_IKFeetHeight, gController.Body.IK_FEET_HEIGHT_CORRECTION, 0f, 0.5f);
                        gController.Body.IK_FEET_FORWARD_CORRECTION = EditorGUILayout.Slider(label_IKFeetForward, gController.Body.IK_FEET_FORWARD_CORRECTION, -0.5f, 0.5f);
                        gController.Body.IK_FEET_HEIGHT_EFFECTOR_CORRECTOR = EditorGUILayout.Slider(label_IKFeetEffectorCorrector, gController.Body.IK_FEET_HEIGHT_EFFECTOR_CORRECTOR, 0f, 0.3f);
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gController.Body.EnableSocialForces = EditorGUILayout.Toggle(label_UseSocialForces, gController.Body.EnableSocialForces);
                if (gController.Body.EnableSocialForces) {
                    agentAttractionForce = gController.Body.AgentAttractionForce = EditorGUILayout.FloatField(label_BodySFAgentsAttractionForce, gController.Body.AgentAttractionForce);
                    agentRepulsionWeight = gController.Body.AgentRepulsionWeight = EditorGUILayout.FloatField(label_AgentRepulsionWeight, gController.Body.AgentRepulsionWeight);
                    distanceTolerance = gController.Body.DistanceTolerance = EditorGUILayout.FloatField(label_AgentRepulsionDistanceTolerance, gController.Body.DistanceTolerance);
                    proximityScaleMultiplier = gController.Body.ProximityScaleMultiplier = EditorGUILayout.FloatField(label_AgentProximityScaleMultiplier, gController.Body.ProximityScaleMultiplier);
                    followAgentsFlow = gController.Body.FollowAgentsFlow = EditorGUILayout.FloatField(label_BodySFMatchAgentsFlow, gController.Body.FollowAgentsFlow);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(label_BodyEnableVelocityModifier);
                    enableVelocityModifier = gController.Body.EnableVelocityModifier = EditorGUILayout.Toggle(gController.Body.EnableVelocityModifier);
                    velocityModifierScale = gController.Body.VelocityModifierScale = EditorGUILayout.FloatField(label_BodyVelocityModifierFactor, gController.Body.VelocityModifierScale);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }

            /* Animations */
            gShowAnimationsSection = EditorGUILayout.Foldout(gShowAnimationsSection, "Animations");
            
            if (gShowAnimationsSection) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gController.Body.EnableBlinking = EditorGUILayout.Toggle(label_EnableBlinking, gController.Body.EnableBlinking);
                if(gController.Body.EnableBlinking) {
                    gController.Body.BlinkingSpeed = EditorGUILayout.FloatField(label_BlinkingSpeed, gController.Body.BlinkingSpeed);
                }
                gController.Body.UseAnimatorController = EditorGUILayout.Toggle(label_AnimatorEnabled, gController.Body.UseAnimatorController);
                gController.Body.UseCurvesAndParams = EditorGUILayout.Toggle(label_UseAnimCurves, gController.Body.UseCurvesAndParams);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                gController.UseAnimatedClips = EditorGUILayout.Toggle(label_UseAduioClips, gController.UseAnimatedClips);

                if (gController.UseAnimatedClips) {

                    if (gController.AnimatedAudioClips == null)
                        gController.AnimatedAudioClips = new NPCAnimatedAudio[0];

                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatedAudioClips").FindPropertyRelative("Array.size"), true);
                    
                    for (int i = 0; i < gController.AnimatedAudioClips.Length; ++i) {
                        NPCAnimatedAudio au = gController.AnimatedAudioClips[i];
                        if (au == null) {
                            gController.AnimatedAudioClips[i] = au = ScriptableObject.CreateInstance<NPCAnimatedAudio>();
                        }
                        EditorGUILayout.LabelField("Clip " + (i+1));
                        SerializedObject so = new SerializedObject(au);
                        // so.Update();
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(so.FindProperty("Name"));
                        EditorGUILayout.PropertyField(so.FindProperty("Type"));
                        EditorGUILayout.PropertyField(so.FindProperty("Clips"),true);

                        if (au.Type == ANIMATED_AUDIO_TYPE.SPEECH)
                            EditorGUILayout.PropertyField(so.FindProperty("Animations"), new GUIContent("Phonemes"), true);
                        else EditorGUILayout.PropertyField(so.FindProperty("Animations"), true);
                        
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(so.FindProperty("RandomizeAnimations"));
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                        so.ApplyModifiedProperties();
                    }
                    EditorGUI.indentLevel--;

                } else gController.AnimatedAudioClips = null;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            foreach (Object o in targets) {
                NPCController c = (NPCController) o;
                c.DebugMode = gController.DebugMode;
                c.UpdateTime = gController.UpdateTime;
                c.Perception.PerceptionRadius = gController.Perception.PerceptionRadius;
                c.AI.PathRecalculationTime = gController.AI.PathRecalculationTime;

                // Social Forces
                if(c.Body.EnableSocialForces) {
                    c.Body.AgentAttractionForce = agentAttractionForce;
                    c.Body.AgentRepulsionWeight = agentRepulsionWeight;
                    c.Body.DistanceTolerance = distanceTolerance;
                    c.Body.ProximityScaleMultiplier = proximityScaleMultiplier;
                    c.Body.FollowAgentsFlow = followAgentsFlow;
                    c.Body.EnableVelocityModifier = enableVelocityModifier;
                    c.Body.VelocityModifierScale = velocityModifierScale;
                }
                
                // Obstacles Detection
                if (c.Body.ObstaclesDetectionEnabled) {
                    c.Body.StairsClimbingForceFactor = stairsClimbingForceFactor;
                    c.Body.HeightAdjustInterpolation = heightAdjustInterpolation;
                    c.Body.StepHeight = stepHeight;
                    c.Body.MaxStepHeight = maxStepHeight;
                    c.Body.ColliderRadiusCorrection = colliderRadiusCorrection;
                    c.Body.StepColliderHeightCorrection = stepColliderHeightCorrection;
                    c.Body.ColliderTestDistance = colliderTestDistance;
                    c.Body.ColliderTestAngle = colliderTestAngle;
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(gController, "Parameter Changed");
                EditorUtility.SetDirty(gController);
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        private void OnSceneGUI() {

            if (gController != null && gController.DebugMode) {

                if (gShowPerception) {

                    Transform t = gController.Perception.PerceptionField.transform;

                    /* Draw View Angle */
                    float angleSplit = gController.Perception.ViewAngle / 2;
                    Debug.DrawRay(t.position,
                        Quaternion.AngleAxis(angleSplit, Vector3.up) * t.rotation * Vector3.forward * gController.Perception.PerceptionRadius * t.lossyScale.z, Color.red);
                    Debug.DrawRay(t.position,
                        Quaternion.AngleAxis((-1) * angleSplit, Vector3.up) * t.rotation * Vector3.forward * gController.Perception.PerceptionRadius * t.lossyScale.z, Color.red);
                }

                if (gController.DebugMode && gController.Body.ObstaclesDetectionEnabled) {
                    Vector3 heightCorrection = (Vector3.up * gController.Body.StepColliderHeightCorrection);
                    Color c = Color.blue;
                    RaycastHit rayHit;
                    if (Physics.Raycast(gController.transform.position + heightCorrection + (gController.transform.forward * gController.Body.ColliderRadiusCorrection),
                        (gController.transform.forward + Vector3.down * gController.Body.ColliderTestAngle), out rayHit, gController.Body.ColliderTestDistance)) {
                        c = Color.red;
                    }
                    Debug.DrawRay(gController.transform.position + heightCorrection + (gController.transform.forward * gController.Body.ColliderRadiusCorrection),
                    (gController.transform.forward + Vector3.down * gController.Body.ColliderTestAngle), c);
                }

                if (gController.Body.EnableSocialForces) {
                    Vector3 pos = gController.transform.position + Vector3.up;
                    Debug.DrawRay(
                        pos, 
                        (((gController.Body.TargetLocation + Vector3.up) - pos).normalized) * gController.Body.Speed,
                        Color.yellow);
                }

            }
            
        }

    }

}
