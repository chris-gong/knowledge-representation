using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.IO;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    namespace Behavior {

        public class NPCBehaviors_Editor : EditorWindow {

            public static NPCBehaviors_Editor Instance;
            private readonly static string DIR_PREFIX = "Assets/Scripts/NPC/Editor/Data";
            public readonly static string TMP_FILE = DIR_PREFIX + "/bts.asset";
            private readonly static string LIBRARY_DIR = DIR_PREFIX + "/TreesLibrary";
            private readonly static string BTS_FILE = DIR_PREFIX + "/bt.asset";
            private readonly static string NODE_ASSERTION_TYPE = "Assertion";
            private readonly static string NODE_ACTION_TYPE = "Affordance";
            private static string m_SideMenuName = "Side Menu";
            private readonly static string COMPLETE_TREE = "Complete";
            private readonly static string PARTIAL_TREE = "Subtree";

            protected readonly static string BEHAVIORS_TITLE  = "Behaviors";
            protected readonly static string PARAMETERS_TITLE = "Parameters";
            protected readonly static string BLACKBOARD_TITLE = "Blackboard";

            public      NodeList SessionNodes;
            public      Library TreeLibrary;

            public      GUIStyle inPointStyle;
            public      GUIStyle outPointStyle;
            private     GUIStyle nodeStyle;
            private     GUIStyle selectedNodeStyle;
            private     GUIStyle hiddingNodeStyle;
            private     GUIStyle hiddingSelectedNodeStyle;
            private     GUIStyle canvasStyle;
            private     GUIStyle sideMenuStyle, sideMenuSelectedStyle;
            private     Rect g_TreeNameRect = new Rect(5, 20, 125, 65);
            protected   GUIStyle affordanceStyle, treeStyle, treeLabelStyle, treeButtonStyle;

            private SideMenu sideMenu;
            private ParametersMenu parametersMenu;
            private Node selectedInPoint;
            private Node selectedOutPoint;
            public Dictionary<Node, Blackboard> BlackboardNodes;
            
            public Node SelectedNode;
            private NPCNode g_LastBakedTree;

            [SerializeField]
            private bool g_DisplayTreeName;
            private string g_CurrentTreeName;

            // Assemblies metadata
            public Dictionary<string, Type> NodeTypes;
            public Dictionary<Type, List<PropertyInfo>> EntitiesProperties;
            public Dictionary<string, MethodInfo> Affordances;

            float panX = -100000 / 2;
            float panY = -100000 / 2;

            private void OnDisable() {
                SaveSession();
            }

            private void OnEnable() {

                Instance = this;

                InitializeData();
                
                nodeStyle = new GUIStyle();
                nodeStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
                nodeStyle.border = new RectOffset(12, 12, 12, 12);

                selectedNodeStyle = new GUIStyle();
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("node6 on") as Texture2D;
                selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

                hiddingNodeStyle = new GUIStyle();
                hiddingNodeStyle.normal.background = EditorGUIUtility.Load("node3") as Texture2D;
                hiddingNodeStyle.border = new RectOffset(12, 12, 12, 12);

                hiddingSelectedNodeStyle = new GUIStyle();
                hiddingSelectedNodeStyle.normal.background = EditorGUIUtility.Load("node3 on") as Texture2D;
                hiddingSelectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
                
                canvasStyle = new GUIStyle();
                canvasStyle.normal.background = EditorGUIUtility.Load("darkviewbackground") as Texture2D;

                inPointStyle = new GUIStyle();
                inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
                inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
                inPointStyle.border = new RectOffset(4, 4, 10, 10);

                outPointStyle = new GUIStyle();
                outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
                outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
                outPointStyle.border = new RectOffset(4, 4, 10, 10);

                affordanceStyle = new GUIStyle();
                affordanceStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
                affordanceStyle.active.background = EditorGUIUtility.Load("node6 on") as Texture2D;
                affordanceStyle.normal.textColor = Color.white;
                affordanceStyle.active.textColor = Color.gray;
                affordanceStyle.alignment = TextAnchor.UpperCenter;
                affordanceStyle.border = new RectOffset(12, 12, 12, 12);
                affordanceStyle.padding = new RectOffset(15, 15, 15, 15);

                treeStyle = new GUIStyle();
                treeStyle.normal.textColor = Color.white;
                treeStyle.padding = new RectOffset(10, 10, 10, 5);
                treeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
                treeStyle.border = new RectOffset(25, 25, 25, 25);
                treeStyle.alignment = TextAnchor.MiddleCenter;

                treeLabelStyle = new GUIStyle();
                treeLabelStyle.normal.textColor = Color.white;
                treeLabelStyle.alignment = TextAnchor.MiddleCenter;
                treeLabelStyle.margin = new RectOffset(5, 5, 5, 5);
                
                treeButtonStyle = new GUIStyle();
                treeButtonStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
                treeButtonStyle.active.background = EditorGUIUtility.Load("node6 on") as Texture2D;
                treeButtonStyle.normal.textColor = Color.white;
                treeButtonStyle.active.textColor = Color.gray;
                treeButtonStyle.alignment = TextAnchor.MiddleCenter;
                treeButtonStyle.border = new RectOffset(12, 12, 12, 12);
                treeButtonStyle.padding = new RectOffset(10, 10, 10, 15);

                ClearConnectionSelection();

            }

            [MenuItem("Window/NPC Behaviors Editor")]
            static void StartWindow() {
                Instance = (GetWindow<NPCBehaviors_Editor>("NPC Behaviors"));
            }

            private void InitializeData() {
                AssetDatabase.Refresh();
                if(!Directory.Exists(DIR_PREFIX))
                    Directory.CreateDirectory(DIR_PREFIX);
                if(!Directory.Exists(LIBRARY_DIR))
                    Directory.CreateDirectory(LIBRARY_DIR);
                if (AssetDatabase.LoadAssetAtPath(TMP_FILE, typeof(NodeList)) != null) {
                    SessionNodes = (NodeList) AssetDatabase.LoadAssetAtPath(TMP_FILE, typeof(NodeList));
                } else {
                    ScriptableObject o = CreateInstance<NodeList>();
                    SessionNodes = (NodeList) o;
                    SessionNodes.Init();
                    AssetDatabase.CreateAsset(o, TMP_FILE);
                }

                // Load all required data here
                sideMenu = new SideMenu(this, new Rect(0, 0, 100, 100), 1);
                parametersMenu = new ParametersMenu(this, 200, 125, 2);

                // Find the available types of nodes in the assembly
                PopulateNodeTypes();
                PopulateEntitiesProperties();
                PopulateAffordances();
                BlackboardNodes = new Dictionary<Node, Blackboard>();
                SelectedNode = null;

                // Load Library
                LoadLibrary();
            }

            #region Unity Methods

            private void OnGUI() {

                // Check if the cursor is on Side Menues
                bool onSideMenues = ProcessMenuesEvents(Event.current);

                // Main Canvas
                GUI.BeginGroup(new Rect(panX, panY, -panX * 2, -panY * 2), canvasStyle);
                if (!onSideMenues) {
                    ProcessNodeEvents(Event.current);
                    ProcessEvents(Event.current);
                }
                DrawNodes();
                GUI.EndGroup();

                // Toolbar
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                DrawToolStrip();
                GUILayout.EndHorizontal();

                // Menues
                GUILayout.BeginVertical();
                BeginWindows();
                sideMenu.Draw();
                parametersMenu.Draw();
                if(g_DisplayTreeName && SelectedNode != null)
                    GUI.Window(999, g_TreeNameRect, DrawNameTree, "Name");
                EndWindows();
                GUILayout.EndVertical();

                if (GUI.changed) {
                    Repaint();
                }
            }

            #endregion

            void DrawSideMenu(int id) {
                GUI.DragWindow();
            }

            void DrawToolStrip() {
                // Session Options
                if (GUILayout.Button("File", EditorStyles.toolbarButton)) {
                    GenericMenu toolsMenu = new GenericMenu();
                    toolsMenu.AddItem(new GUIContent("Save"), false, SaveSession, null);
                    toolsMenu.AddItem(new GUIContent("Close"), false, CloseWindow, null);
                    toolsMenu.ShowAsContext();
                    EditorGUIUtility.ExitGUI();
                }
                // Tree Options
                if (GUILayout.Button("Tree", EditorStyles.toolbarButton)) {
                    GenericMenu toolsMenu = new GenericMenu();
                    if(Instance.SelectedNode != null && Instance.SelectedNode.Children.Count > 0)
                        toolsMenu.AddItem(new GUIContent("Display Name"), false, NameTree, null);
                    if(Instance.SelectedNode != null &&  Instance.SelectedNode.IsTree) {
                        if(Instance.SelectedNode.Hidding) {
                            toolsMenu.AddItem(new GUIContent("Show Subtree"), false, ShowHideTree, false);
                        } else {
                            toolsMenu.AddItem(new GUIContent("Hide Subtree"), false, ShowHideTree, true);
                        }
                    }
                    if (g_LastBakedTree != null)
                        toolsMenu.AddItem(new GUIContent("Load in Agents"), false, LoadTreeInAgents, null);
                    else
                        toolsMenu.AddDisabledItem(new GUIContent("Load in Agents"));
                    if (SessionNodes.Nodes.Count > 0) {
                        toolsMenu.AddItem(new GUIContent("Clear"), false, ClearSession, null);
                    } else {
                        toolsMenu.AddDisabledItem(new GUIContent("Clear"));
                    }
                    if (Instance.SelectedNode != null && Instance.SelectedNode.IsTree) {
                        if(TreeLibrary.GetTree(Instance.SelectedNode.TreeName) == null) {
                            toolsMenu.AddItem(new GUIContent("Add To Library"), false, ManageLibrary, "add");
                        } else {
                            toolsMenu.AddItem(new GUIContent("Rewrite In Library"), false, null, "rewrite");
                        }
                    }
                    toolsMenu.ShowAsContext();
                    EditorGUIUtility.ExitGUI();
                }
                // Node Options
                if(GUILayout.Button("Node", EditorStyles.toolbarButton)) {
                    GenericMenu toolsMenu = new GenericMenu();
                    if (!(Instance.SelectedNode == null || Instance.SelectedNode.isLeaf)) {
                        if (Instance.SelectedNode.Blackboard != null) {
                            toolsMenu.AddItem(new GUIContent("Remove Blackboard"), false, RemoveBlackboard, Instance.SelectedNode);
                        } else {
                            toolsMenu.AddItem(new GUIContent("Attach Blackboard"), false, AttachBlackboard, Instance.SelectedNode);
                        }
                    } else {
                        toolsMenu.AddDisabledItem(new GUIContent("Attach Blackboard"));
                    }
                    toolsMenu.ShowAsContext();
                    EditorGUIUtility.ExitGUI();
                }
                // Bake Options
                if (GUILayout.Button("Bake...", EditorStyles.toolbarButton)) {
                    GenericMenu toolsMenu = new GenericMenu();
                    if (Selection.activeGameObject != null && SelectedNode != null) {
                        toolsMenu.AddItem(new GUIContent("Complete..."), false, BakeTree, COMPLETE_TREE);
                        toolsMenu.AddItem(new GUIContent("Subtree..."), false, BakeTree, PARTIAL_TREE);
                    } else {
                        toolsMenu.AddDisabledItem(new GUIContent("Complete..."));
                        toolsMenu.AddDisabledItem(new GUIContent("Subtree..."));
                    }
                    toolsMenu.ShowAsContext();
                    EditorGUIUtility.ExitGUI();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Tools", EditorStyles.toolbarDropDown)) {
                    GenericMenu toolsMenu = new GenericMenu();
                    if (SessionNodes.Nodes.Count > 0 && SelectedNode != null) {
                        toolsMenu.AddItem(new GUIContent("Validate Selected Tree"), false, ValidateTree, SelectedNode);
                        Debug.Log("Feature coming soon...");
                    } else
                        toolsMenu.AddDisabledItem(new GUIContent("Validate Tree"));
                    toolsMenu.AddSeparator("");
                    toolsMenu.AddItem(new GUIContent("Help..."), false, null);
                    // Offset menu from right of editor window
                    toolsMenu.DropDown(new Rect(Screen.width - 216 - 40, 0, 0, 16));
                    EditorGUIUtility.ExitGUI();
                }
            }

            #region Window Options

            private void SaveSession(object obj = null) {
                EditorUtility.SetDirty(SessionNodes);
                foreach (Node n in SessionNodes.Nodes)
                    EditorUtility.SetDirty(n);
                AssetDatabase.SaveAssets();
            }

            private void ValidateTree(object obj) {
                Debug.Log("Feature coming soon ...");
            }

            private void ClearSession(object obj) {
                List<Node> toRemove = new List<Node>(SessionNodes.Nodes);
                foreach (Node n in toRemove)
                    OnClickRemoveNode(n);
            }

            private void CloseWindow(object o) {
                SaveSession();
                GetWindow<NPCBehaviors_Editor>().Close();
            }

            #endregion

            private void DrawNodes() {
                if (SessionNodes != null) {
                    for (int i = 0; i < SessionNodes.Nodes.Count; i++) {
                        SessionNodes.Nodes[i].NodeID = i;
                        if (SessionNodes.Nodes[i].Parent == null)
                            DrawTree(SessionNodes.Nodes[i]);
                    }
                }
            }
            
            // Recursively draw each tree
            private void DrawTree(Node root) {
                root.Draw();
                foreach (Node c in root.Children) {
                    DrawTree(c);
                }
            }
            
            private void ProcessNodeEvents(Event e) {
                if (SessionNodes != null) {
                    for (int i = SessionNodes.Nodes.Count - 1; i >= 0; i--) {
                        bool guiChanged = false;
                        if (!SessionNodes.Nodes[i].Hidden)
                            guiChanged = SessionNodes.Nodes[i].ProcessEvents(e);
                        if (guiChanged) {
                            GUI.changed = true;
                        }
                    }
                }
            }

            private void ProcessEvents(Event e) {

                switch (e.type) {
                    case EventType.MouseDown:
                        if (e.button == 1) {
                            ProcessContextMenu(e.mousePosition);
                        }
                        break;
                    case EventType.MouseDrag:
                        SelectedNode = null;
                        panX += e.delta.x;
                        panY += e.delta.y;
                        GUI.changed = true;
                        break;
                }
            }

            private bool ProcessMenuesEvents(Event e) {
                bool onMenu = sideMenu.ProcessSideMenuEvents(e);
                if (!onMenu)
                    onMenu = parametersMenu.ProcessParamsMenuEvents(e);
                if (!onMenu)
                    onMenu = e.mousePosition.y < 15;
                if(!onMenu) {
                    onMenu = g_DisplayTreeName && g_TreeNameRect.Contains(e.mousePosition);
                }
                return onMenu;
            }

            private void ProcessContextMenu(Vector2 mousePosition) {
                GenericMenu genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
                genericMenu.ShowAsContext();
            }

            private void OnClickAddNode(Vector2 mousePosition) {
                Node node = (Node) CreateInstance(typeof(Node));
                AssetDatabase.AddObjectToAsset(node,TMP_FILE);
                node.Init(mousePosition, 135, 60, nodeStyle, selectedNodeStyle, hiddingNodeStyle, hiddingSelectedNodeStyle, this, inPointStyle, outPointStyle);
                SessionNodes.Nodes.Add(node);
            }

            #region Connections

            public void OnClickInPoint(Node inPoint) {
                selectedInPoint = inPoint;
                if (selectedOutPoint != null) {
                    if (selectedOutPoint != selectedInPoint) {
                        CreateConnection();
                        ClearConnectionSelection();
                    } else {
                        ClearConnectionSelection();
                    }
                }
            }

            public void OnClickOutPoint(Node outPoint) {
                ClearConnectionSelection();
                if (!outPoint.isLeaf) {
                    selectedOutPoint = outPoint;
                    if (selectedInPoint != null) {
                        if (selectedOutPoint != selectedInPoint) {
                            if (SessionNodes.Nodes.Contains(selectedOutPoint) && SessionNodes.Nodes.Contains(selectedInPoint))
                                CreateConnection();
                            ClearConnectionSelection();
                        } else {
                            ClearConnectionSelection();
                        }
                    }
                }
            }

            public void OnClickRemoveConnection(Node parent, Node child) {
                child.Parent = null;
				if (child.Blackboard != null && child.Blackboard.Root != child)
                    child.Blackboard = null;
                parent.Children.Remove(child);
            }

            private void CreateConnection() {
                if (selectedInPoint.Parent == null) {
                    selectedInPoint.Parent = selectedOutPoint;
                    selectedOutPoint.Children.Add(selectedInPoint);
                    // Cascade Blackboard
                    Node p = selectedInPoint.Parent;
                    if(p.Blackboard != null) {
                        SetBlackboard(selectedInPoint, p.Blackboard);
                    }
                    //Node p = selectedInPoint.Parent;
                    //while (p.Parent != null) {
                    //	p = p.Parent;
                    //}
                    //selectedInPoint.Blackboard = p.Blackboard;
                }
            }

            private void ClearConnectionSelection() {
                selectedInPoint = null;
                selectedOutPoint = null;
            }

            #endregion

            public void OnClickRemoveNode(Node node) {
                if (node.Parent != null) {
                    OnClickRemoveConnection(node.Parent, node);
                }
                List<Node> children = new List<Node>(node.Children.ToArray());
                foreach (Node c in children) {
                    if (c.Hidden) OnClickRemoveNode(c);
                    else OnClickRemoveConnection(node, c);
                }
                SessionNodes.Nodes.Remove(node);
                RemoveBlackboard(node);
                SelectedNode = null;
                if (node.Blackboard != null)
                    DestroyImmediate(node.Blackboard, true);
                DestroyImmediate(node, true);
            }

            private void PopulateNodeTypes() {
                List<Type> types = new List<Type>(typeof(NPCNode).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(NPCNode))));
                string prefix = "NPC";
                NodeTypes = new Dictionary<string, Type>();
                foreach (Type t in types) {
                    string keyName = t.FullName.Substring(t.FullName.LastIndexOf(prefix) + prefix.Length);
                    NodeTypes.Add(keyName, t);
                }
            }

            private void PopulateEntitiesProperties() {
                EntitiesProperties = new Dictionary<Type, List<PropertyInfo>>();
                Type npcAgent = typeof(NPCController);
                Type npcObject = typeof(NPCObject);
                Type npcEntity = typeof(INPCPerceivable);
                EntitiesProperties.Add(npcAgent, new List<PropertyInfo>());
                EntitiesProperties.Add(npcObject, new List<PropertyInfo>());
                EntitiesProperties.Add(npcEntity, new List<PropertyInfo>());
                // Populate available attributes on the agent's AI
                foreach(PropertyInfo prop in typeof(NPCAI).GetProperties()) {
                    if (Attribute.IsDefined(prop, typeof(NPCAttribute))) {
                        EntitiesProperties[npcAgent].Add(prop);
                    }
                }
                foreach (PropertyInfo prop in typeof(NPCController).GetProperties()) {
                    if (Attribute.IsDefined(prop, typeof(NPCAttribute))) {
                        EntitiesProperties[npcAgent].Add(prop);
                    }
                }
            }

            private void PopulateAffordances() {
                Affordances = NPCAI.GetAffordances();
            }

            #region Tree Processing

            private bool IsAssert(Node n) {
                return n.NodeTypeName == "Assert" ||
                    n.NodeType == typeof(NPCAssert);
            }

            private void LoadLibrary() {
                TreeLibrary = new Library();
                var trees = AssetDatabase.FindAssets("t:nodelist", new string[] { LIBRARY_DIR });
                foreach (var t in trees) {
                    NodeList l = AssetDatabase.LoadAssetAtPath<NodeList>(AssetDatabase.GUIDToAssetPath(t));
                    l.TreeName = l.name;
                    TreeLibrary.AddTree(l);
                }
            }

            private void ManageLibrary(object o) {
                string action = (string) o;
                if (string.IsNullOrEmpty(Instance.SelectedNode.TreeName)) {
                    Debug.Log("Tree must be named before being added to Library");
                } else {
                    switch (action) {
                        case "add":
                            SessionNodes.TreeName = Instance.SelectedNode.TreeName;
                            AssetDatabase.CopyAsset(TMP_FILE, LIBRARY_DIR + "/" + Instance.SelectedNode.TreeName + ".asset");
                            SaveSession();
                            break;
                        case "rewrite":
                            break;
                    }
                    LoadLibrary();
                }
            }

            private void ManageLibraryTree(string action, string treeName) {
                switch (action) {
                    case "add_workspace":
                        NodeList tree = TreeLibrary.Trees.SingleOrDefault(t => t.TreeName == treeName);
                        if(tree != null) {
                            Blackboard bb = null;
                            // retrieve an arbitrary node
                            Node root = tree.Nodes[0];
                            // Find top-most node
                            while (root.Parent != null) {
                                root = root.Parent;
                            }
                            if (root.Blackboard != null)
                                bb = root.Blackboard;
                            // Create a clone of the asset
                            Dictionary<Node, Node> origCopy = new Dictionary<Node, Node>();
                            root = TreeLibrary.CopyTree(root, null, origCopy);
                            // Set initial position
                            root.rect.position = new Vector2(-panX + 50, -panY + 50);
                            // Adjust position relative to parent's
                            AdjustRelativePosition(root, root.rect.position);
                            // Hide children
                            root.Hidding = true;
                            foreach (Node c in root.Children) {
                                RecurseHide(c, true);
                            }
                            // Add to current session
                            List<Node> nodes = TreeLibrary.GetNodesList(root);
                            foreach(Node n in nodes) {
                                if(n.Blackboard != null && n.Blackboard.Root == origCopy[n]) {
                                    bb = Instantiate(n.Blackboard);
                                    n.Blackboard = bb;
                                    bb.Root = n;
                                    foreach(Node c in n.Children)
                                        SetBlackboard(n, bb);
                                    AssetDatabase.AddObjectToAsset(bb, TMP_FILE);
                                }
                            }
                            foreach (Node n in nodes)
                                AssetDatabase.AddObjectToAsset(n, TMP_FILE);
                            Instance.SessionNodes.Nodes.AddRange(nodes);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        break;
                    case "delete":
                        if (EditorUtility.DisplayDialog("Delete Tree", "Are you sure you want to permanently delete " + treeName + "?", "Yes", "Cancel")) {
                            AssetDatabase.DeleteAsset(LIBRARY_DIR + "/" + treeName + ".asset");
                        }
                        break;
                }
                LoadLibrary();
            }

            private void AdjustRelativePosition(Node n, Vector2 rootPosition) {
                if(n.Parent != null) {
                    n.rect.position = rootPosition + n.ParentOffset;
                }
                foreach (Node c in n.Children)
                    AdjustRelativePosition(c, n.rect.position);
            }

            private void ShowHideTree(object o) {
                Node parent = Instance.SelectedNode;
                bool hide = (bool) o;
                parent.Hidding = hide;
                foreach(Node c in parent.Children) {
                    RecurseHide(c, hide);
                }
            }

            private void RecurseHide(Node n, bool val) {
                if (!n.Hidding) {
                    foreach (Node c in n.Children) {
                        RecurseHide(c, val);
                    }
                }
                n.Hidden = val;
            }

            public void BakeTree(object Target) {
                List<NPCController> agents = new List<NPCController>();
                foreach(GameObject o in Selection.gameObjects) {
                    NPCController c = o.GetComponent<NPCController>();
                    if (c != null)
                        agents.Add(c);
                }
                if(agents.Count == 0) {
                    Debug.Log("No agent selected to bake tree: " + Selection.activeObject);
                } else {
                    string target = Target.ToString();
                    Node root = null;
                    if (target == COMPLETE_TREE) {
                        Node currNode = SelectedNode;
                        while (currNode.Parent != null) {
                            currNode = currNode.Parent;
                        }
                        root = currNode;
                    } else {
                        root = SelectedNode;
                    }
                    if(!root.isLeaf) {
                        try {
                            string path = AssetDatabase.GenerateUniqueAssetPath(BTS_FILE);
                            if (!string.IsNullOrEmpty(root.TreeName)) {
                                path = path.Replace("bt.asset", root.TreeName + ".asset");
                            }
                            g_LastBakedTree = CreateTree(root,path);
                            LoadTreeInAgents(agents);
                            MarkTreeDirty(g_LastBakedTree);
                            AssetDatabase.SaveAssets();
                        } catch(Exception e) {
                            Debug.LogError("Fatal error while baking tree: " + e.Message);
                        }
                    } else {
                        Debug.Log("Invalid root node selected: " + root);
                    }
                }
            }

            private NPCNode CreateTree(Node parent, string path) {
                NPCNode n = MakeNode(parent);
                if(parent.Parent == null) {
                    AssetDatabase.CreateAsset(n, path);
                }
                foreach(Node c in parent.Children) {
                    NPCNode node = CreateTree(c, path);
                    if (!n.AddChild(node))
                        Debug.Log("Failed adding child " + c + " to parent " + parent);
                    else {
                        AssetDatabase.AddObjectToAsset(node, path);
                    }
                }
                if(parent.Blackboard != null && parent.Blackboard.Root == parent) {
                    NPCBlackboard bb = CreateInstance<NPCBlackboard>();
                    List<Parameter> pms = new List<Parameter>();
                    foreach (NodeParam np in parent.Blackboard.Parameters) {
                        Parameter p = new Parameter();
                        p.AssignedType = np.AssignedType;
                        p.ParameterName = np.Name;
                        p.SetValue(np.GetValue());
                        pms.Add(p);
                    }
                    bb.Init(pms);
                    bb.Root = n;
                    n.Blackboard = bb;
                    AssetDatabase.AddObjectToAsset(bb, path);
                }
                return n;
            }

            private NPCNode MakeNode(Node n) {
                NPCNode node = null;
                object[] pms = new object[] { null };
                if (n.isLeaf) {
                    List<object> parameters = new List<object>();
                    int index = 0;
                    foreach(ParameterInfo p in Instance.NodeTypes[n.NodeTypeName].GetConstructors()[0].GetParameters()) {
                        parameters.Add(n.Parameters[index].GetValue());
                        index++;
                    }
                    pms = parameters.ToArray();
                }
                node = NPCNode.CreateInstance(NodeTypes[n.NodeTypeName]);
                node.Initialize(pms);
                return node;
            }

            private void MarkTreeDirty(NPCNode node) {
                foreach(NPCNode n in node.Children) {
                    MarkTreeDirty(n);
                }
                EditorUtility.SetDirty(node);
            }

            private void NameTree(object o) {
                g_DisplayTreeName = !g_DisplayTreeName;
            }

            public void DrawNameTree(int id) {
                if (Instance.SelectedNode == null)
                    g_DisplayTreeName = false;
                else {
                    Instance.SelectedNode.TreeName = GUILayout.TextField(Instance.SelectedNode.TreeName);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Close", GUILayout.Width(65))) {
                        g_DisplayTreeName = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            private void LoadTreeInAgents(object o) {
                List<NPCController> agents = null;
                if(o == null) {
                    agents = new List<NPCController>();
                    foreach (GameObject obj in Selection.gameObjects) {
                        NPCController c = obj.GetComponent<NPCController>();
                        if (c != null)
                            agents.Add(c);
                    }
                } else {
                    agents = (List<NPCController>) o;
                }
                foreach (NPCController agent in agents) {
                    agent.AI.LoadBehavior(g_LastBakedTree);
                    UnityEngine.Object prefab = PrefabUtility.GetPrefabObject(agent);
                    if (prefab != null)
                        PrefabUtility.ReplacePrefab(agent.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(agent), ReplacePrefabOptions.Default);
                }
            }

            private void AttachBlackboard(object obj) {
                Node node = (Node) obj;
                Blackboard bb = CreateInstance<Blackboard>();
                bb.Init(node);
                node.Blackboard = bb;
                AssetDatabase.AddObjectToAsset(bb, TMP_FILE);
                foreach(Node n in node.Children) {
                    SetBlackboard(n, bb);
                }
            }

            public void SetBlackboard(Node n, Blackboard blackboard) {
                foreach(Node c in n.Children) {
                    SetBlackboard(c, blackboard);
                }
                if(n.isLeaf)
                    n.Blackboard = blackboard;
            }

            public void RemoveBlackboard(object obj) {
                Node node = (Node) obj;
                if (node.Blackboard != null) {
                    Blackboard bb = node.Blackboard;
                    node.Blackboard = null;
					foreach (Node n in SessionNodes.Nodes) {
						// To ensure removal even if connections were removed
						if(n.Blackboard == bb)
							SetBlackboard (n, null);
					}
                    DestroyImmediate(bb, true);
                }
            }

            #endregion

            public class ParametersMenu {

                public Vector2 g_ScrollPosition = new Vector2(0, 0);
                public int g_Width, g_Height;
                public Rect g_Rect;
                public int g_Id;
                public bool g_Dragged = false;

                public ParametersMenu(NPCBehaviors_Editor parent, int width, int height, int id) {
                    g_Width = width;
                    g_Height = height;
                    g_Id = id;
                }

                public void Draw() {
                    Node node = Instance.SelectedNode;
                    if (node != null && !string.IsNullOrEmpty(node.NodeTypeName) && 
                        (node.isLeaf || (node.Blackboard != null && node.Blackboard.Root == node))) {
                        g_Rect = new Rect(5, Instance.position.height - g_Height, g_Width, g_Height - 5);
                        if (node.Blackboard == null || node.Blackboard.Root != node) {
                            // Draw regular parameters
                            g_Rect = GUI.Window(g_Id, g_Rect, DrawParametersMenu, PARAMETERS_TITLE);
                        } else {
                            // Draw the blackboard
                            Rect extended = g_Rect;
                            extended.width += 70;
                            extended.height += 50;
                            extended.y -= 50;
                            g_Rect = GUI.Window(g_Id, extended, node.Blackboard.Draw, BLACKBOARD_TITLE);
                        }
                    }
                }

                private void DrawParametersMenu(int id) {
                    if (!Instance.NodeTypes.ContainsKey(Instance.SelectedNode.NodeTypeName)) return;
                    Node node = Instance.SelectedNode;
                    // this would happen if Unity reloads scripts and the root of the blackboard hasn't been drawn.
                    if(node.Blackboard != null && node.Blackboard.AvailableParameters == null) node.Blackboard.LoadBlackboardData();
                    bool hasBlackboard = node.Blackboard != null;
                    Type currentType = Instance.NodeTypes[node.NodeTypeName];
                    List<ParameterInfo> parameters = new List<ParameterInfo>();
                    ConstructorInfo cons;
                    if (node.Parameters == null)
                        node.Parameters = new List<NodeParam>();
                    g_ScrollPosition = GUILayout.BeginScrollView(g_ScrollPosition);
                    if (
                        (cons = currentType.GetConstructor(new Type[] { typeof(long) })) != null ||
                        (cons = currentType.GetConstructor(new Type[] { typeof(NPCAffordance) })) != null ||
                        (cons = currentType.GetConstructor(new Type[] { typeof(NPCAssertion) })) != null) {
                        parameters.AddRange(cons.GetParameters());
                    }
                    int index = 0;
                    foreach (ParameterInfo p in parameters) {
                        GUILayout.Label(p.Name);
                        if (IsNumeric(p.ParameterType)) { 
                            NodeParam param = node.Parameters.SingleOrDefault(par => IsNumeric(par.GetValue()));
                            if (param == null) {
                                param = new NodeParam();
                                param.SetValue(0);
                                node.Parameters.Add(param);
                            }
                            long val = EditorGUILayout.IntField(Convert.ToInt32(param.GetValue()));
                            node.Parameters[index].SetValue(val, typeof(long));
                        } else if (p.ParameterType == typeof(NPCAssertion)) {
                            NodeParam param = node.Parameters.SingleOrDefault(par => par.GetValue() is NPCAssertion);
                            if (param == null) {
                                NodeParam par = new NodeParam();
                                par.SetValue(new NPCAssertion());
                                node.Parameters.Add(par);
                            }
                            NPCAssertion assertion = (NPCAssertion) node.Parameters[index].GetValue();
                            GUILayout.BeginVertical("Box");
                            if (hasBlackboard && node.Blackboard.Parameters.Count > 0) {
                                GUILayout.BeginVertical("Box");
                                GUILayout.Label("Set Result in Blackboard");
                                GUILayout.BeginHorizontal();
                                assertion.SetResultInBlackboard = EditorGUILayout.Toggle(assertion.SetResultInBlackboard);
                                if (assertion.SetResultInBlackboard) {
                                    if (GUILayout.Button((assertion.BlackboardValue == null ? "" : assertion.BlackboardValue), GUILayout.Width(100))) {
                                        GenericMenu genericMenu = new GenericMenu();
                                        foreach (NodeParam nodeParam in node.Blackboard.Parameters) {
                                            genericMenu.AddItem(new GUIContent(nodeParam.Name), false, (object val) => {
                                                assertion.BlackboardValue = ((NodeParam)val).Name;
                                            }, nodeParam);
                                        }
                                        genericMenu.ShowAsContext();
                                    }
                                } else assertion.BlackboardValue = null;
                                GUILayout.EndHorizontal();
                                GUILayout.EndVertical();
                            } else assertion.SetResultInBlackboard = false;
                            NPCAssertion.TARGET origTarget = assertion.Target;
                            NPCAssertion.ASSERT origAssert = assertion.Assert;
                            // Set the target and the type of assertion
                            assertion.Target = (NPCAssertion.TARGET) EditorGUILayout.EnumPopup(assertion.Target);
                            assertion.Assert = (NPCAssertion.ASSERT) EditorGUILayout.EnumPopup(assertion.Assert);
                            // Reset the Property/Assert if conditions changed
                            if (assertion.Target != origTarget && assertion.Assert == NPCAssertion.ASSERT.PROPERTY)
                                assertion.Property = null;
                            if (assertion.Assert != origAssert)
                                assertion.TargetValue = new Parameter();
                            // Populate params fields
                            switch (assertion.Assert) {
                                case NPCAssertion.ASSERT.PROPERTY:
                                    GUILayout.Label("Property");
                                    if(GUILayout.Button((assertion.Property == null ? "" : assertion.Property.Name))) {
                                        GenericMenu genericMenu = new GenericMenu();
                                        if (assertion.Target == NPCAssertion.TARGET.AGENT || assertion.Target == NPCAssertion.TARGET.SELF) {
                                            foreach (PropertyInfo prop in Instance.EntitiesProperties[typeof(NPCController)]) {
                                                genericMenu.AddItem(new GUIContent(prop.Name), false, (object val) => {
                                                    assertion.Property = (PropertyInfo) val;
                                                    assertion.TargetValue = new Parameter();
                                                }, prop);
                                            }
                                        }
                                        genericMenu.ShowAsContext();
                                    }
                                    if(assertion.Property != null) {
                                        GUILayout.Label("Target Value");
                                        GUILayout.BeginHorizontal();
                                        // Is Numeric
                                        if (assertion.Property.PropertyType == typeof(float) || assertion.Property.PropertyType == typeof(int)) {
                                            assertion.EqualityOperation = (NPCAssertion.OPERATION)EditorGUILayout.EnumPopup(assertion.EqualityOperation, GUILayout.Width(g_Rect.width * 0.5f));
                                            float val = assertion.TargetValue.GetValue() != null ? Convert.ToSingle(assertion.TargetValue.GetValue()) : 0;
                                            assertion.TargetValue.SetValue(EditorGUILayout.FloatField(val));
                                        }
                                        // Is Boolean
                                        if (assertion.Property.PropertyType == typeof(bool)) {
                                            try {
                                                assertion.TargetValue.SetValue(GUILayout.Toggle(
                                                    Convert.ToBoolean(assertion.TargetValue.GetValue() ?? Boolean.FalseString), 
                                                    assertion.Property.Name));
                                            } catch {
                                                assertion.TargetValue = null;
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                    break;
                                case NPCAssertion.ASSERT.TAG:
                                    // TODO - fix this
                                    GUILayout.Label("Has Tag");
                                    assertion.TargetValue.SetValue(GUILayout.TextField((assertion.TargetValue.GetValue() ?? String.Empty).ToString()));
                                    break;
                                case NPCAssertion.ASSERT.TRANSFORM:
                                    GUILayout.Label("Criteria");
                                    assertion.TransformAssert = (NPCAssertion.TRANSFORM_ASSERT) EditorGUILayout.EnumPopup(assertion.TransformAssert);
                                    GUILayout.BeginHorizontal();
                                    assertion.EqualityOperation = (NPCAssertion.OPERATION)EditorGUILayout.EnumPopup(assertion.EqualityOperation, GUILayout.Width(g_Rect.width * 0.5f));
                                    assertion.TargetValue.SetValue(GUILayout.TextField((assertion.TargetValue.GetValue() ?? String.Empty).ToString()));
                                    GUILayout.EndHorizontal();
                                    break;
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Negate Result");
                            assertion.Negate = EditorGUILayout.Toggle(assertion.Negate);
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        } else if (p.ParameterType == typeof(NPCAffordance)) { 
                            NodeParam par = node.Parameters.SingleOrDefault(param => param.GetValue() is NPCAffordance);
                            NPCAffordance selectedAffordance = par == null ? null : (NPCAffordance)node.Parameters[index].GetValue();
                            GUILayout.BeginVertical("Box");
                            GUILayout.Label(selectedAffordance == null ? "None Selected" :
                                selectedAffordance.Name);
                            GUILayout.EndVertical();
                            if (selectedAffordance != null) {
                                MethodInfo mi = Instance.Affordances[selectedAffordance.Name];
                                GUILayout.BeginVertical("Box");
                                int paramIndex = 0;
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Override Agent");
                                selectedAffordance.OverrideAgent = EditorGUILayout.Toggle(selectedAffordance.OverrideAgent);
                                GUILayout.EndHorizontal();
                                if (selectedAffordance.OverrideAgent) {
                                    GUILayout.Label("Agent");
									if (hasBlackboard && node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.AGENT].Count > 0) {
                                        if(GUILayout.Button(new GUIContent(selectedAffordance.AgentName))) {
                                            GenericMenu toolsMenu = new GenericMenu();
                                            foreach (NodeParam agentParam in node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.AGENT]) {
                                                string agentName = agentParam.Name;
                                                toolsMenu.AddItem(new GUIContent(agentName), false, (o) => {
                                                    selectedAffordance.AgentName = agentParam.Name;
                                                    //System.Object obj = agentParam.GetValue();
                                                    //if (obj != null) {
                                                    //    GameObject a = (GameObject) obj;
                                                    //    if (a != null && a.GetComponent<NPCController>() != null)
                                                    //        selectedAffordance.AgentName = a.name;
                                                    //}
                                                }, agentParam);
                                            }
                                            toolsMenu.ShowAsContext();
                                            EditorGUIUtility.ExitGUI();
                                        }
									} else {
										GameObject obj = (GameObject)EditorGUILayout.ObjectField (selectedAffordance.AgentName == null ? null :
                                        GameObject.Find(selectedAffordance.AgentName), typeof(GameObject), true);
										if (obj != null && obj.GetComponent<NPCController> () != null)
											selectedAffordance.AgentName = obj.GetComponent<NPCController>().name;
									}
                                }
                                foreach (ParameterInfo param in mi.GetParameters()) {
									GUILayout.Label(param.Name);
									if (hasBlackboard) {
                                        Parameter nodeParam = selectedAffordance.Parameters[paramIndex];
                                        List<NodeParam> availableParams = null;
                                        Parameter.PARAM_TYPE type = Parameter.PARAM_TYPE.UNASSIGNED;
                                        if (param.ParameterType == typeof(Transform)) {
                                            type = Parameter.PARAM_TYPE.TRANSFORM;
                                            availableParams = node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.TRANSFORM];
                                            availableParams.AddRange(node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.GAMEOBJECT]);
                                        } else if (param.ParameterType == typeof(bool)) {
                                            type = Parameter.PARAM_TYPE.BOOL;
                                            availableParams = node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.BOOL];
                                        } else if (param.ParameterType == typeof(GameObject)) {
                                            type = Parameter.PARAM_TYPE.GAMEOBJECT;
                                            availableParams = node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.GAMEOBJECT];
                                        } else if (param.ParameterType == typeof(Single) || param.ParameterType == typeof(int)) {
                                            type = Parameter.PARAM_TYPE.NUMERICAL;
                                            availableParams = node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.NUMERICAL];
                                        } else if (param.ParameterType == typeof(Single) || param.ParameterType == typeof(string)) {
                                            type = Parameter.PARAM_TYPE.STRING;
                                            availableParams = node.Blackboard.AvailableParameters[Parameter.PARAM_TYPE.STRING];
                                        }
										if (availableParams != null && availableParams.Count > 0) {
											string currParam = selectedAffordance.Parameters [paramIndex].ParameterName ?? "";
											if (GUILayout.Button (new GUIContent (currParam))) {
												GenericMenu toolsMenu = new GenericMenu ();
												foreach (NodeParam np in availableParams) {
													toolsMenu.AddItem (new GUIContent (np.Name), false, (o) => {
														selectedAffordance.Parameters[paramIndex].ParameterName = (string) o;
                                                        selectedAffordance.Parameters[paramIndex].SetValue(node.Blackboard.GetParamValue((string) o, type));
                                                    }, np.Name);
												}
												toolsMenu.ShowAsContext ();
												EditorGUIUtility.ExitGUI ();
											}
											paramIndex++;
											continue;
										}
                                    }
                                    selectedAffordance.Parameters[paramIndex].ParameterName = param.Name;
                                    object val = selectedAffordance.Parameters[paramIndex].GetValue();
                                    if (param.ParameterType == typeof(Transform)) {
										if (!(val is Transform)) val = null;
                                        object o = EditorGUILayout.ObjectField((Transform) val, typeof(Transform), true);
                                        if (o != null)
                                            selectedAffordance.Parameters[paramIndex].SetValue(o, typeof(Transform));
                                    } else if (param.ParameterType == typeof(bool)) {
										if (!(val is Boolean)) val = null;
                                        selectedAffordance.Parameters[paramIndex].SetValue(GUILayout.Toggle((bool)(val ?? false), ""));
                                    } else if (param.ParameterType.IsEnum) {
										if (!(val is Enum)) val = null;
                                        // If none has been selected, pick the first one by default
                                        Enum v = val == null ? (Enum)(Enum.GetValues(param.ParameterType).GetValue(0)) : (Enum)val;
                                        selectedAffordance.Parameters[paramIndex].SetValue(
                                            EditorGUILayout.EnumPopup(v)
                                            );
                                        // For any other parameter types, take text and let the caller 
                                        // be responsible for properly casting
                                    } else if (param.ParameterType == typeof(System.Object)) {
										if (!(val is System.Object)) val = null;
                                        selectedAffordance.Parameters[paramIndex].SetValue(
                                            EditorGUILayout.TextField(val == null ? "" : val.ToString())
                                            );
                                    } else if (IsNumeric(param.ParameterType)) {
                                        if (!(val is Single)) val = null;
                                        selectedAffordance.Parameters[paramIndex].SetValue(
                                            EditorGUILayout.FloatField(val == null ? 0 : Convert.ToSingle(val))
                                            );
                                    }
                                    paramIndex++;
                                }
                                GUILayout.EndVertical();
                                
                            }
                        }
                        index++;
                    }
                    GUILayout.EndScrollView();
                }

                public bool ProcessParamsMenuEvents(Event e) {
                    switch (e.type) {
                        case EventType.MouseDown:
                            if (g_Rect.Contains(e.mousePosition)) {
                                g_Dragged = true;
                                return true;
                            }
                            break;
                        case EventType.MouseUp:
                            if (g_Dragged) {
                                g_Dragged = false;
                            }
                            break;
                        case EventType.MouseDrag:
                            if (g_Dragged)
                                return true;
                            break;
                    }
                    return false;
                }

                private bool IsNumeric(Type t) {
                    return t != null &&
                        (t == typeof(float)
                        || t == typeof(int)
                        || t == typeof(long));
                }

                private bool IsNumeric(object v) {
                    return v is float || v is long || v is int;
                }
            }
            
            public class SideMenu {

                public enum TAB {
                    AFFORDANCES = 0,
                    LIBRARY = 1
                }

                public string g_AffFilter, g_LibraryFilter;
                public int g_Id;
                private Vector2 g_AffordancesScrollPosition;
                private Vector2 g_LibraryScrollPosition;
                public Rect g_Rect;
                public bool g_Selected = false;
                public bool g_Dragged = false;
                public int g_Width;
                public GUIStyle currentStyle;
                public GUIStyle g_Style;
                public TAB g_SelectedTab = TAB.AFFORDANCES;


                public SideMenu(NPCBehaviors_Editor parent, Rect rect, int id) {
                    g_AffordancesScrollPosition = new Vector2();
                    g_LibraryScrollPosition = new Vector2();
                    g_Width = 200;
                    g_Id = id;
                }

                public void Draw() {
                    g_Rect = new Rect(Instance.position.width - g_Width - 5, 20, g_Width, Instance.position.height - 25);
                    g_Rect = GUI.Window(g_Id, g_Rect, DrawSideMenu, BEHAVIORS_TITLE);
                }

                private void DrawSideMenu(int id) {
                    GUILayout.BeginVertical();
                    g_SelectedTab = (TAB)GUILayout.Toolbar((int)g_SelectedTab, new string[] { "Affordances", "Library" });
                    
                    // Draw Tabs
                    switch (g_SelectedTab) {
                        case TAB.AFFORDANCES:
                            DrawAffordances();
                            break;
                        case TAB.LIBRARY:
                            DrawLibrary();
                            break;
                    }

                    GUILayout.EndVertical();
                }

                private void DrawLibrary() {
                    if(Instance.TreeLibrary.Trees.Count > 0) {
                        // Draw Filter
                        GUILayout.BeginVertical("Box");
                        GUILayout.Label("Filter");
                        g_LibraryFilter = EditorGUILayout.TextField(g_LibraryFilter);
                        GUILayout.EndVertical();
                        bool populated = false;
                        g_LibraryScrollPosition = GUILayout.BeginScrollView(g_LibraryScrollPosition, new GUIStyle("Box"));
                        foreach (var val in Instance.TreeLibrary.Trees) {
                            if (String.IsNullOrEmpty(g_LibraryFilter) || val.TreeName.ToLower().IndexOf(g_LibraryFilter.ToLower()) == 0) {
                                GUILayout.BeginVertical(Instance.treeStyle);
                                GUILayout.Label(val.TreeName, Instance.treeLabelStyle, GUILayout.ExpandWidth(true));
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Add", Instance.treeButtonStyle, GUILayout.Width(80))) {
                                    Instance.ManageLibraryTree("add_workspace", val.TreeName);
                                }
                                if(GUILayout.Button("Remove", Instance.treeButtonStyle, GUILayout.Width(80))) {
                                    Instance.ManageLibraryTree("delete", val.TreeName);
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.EndVertical();
                                populated = true;
                            }
                        }
                        if (populated) {

                        } else {
                            GUILayout.Label("No matches", new GUIStyle("Box"), GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndScrollView();
                    } else {
                        GUILayout.Label("No Trees Available", new GUIStyle("Box"), GUILayout.ExpandWidth(true));
                    }
                }

                private void DrawAffordances() {
                    if(Instance.Affordances.Count > 0) {
                        // Draw Filter
                        GUILayout.BeginVertical("Box");
                        GUILayout.Label("Filter");
                        g_AffFilter = EditorGUILayout.TextField(g_AffFilter);
                        GUILayout.EndVertical();
                        bool populated = false;
                        g_AffordancesScrollPosition = GUILayout.BeginScrollView(g_AffordancesScrollPosition, new GUIStyle("Box"));
                        foreach (var val in Instance.Affordances) {
                            if (String.IsNullOrEmpty(g_AffFilter) || val.Key.Contains(g_AffFilter)) {
                                populated = true;
                                if (GUILayout.Button(new GUIContent(val.Key), Instance.affordanceStyle)) {
                                    if(Instance.SelectedNode != null && Instance.SelectedNode.NodeTypeName != null 
                                        && Instance.NodeTypes[Instance.SelectedNode.NodeTypeName] == typeof(NPCAction)) {
                                        NodeParam curr = Instance.SelectedNode.Parameters.SingleOrDefault(p => p.GetValue() is NPCAffordance);
                                        if (curr == null || ((NPCAffordance)curr.GetValue()).Name != val.Key) {
                                            NPCAffordance aff = new NPCAffordance(val.Key);
                                            NodeParam par = new NodeParam();
                                            par.SetValue(aff);
                                            if (curr != null) Instance.SelectedNode.Parameters.Remove(curr);
                                            Instance.SelectedNode.Parameters.Add(par);
                                            aff.Method = val.Value;
                                        }
                                    }
                                }
                            }
                        }
                        if(!populated) {
                            GUILayout.Label("No Affordances Available", GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndScrollView();
                    }
                }

                public void Drag(Vector2 delta) {
                    g_Rect.position += delta;
                }

                public bool ProcessSideMenuEvents(Event e) {
                    switch (e.type) {
                        case EventType.MouseDown:
                            if (g_Rect.Contains(e.mousePosition)) {
                                g_Dragged = true;
                                return true;
                            }
                            break;
                        case EventType.MouseUp:
                            if (g_Dragged) {
                                g_Dragged = false;
                            }
                            break;
                        case EventType.MouseDrag:
                            if (g_Dragged)
                                return true;
                            break;
                    }
                    return false;
                }
            }
        }   
    }
}