using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// Evaluates the given lambda function as a boolean condition.
    /// Returns success if the condition is true, failure otherwise.
    /// </summary>
    [Serializable]
    public class NPCAssert : NPCNode {

        private Func<bool> g_Assertion;

        [SerializeField]
        private NPCAssertion g_NPCAssertion;

        public NPCAssert(NPCAssertion Assertion) : base() {
            g_Assertion = Assertion.Evaluate;
        }

        public NPCAssert(Func<bool> Assertion) : base() {
            g_Assertion = Assertion;
        }

        public override void SetMainAgent(NPCController Agent) {
            if (g_NPCAssertion != null)
                g_NPCAssertion.Agent = Agent;
        }

        public override void Initialize(object[] parameters) {
            if (parameters[0] is NPCAssertion) {
                g_NPCAssertion = ((NPCAssertion) parameters[0]);
            } else {
                g_Assertion = (Func<bool>)parameters[0];
            }
        }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            switch (g_Status) {
                case BEHAVIOR_STATUS.STARTED:
                    if (g_NPCAssertion != null) g_Assertion = g_NPCAssertion.Evaluate;
                    bool result = g_Assertion.Invoke();
                    if (result) {
                        if (g_NPCAssertion != null && g_NPCAssertion.SetResultInBlackboard && Blackboard != null) {
                            if(Blackboard.HasParameter(g_NPCAssertion.BlackboardValue)) {
							
							if (!Blackboard.AddValue(g_NPCAssertion.BlackboardValue, g_NPCAssertion.Result)) {
                                    Debug.Log("Failed to add NPCAssertion result int NPCBlackboard");
                                }
                            }
                        }
                        g_Status = BEHAVIOR_STATUS.SUCCESS;
                    } else
                        g_Status = BEHAVIOR_STATUS.FAILURE;
                    break;
            }
            yield return g_Status;
        }
    }

}