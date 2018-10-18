using UnityEngine;
using System;
using System.Collections.Generic;
using NPC;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

[System.Serializable]
public class NavIncAStar : MonoBehaviour, INPCPathfinder, INPCModule {

    #region Members
    string g_Winner;
    private NPCController g_NPCController;
    private Vector3 g_TargetLocation;
    NavNode g_FromNode, g_GoalNode;
    HashSet<NavNode> g_ClosedListA;
    HashSet<NavNode> g_ClosedListH;
    HashSet<NavNode> g_OpenList;
    private SortedList<float, NavNode> g_Fringe;
    Dictionary<NavNode, float> g_gVal, g_fVal;
    Dictionary<NavNode, NavNode> g_Parents;
    private NavGrid g_Grid;
    private float g_CurrentPathValue = 0f;
    private int g_PathLengthNodes;
    List<Vector3> g_PathList;
    private List<NavIncAStar> g_Pathfinders;
    float g_SecondaryHeuristicsWeight;
    #endregion

    #region Public_Functions

    public virtual void InitializeFinder(NavNode from, Vector3 to, NavGrid grid) {
        g_Grid = grid;
        g_FromNode = from;
        g_TargetLocation = to;
        g_gVal = new Dictionary<NavNode, float>();
        g_fVal = new Dictionary<NavNode, float>();
        g_ClosedListA = new HashSet<NavNode>();
        g_ClosedListH = new HashSet<NavNode>();
        g_OpenList = new HashSet<NavNode>();
        g_Parents = new Dictionary<NavNode, NavNode>();
        g_Fringe = new SortedList<float, NavNode>(new NPCUtils.DuplicateKeyComparer<float>());
        foreach (NavIncAStar s in g_Pathfinders) {
            s.g_Grid = g_Grid;
            s.g_gVal = g_gVal;
            s.g_fVal = g_fVal;
            s.g_FromNode = g_FromNode;
            s.g_TargetLocation = g_TargetLocation;
            s.g_Parents = g_Parents;
            s.g_ClosedListA = g_ClosedListA;
            s.g_ClosedListH = g_ClosedListH;
            s.g_OpenList = new HashSet<NavNode>();
            s.g_Fringe = new SortedList<float, NavNode>(new NPCUtils.DuplicateKeyComparer<float>());
            s.g_Pathfinders = g_Pathfinders;
            s.g_Fringe.Add(s.ComputeNodeHeuristic(s.g_FromNode), s.g_FromNode);
            s.g_OpenList.Add(s.g_FromNode);
        }
    }

    public void ExpandNode(NavNode n) {

        foreach (NavIncAStar pf in g_Pathfinders) {
            pf.RemoveFromFringe(n);
            if(pf.DynamicWeight) {
                pf.HeuristicWeight += pf.DynamicWeightStep;
            }
        }

        RemoveFromFringe(n);
        
        // loop adjacent
        Dictionary<NavNode, GRID_DIRECTION> neighbors = g_Grid.GetNeighborNodes(n);
        foreach (NavNode neighbor in neighbors.Keys) {
            if (neighbor.IsWalkable()) {
                if (!g_gVal.ContainsKey(neighbor)) {
                    g_gVal.Add(neighbor, float.MaxValue);
                }
                float val = g_gVal[n] + ComputeNodeCost(n, neighbor, neighbors[neighbor]);
                if (g_gVal[neighbor] > val) {
                    g_gVal[neighbor] = val;
                    g_Parents.Remove(neighbor);
                    g_Parents.Add(neighbor, n);
                    if (!g_ClosedListA.Contains(neighbor)) {
                        if (g_OpenList.Contains(neighbor)) {
                            RemoveFromFringe(neighbor);
                        }
                        g_OpenList.Add(neighbor);
                        g_Fringe.Add(val, neighbor);
                        if(!g_ClosedListH.Contains(neighbor)) {
                            foreach (NavIncAStar pf in g_Pathfinders) {
                                float totVal = val + pf.ComputeNodeHeuristic(neighbor);
                                if(totVal <= (pf.g_SecondaryHeuristicsWeight * val)) {
                                    pf.RemoveFromFringe(neighbor);
                                    pf.g_OpenList.Add(neighbor);
                                    pf.g_Fringe.Add(totVal, neighbor);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public bool RemoveFromFringe(NavNode n) {
        if (g_OpenList.Contains(n)) {
            for (int q = 0; q < g_Fringe.Count; q++) {
                if (g_Fringe.Values[q].Equals(n)) {
                    g_OpenList.Remove(n);
                    g_Fringe.RemoveAt(q); return true;
                }
            }
        }
        return false;
    }

    public List<Vector3> FindPath(Vector3 from, Vector3 to) {
        ClearPath();
        foreach (NavIncAStar s in g_Pathfinders) {
            s.ClearPath();
        }
        RaycastHit hit;
        g_PathList = new List<Vector3>();

        if (Physics.Raycast(new Ray(transform.position + (transform.up * 0.2f), -1 * transform.up), out hit)) {
            // Ensure we are on a grid
            g_Grid = hit.collider.GetComponent<NavGrid>();
            if (CleanPathOnRestart)
                g_Grid.CleanAll();
            g_FromNode = g_FromNode == null ? g_Grid.GetOccupiedNode(this) : g_FromNode;
            if (g_FromNode == null) {
                g_NPCController.Debug("NavAStar --> Agent is currently navigating in between nodes, try again please");
                return g_PathList;
            }

            /* Initialize all heuristics here */

            InitializeFinder(g_FromNode, to, g_Grid);

            /* Prime the anchor */
            g_OpenList.Add(g_FromNode);
            g_Fringe.Add(0f, g_FromNode);
            g_gVal.Add(g_Fringe.Values[0], 0f);

            while (g_OpenList.Count > 0) {

                // Then Heuristic-based search
                foreach (NavIncAStar nav in g_Pathfinders) {
                    float hVal = nav.g_Fringe.Count > 0 ? nav.g_Fringe.Keys[0] : float.MaxValue;
                    if (hVal <= (nav.g_SecondaryHeuristicsWeight * g_gVal[g_Fringe.Values[0]])) {
                        if (IsCurrentStateGoal(new System.Object[] { nav.g_Fringe.Values[0] })) {
                            NavNode n = nav.g_Fringe.Values[0];
                            g_Winner = "Heuristic won: " + nav.Name();
                            n = nav.g_Fringe.Values[0];
                            n.SetHighlightTile(true, Color.green, 1f);
                            g_GoalNode = n;
                            g_PathList = ConstructPath(n, nav.g_Parents);
                            g_CurrentPathValue = nav.g_gVal[n];
                            goto exit_pathfinding;
                        } else {
                            NavNode s = nav.g_Fringe.Values[0];
                            s.SetHighlightTile(true, Color.white, 0.4f);
                            ExpandNode(s);
                            g_ClosedListH.Add(s);
                        }
                    } else if (IsCurrentStateGoal(new System.Object[] { g_Fringe.Values[0] })) {
                        NavNode n = g_Fringe.Values[0];
                        g_Winner = "Search won: " + nav.Name();
                        n.SetHighlightTile(true, Color.green, 1f);
                        g_GoalNode = n;
                        g_PathList = ConstructPath(g_GoalNode, g_Parents);
                        g_CurrentPathValue = g_gVal[n];
                        goto exit_pathfinding;
                    } else {
                        NavNode n = g_Fringe.Values[0];
                        n.SetHighlightTile(true, Color.white, 0.4f);
                        ExpandNode(n);
                        g_ClosedListA.Add(n);
                    }
                }
            }
        } else {
            g_NPCController.Debug("NavAStar --> Pathfinder not on grid");
        }
    exit_pathfinding:
        g_FromNode = null;
        return g_PathList;
    }

    public NavIncAStar() {
        g_Fringe = new SortedList<float, NavNode>(new NPCUtils.DuplicateKeyComparer<float>());
        g_ClosedListA = new HashSet<NavNode>();
        g_OpenList = new HashSet<NavNode>();
    }

    public void DryRunAlgorithm() {
        if (DryRunAlgo) {
            RaycastHit hit;
            List<Vector3> pathList = new List<Vector3>();
            if (Physics.Raycast(new Ray(transform.position + (transform.up * 0.2f), -1 * transform.up), out hit)) {
                g_Grid = hit.collider.GetComponent<NavGrid>();
                int succeed = 0;
                while (succeed < DryRunTests) {
                    int x = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1)),
                        y = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.y - 1));
                    g_FromNode = g_Grid.GetGridNode(x, y);
                    if (g_FromNode != null) {
                        NavNode to = null;
                        do {
                            x = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1));
                            y = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1));
                            to = g_Grid.GetGridNode(x, y);
                        } while (to == null);
                        g_FromNode.SetHighlightTile(true, Color.black, 1f);
                        FindPath(g_FromNode.Position, to.Position);
                        succeed++;
                    }
                }
            }
            g_NPCController.Debug("Finished algorithm dry run");
        }
        DryRunAlgo = false;
        g_FromNode = null;
    }

    public void Benchmark() {
        if (BenchmarkAll) {
            List<float> times = new List<float>();
            for (int r = 0; r < 5; r++) {
                float globalBenchMarkTime = Time.realtimeSinceStartup;
                Dictionary<string, int> hCounter = new Dictionary<string, int>();
                Dictionary<string, float> timeAvg = new Dictionary<string, float>();
                RaycastHit hit;
                List<Vector3> pathList = new List<Vector3>();
                if (Physics.Raycast(new Ray(transform.position + (transform.up * 0.2f), -1 * transform.up), out hit)) {
                    g_Grid = hit.collider.GetComponent<NavGrid>();
                    int succeed = 0;
                    while (succeed < DryRunTests) {
                        int x = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1)),
                            y = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.y - 1));
                        g_FromNode = g_Grid.GetGridNode(x, y);
                        if (g_FromNode != null) {
                            NavNode to = null;

                            do {
                                x = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1));
                                y = Mathf.RoundToInt(UnityEngine.Random.Range(0, g_Grid.GridDimensions.x - 1));
                                to = g_Grid.GetGridNode(x, y);
                            } while (to == null);

                            NavNode from = g_FromNode;

                            // benchmark each path
                            for (int i = 0; i < 10; ++i) {

                                string log = "";
                                float w2 = 1f;
                                if (i == 0) {
                                    foreach (NavIncAStar n in g_Pathfinders) {
                                        n.g_SecondaryHeuristicsWeight = w2;
                                    }
                                } else if (i == 1) {
                                    foreach (NavIncAStar n in g_Pathfinders) {
                                        w2 += 0.25f;
                                        n.g_SecondaryHeuristicsWeight = w2;
                                    }
                                } else if (i == 2) {
                                    foreach (NavIncAStar n in g_Pathfinders) {
                                        w2 += 0.5f;
                                        n.g_SecondaryHeuristicsWeight = w2;
                                    }
                                } else if (i == 3) {
                                    foreach (NavIncAStar n in g_Pathfinders) {
                                        w2 += 0.75f;
                                        n.g_SecondaryHeuristicsWeight = w2;
                                    }
                                } else {
                                    foreach (NavIncAStar n in g_Pathfinders) {
                                        w2 += 1f;
                                        n.g_SecondaryHeuristicsWeight = w2;
                                    }
                                }

                                float now = Time.realtimeSinceStartup;
                                from.SetHighlightTile(true, Color.black, 1f);
                                FindPath(from.Position, to.Position);
                                if (hCounter.ContainsKey(g_Winner)) {
                                    hCounter[g_Winner] += 1;
                                } else hCounter.Add(g_Winner, 1);
                                log += "W2: " + w2 + "\n";
                                float t = (Time.realtimeSinceStartup - now);
                                log += "Time: " + t + "ms \n";
                                if (timeAvg.ContainsKey(g_Winner)) {
                                    timeAvg[g_Winner] += t;
                                } else timeAvg.Add(g_Winner, t);
                                //log += "Expanded: " + g_Fringe.Count + 1 + "\n";
                                //log += "Generated: " + g_ClosedList.Count + "\n";
                                //log += "Path Value: " + g_CurrentPathValue + "\n";
                                //log += "Path Length: " + g_PathLengthNodes;
                                UnityEngine.Debug.Log(log);
                                g_CurrentPathValue = 0f;
                                g_PathLengthNodes = 0;
                            }
                            foreach (string s in hCounter.Keys) {
                                Debug.Log(s + " average time: " + timeAvg[s] / hCounter[s] + "\n");
                            }
                            succeed++;
                            timeAvg.Clear();
                            hCounter.Clear();
                        }

                    }
                }
                times.Add((Time.realtimeSinceStartup - globalBenchMarkTime));
            }
            foreach(float f in times) {
                Debug.Log(f);
            }
        }
        DryRunAlgo = false;
        g_FromNode = null;
    }

    // f(n) = g(n) + h(n)*e
    public float ComputeNodeCost(NavNode from, NavNode to, GRID_DIRECTION dir) {
        float totalCost = 0f;
        if (GRID_DIRECTION.CURRENT != dir) {
            NavNode.NODE_STATUS fromStatus = from.NodeStatus;
            switch (dir) {
                // diagonals
                case GRID_DIRECTION.NORTH_EAST:
                case GRID_DIRECTION.NORTH_WEST:
                case GRID_DIRECTION.SOUTH_EAST:
                case GRID_DIRECTION.SOUTH_WEST:
                    totalCost += Mathf.Sqrt(2);
                    if (from.NodeType == NavNode.NODE_TYPE.WALKABLE) {
                        totalCost = to.NodeType == NavNode.NODE_TYPE.HARD_TO_WALK ?
                            (Mathf.Sqrt(2f) + Mathf.Sqrt(8f)) / 2f :
                            Mathf.Sqrt(2);
                    } else if (from.NodeType == NavNode.NODE_TYPE.HARD_TO_WALK) {
                        totalCost = to.NodeType == NavNode.NODE_TYPE.HARD_TO_WALK ?
                            Mathf.Sqrt(8) :
                            Mathf.Sqrt(2);
                    }
                    if (to.NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY || to.NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY) {
                        totalCost -= (from.NodeType == NavNode.NODE_TYPE.WALKABLE) ?
                             (to.NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY ? 0.5f : 0.375f) :
                             (to.NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY ? 0.375f : 0.25f);
                    }
                    break;
                // straights
                default:
                    if (from.NodeType == NavNode.NODE_TYPE.WALKABLE) {
                        totalCost = to.NodeType == NavNode.NODE_TYPE.HARD_TO_WALK ?
                            (float)to.NodeType - 0.5f :
                            (float)to.NodeType;
                    } else if (from.NodeType == NavNode.NODE_TYPE.HARD_TO_WALK) {
                        totalCost = (float)to.NodeType - 0.5f;
                    }
                    // Highways
                    if (to.NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY ||
                        to.NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY) {
                        totalCost = (from.NodeStatus == NavNode.NODE_STATUS.REGULAR) ?
                            (to.NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY ? 0.25f : 0.375f) :
                            (to.NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY ? 0.5f : 0.375f);
                    }
                    break;
            }
        }
        return totalCost;
    }

    public float ComputeNodeHeuristic(NavNode n) {
        float totalCost = 0f;
        if (UseHeuristic) {
            // euclidean distance heuristic
            totalCost += Vector3.Distance(n.Position, g_TargetLocation)
                * (WeightHeuristic ? HeuristicWeight : 1f);
        }
        return totalCost;
    }

    public List<Vector3> ConstructPath(NavNode goal, Dictionary<NavNode, NavNode> parents) {
        List<Vector3> path = new List<Vector3>(parents.Count + 1);
        List<NavNode> pathToPrint = new List<NavNode>(parents.Count + 1);
        path.Insert(0, goal.Position);
        pathToPrint.Insert(0, goal);
        bool done = false;
        NavNode curr = goal;
        g_PathLengthNodes++;
        while (!done) {
            if (parents.ContainsKey(curr)) {
                if (parents[curr].Equals(curr)) done = true;
                curr.SetHighlightTile(true, curr == goal ? Color.green : Color.yellow, 0.8f);
                curr = parents[curr];
                path.Insert(0, curr.Position);
                pathToPrint.Insert(0, curr);
                g_PathLengthNodes++;
            } else done = true;
        }
        if (g_Grid != null) g_Grid.WritePathToFile(pathToPrint);
        return path;
    }

    public void ClearPath() {
        g_ClosedListA.Clear();
        g_OpenList.Clear();
        g_Fringe.Clear();
        if (g_gVal != null) g_gVal.Clear();
        if (g_fVal != null) g_fVal.Clear();
        if (g_Parents != null) g_Parents.Clear();
        if (g_PathList != null) g_PathList.Clear();
        if (DynamicWeight) HeuristicWeight = 1.0f;
        if(g_Pathfinders != null) {
            foreach (NavIncAStar pf in g_Pathfinders) {
                // reset searchers state as needed per search
                if (pf.DynamicWeight) pf.HeuristicWeight = 1.0f;
            }
        }
    }

    public bool IsEnabled() {
        return EnableNPCModule;
    }

    public bool IsReachable(Vector3 from, Vector3 target) {
        throw new NotImplementedException();
    }

    public string NPCModuleName() {
        return "Incremental A*";
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.PATHFINDER;
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.AI;
    }

    public void SetEnable(bool e) {
        EnableNPCModule = e;
    }

    public string ObjectIdentifier() {
        return gameObject.name;
    }

    public void RemoveNPCModule() {
        GetComponent<NPCController>().RemoveNPCModule(this);
    }

    public void InitializeModule() { /* Purposely empty */ }

    #endregion

    #region Properties

    [SerializeField]
    public float GlobalHeuristicsWeight;

    public bool DryRunAlgo = false;

    public int DryRunTests = 0;

    [SerializeField]
    public bool CleanPathOnRestart = true;

    [SerializeField]
    public bool EnableNPCModule = true;

    [SerializeField]
    public bool ClearPathOnArrival = false;

    [SerializeField]
    public bool UseHeuristic = false;

    [SerializeField]
    public bool WeightHeuristic = false;

    [SerializeField]
    public bool DynamicWeight = false;

    [SerializeField]
    public float DynamicWeightStep;

    [SerializeField]
    public float HeuristicWeight;

    [SerializeField]
    public int BeaconFringeLimit;

    [SerializeField]
    bool UseBeaconHeuristic = false;

    public bool BenchmarkAll = false;
    #endregion

    #region Unity_methods

    void Start() {
        g_NPCController = GetComponent<NPCController>();
        // Add secondary finders - for the purpose of implementing different heuristics, etc...
        g_Pathfinders = new List<NavIncAStar> {
            new AStar(GlobalHeuristicsWeight != 0f ? GlobalHeuristicsWeight : 1f),
            new WeightedAStar(GlobalHeuristicsWeight != 0f ? GlobalHeuristicsWeight : 1f,HeuristicWeight),
            new DynamicWeightedAStar(GlobalHeuristicsWeight != 0f ? GlobalHeuristicsWeight : 1f,DynamicWeightStep)
            /*new BeamSearch(GlobalHeuristicsWeight != 0f ? GlobalHeuristicsWeight : 1f, BeaconFringeLimit)*/
        };
        if (DryRunAlgo) {
            DryRunAlgorithm();
        } else if (BenchmarkAll) {
            Benchmark();
        }
    }

    void Update() {
        if (g_TargetLocation != Vector3.zero
            && ClearPathOnArrival
            && Vector3.Distance(g_NPCController.transform.position, g_TargetLocation) < 0.5f) {
            ClearPath();
            g_TargetLocation = Vector3.zero;
        }
    }

    void OnDestroy() {
        RemoveNPCModule();
    }

    public bool IsCurrentStateGoal(System.Object[] states = null) {
        NavNode curNode = (NavNode)states[0];
        return Vector3.Distance(curNode.Position, g_TargetLocation) < curNode.Radius * 2f;
    }
    string g_Name = "UniformCostSearch";
    public virtual string Name() {
        return g_Name;
    }

    public bool IsUpdateable() {
        return false;
    }

    public void TickModule() {
        throw new NotImplementedException();
    }

    public void CleanupModule() {
        throw new NotImplementedException();
    }

    #endregion

    #region Subclasses

    class AStar : NavIncAStar {
        public AStar(float secWeight) : base() {
            this.g_SecondaryHeuristicsWeight = secWeight;
            this.UseHeuristic = true;
            this.HeuristicWeight = 1.0f;
            g_Name = "A*";
        }
    }

    class WeightedAStar : NavIncAStar {
        public WeightedAStar(float secweight, float weight) {
            this.UseHeuristic = true;
            this.HeuristicWeight = weight;
            this.g_SecondaryHeuristicsWeight = secweight;
            g_Name = "Weighted A*";
        }
    }

    class DynamicWeightedAStar : NavIncAStar {
        public DynamicWeightedAStar(float weight, float step) : base() {
            this.UseHeuristic = true;
            this.DynamicWeightStep = step;
            this.DynamicWeight = true;
            this.HeuristicWeight = 1.0f;
            this.g_SecondaryHeuristicsWeight = weight;
            g_Name = "Dynamic Weighted A*";
        }
    }

    class BeamSearch : NavIncAStar {
        public BeamSearch(float weight, int limit) {
            this.g_SecondaryHeuristicsWeight = weight;
            this.UseBeaconHeuristic = true;
            this.BeaconFringeLimit = limit;
            g_Name = "Beam Search";
        }
    }

    #endregion

}
