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
    /// This class is used to represent an executable action from an entity.
    /// This action, can be used via Execute, and will have a BEHAVIOR_STATUS result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Serializable]
    public class NPCAffordance : System.Attribute {

        #region Members
        
        private MethodInfo g_Method;
        
        [SerializeField]
        private string MethodName;
        
        /// <summary>
        /// Informal name of the affordance
        /// </summary>
        public string Name;

        /// <summary>
        /// Ordered objects to parameterize affordance
        /// </summary>
        public List<Parameter> Parameters;

        /// <summary>
        /// Set to true if other agent than the main one is to execute the affordance
        /// within the same tree
        /// </summary>
        public bool OverrideAgent;

        /// <summary>
        /// To be set in editor mode if a secondary agent is to be specified
        /// </summary>
        [SerializeField]
        private string g_AgentName;

        /// <summary>
        /// The instance of the entity which contains that method
        /// </summary>
        [NonSerialized, HideInInspector]
        public NPCController Agent;
        
        #endregion

        #region Properties

        /// <summary>
        /// Getter and setter for alternative execution agent.
        /// </summary>
        public string AgentName {
            get {
                return g_AgentName;
            }
            set {
                g_AgentName = value;
            }
        }

        /// <summary>
        /// Convenience to parameterize and capture the method's data
        /// </summary>
        public MethodInfo Method {
            get {
                return g_Method;
            }
            set {
                Parameters = new List<Parameter>();
                foreach (var val in value.GetParameters())
                    Parameters.Add(new Parameter());
                MethodName = value.Name;
                g_Method = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Allows to create an empty NPCAffordance.
        /// </summary>
        public NPCAffordance() {
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// Initialize a NPCAffordance with an informal name.
        /// </summary>
        /// <param name="name"></param>
        public NPCAffordance(string name) : this() {
            Name = name;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes and returns the value of the intended method, for the specified entity instance.
        /// </summary>
        /// <returns></returns>
        public BEHAVIOR_STATUS Execute() {
            try {
                object[] pms = new object[Parameters.Count];
                for (var i = 0; i < Parameters.Count; ++i) {
                    pms[i] = Parameters[i].GetValue();
                }
                if(OverrideAgent) {
                    GameObject go = GameObject.Find(AgentName);
                    if (go != null) {
                        NPCController agent = go.GetComponent<NPCController>();
                        if (agent == null) {
                            Debug.LogWarning("GameObject 'AgentName' is not an agent: " + this);
                        } else {
                            AgentName = agent.name;
                            Agent = agent;
                        }
                    } else {
                        Debug.LogWarning("Invalid AgentName for affordance: " + this);
                    }
                }
                if(Agent.AI != null)
                {
                    return (BEHAVIOR_STATUS)Agent.AI.GetType().GetMethod(MethodName).Invoke(Agent.AI, pms);
                }
                return BEHAVIOR_STATUS.SUCCESS;
            } catch(Exception e) {
                Debug.LogError("Couldn't execute affordance due to: " + e.Message);
                return BEHAVIOR_STATUS.FAILURE;

            }
        }
        
        #endregion
        
    }

}
