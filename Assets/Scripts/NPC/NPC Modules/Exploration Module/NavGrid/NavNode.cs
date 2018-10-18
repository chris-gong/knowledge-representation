using UnityEngine;
using System;
using System.Collections;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [Serializable]
    public class NavNode {

        #region Enums
        public enum NODE_TYPE {
            HIGHWAY = 0,
            WALKABLE = 1,
            HARD_TO_WALK = 2,
            NONWALKABLE = 100
        }

        public enum NODE_STATUS {
            BLOCKED = 0,
            REGULAR = 1,
            HARD_TO_WALK = 2,
            HARD_BLOCKED = 3,
            REGULAR_HIGHWAY = 'a',
            HARD_HIGHWAY = 'b'
        }
        #endregion

        #region Constructor
        public NavNode(Vector3 position, Vector2 gridPos, bool walkable, float radius, NavGrid grid) {
            g_Radius = radius;
            g_Position = position;
            g_Walkable = walkable;
            g_NodeType = walkable ? NODE_TYPE.WALKABLE  // hard to walk is also walkable at this point
                : NODE_TYPE.NONWALKABLE;
            g_NodeStatus = IsWalkable() ? NODE_STATUS.REGULAR : NODE_STATUS.BLOCKED;
            g_GridPosition = gridPos;
            g_Grid = grid;
        }
        public NavNode(Vector3 position, Vector2 gridPos, Vector3 up, bool walkable, float radius, NavGrid grid) 
            : this(position,gridPos, walkable, radius, grid) {
            g_Up = up;
        }
        public NavNode(Vector3 position, Vector2 gridPos, Vector3 up, float blockingHeight, bool walkable, float radius, NavGrid grid) 
            : this(position, gridPos, up, walkable, radius, grid) {
            g_BlockingHeight = blockingHeight;
        }
        #endregion

        #region Properties

        public int HighwayId;
        
        public NODE_TYPE NodeType {
            get { 
                if (IsType(NODE_TYPE.HIGHWAY))       return NODE_TYPE.HIGHWAY;
                else if (IsType(NODE_TYPE.WALKABLE)) return NODE_TYPE.WALKABLE;
                if (IsType(NODE_TYPE.HARD_TO_WALK))  return NODE_TYPE.HARD_TO_WALK;
                else return NODE_TYPE.NONWALKABLE;
            }
        }

        public bool Available;

        public float Radius {
            get { return g_Radius; }
        }
        public Vector3 Position {
            get { return g_Position; }
        }
        public Vector2 GridPosition {
            get {  return g_GridPosition; }             
        }
        public float BlockingHeight {
            get { return g_BlockingHeight; }
        }
        public Vector3 Up {
            get { return g_Up; }
        }
        public bool Walkable {
            get { return g_Walkable; }
        }
        public float Weight {
            get { return g_Weight; }
            set { g_Weight = value;  }
        }

        public string DisplayWeight;

        public bool Selected;

        public NODE_STATUS NodeStatus {
            get {
                return g_NodeStatus;
            }
            set {
                g_NodeStatus = value;
            }
        }

        #endregion

        #region Members
        private NODE_TYPE   g_NodeType;
        private NODE_STATUS g_NodeStatus; // might deprecte g_NodeType
        private GameObject  g_Tile;
        private GameObject  g_TileText;
        private NavGrid     g_Grid;
        private float       g_Radius;
        private Vector3     g_Position;
        private Vector3     g_Up;
        private float       g_BlockingHeight;
        private bool        g_Walkable;
        private float       g_Weight= 1;
        private Vector2     g_GridPosition;
        #endregion

        #region Public_Functions

        public void SetHighlightTile() {
            Color c = Color.white;
            switch(NodeStatus) {
                case NODE_STATUS.BLOCKED:
                    c = Color.red;
                    break;
                case NODE_STATUS.REGULAR:
                    c = Color.green;
                    break;
                case NODE_STATUS.HARD_TO_WALK:
                    c = Color.yellow;
                    break;
                case NODE_STATUS.REGULAR_HIGHWAY:
                    c = Color.blue;
                    break;
                case NODE_STATUS.HARD_HIGHWAY:
                    c = Color.gray;
                    break;
            }
            SetHighlightTile(true, c, 0.4f);
        }

        public void SetHighlightTile(bool h, Color c, float alpha) {
            if(h) {
                if (g_Tile != null) goto destroy_tile;
                g_Tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g_Tile.GetComponent<BoxCollider>().isTrigger = true;
                CreateTileText();
                Material m = new Material(Shader.Find("Standard"));
                c.a = alpha;
                m.SetColor("_Color", c);
                /* all these just to make the color's alpha */
                m.SetFloat("_Mode", 3);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
                /* ---------------------- */
                g_Tile.GetComponent<Renderer>().material = m;
                g_Tile.layer = LayerMask.NameToLayer("Ignore Raycast");
                g_Tile.transform.position = Position + Up * 0.05f;
                g_Tile.transform.localScale = new Vector3(Radius * 2f, 0.01f, Radius * 2f);
                g_Tile.transform.parent = g_Grid.transform;
                return;
            }
destroy_tile:
            if(g_Tile != null) {
                GameObject.DestroyImmediate(g_Tile);
                g_Tile = null;
            }
            if (h) SetHighlightTile(h, c, alpha);
        }

        public override string ToString() {
            return "NavNode @ ["+GridPosition.x+","+GridPosition.y+"] @ "+g_Position+"]";
        }

        public void SetActiveTileText(bool active) {
            if (g_TileText != null)
                g_TileText.SetActive(active);
        }
        
        public bool IsWalkable() {
            
            if (g_Up != null) {
                Ray ray = new Ray(g_Position, g_Up);
                RaycastHit hit;
                Available =  !(Physics.Raycast(Position, Up, out hit, g_BlockingHeight) ||
                          Physics.Raycast(Position + new Vector3(Radius,0,0), Up, g_BlockingHeight) ||
                          Physics.Raycast(Position + new Vector3(-Radius, 0, 0), Up, g_BlockingHeight) ||
                          Physics.Raycast(Position + new Vector3(0, 0, Radius), Up, g_BlockingHeight) ||
                          Physics.Raycast(Position + new Vector3(0, 0, -Radius), Up, g_BlockingHeight));
                if(hit.collider) {
                    INPCPathfinder[] finder = hit.collider.GetComponents<INPCPathfinder>();
                    foreach(INPCPathfinder ipf in finder) {
                        g_Grid.AddINPCPathfinderNode(ipf, this);
                    }
                }
                return Available;
            } else return true;
        }

        public bool IsType(NavNode.NODE_TYPE t) {
            if (t == NODE_TYPE.HIGHWAY)
                return Weight >= (float) NODE_TYPE.HIGHWAY && Weight < (float) NODE_TYPE.WALKABLE;
            if (t == NODE_TYPE.WALKABLE)
                return Weight >= (float)NODE_TYPE.WALKABLE && Weight < (float)NODE_TYPE.HARD_TO_WALK;
            if (t == NODE_TYPE.HARD_TO_WALK)
                return Weight >= (float)NODE_TYPE.HARD_TO_WALK && Weight < (float)NODE_TYPE.NONWALKABLE;
            else
                return Weight > (float)NODE_TYPE.HARD_TO_WALK;
        }

        public override int GetHashCode() {
            int hash = 13;
            hash = (hash * 7) + (int) (g_GridPosition.x + g_GridPosition.y);
            hash = (hash * 7) + (int)(g_GridPosition.x + g_GridPosition.y);
            hash = (hash * 7) + (int)(g_GridPosition.x + g_GridPosition.y);
            hash = (hash * 7) + (int)(g_GridPosition.x + g_GridPosition.y);
            return hash;
        }

        public override bool Equals(object obj) {
            if (obj != null) {
                NavNode other = (NavNode) obj;
                return other != null && ((int)g_GridPosition.x == (int)other.g_GridPosition.x) 
                    && ((int)g_GridPosition.y == (int)other.g_GridPosition.y);
            }
            return false;
        }

        #endregion

        #region Private_Functions
        private void CreateTileText() {
            if (g_TileText != null) GameObject.Destroy(g_TileText);
            g_TileText = new GameObject();
            g_TileText.name = "TileText";
            g_TileText.transform.Rotate(Up, 90f);
            g_TileText.transform.Rotate(g_Tile.transform.right, 90f);
            g_TileText.transform.localScale = new Vector3(Radius, Radius, Radius);
            g_TileText.transform.localPosition = g_Tile.transform.position + (Up * 0.2f);
            TextMesh tm = g_TileText.AddComponent<TextMesh>();
            tm.color = Color.green;
            tm.fontSize = 20;
            tm.characterSize = 0.2f;
            tm.anchor = TextAnchor.UpperCenter;
            g_TileText.transform.parent = g_Tile.transform;
            tm.text = "Weight: " + DisplayWeight;
        }
        
        #endregion
    }
}