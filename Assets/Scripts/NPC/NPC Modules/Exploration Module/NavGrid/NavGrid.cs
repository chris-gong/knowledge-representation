using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NPC;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

public class NavGrid : MonoBehaviour {

    #region Properties
    #endregion

    #region Members
    [SerializeField]
    private Dictionary<INPCPathfinder, NavNode> g_WalkedOnNodes;
    public bool         WriteGridToFile = false;
    public string       FileName = "Grid_Description.txt";
    public bool         LoadFromFile;
    public bool         RedrawGrid;
    public bool         AddBlockerOnScene;
    public float        MinimunBlocked;
    public LayerMask    UnwalkableMask;

    [SerializeField]
    public Vector2      GridDimensions;

    public GRID_SCALE   GridScale = GRID_SCALE.ONE;
    private float       g_GridScale = 1.0f;
    public bool         PaintGridOnScene = false;
    public bool         PaintPathdOnPlay = false;
    public bool         DisplayTileText = false;
    public float        GridTransparency = 1.0f;
    private float       g_NodeRadius = 0.5f;
    public int          RandomHeavyAreas = 8;       // Default value
    public int          RandomHighways = 4;         // Default value
    public float        BlockingHeight = 2.0f;

    [SerializeField]
    NavNode[,]          g_Grid;

    public float        EasyWeight = (float)        NavNode.NODE_TYPE.HIGHWAY;
    public float        NormalWeight = (float)      NavNode.NODE_TYPE.WALKABLE;
    public float        MediumWeight = (float)      NavNode.NODE_TYPE.HARD_TO_WALK;
    public float        NotAvailableWeight = (float)NavNode.NODE_TYPE.NONWALKABLE;
    private NavNode     g_SelectedTile;
    public float        SelectedTileWeight = 1;
    public Vector2      SelectedTile;
    #endregion

    #region Private_Functions

    private void PopulateGrid() {
            
        int blocked = 0;
        g_WalkedOnNodes = new Dictionary<INPCPathfinder, NavNode>();
        g_GridScale = 1f / (float) GridScale;
        float nodeDiameter = g_NodeRadius * 2 * g_GridScale;
        GridDimensions.x = Mathf.RoundToInt(transform.localScale.x / nodeDiameter);
        GridDimensions.y = Mathf.RoundToInt(transform.localScale.z / nodeDiameter);
        int tilesX = (int) GridDimensions.x,
            tilesY = (int) GridDimensions.y;
        g_Grid = new NavNode[tilesX, tilesY];
        Vector3 gridWorldBottom = (transform.position - (transform.right * GridDimensions.x / 2) -
            (transform.forward * GridDimensions.y / 2) + new Vector3(g_NodeRadius,0.0f,g_NodeRadius)) * g_GridScale;
        for(int row = 0; row < tilesX; ++row) {
            for (int col = 0; col < tilesY; ++col) {
                NavNode node = new NavNode(
                    gridWorldBottom + (transform.right * (nodeDiameter) * row) + transform.forward * (nodeDiameter) * col,
                    new Vector2(row,col),
                    transform.up,
                    BlockingHeight,
                    true,
                    g_NodeRadius * g_GridScale,
                    this);
                if (!node.IsWalkable()) {
                    blocked++;
                    node.NodeStatus = NavNode.NODE_STATUS.BLOCKED;
                }
                g_Grid[row, col] = node;
            }
        }
        if (LoadFromFile && Application.isPlaying) {
            WriteGridToFile = false;
            ReadGridFromFile();
        } else {
            int targetBlocked = (int)(MinimunBlocked * (tilesX * tilesY));
            RandomizeHardWalkingAreas(RandomHeavyAreas, 31);
            RandomizeHighways(RandomHighways);
            if (AddBlockerOnScene && (blocked < targetBlocked)) {
                RandomizeBlockers(targetBlocked - blocked);
                AddBlockerOnScene = false;
            }
        }
    }

    private void RandomizeBlockers(int blockers) {
        bool done = false;
        int success = 0;
        GameObject obstaclesGO = GameObject.Find("Obstacles");
        Transform obstacles = null;
        if(obstaclesGO != null) {
            GameObject.DestroyImmediate(obstaclesGO);
        }
        obstaclesGO = new GameObject();
        obstaclesGO.name = "Obstacles";
        obstacles = obstaclesGO.transform;
        while (!done) {
            int x = (int)UnityEngine.Random.Range(0, g_Grid.GetLength(0));
            int y = (int)UnityEngine.Random.Range(0, g_Grid.GetLength(1));
            if (!g_Grid[x, y].Available) continue;
            NavNode n = g_Grid[x, y];
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(n.Radius * 1.5f, n.Radius * 2, n.Radius * 1.5f);
            cube.transform.position = n.Position + (new Vector3(0f,n.Radius,0f));
            cube.AddComponent<Rigidbody>();
            cube.GetComponent<BoxCollider>().size = new Vector3(0.8f, 0.8f, 0.8f);
            cube.transform.parent = obstacles;
            n.NodeStatus = NavNode.NODE_STATUS.BLOCKED;
            success++;
            if (success >= blockers) done = true;
        }
    }

    private void RandomizeHardWalkingAreas(int areas, int spread) {

        Dictionary<int, int> a = new Dictionary<int, int>();

        int limitX = g_Grid.GetLength(0) - 1, 
            limitY = g_Grid.GetLength(1) - 1;

        Vector2[] randomAreas = new Vector2[areas];

        for(int i = 0; i < areas; ++i) {

            Vector2 pos = new Vector2(
                Mathf.RoundToInt(UnityEngine.Random.Range(0, limitX)),
                Mathf.RoundToInt(UnityEngine.Random.Range(0, limitY)));

            int xOff = (int)(pos.x - (spread / 2)),
                yOff = (int)(pos.y - (spread / 2));

            for (int r = 0; r < spread; ++r) {
                for (int c = 0; c < spread; ++c) {
                    if (IsValid(new Vector2(xOff + r, yOff + c))) {
                        g_Grid[xOff + r, yOff + c].Weight = (float) NavNode.NODE_TYPE.HARD_TO_WALK;
                        if(g_Grid[xOff + r, yOff + c].IsWalkable()) {
                            g_Grid[xOff + r, yOff + c].NodeStatus = NavNode.NODE_STATUS.HARD_TO_WALK;
                        } else {
                            g_Grid[xOff + r, yOff + c].NodeStatus = NavNode.NODE_STATUS.HARD_BLOCKED;
                        }
                    }
                }
            }
        }
    }

    private void RandomizeHighways(int randomHighways) {

        int totalRes = 0;

restart_highways:

        Dictionary<NavNode,bool> totalChange = new Dictionary<NavNode, bool>();
        bool done = false;
        int restarts = -1;
            
        while (!done) {

            // for each path
            for (int i = 0; i < RandomHighways; ++i) {

            start_highway:
                restarts++;
                if(restarts > 400) {
                    Debug.Log("Restarting all highways");
                    totalRes++;
                    if (totalRes > 20) return;
                    goto restart_highways;
                }
                // get base
                int xBase = 0,
                    yBase = 0;

                bool onX = false;
                if(UnityEngine.Random.Range(0f,1f) <= 0.5f) {
                    xBase = (int)UnityEngine.Random.Range(0, g_Grid.GetLength(0) - 1);
                    yBase = 0;
                    onX = true;
                } else {
                    xBase = 0;
                    yBase = (int) UnityEngine.Random.Range(0 , g_Grid.GetLength(1) - 1);
                }

                Dictionary<NavNode,bool> toChange = new Dictionary<NavNode, bool>();
                int count = 0;

                // is it a valid start?
                if (IsValid(new Vector2(xBase, yBase))
                    // first node is not a highway
                    && !g_Grid[xBase, yBase].IsType(NavNode.NODE_TYPE.HIGHWAY)) {

                    NavNode n = g_Grid[xBase, yBase];

                    // 20 forth by default
                    for (int f = 0; f < 20; ++f) {
                        if (IsValid(new Vector2(xBase, yBase))
                            && !g_Grid[xBase, yBase].IsType(NavNode.NODE_TYPE.HIGHWAY)) {
                            if(!onX)
                                toChange.Add(g_Grid[xBase++, yBase], true);
                            else
                                toChange.Add(g_Grid[xBase, yBase++], true);
                            ++count;
                        } else goto start_highway;
                    }

                    bool randomWalk = true;

                    bool xWalk = !onX, yWalk = onX;

                    while (randomWalk) {
                            
                        if (xWalk)
                            yBase += 1;
                        else
                            xBase += yWalk ? 1 : -1;

                        bool error = false;

                        if (IsValid(new Vector2(xBase, yBase))) {
                            if (!totalChange.ContainsKey(g_Grid[xBase, yBase])) {
                                toChange.Add(g_Grid[xBase, yBase], true);
                                ++count;
                            } else {
                                Debug.Log("Collision on random walk");
                                error = true;
                            }
                        } else {
                            if (count < 100) {
                                error = true;
                            } else {
                                randomWalk = false;
                            }
                        }

                        if(error) {
                            goto start_highway;
                        }

                        if (count % 20 == 0) {
                            float turn = UnityEngine.Random.Range(0f, 1f);
                            if (turn >= 0.6f) {
                                if (xBase == 0)
                                    xWalk = true;
                                else if (yBase == 0)
                                    yWalk = true;
                                else {
                                    xWalk = !xWalk;
                                    yWalk = !yWalk;
                                }
                            }
                        }
                    }

                    foreach (NavNode node in toChange.Keys) {
                        if (totalChange.ContainsKey(node)) {
                            Debug.Log("Path collides, re-start path");
                            goto start_highway;
                        } else totalChange.Add(node, true);
                    }

                    foreach (NavNode node in toChange.Keys) {
                        node.HighwayId = i;
                    }

                } else --i; // repeat
            }
            done = true;
        }
        foreach(NavNode n in totalChange.Keys) {
            if (n.NodeStatus == NavNode.NODE_STATUS.HARD_TO_WALK)
                n.NodeStatus = NavNode.NODE_STATUS.HARD_HIGHWAY;
            else
                n.NodeStatus = NavNode.NODE_STATUS.REGULAR_HIGHWAY;
            n.Weight -= n.Weight + (float) NavNode.NODE_TYPE.HIGHWAY;
        }
    }

    private void ReadGridFromFile() {
        if (File.Exists(FileName)) {
            GameObject obstaclesGO = GameObject.Find("Obstacles");
            Transform obstacles = null;
            if (obstaclesGO != null) {
                GameObject.DestroyImmediate(obstaclesGO);
            }
            obstaclesGO = new GameObject();
            obstaclesGO.name = "Obstacles";
            obstacles = obstaclesGO.transform;
            string[] lines = File.ReadAllLines(FileName);
            int r = -1;
            NavNode n = null;
            GameObject cube = null;
            // each row
            foreach (string l in lines) {
                string[] line = l.Split(',');
                if(r < 0) {
                    int x = Int32.Parse(line[0]),
                        y = Int32.Parse(line[1]);
                    if (x != g_Grid.GetLength(0) || y != g_Grid.GetLength(1)) {
                        Debug.Log("NavGrid --> Loading from file failed since the current grid has not the same dimensions as the file's one");
                    }
                } else {
                    //each column
                    for (int c = 0; c < g_Grid.GetLength(1); ++c) {
                        try {
                            int val = Int32.Parse(line[c]);
                            switch(val) {
                                case (int) NavNode.NODE_STATUS.BLOCKED:
                                    g_Grid[r, c].Weight = (float)NavNode.NODE_TYPE.NONWALKABLE;
                                    g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.BLOCKED;
                                    // spawn cube
                                    n = g_Grid[r, c];
                                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    cube.transform.localScale = new Vector3(n.Radius * 1.5f, n.Radius * 2, n.Radius * 1.5f);
                                    cube.transform.position = n.Position + (new Vector3(0f, n.Radius, 0f));
                                    cube.AddComponent<Rigidbody>();
                                    cube.GetComponent<BoxCollider>().size = new Vector3(0.8f, 0.8f, 0.8f);
                                    cube.transform.parent = obstacles;
                                    break;
                                case (int)NavNode.NODE_STATUS.REGULAR:
                                    g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.REGULAR;
                                    break;
                                case (int)NavNode.NODE_STATUS.HARD_TO_WALK:
                                    g_Grid[r,c].Weight = (float)NavNode.NODE_TYPE.HARD_TO_WALK;
                                    g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.HARD_TO_WALK;
                                    break;
                                case (int)NavNode.NODE_STATUS.HARD_BLOCKED:
                                    g_Grid[r, c].Weight = (float)NavNode.NODE_TYPE.HARD_TO_WALK;
                                    g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.HARD_TO_WALK;
                                    n = g_Grid[r, c];
                                    cube  = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    cube.transform.localScale = new Vector3(n.Radius * 1.5f, n.Radius * 2, n.Radius * 1.5f);
                                    cube.transform.position = n.Position + (new Vector3(0f, n.Radius, 0f));
                                    cube.AddComponent<Rigidbody>();
                                    cube.GetComponent<BoxCollider>().size = new Vector3(0.8f, 0.8f, 0.8f);
                                    cube.transform.parent = obstacles;
                                    break;

                            }
                        } catch(System.FormatException e) {
                            // easy highway
                            if(line[c].Contains((char)NavNode.NODE_STATUS.REGULAR_HIGHWAY + "")) {
                                g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.REGULAR_HIGHWAY;
                            } 
                            // hard highway
                            else {
                                g_Grid[r, c].NodeStatus = NavNode.NODE_STATUS.HARD_HIGHWAY;
                            }
                            g_Grid[r, c].Weight -= g_Grid[r, c].Weight + (float) NavNode.NODE_TYPE.HIGHWAY;
                        }
                    }
                }
                ++r;
            }
        }
        LoadFromFile = false;
    }

    #endregion Private_Functions

    #region Unity_Methods

    // Use this for initialization
    void Reset() {
        PopulateGrid();
    }
        
    void Awake() {
        if(g_Grid == null)
            PopulateGrid();
        // current node occupied by an agent
        g_WalkedOnNodes = new Dictionary<INPCPathfinder, NavNode>();
        if(WriteGridToFile) {
            LoadFromFile = false;
            if(File.Exists(FileName)) {
                FileName = FileName.Substring(0, FileName.IndexOf(".txt")) + "_copy.txt";
                Debug.Log("NavGrid --> Creating copy of existing file");
            }
            StreamWriter sw = File.CreateText(FileName);
            sw.WriteLine(g_Grid.GetLength(0)+","+g_Grid.GetLength(1));
            for(int r = 0; r < (int)GridDimensions.x; ++r) {
                for (int c = 0; c < (int)GridDimensions.y; ++c) {
                    string s = (g_Grid[r, c].NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY || g_Grid[r, c].NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY) ?
                        ((char)g_Grid[r, c].NodeStatus).ToString() + g_Grid[r,c].HighwayId : ((int) g_Grid[r, c].NodeStatus).ToString();
                    s += ((c + 1) == (int)GridDimensions.y ? "\n" : ",");
                    sw.Write(s);
                }
            }
            sw.Close();
        }
        if(PaintPathdOnPlay) {
            foreach(NavNode n in g_Grid) {
                n.SetHighlightTile();
            }
        }
    }

    void Update() {
        try {
        SelectedTile.x = Mathf.Clamp(SelectedTile.x, 0, g_Grid.GetLength(0) - 1);
        SelectedTile.y = Mathf.Clamp(SelectedTile.y, 0, g_Grid.GetLength(1) - 1);
        } catch (System.Exception e) {
            SelectedTile.x = SelectedTile.y = 0f;
        }
        foreach (NavNode node in g_Grid) {
            node.IsWalkable();
            node.SetActiveTileText(DisplayTileText);
                
        }
    }

    void OnDestroy() {
        foreach(Transform t in transform) {
            GameObject.DestroyImmediate(t.gameObject);
        }
    }

    // Update is called once per frame
        
    void OnDrawGizmos() {

        if (NotAvailableWeight < MediumWeight) {
            NotAvailableWeight = MediumWeight + 1;
        } else if (MediumWeight < NormalWeight) {
            MediumWeight = NormalWeight + 1;
        } else if (NormalWeight <= 0)
            NormalWeight = 1f;

        // x for rows, y for cols
        Gizmos.DrawWireCube(transform.position, new Vector3(GridDimensions.x * g_GridScale, transform.position.y , GridDimensions.y * g_GridScale));
        if(g_Grid == null || g_Grid.GetLength(0) != GridDimensions.x || g_Grid.GetLength(1) != GridDimensions.y || RedrawGrid) {
            Reset();
            RedrawGrid = false;
        } else if (PaintGridOnScene) {

            SelectedTile.x = Mathf.Clamp(SelectedTile.x, 0, g_Grid.GetLength(0) - 1);
            SelectedTile.y = Mathf.Clamp(SelectedTile.y, 0, g_Grid.GetLength(1) - 1);

            if (g_SelectedTile != null) {
                g_SelectedTile.Weight = SelectedTileWeight > 0 ? SelectedTileWeight : 1;
                g_SelectedTile.Selected = false;
            }
            NavNode tmp = g_Grid[(int)SelectedTile.x, (int)SelectedTile.y];
            if (g_SelectedTile != tmp) {
                SelectedTileWeight = tmp.Weight;
            }
            g_SelectedTile = tmp;
            g_SelectedTile.Selected = true;
            g_SelectedTile = tmp;

            foreach(NavNode node in g_Grid) {
                if(!Application.isPlaying)
                    node.IsWalkable();
                Color c = Color.white;
                if(node.Selected) {
                    c = Color.white;
                    c.a = 1.0f * GridTransparency;
                } else {
                    if (node.Weight >= NotAvailableWeight || !node.Available) {
                        c = Color.red;
                        c.a = 0.5f * GridTransparency;
                    } else if (node.Weight >= MediumWeight) {
                        c = Color.yellow;
                        c.a = 0.3f * GridTransparency;
                    } else if (node.Weight >= NormalWeight) {
                        c = Color.green;
                        c.a = 0.1f * GridTransparency;
                    }
                }

                if(node.NodeStatus == NavNode.NODE_STATUS.REGULAR_HIGHWAY ||
                    node.NodeStatus == NavNode.NODE_STATUS.HARD_HIGHWAY) {
                    c = Color.blue;
                    c.a = 0.8f * GridTransparency;
                }

                Gizmos.color = c;
                float diam = node.Radius * 2;
                Gizmos.DrawWireCube(node.Position, new Vector3(diam, transform.position.y, diam));
                Gizmos.color = Color.white;
            }
        }
    }
    #endregion

    #region Public_Functions
        
    public void WritePathToFile(List<NavNode> nodes) {
        if(WriteGridToFile) {
            if(File.Exists(FileName)) {
                StreamWriter sw = File.AppendText(FileName + "_pathRecord.txt");
                NavNode g = nodes[nodes.Count - 1];
                sw.WriteLine((int) g.GridPosition.x + "," + (int) g.GridPosition.y);
                for (int i = 0; i < nodes.Count - 1; ++i) {
                    sw.WriteLine("(" + (int) nodes[i].GridPosition.x + "," + nodes[i].GridPosition.y + ")");
                }
                sw.Close();
                Debug.Log("NavGrid --> Path recorded!");
            } else {
                Debug.Log("NavGrid --> No file created for the grid: " + FileName);
            }
        }
    }

    public void AddINPCPathfinderNode(INPCPathfinder ipf, NavNode node) {
        if (g_WalkedOnNodes.ContainsKey(ipf) && g_WalkedOnNodes[ipf] != node) {
            if (Application.isPlaying) g_WalkedOnNodes[ipf].SetHighlightTile(false, Color.grey, 0.5f);
            g_WalkedOnNodes.Remove(ipf);
        } else if (!g_WalkedOnNodes.ContainsKey(ipf)) {
            g_WalkedOnNodes.Add(ipf, node);
            if (Application.isPlaying) node.SetHighlightTile(true, Color.red, 0.5f);
        }
    }

    public NavNode GetOccupiedNode(INPCPathfinder ipf) {
        return g_WalkedOnNodes.ContainsKey(ipf) ? g_WalkedOnNodes[ipf] : null;
    }

    public bool IsValid(Vector2 coord) {
        return 
            !(coord.x < 0 || coord.y < 0) &&
            (coord.x < g_Grid.GetLength(0)) && 
            (coord.y < g_Grid.GetLength(1));
    }

    public NavNode GetGridNode(int x, int y) {
        if (IsValid(new Vector2(x,y)) && g_Grid[x, y].IsWalkable()) {
            return g_Grid[x, y];
        } else return null;
    }

    public void CleanAll() {
        foreach(NavNode n in g_Grid) {
            n.SetHighlightTile(false, Color.black, 1f);
        }
    }

    public NavNode GetNeighborNode(NavNode current, GRID_DIRECTION dir) {
        return null;
    }

    /// <summary>
    /// Returns all exisitng neighbors
    /// </summary>
    /// <param name="current node"></param>
    /// <returns></returns>
    public Dictionary<NavNode,GRID_DIRECTION> GetNeighborNodes(NavNode current) {

        Dictionary<NavNode, GRID_DIRECTION> neighbors = new Dictionary<NavNode, GRID_DIRECTION>();
        int x = (int) current.GridPosition.x, y = (int) current.GridPosition.y;
            
        for(int i = -1; i < 2; ++i) {

            if (x + i >= g_Grid.GetLength(0)) continue;             // skip east
            else if (x + i < 0) continue;                           // skip west

            for (int j = 1; j > -2; --j) {

                if (i == 0 && j == 0) continue;                     // skip center
                else if (y + j >= g_Grid.GetLength(1)) continue;    // skip north
                else if (y + j < 0) continue;                       // skip south 

                GRID_DIRECTION dir = GRID_DIRECTION.CURRENT;        // dummy
                if      (i == -1 && j ==  1) dir = GRID_DIRECTION.NORTH_WEST;
                else if (i ==  0 && j ==  1) dir = GRID_DIRECTION.NORTH;
                else if (i ==  1 && j ==  1) dir = GRID_DIRECTION.NORTH_EAST;
                else if (i == -1 && j ==  0) dir = GRID_DIRECTION.WEST;
                else if (i ==  1 && j ==  0) dir = GRID_DIRECTION.EAST;
                else if (i ==  0 && j == -1) dir = GRID_DIRECTION.SOUTH;
                else if (i == -1 && j == -1) dir = GRID_DIRECTION.SOUTH_WEST;
                else if (i ==  1 && j == -1) dir = GRID_DIRECTION.SOUTH_EAST;
                neighbors.Add(g_Grid[x + i, y + j], dir);

            }
        }

        return neighbors;
    }
    #endregion
}

