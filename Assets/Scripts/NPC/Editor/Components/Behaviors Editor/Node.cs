using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    namespace Behavior {

        [Serializable]
        public class Node : ScriptableObject {

            Rect inOutPointRect = new Rect(0, 0, 20f, 10f);

            public Type NodeType;
            public List<Node> Children;
            public List<NodeParam> Parameters;
            public Node Parent;
            public Rect rect;
            public Rect InPointRect, OutPointRect;
            public string title;
            public Blackboard Blackboard;
            public int NodeID;
            public string TreeName;
            // Node is hidden
            public bool Hidden;
            // Hidding subtree
            public bool Hidding;

            [HideInInspector]
            public Vector2 ParentOffset;

            [NonSerialized]
            public bool isDragged;

            [NonSerialized]
            public bool isSelected;

            public GUIStyle style;
            public GUIStyle defaultNodeStyle;
            public GUIStyle selectedNodeStyle;
            public GUIStyle hiddingNodeStyle;
            public GUIStyle hiddingSelectedNodeStyle;

            public string NodeTypeName = "";

            public void Init(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle hiddingStyle, GUIStyle hiddingSelected, NPCBehaviors_Editor parent, GUIStyle inPointStyle, GUIStyle outPointStyle) {
                rect = new Rect(position.x, position.y, width, height);
                style = nodeStyle;
                defaultNodeStyle = nodeStyle;
                selectedNodeStyle = selectedStyle;
                hiddingNodeStyle = hiddingStyle;
                hiddingSelectedNodeStyle = hiddingSelected;
                Children = new List<Node>();
                Parameters = new List<NodeParam>();
                ParentOffset = new Vector2();
            }

            public void OnEnable() {
                hideFlags = HideFlags.HideInHierarchy;
            }

            public bool isLeaf {
                get {
                    return 
                        string.IsNullOrEmpty(NodeTypeName) ||
                        NodeTypeName.Contains("Wait") ||
                        NodeTypeName.Contains("Action") ||
                        NodeTypeName.Contains("Assert");
                }
            }

            public bool IsTree {
                get {
                    return !isLeaf && Children.Count > 0;
                }
            }

            public void Drag(Vector2 delta) {
                rect.position += delta;
            }

            public void Draw() {
                // Capture the parent offset
                if (Parent != null)
                    ParentOffset = rect.position - Parent.rect.position;
                else
                    ParentOffset = new Vector2();
                
                // Check Hidden
                if (Hidden) return;
                
                // Check Selected
                if (isSelected) {
                    style = Hidding ? hiddingSelectedNodeStyle : selectedNodeStyle;
                } else {
                    style = Hidding ? hiddingNodeStyle : defaultNodeStyle;
                }
                
                // Draw Connections
                inOutPointRect.x = rect.x + (rect.width * 0.5f) - inOutPointRect.width * 0.5f;
                InPointRect = OutPointRect = inOutPointRect;
                InPointRect.y = rect.y - InPointRect.height + 10f;
                if (GUI.Button(InPointRect, "", NPCBehaviors_Editor.Instance.inPointStyle)) {
                    NPCBehaviors_Editor.Instance.OnClickInPoint(this);
                }
                if (!(isLeaf || Hidding)) {
                    OutPointRect.y = rect.y + rect.height - 10f;
                    if (GUI.Button(OutPointRect, "", NPCBehaviors_Editor.Instance.outPointStyle)) {
                        NPCBehaviors_Editor.Instance.OnClickOutPoint(this);
                    }
                }
                
                Node toRemove = null;

                foreach (Node c in Children)
                    DrawConnection(c, out toRemove);

                if (toRemove != null) {
                    NPCBehaviors_Editor.Instance.OnClickRemoveConnection(this, toRemove);
                }

                // Draw Content
                style.padding = new RectOffset(10, 10, 10, 10);
                style.normal.textColor = Color.white;
                GUILayout.BeginArea(rect, title, style);
                GUIStyle labelStyle = new GUIStyle(); labelStyle.normal.textColor = Color.white;
                GUILayout.Label("Node: " + NodeID, labelStyle);
                if (!Hidding) {
                    if (GUILayout.Button(new GUIContent(NodeTypeName))) {
                            GenericMenu genericMenu = new GenericMenu();
                            foreach (string nodeKey in NPCBehaviors_Editor.Instance.NodeTypes.Keys) {
                                genericMenu.AddItem(new GUIContent(nodeKey), false, UpdateNodeType, nodeKey);
                            }
                            genericMenu.ShowAsContext();
                    }
                } else {
                    GUILayout.Label(new GUIContent(NodeTypeName), new GUIStyle("Box"), GUILayout.ExpandWidth(true));
                }
                GUILayout.EndArea();
            }

            public void DrawConnection(Node child, out Node n) {
                n = null;
                if (child.Hidden) return;
                Handles.DrawBezier(
                    child.InPointRect.center,
                    this.OutPointRect.center,
                    child.InPointRect.center + Vector2.down * 50f,
                    this.OutPointRect.center - Vector2.down * 50f,
                    Color.white,
                    null,
                    2f
                );

                if (Handles.Button((child.rect.center + this.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap)) {
                    n = child;
                }
            }

            private void UpdateNodeType(object o) {
                string nodeKey = (string)o;
                NodeType = NPCBehaviors_Editor.Instance.NodeTypes[nodeKey];
                NodeTypeName = nodeKey;
                Parameters.Clear();
                if (isLeaf && Children.Count > 0) {
                    List<Node> children = new List<Node>(Children.ToArray());
                    foreach (Node c in children)
                        NPCBehaviors_Editor.Instance.OnClickRemoveConnection(this, c);
                }
                if (isLeaf)
                    NPCBehaviors_Editor.Instance.RemoveBlackboard(this);
            }

            public bool ProcessEvents(Event e) {
                switch (e.type) {
                    case EventType.MouseDown:
                        if (e.button == 0) {
                            if (rect.Contains(e.mousePosition)) {
                                isDragged = true;
                                isSelected = true;
                                NPCBehaviors_Editor.Instance.SelectedNode = this;
                                GUI.changed = true;
                            } else {
                                isSelected = false;
                                GUI.changed = true;
                            }
                        }

                        if (e.button == 1 && isSelected && rect.Contains(e.mousePosition)) {
                            ProcessContextMenu();
                            e.Use();
                        }

                        break;

                    case EventType.MouseUp:
                        isDragged = false;
                        break;

                    case EventType.MouseDrag:
                        if (e.button == 0 && isDragged) {
                            Drag(e.delta);
                            if (Hidding)
                                foreach (Node c in Children)
                                    c.RecurseDrag(c, e.delta);
                            e.Use();
                            return true;
                        }
                        break;
                }
                return false;
            }

            private void RecurseDrag(Node n, Vector2 delta) {
                foreach (Node c in n.Children)
                    c.Drag(delta);
                n.Drag(delta);
            }

            private void ProcessContextMenu() {
                GenericMenu genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Remove node"), false, () => { NPCBehaviors_Editor.Instance.OnClickRemoveNode(this); });
                genericMenu.ShowAsContext();
            }

        }
    }
}