using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using YieldProlog;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    
    public class NPCAI: MonoBehaviour {
        
        #region NPC_Modules
        private NPCController gNPCController;
        #endregion

        #region NPC_Goals
        private Stack<NPCGoal> gGoals;
        #endregion

        #region Members
        
        public NPCNode BehaviorTree;

        [SerializeField, HideInInspector]
        private Dictionary<string, NPCAttribute> gAttributes;

        [SerializeField, HideInInspector]
        private Dictionary<string, INPCPathfinder> gPathfinders;

        [HideInInspector]
        public float g_NextRecalculateTime;

        private bool g_GestureRunning = false;

        #endregion

        #region Properties

        public BEHAVIOR_STATUS Status {
            get {
                return
                    BehaviorTree == null ?
                    BEHAVIOR_STATUS.INACTIVE :
                    BehaviorTree.Status;
            }
        }

        public bool Finished {
            get {
                return BehaviorTree.Finished;
            }
        }

        [SerializeField]
        public string SelectedPathfinder = "None";

        [SerializeField]
        public bool NavMeshAgentPathfinding = false;

        [SerializeField]
        public INPCPathfinder CurrentPathfinder;

        [SerializeField, HideInInspector]
        private UnityEngine.AI.NavMeshAgent gNavMeshAgent;

        public Dictionary<string,INPCPathfinder> Pathfinders {
            get {
                if (gPathfinders == null) InitPathfinders();
                return gPathfinders;
            }
        }

        [SerializeField]
        public float PathRecalculationTime;

        public bool PathUpdateable {
            get {
                if(Time.time >= g_NextRecalculateTime) {
                    g_NextRecalculateTime = Time.time + PathRecalculationTime;
                    return true;
                } return false;
            }
        }

        #endregion

        #region Unity_Methods

        void Reset() {
            this.gNPCController = gameObject.GetComponent<NPCController>();
            InitPathfinders();
        }

        #endregion

        #region Public_Functions

        public void StartBehavior() {
            if (Status != BEHAVIOR_STATUS.INACTIVE)
                BehaviorTree.Start();
        }

        public void StopBehavior() {
            if (Status != BEHAVIOR_STATUS.INACTIVE)
                BehaviorTree.Stop();
        }

        public void AddBehavior(NPCNode tree) {
            if (Status == BEHAVIOR_STATUS.INACTIVE) {
                BehaviorTree = tree;
            } else {
                // TODO - check status of behavior here
            }
        }
        
        /// <summary>
        /// Only to be called in editor to replace the preloaded instance of the behavior tree.
        /// </summary>
        /// <param name="node"></param>
        public void LoadBehavior(NPCNode node) {
            if(!Application.isPlaying) {
                BehaviorTree = node;
            }
        }

        public void UpdateBehavior() {
            if (Status != BEHAVIOR_STATUS.INACTIVE) {
                if (Status == BEHAVIOR_STATUS.PENDING)
                    BehaviorTree.Start();
                else if (Status == BEHAVIOR_STATUS.FAILURE)
                    Debug.LogError("Tree execution failed");
                else
                    BehaviorTree.UpdateNode();
            }
        }

        /// <summary>
        /// Update behaviors and AI related state
        /// </summary>
        public void UpdateAI() {
            UpdateBehavior();
        }
        
        public void InitializeAI() {
            gNPCController = GetComponent<NPCController>();
            CurrentPathfinder = Pathfinders[SelectedPathfinder];
            g_NextRecalculateTime = Time.time + PathRecalculationTime;
            InitBehaviorTree();
        }

        public static Dictionary<string, MethodInfo> GetAffordances() {
            Dictionary<string, MethodInfo> affordances = new Dictionary<string, MethodInfo>();
            // Pick up component's affordances
            foreach (MethodInfo method in typeof(NPCAI).GetMethods()) {
                if (Attribute.IsDefined(method, typeof(NPCAffordance))) {
                    NPCAffordance aff = (NPCAffordance) Attribute.GetCustomAttribute(method, typeof(NPCAffordance));
                    affordances.Add(aff.Name, method);
                }
            }
            // Look for affordances in modules - this does not guarantees an affordance will be usable if the
            // module hasn't been added to the NPCController instance
            foreach (Type mytype in Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(INPCModule)))) {
                foreach(MethodInfo method in mytype.GetMethods()) {
                    if (Attribute.IsDefined(method, typeof(NPCAffordance))) {
                        NPCAffordance aff = (NPCAffordance)Attribute.GetCustomAttribute(method, typeof(NPCAffordance));
                        affordances.Add(aff.Name, method);
                    }
                }
            }
            return affordances;
        }

        public void SetNPCModule(INPCModule mod) {
            if (gPathfinders == null) InitPathfinders();
            switch(mod.NPCModuleType()) {
                case NPC_MODULE_TYPE.PATHFINDER:
                    gPathfinders.Add(mod.NPCModuleName(),mod as INPCPathfinder);
                    break;
            }
        }

        public List<Vector3> FindPath(Vector3 target) {
            List<Vector3> path = new List<Vector3>();
            if (NavMeshAgentPathfinding && PathUpdateable) {
                UnityEngine.AI.NavMeshPath navMeshPath = new UnityEngine.AI.NavMeshPath();
                UnityEngine.AI.NavMesh.CalculatePath(transform.position, target, UnityEngine.AI.NavMesh.AllAreas, navMeshPath);
                if (navMeshPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
                    path.AddRange(navMeshPath.corners);
                }
                return path;
            } else if (CurrentPathfinder == null) {
                path.Add(target);
                return path;
            } else {
                return CurrentPathfinder.FindPath(gNPCController.transform.position, target);
            }
        }
        #endregion

        #region Private_Functions

        private void InitPathfinders() {
            gPathfinders = new Dictionary<string, INPCPathfinder>();
            gPathfinders.Add("None", null);
            gNavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        private void InitBehaviorTree() {
            // Load from asset?
            if (BehaviorTree != null) {
                BehaviorTree = LoadTree(BehaviorTree);
            }
        }

        /// <summary>
        /// Loads an asset tree from memory into a runtime copy
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private NPCNode LoadTree(NPCNode node, NPCBlackboard bb = null) {
            NPCNode parent = Instantiate(node);
            parent.Children.Clear();
            if (node.Blackboard != null && node.Blackboard.Root == node)
                bb = node.Blackboard;
            foreach (NPCNode n in node.Children) {
                NPCNode child = LoadTree(n, bb);
                child.Blackboard = bb;
                parent.AddChild(child);
            }
            bb = null;
            parent.SetMainAgent(gNPCController);
            return parent;
        }



        #endregion

        #region Behaviors
        
        [NPCAffordance("Go To")]
        public BEHAVIOR_STATUS Behavior_GoToPosition(Transform Position, bool Run = false) {
            return Behavior_GoTo(Position.position, Run);
        }

        [NPCAffordance("Go To Object")]
        public BEHAVIOR_STATUS Behavior_GoToObjectInteractionPoint(Transform Object, bool Run) {
            NPCObject obj = Object.GetComponent<NPCObject>();
            if (obj != null) {
                Transform point = obj.MainInteractionPoint;
                return Behavior_GoTo(point.position, Run);
            } else
                return BEHAVIOR_STATUS.FAILURE;
        }

        [NPCAffordance("Do Gesture")]
        public BEHAVIOR_STATUS Behavior_DoGesture(GESTURE_CODE Gesture, System.Object Value = null, bool Timed = false) {
            if (gNPCController.Body.IsTimedGesturePlaying(Gesture)) {
                return BEHAVIOR_STATUS.RUNNING;
            } else if (g_GestureRunning) {
                g_GestureRunning = false;
                return BEHAVIOR_STATUS.SUCCESS;
            } else {
                try {
                    gNPCController.Body.DoGesture(Gesture, Value, Timed);
                    g_GestureRunning = true;
                    return BEHAVIOR_STATUS.RUNNING;
                } catch (System.Exception e) {
                    gNPCController.Debug("Could not initialize gesture with error: " + e.Message);
                    return BEHAVIOR_STATUS.FAILURE;
                }
            }

        }
        
        [NPCAffordance("Override Speed")]
        public BEHAVIOR_STATUS Behavior_OverrideSpeed(float Walk, float Run) {
            gNPCController.Body.OverrideMaxSpeedValues(true, Walk, Run);
            return BEHAVIOR_STATUS.SUCCESS;
        }

        [NPCAffordance("Remove Speed Override")]
        public BEHAVIOR_STATUS Behavior_RemoveSpeedOverride(float Walk, float Run) {
            gNPCController.Body.OverrideMaxSpeedValues(false);
            return BEHAVIOR_STATUS.SUCCESS;
        }

        [NPCAffordance("Orient Towards")]
        public BEHAVIOR_STATUS Behavior_OrientTowards(Transform Position, bool Transform, bool MatchFwd, bool Negate) {
            return Behavior_OrientTo(Position.forward, Transform, MatchFwd, Negate);
        }
        
        [NPCAffordance("Go To Random Point")]
        public BEHAVIOR_STATUS Behavior_GoToRandomPoint(Transform PointsParent, bool Run = false) {
            List<Transform> points = new List<Transform>();
            foreach (Transform t in PointsParent) {
                points.Add(t);
            }
            return Behavior_GoToRandomPoints(points.ToArray(), Run);
        }
        
        [NPCAffordance("Look At")]
        public BEHAVIOR_STATUS Behavior_LookAt(Transform Target) {
            gNPCController.Body.StartLookAt(Target);
            return BEHAVIOR_STATUS.SUCCESS;
        }

        [NPCAffordance("Stop Look At")]
        public BEHAVIOR_STATUS Behavior_StopLookAt() {
            gNPCController.Body.StopLookAt();
            return BEHAVIOR_STATUS.SUCCESS;
        }

        [NPCAffordance("Go To At Distance")]
        public BEHAVIOR_STATUS Behavior_GoToDistance(Transform Position , float Distance, bool Run = false) {
            float currentDistance = Vector3.Distance(transform.position, Position.position);
            if (currentDistance <= Distance || gNPCController.Body.IsAtTargetLocation(Position.position)) {
                gNPCController.Body.StopNavigation();
                return BEHAVIOR_STATUS.SUCCESS;
            } else {
                try {
                    if (Run)
                        gNPCController.Body.RunTo(Position.position);
                    else gNPCController.Body.GoTo(Position.position);
                    return BEHAVIOR_STATUS.RUNNING;
                } catch {
                    return BEHAVIOR_STATUS.FAILURE;
                }
            }
        }

        [NPCAffordance("Wander At Points")]
        public BEHAVIOR_STATUS Behavior_Wander(Transform PointsParent, bool Run) {
            List<Transform> ps = new List<Transform>();
            foreach (Transform t in PointsParent) {
                ps.Add(t);
            }
            Transform[] points = ps.ToArray();
            if (!gNPCController.Body.Navigating) {
                try {
                    Transform t = points[(int)(UnityEngine.Random.value * (points.Length - 1))];

                    Vector3 targetPosition = t.position;
                    float rand = UnityEngine.Random.value;

                    if (rand < 0.33f) {
                        targetPosition -= transform.right * gNPCController.Body.NavDistanceThreshold;
                    } else if (rand < 0.66f) {
                        targetPosition -= transform.forward * gNPCController.Body.NavDistanceThreshold;
                    } else {
                        targetPosition += transform.right * gNPCController.Body.NavDistanceThreshold;
                    }

                    if (rand < .65f && Mathf.Abs(targetPosition.y - transform.position.y) > 1.5f)
                        targetPosition = transform.position;

                    if (Run)
                        gNPCController.Body.RunTo(targetPosition);
                    else gNPCController.Body.GoTo(targetPosition);
                    return BEHAVIOR_STATUS.RUNNING;
                } catch (System.Exception e) {
                    gNPCController.Debug(e.Message);
                    return BEHAVIOR_STATUS.FAILURE;
                }
            } else { return BEHAVIOR_STATUS.SUCCESS; }
        }

        [NPCAffordance("Grab Right Hand")]
        public BEHAVIOR_STATUS Behavior_GrabRightHand(NPCObject Object, bool Grab = true) {
            gNPCController.Body.GrabRightHand(Object, Grab);
            return BEHAVIOR_STATUS.SUCCESS;
        }

        public BEHAVIOR_STATUS Behavior_OrientTo(Vector3 Position, bool Transform, bool MatchFwd = false, bool Negate = false) {
            Vector3 target = Vector3.zero;
            if (Transform) target = (Position - transform.position).normalized;
            else if (MatchFwd) target = Position;
            else target = (Position * -1).normalized;
            target = (Negate ? -1 : 1) * target;
            gNPCController.Body.OrientTowards(new Vector3(target.x, 0, target.z));
            return gNPCController.Body.Oriented ? BEHAVIOR_STATUS.SUCCESS : BEHAVIOR_STATUS.RUNNING;
        }

        public BEHAVIOR_STATUS Behavior_GoToRandomPoints(Transform[] Points, bool Run = false) {
            if (gNPCController.Body.Navigating) {
                return Behavior_GoTo(gNPCController.Body.TargetLocation, Run);
            } else {
                Transform t = Points[(int)(UnityEngine.Random.value * (Points.Length - 1))];
                if (Points.Length == 1 && Vector3.Distance(gNPCController.GetPosition(), t.position) < 1f)
                    return BEHAVIOR_STATUS.SUCCESS;
                return Behavior_GoToPosition(t, Run);
            }
        }

        public BEHAVIOR_STATUS Behavior_GoTo(Vector3 Position, bool Run) {
            if (gNPCController.Body.IsAtTargetLocation(Position)) {
                return BEHAVIOR_STATUS.SUCCESS;
            } else {
                try {
                    if (Run)
                        gNPCController.Body.RunTo(Position);
                    else gNPCController.Body.GoTo(Position);
                    return BEHAVIOR_STATUS.RUNNING;
                } catch {
                    return BEHAVIOR_STATUS.FAILURE;
                }
            }
        }

        [NPCAffordance("Hide")]
        public BEHAVIOR_STATUS GoToHidingSpot(Transform levelController)
        {
            Debug.Log("hiding affordance activated");
            List<Transform> hidingSpots = levelController.GetComponent<LevelController>().GetHidingSpots();
            UnityEngine.AI.NavMeshAgent agent = gNPCController.AI.gNavMeshAgent;
            System.Random rand = new System.Random();
            int index = rand.Next(hidingSpots.Count);
            //Debug.Log(hidingSpots[index].position);
            agent.SetDestination(hidingSpots[index].position);
            agent.speed = agent.speed * 3;
            return BEHAVIOR_STATUS.SUCCESS;
        }

        [NPCAffordance("Wander Around")]
        public BEHAVIOR_STATUS WanderAround(Transform levelController)
        {
            Debug.Log("Wander around Affordance activated");
            UnityEngine.AI.NavMeshAgent agent = gNPCController.AI.gNavMeshAgent;
            LevelController lc = levelController.GetComponent<LevelController>();
            //AgentInfo info = agentInfo.GetComponent<AgentInfo>();
            List<Transform> wanderingSpots = lc.GetWanderingSpots();

            if (wanderingSpots.Count == 0)
            {
                return BEHAVIOR_STATUS.SUCCESS;
            }

            //NOTE: only use unity random, NOT system random
            int index = UnityEngine.Random.Range(0, wanderingSpots.Count);
            Debug.Log("Going to " + wanderingSpots[index].position);
            Vector3 newPosition = wanderingSpots[index].position;
            float offsetXRange = UnityEngine.Random.Range(-1, 1);
            float offsetZRange = UnityEngine.Random.Range(-1, 1);
            newPosition = new Vector3(newPosition.x + offsetXRange, newPosition.y, newPosition.z + offsetZRange);
            agent.SetDestination(newPosition);

            return BEHAVIOR_STATUS.SUCCESS;
        }

        #endregion

        #region Traits

        /* For the purpose of initialization */
        private bool gRandomizeTraits;

        bool RandomizeTraits {

            get {
                return gRandomizeTraits;
            }
            set {
                gRandomizeTraits = value;
            }
        }

        protected void InitializeTraits() {
            foreach (PropertyInfo pi in this.GetType().GetProperties()) {
                object[] attribs = pi.GetCustomAttributes(true);
                if(attribs.Length > 0) {

                }
            }
        }

        #endregion

        #region Attributes
    
        [NPCAttribute("NPC",typeof(bool))]
        public bool NPC { get; set; }

        [NPCAttribute("Fear", typeof(float))]
        public float Fear { get; set; }

        [NPCAttribute("Charisma",typeof(float))]
        public float Charisma { get; set; }

        [NPCAttribute("Friendliness",typeof(float))]
        public float Friendliness { get; set; }
    
        [NPCAttribute("Strength",typeof(int))]
        public int Strength { get; set; }

        [NPCAttribute("Intelligence",typeof(int))]
        public int Intelligence { get; set; }

        [NPCAttribute("Dexterity",typeof(int))]
        public int Dexterity { get; set; }

        [NPCAttribute("Constitution",typeof(int))]
        public int Constitution { get; set; }

        [NPCAttribute("Hostility", typeof(float))]
        public float Hostility { get; set; }

        [NPCAttribute("Location",typeof(Vector3))]
        public Vector3 Location { get; set; }

        #endregion
        
    }
}