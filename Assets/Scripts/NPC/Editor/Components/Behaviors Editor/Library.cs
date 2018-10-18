using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    namespace Behavior {
        
        public class Library {
            
            public List<NodeList> Trees;

            public Library() {
                Trees = new List<NodeList>();
            }

            /// <summary>
            /// Returns a tree if exists, null otherwise.
            /// </summary>
            /// <param name="Name"></param>
            /// <returns>NodeList Tree or null</returns>
            public NodeList GetTree(string Name) {
                return Trees.SingleOrDefault((t => t.TreeName == Name));
            }

            /// <summary>
            /// Adds a new tree to the library
            /// </summary>
            /// <param name="Tree"></param>
            public void AddTree(NodeList Tree) {
                Trees.Add(Tree);
            }

            /// <summary>
            /// Removes a tree from the library
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            public bool RemoveTree(string Name) {
                NodeList t = GetTree(Name);
                if (t != null) {
                    Trees.Remove(t);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Creates a copy of a library tree
            /// </summary>
            /// <param name="root"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public Node CopyTree(Node root, Node parent = null, Dictionary<Node,Node> rels = null) {
                Node newNode = ScriptableObject.Instantiate<Node>(root);
                newNode.Parent = parent;
                newNode.Children.Clear();
                foreach(Node c in root.Children) {
                    newNode.Children.Add(CopyTree(c, newNode, rels));
                }
                if (rels != null)
                    rels.Add(newNode, root);
                return newNode;
            }

            /// <summary>
            /// Generates a sequential list of a hierarchy of nodes
            /// </summary>
            /// <param name="root"></param>
            /// <returns>A List of nodes</returns>
            public List<Node> GetNodesList(Node root) {
                List<Node> list = new List<Node>();
                AddToList(root, list);
                return list;
            }
            
            private void AddToList(Node n, List<Node> list) {
                list.Add(n);
                foreach (Node c in n.Children)
                    AddToList(c, list);
            }
        }
    }
}
