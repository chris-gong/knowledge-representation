using UnityEngine;
using System;
using System.Collections.Generic;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public enum AGENT_TYPE {
        MALE,
        FEMALE,
        ANIMAL,
        BEAST,
        OTHER
    }

    public class NPCController : MonoBehaviour, INPCPerceivable {

        #region Members

        private string g_CurrentContext;

        [SerializeField]
        private NPCAI gAI;

        [SerializeField]
        private NPCBody gBody;

        [SerializeField]
        private NPCPerception gPerception;

        [SerializeField]
        Dictionary<string, INPCModule> g_NPCModules;
        
        [SerializeField]
        private bool gMainAgent = false;

        [SerializeField]
        private bool gInitialized = false;

        private bool g_Selected;
        private float g_NextUpdateTime;
        
        private HashSet<string> g_NPCTags;
        private AudioSource g_AgentAudioSource;

        #endregion

        #region Properties
        
        [SerializeField]
        public bool UseAnimatedClips;

        [SerializeField]
        public bool TestNPC;

        [SerializeField]
        public float UpdateTime = 0.01f;

        [SerializeField]
        public Transform TestTargetLocation;

        public HashSet<INPCPerceivable> PerceivedEntities {
            get {
                return gPerception.PerceivedEntities;
            }
        }

        public HashSet<INPCPerceivable> PerceivedAgents {
            get {
                return gPerception.PerceivedAgents;
            }
        }

        public Material MainMaterial {
            get {
                return GetComponentInChildren<Renderer>().sharedMaterial;
            }
        }

        public Renderer[] AllRenderers {
            get {
                return GetComponentsInChildren<Renderer>();
            }
        }

        [SerializeField]
        public bool DebugMode = true;
        
        public INPCModule[] NPCModules {
            get {
                if (g_NPCModules == null) return new INPCModule[0];
                INPCModule[] mods = new INPCModule[g_NPCModules.Count];
                g_NPCModules.Values.CopyTo(mods, 0);
                return mods;
            }
        }

        [SerializeField]
        public AGENT_TYPE AgentType;

        [SerializeField]
        public PERCEIVEABLE_TYPE EntityType;

        [SerializeField]
        public bool EnableAudio = true;

        [SerializeField]
        public string[] AgentTags;

        [SerializeField]
        public NPCPerception Perception {
            get { return gPerception; }
        }
        
        public NPCAI AI {
            get { return gAI; }
        }
        
        public NPCBody Body {
            get { return gBody; }
        }

        [SerializeField]
        public bool MainAgent {
            get { return gMainAgent; }
            set { gMainAgent = value; }
        }

        public Transform FollowTarget;

        public bool Following {
            get {
                return FollowTarget != null;
            }
        }

        [SerializeField]
        public NPCAnimatedAudio[] AnimatedAudioClips;

        #endregion

        #region Public_Functions
        
        public bool HasNPCTag(string tag) {
            return g_NPCTags.Contains(tag);
        }

        public void SetNPCTag(string tag) {
            if (!g_NPCTags.Contains(tag))
                g_NPCTags.Add(tag);
        }

        public void RemoveNPCTag(string tag) {
            if (g_NPCTags.Contains(tag))
                g_NPCTags.Remove(tag);
        }

        public void Debug(string msg, LOG_TYPE type) {
            if (DebugMode) {
                switch(type) {
                    case LOG_TYPE.INFO:
                        UnityEngine.Debug.Log(msg);
                        break;
                    case LOG_TYPE.ERROR:
                        UnityEngine.Debug.LogError(msg);
                        break;
                    case LOG_TYPE.WARNING:
                        UnityEngine.Debug.LogWarning(msg);
                        break;
                }
            }
        }

        public void Debug(string msg) {
            Debug(msg, LOG_TYPE.INFO);
        }

        public void DebugLine(Vector3 from, Vector3 to, Color c) {
            if (DebugMode) {
                UnityEngine.Debug.DrawLine(from, to, c);
            }
        }

        public void DebugRay(Ray ray, Color c) {
            DebugRay(ray.origin, ray.direction, c);
        }

        public void DebugRay(Vector3 from, Vector3 to, Color c) {
            if (DebugMode) {
                UnityEngine.Debug.DrawRay(from, to, c);
            }
        }

        public void PlayAudioClip(AudioClip clip) {
            g_AgentAudioSource.clip = clip;
            g_AgentAudioSource.Play();
        }

        public void StopAudioClip() {
            if (g_AgentAudioSource.clip != null)
                g_AgentAudioSource.Stop();
        }

        #region NPCEditor

        public void LoadNPCModules() {
            if (g_NPCModules == null)
                g_NPCModules = new Dictionary<string, INPCModule>();
            INPCModule[] modules = gameObject.GetComponents<INPCModule>();
            foreach (INPCModule m in modules) {
                if (!ContainsModule(m)) {
                    Debug("Loading NPC Module -> " + m.NPCModuleName());
                    if (Application.isPlaying) {
                        m.InitializeModule();
                    }
                    if (!AddNPCModule(m)) {
                        GameObject.DestroyImmediate((UnityEngine.Object)m);
                    }
                }
            }
        }

        public void RemoveNPCModule(INPCModule mod) {
            if (g_NPCModules.ContainsKey(mod.NPCModuleName()))
                g_NPCModules.Remove(mod.NPCModuleName());
        }

        public void SetSelected(bool sel) {
            g_Selected = sel;
        }

        public bool ContainsModule(INPCModule mod) {
            return g_NPCModules != null && g_NPCModules.ContainsKey(mod.NPCModuleName());
        }

        public bool AddNPCModule(INPCModule mod) {
            if (g_NPCModules == null) g_NPCModules = new Dictionary<string, INPCModule>();
            if (g_NPCModules.ContainsKey(mod.NPCModuleName())) return false;
            switch (mod.NPCModuleTarget()) {
                case NPC_MODULE_TARGET.AI:
                    gAI.SetNPCModule(mod);
                    break;
                case NPC_MODULE_TARGET.BODY:
                    break;
                case NPC_MODULE_TARGET.PERCEPTION:
                    break;
            }
            g_NPCModules.Add(mod.NPCModuleName(), mod);
            return true;
        }

        #endregion

        #endregion 

        #region Unity_Runtime

        void Awake () {

            // Initialize components
            Perception.InitializePerception();
            Body.InitializeBody();
            AI.InitializeAI();

            // Flag agent type
            switch(AgentType) {
                case AGENT_TYPE.FEMALE:
                    Body.DoGesture(GESTURE_CODE.FEMALE_FLAG, true);
                    break;
                case AGENT_TYPE.ANIMAL:
                    // TODO - add GESTURE flag
                    break;
                case AGENT_TYPE.BEAST:
                    // TODO - add GESTURE flag
                    break;
            }
            
            // Handle main agent flag
            SetSelected(MainAgent);

            // Set up controller update time
            g_NextUpdateTime = Time.time + UpdateTime;

            // handle agent's tags
            g_NPCTags = new HashSet<string>();
            
            foreach(string tag in AgentTags) {
                g_NPCTags.Add(tag);
            }

            // Add agent audio source
            if(EnableAudio) {
                AudioSource aSource = GetComponent<AudioSource>();
                if (aSource != null && aSource.name.Equals("Agent_Audio_Source")) {
                    g_AgentAudioSource = aSource;
                } else {
                    g_AgentAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // Generate individual materials
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
                r.sharedMaterial = Instantiate(r.material) as Material;
            }

            // Initialize all loaded modules
            LoadNPCModules();
        }
	
        void Update() {

            if (Time.time > g_NextUpdateTime) {

                g_NextUpdateTime += UpdateTime;

                gPerception.UpdatePerception();
                gBody.UpdateBody();
                gAI.UpdateAI();

                // Main NPC Modular updating call
                foreach (INPCModule m in g_NPCModules.Values) {
                    if (m.IsUpdateable()) {
                        m.TickModule();
                    }
                }
            }
        }
        
        void Reset() {
            if(!gInitialized) {
                g_NPCModules = new Dictionary<string, INPCModule>();
                Debug("Creating NPCController");
                gMainAgent = false;
                if (GetComponent<NPCBody>() != null) DestroyImmediate(GetComponent<NPCBody>());
                if (GetComponent<NPCPerception>() != null) DestroyImmediate(GetComponent<NPCPerception>());
                InitializeNPCComponents();
                gInitialized = true;
            } else {
                Debug("Loading existing NPCController settings");
            }
        }

        private void OnApplicationQuit() {
            foreach (INPCModule m in g_NPCModules.Values) {
                try {
                    m.CleanupModule();
                } catch (NotImplementedException nie) {
                    Debug(this + " - Error on application quit: " + nie.Message);
                }
            }
        }

        #endregion

        #region Private_Functions

        private void InitializeNPCComponents() {
            gAI = gameObject.AddComponent<NPCAI>();
            gPerception = gameObject.AddComponent<NPCPerception>();
            gBody = gameObject.AddComponent<NPCBody>();
            // hide flags
            gAI.hideFlags = HideFlags.HideInInspector;
            gBody.hideFlags = HideFlags.HideInInspector;
            gPerception.hideFlags = HideFlags.HideInInspector;
        }

        #endregion

        #region INPCPerceivable

        public GameObject GetGameObject() {
            return gameObject;
        }

        public bool HasTag(string tag) {
            return g_NPCTags.Contains(tag);
        }

        public void SetCurrentContext(string context) {
            g_CurrentContext = context;
        }

        public string GetCurrentContext() {
            return g_CurrentContext;
        }

        public float GetCurrentSpeed() {
            return gBody.Speed;
        }

        public virtual PERCEIVE_WEIGHT GetPerceptionWeightType() {
            return PERCEIVE_WEIGHT.WEIGHTED;
        }

        public virtual Transform GetTransform() {
            return this.transform;
        }

        public Vector3 GetCurrentVelocity() {
            return gBody.Velocity;
        }

        public virtual Vector3 GetPosition() {
            return transform.position;
        }

        public Vector3 GetForwardDirection() {
            return transform.forward;
        }

        public float GetAgentRadius() {
            return gBody.AgentRadius;
        }

        public virtual PERCEIVEABLE_TYPE GetNPCEntityType() {
            return PERCEIVEABLE_TYPE.NPC;
        }

        public virtual Transform GetMainLookAtPoint() {
            return gBody.Head;
        }

        public float GetPerceptionWeight() {
            return gPerception.PerceptionWeight;
        }

        #endregion

        #region Body Attributes

        [NPCAttribute("Hit Points", typeof(float))]
        public int Health { get; set; }

        [NPCAttribute("Stamina", typeof(float))]
        public int Stamina { get; set; }

        #endregion

    }

}
