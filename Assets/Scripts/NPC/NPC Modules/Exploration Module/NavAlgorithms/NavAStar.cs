using UnityEngine;
using System;
using System.Collections.Generic;
using NPC;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

[System.Serializable]
public class NavAStar : MonoBehaviour, INPCPathfinder, INPCModule {

    #region Members
    [SerializeField]
    public bool CleanPathOnRestart = true;
    
    [SerializeField]
    public bool EnableNPCModule = true;

    [SerializeField]
    public bool ClearPathOnArrival = false;

    [SerializeField]
    public float DiagonalPenalty = 0f;

    [SerializeField]
    public bool UseHeuristic = true;

    [SerializeField]
    public bool WeightHeuristic = false;


    [SerializeField]
    public bool DynamicWeight = false;

    [SerializeField]
    public float HeuristicWeight = 1f;

    [SerializeField]
    public bool UseBeaconHeuristic = false;

    [SerializeField]
    public int BeaconFringeLimit = -1;

    public bool BenchmarkAll = false;



    private NPCController g_NPCController;
    private Vector3 g_TargetLocation;
    NavNode g_FromNode;
    HashSet<NavNode> g_ClosedList;
    HashSet<NavNode> g_OpenList;
    private SortedList<float,NavNode> g_Fringe;
    private NavGrid g_Grid;
    public bool DryRunAlgo = false;
    public int DryRunTests = 0;
    private float g_CurrentPathValue = 0f;
    private int g_PathLengthNodes;

    #endregion

    #region Public_Functions

    public void InitializeModule() { /* Purposely empty */ }

    public void DryRunAlgorithm() {
        if(DryRunAlgo) {
            RaycastHit hit;
            List<Vector3> pathList = new List<Vector3>();
            if (Physics.Raycast(new Ray(transform.position + (transform.up * 0.2f), -1 * transform.up), out hit)) {
                g_Grid = hit.collider.GetComponent<NavGrid>();
                int succeed = 0;
                while(succeed < DryRunTests) {
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
                        for (int i = 0; i < 5; ++i) {

                            string log = "";

                            if (i == 0) {
                                UseHeuristic = false;
                                log += "Uniform Cost Search: \n";
                                DynamicWeight = false;
                                UseHeuristic = false;
                            } else if (i == 1) {
                                log += "A*: \n";
                                UseHeuristic = true;
                                WeightHeuristic = false;
                            } else if (i == 2) {
                                log += "Heavy Weighted A*: \n";
                                UseHeuristic = true;
                                WeightHeuristic = true;
                                HeuristicWeight = 5.0f;
                            } else if (i == 3) {
                                log += "Dynamic Weight A*: \n";
                                UseHeuristic = true;
                                WeightHeuristic = true;
                                HeuristicWeight = 1.0f;
                                DynamicWeight = true;
                            } else {
                                log += "Weighted Beam Heuristic: \n";
                                BeaconFringeLimit = 15;
                                UseBeaconHeuristic = true;
                                UseHeuristic = true;
                                WeightHeuristic = true;
                                HeuristicWeight = 1.0f;
                                DynamicWeight = false;
                            }
                            
                            float now = Time.realtimeSinceStartup;
                            from.SetHighlightTile(true, Color.black, 1f);
                            FindPath(from.Position, to.Position);
                            log += "Time: " + (Time.realtimeSinceStartup - now) + "ms \n";
                            log += "Expanded: " + g_Fringe.Count + 1 + "\n";
                            log += "Generated: " + g_ClosedList.Count + "\n";
                            log += "Path Value: " + g_CurrentPathValue + "\n";
                            log += "Path Length: " + g_PathLengthNodes;
                            g_NPCController.Debug(log);
                            g_CurrentPathValue = 0f;
                            g_PathLengthNodes = 0;
                        }
                        succeed++;
                    }
                    
                }
            }
            g_NPCController.Debug("Finished algorithm dry run");
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
                * (WeightHeuristic ? HeuristicWeight : 1.0f);
        }
        return totalCost;
    }

    public List<Vector3> ConstructPath(NavNode goal, Dictionary<NavNode,NavNode> parents) {
        List<Vector3> path = new List<Vector3>(parents.Count + 1);
        List<NavNode> pathToPrint = new List<NavNode>(parents.Count + 1);
        path.Insert(0, goal.Position);
        pathToPrint.Insert(0, goal);
        bool done = false;
        NavNode curr = goal;
        g_PathLengthNodes++;
        while (!done) {
            if (curr == parents[curr]) done = true;
            curr.SetHighlightTile(true, curr == goal ? Color.green : Color.yellow, 0.8f);
            curr = parents[curr];
            path.Insert(0, curr.Position);
            pathToPrint.Insert(0, curr);
            g_PathLengthNodes++;
        }
        if(g_Grid != null) g_Grid.WritePathToFile(pathToPrint);
        return path;
    }

    public List<Vector3> FindPath(Vector3 from, Vector3 to) {
        ClearPath();
        RaycastHit hit;
        List<Vector3> pathList = new List<Vector3>();
        if (Physics.Raycast(new Ray(transform.position + (transform.up * 0.2f), -1 * transform.up), out hit)) {
            g_Grid = hit.collider.GetComponent<NavGrid>();
            if(CleanPathOnRestart)
                g_Grid.CleanAll();
            g_FromNode = g_FromNode == null ? g_Grid.GetOccupiedNode(this) : g_FromNode;
            if(g_FromNode == null) {
                g_NPCController.Debug("NavAStar --> Agent is currently navigating in between nodes, try again please");
                return pathList;
            }
                
            NavNode goalNode = null;
            g_TargetLocation = to;
            Dictionary<NavNode, float> gVal = new Dictionary<NavNode, float>();
            Dictionary<NavNode, float> fVal = new Dictionary<NavNode, float>();
            g_ClosedList = new HashSet<NavNode>();
            g_OpenList = new HashSet<NavNode>();
            Dictionary<NavNode, NavNode> parents = new Dictionary<NavNode, NavNode>();
            
            g_Fringe = new SortedList<float, NavNode>(new NPCUtils.DuplicateKeyComparer<float>());
            parents.Add(g_FromNode, g_FromNode);
            gVal.Add(g_FromNode, 0f);
            fVal.Add(g_FromNode,ComputeNodeHeuristic(g_FromNode));
            g_Fringe.Add(fVal[g_FromNode], g_FromNode);
            g_OpenList.Add(g_FromNode);
            g_ClosedList.Add(g_FromNode);

            while (g_OpenList.Count > 0) {

                // Dynamically change the heuristic
                if (DynamicWeight) HeuristicWeight += 0.2f;

                // get next best node
                NavNode n = g_Fringe.Values[0];
                g_Fringe.RemoveAt(0);
                g_OpenList.Remove(n);
                g_ClosedList.Add(n);
                
                // test goal
                if (IsCurrentStateGoal(new System.Object[] { n })) {
                    n.SetHighlightTile(true, Color.green, 1f);
                    goalNode = n;
                    pathList = ConstructPath(goalNode, parents);
                    g_CurrentPathValue = fVal[n];
                    goto exit_pathfinding;
                }

                // loop adjacent
                Dictionary<NavNode, GRID_DIRECTION> neighbors = g_Grid.GetNeighborNodes(n);
                foreach(NavNode neighbor in neighbors.Keys) {
                    if(!g_ClosedList.Contains(neighbor) && neighbor.IsWalkable()) {
                        float val = gVal[n] + ComputeNodeCost(n, neighbor, neighbors[neighbor]);
                        bool inFringe = g_OpenList.Contains(neighbor);
                        if (!inFringe) {
                            gVal.Add(neighbor, val);
                            fVal.Add(neighbor, UseHeuristic ? ComputeNodeHeuristic(neighbor) + val : gVal[neighbor]);
                            parents.Add(neighbor, n);
                            g_OpenList.Add(neighbor);
                            g_Fringe.Add(fVal[neighbor], neighbor);
                            neighbor.DisplayWeight = fVal[neighbor].ToString();
                            neighbor.SetHighlightTile(true, Color.white, 0.4f);
                            if(UseBeaconHeuristic && g_Fringe.Count > BeaconFringeLimit) {
                                NavNode last = g_Fringe.Values[g_Fringe.Count - 1];
                                gVal.Remove(last);
                                fVal.Remove(last);
                                parents.Remove(last);
                                g_OpenList.Remove(last);
                                g_Fringe.RemoveAt(g_Fringe.Count-1);
                            }
                        }
                        if (val < gVal[n]) {
                            if (parents.ContainsKey(neighbor)) parents.Remove(neighbor);
                            parents.Add(neighbor, n);
                        }
                    }
                }
            }
        } else {
            g_NPCController.Debug("NavAStar --> Pathfinder not on grid");    
        }
    exit_pathfinding:
        if (DynamicWeight) HeuristicWeight = 1.0f;
        g_FromNode = null;
        return pathList;
    }

    public bool IsEnabled() {
        return EnableNPCModule;
    }

    public bool IsReachable(Vector3 from, Vector3 target) {
        throw new NotImplementedException();
    }

    public string NPCModuleName() {
        return "A*";
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
    #endregion

    #region Unity_methods

    void Start() {
        g_NPCController = GetComponent<NPCController>();
        g_Fringe = new SortedList<float, NavNode>(new NPCUtils.DuplicateKeyComparer<float>());
        g_ClosedList = new HashSet<NavNode>();
        g_OpenList = new HashSet<NavNode>();
        if(DryRunAlgo) {    
            DryRunAlgorithm();
        } else if (BenchmarkAll) {
            Benchmark();
        }
    }

    void Update() {
        if(g_TargetLocation != Vector3.zero
            && ClearPathOnArrival 
            && Vector3.Distance(g_NPCController.transform.position, g_TargetLocation) < 0.5f) {
            g_Grid.CleanAll();
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

    public void ClearPath() {
        if (g_Fringe != null) g_Fringe.Clear();
        if (g_OpenList != null) g_OpenList.Clear();
        if (g_ClosedList != null) g_ClosedList.Clear(); 
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

}
