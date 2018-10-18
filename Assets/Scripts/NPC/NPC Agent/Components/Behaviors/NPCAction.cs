using System;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// Executes the specific affordance until this either fails,
    /// returning failure or finishes, returning success.
    /// </summary>
    [Serializable]
    public class NPCAction : NPCNode {

        private Func<BEHAVIOR_STATUS> g_Action;

        [SerializeField]
        private NPCAffordance g_Affordance;

        public NPCAction(NPCAffordance Affordance) : base() {
            g_Affordance = Affordance;
        }

        public NPCAction(Func<BEHAVIOR_STATUS> Affordance) : base() {
            g_Action = Affordance;
        }

        public override void SetMainAgent(NPCController agent) {
            if (g_Affordance != null) {
                g_Affordance.Agent = agent;
            }
        }

        public NPCAffordance Affordance {
            get {
                return g_Affordance;
            }
        }

        public override void Initialize(object[] parameters) {
            if (parameters[0] is NPCAffordance) {
                g_Affordance = (NPCAffordance) parameters[0];
            } else {
                g_Action = (Func<BEHAVIOR_STATUS>) parameters[0];
            }
        }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            while (!Finished) {
                switch (g_Status) {
                    case BEHAVIOR_STATUS.STOPPED:
                        yield break;
                    default:
                        // The execution is responsible for always returning the correct
                        // BEHAVIOR_STATUS value
                        if (g_Affordance != null) {
                            // Parameterize affordance if needed
                            if (Blackboard != null) {
                                foreach (Parameter p in g_Affordance.Parameters) {
                                    if (Blackboard.HasParameter(p.ParameterName))
                                        p.SetValue(Blackboard.GetValue(p.ParameterName));
                                }
                                if (g_Affordance.OverrideAgent && Blackboard.HasParameter(g_Affordance.AgentName)) {
                                    g_Affordance.AgentName = ((GameObject)Blackboard.GetValue(g_Affordance.AgentName)).name;
                                }
                            }
                                yield return g_Affordance.Execute();
                        } else
                            yield return g_Action.Invoke();
                        break;
                }
            }
        }
    }

}