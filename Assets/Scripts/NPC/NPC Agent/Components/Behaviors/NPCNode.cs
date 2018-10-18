using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// This is the main class which provides basic members and
    /// functionality for all the possible types of nodes.
    /// It was originally abstract, but due to Serialization requirements
    /// was instead turned into a regular class
    /// </summary>
    [Serializable]
    public abstract class NPCNode : ScriptableObject {

        #region Members

        public List<NPCNode> Children;

        [SerializeField, HideInInspector]
        protected BEHAVIOR_STATUS g_Status;
        
        protected IEnumerator<BEHAVIOR_STATUS> g_Current;
        
        protected NPCController MainAgent;

        [SerializeField]
        public NPCBlackboard Blackboard;

        #endregion

        #region Properties

        /// <summary>
        /// Latest updated status of this behavior.
        /// </summary>
        public BEHAVIOR_STATUS Status {
            get {
                return g_Status;
            }
            private set {
                g_Status = value;
            }
        }

        public bool Finished {
            get {
                return g_Status == BEHAVIOR_STATUS.FAILURE
                    || g_Status == BEHAVIOR_STATUS.SUCCESS
                    || g_Status == BEHAVIOR_STATUS.STOPPED;
            }
        }

        #endregion

        #region Public
        
        public abstract void Initialize(object[] parameters);

        public virtual void SetMainAgent(NPCController Agent) {
            MainAgent = Agent;
        }

        public new static NPCNode CreateInstance(Type nodeType) {
            NPCNode obj = (NPCNode) ScriptableObject.CreateInstance(nodeType);
            obj.Children = new List<NPCNode>();
            obj.Status = BEHAVIOR_STATUS.PENDING;
            return obj;
        }

        public void Start() {
            g_Status = BEHAVIOR_STATUS.STARTED;
            g_Current = Execute().GetEnumerator();
        }

        public void Stop() {
            g_Status = BEHAVIOR_STATUS.STOPPED;
        }

        public void Interrupt() {
            g_Status = BEHAVIOR_STATUS.INTERRUPTED;
        }

        public BEHAVIOR_STATUS UpdateNode() {
            if (g_Current.MoveNext())
                g_Status = g_Current.Current;
            return g_Status;
        }

        public virtual bool AddChild(NPCNode n) {
            if (Children != null) {
                Children.Add(n);
                return true;
            }
            return false;
        }

        #endregion

        #region Protected

        protected abstract IEnumerable<BEHAVIOR_STATUS> Execute();
    
        protected NPCNode() {
            Children = new List<NPCNode>();
            Status = BEHAVIOR_STATUS.PENDING;
        }

        protected NPCNode(NPCNode[] children) : this() {
            if(children != null)
                Children.AddRange(children);
        }

        #endregion
    }
    
}
