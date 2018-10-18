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
        public class Blackboard : ScriptableObject {

            [SerializeField]
            Node TreeRoot;

            [SerializeField]
            public List<NodeParam> Parameters;
            
            public Dictionary<Parameter.PARAM_TYPE, List<NodeParam>> AvailableParameters;
            
            List<Node> ActionNodes;

            Vector2 g_ScrollPosition;

            public Node Root {
                get {
                    return TreeRoot;
                }
                set {
                    TreeRoot = value;
                }
            }

            public void OnEnable() {
                ActionNodes = new List<Node>();
                hideFlags = HideFlags.HideInHierarchy;
            }

            public void Init(Node root) {
                TreeRoot = root;
                Parameters = new List<NodeParam>();
                ActionNodes = new List<Node>();
                g_ScrollPosition = new Vector2(0, 0);
                AvailableParameters = new Dictionary<Parameter.PARAM_TYPE, List<NodeParam>>();
            }

            public void Draw(int id) {
                // Init available params
                AvailableParameters = new Dictionary<Parameter.PARAM_TYPE, List<NodeParam>>();
                foreach (var val in Enum.GetValues(typeof(Parameter.PARAM_TYPE))) {
                    AvailableParameters.Add((Parameter.PARAM_TYPE)val, new List<NodeParam>());
                }
                g_ScrollPosition = GUILayout.BeginScrollView(g_ScrollPosition);
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(new GUIContent("Parameters"));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("+"))) {
                    AddParameter();
                }
                GUILayout.EndHorizontal();
                // Draw Parameters
                if (Parameters.Count > 0) {
                    NodeParam toRemove = null;
                    GUILayout.BeginVertical("Box");
                    int index = 1;
                    foreach (NodeParam p in Parameters) {
                        GUILayout.BeginVertical("Box");
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if(GUILayout.Button(new GUIContent("-"))) {
                            toRemove = p;
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        string paramName = string.IsNullOrEmpty(p.Name) ? "P" + index : p.Name;
                        GUILayout.Label(new GUIContent("Name"));
                        p.Name = GUILayout.TextField(paramName);
                         // = EditorGUILayout.TextField("Name", paramName);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Type");
                        GUILayout.FlexibleSpace();
                        p.AssignedType = (Parameter.PARAM_TYPE) EditorGUILayout.EnumPopup(p.AssignedType);
                        AvailableParameters[p.AssignedType].Add(p);
                        GUILayout.EndHorizontal();
                        var val = p.GetValue();
                        switch (p.AssignedType) {
                            case Parameter.PARAM_TYPE.BOOL:
                                if (!(val is Boolean)) val = false;
                                p.SetValue(GUILayout.Toggle((bool) val, new GUIContent("Value")));
                                break;
                            case Parameter.PARAM_TYPE.AGENT:
                                if (!(val is UnityEngine.Object)) val = null;
                                UnityEngine.Object a = EditorGUILayout.ObjectField((UnityEngine.Object)val ?? null, typeof(GameObject), true);
                                if (a != null && a is GameObject && ((GameObject)a).GetComponent<NPCController>() != null) {
                                    p.SetValue(a, typeof(UnityEngine.Object));
                                }
                                break;
                            case Parameter.PARAM_TYPE.GAMEOBJECT:
                                if (!(val is UnityEngine.Object)) val = null;
                                UnityEngine.Object o = EditorGUILayout.ObjectField((UnityEngine.Object) val ?? null, typeof(GameObject), true);
                                if (o != null) {
                                    p.SetValue(o, typeof(UnityEngine.Object));
                                }
                                break;
                            case Parameter.PARAM_TYPE.NUMERICAL:
                                if (!(val is Single)) val = 0;
                                p.SetValue(EditorGUILayout.FloatField(Convert.ToSingle(val)));
                                break;
                            case Parameter.PARAM_TYPE.TRANSFORM:
                                if (!(val is Transform)) val = null;
                                Transform t = (Transform) EditorGUILayout.ObjectField((Transform) val ?? null, typeof(Transform), true);
                                if (t != null)
                                    p.SetValue(t, typeof(Transform));
                                break;
                            case Parameter.PARAM_TYPE.STRING:
                                if (!(val is string)) val = null;
                                p.SetValue(EditorGUILayout.TextField(val == null ? "" : val.ToString()));
                                break;
                        }
                        GUILayout.EndVertical();
                        index++;
                    }
                    if(toRemove != null)
                        RemoveParameter(toRemove);
                    GUILayout.EndVertical();
                }
                // Draw Children
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label(new GUIContent("Action Nodes"));
                GUILayout.EndHorizontal();
                CollectActionChildren(TreeRoot);
                ActionNodes.Sort((Node n1, Node n2) => { return n1.NodeID.CompareTo(n2.NodeID); });
                foreach(Node n in ActionNodes) {
                    NodeParam currParam = n.Parameters.SingleOrDefault(param => param.GetValue() is NPCAffordance);
                    if (currParam != null) {
                        GUILayout.BeginVertical("Box");
                        NPCAffordance affordance = (NPCAffordance) currParam.GetValue();
                        GUILayout.Label("Node: " + n.NodeID + " - " + affordance.Name);
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndScrollView();
            }

            private void AddParameter() {
                NodeParam p = new NodeParam();
                Parameters.Add(p);
            }

            private void RemoveParameter(NodeParam param) {
                Parameters.Remove(param);
            }

            private void CollectActionChildren(Node n) {
                if (n == TreeRoot)
                    ActionNodes.Clear();
                foreach(Node c in n.Children) {
                    CollectActionChildren(c);
                    if (c.NodeTypeName.Contains("Action"))
                        ActionNodes.Add(c);
                }

            }

            public object GetParamValue(string s, Parameter.PARAM_TYPE type) {
                object o = null;
                foreach (NodeParam p in AvailableParameters[type]) {
                    if (p.Name == s)
                        o = p.GetValue();
                }
                return o;
            }

            public void LoadBlackboardData() {
                AvailableParameters = new Dictionary<Parameter.PARAM_TYPE, List<NodeParam>>();
                foreach (var val in Enum.GetValues(typeof(Parameter.PARAM_TYPE))) {
                    AvailableParameters.Add((Parameter.PARAM_TYPE)val, new List<NodeParam>());
                }
                foreach (NodeParam p in Parameters) {
                    AvailableParameters[p.AssignedType].Add(p);
                }
            }

        }

    }
}