using System;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// Executes its child until it fails, restarting everytime upon completion otherwise.
    /// If an assertion is provided, the child status is ignored, and always restarted upon termination,
    /// and it will only stop if the assertion returns failure.
    /// </summary>
    [Serializable]
    public class NPCDecoratorLoop : NPCNode {

        [SerializeField]
        private NPCAssert g_Assertion;
        
        private NPCNode g_Child;

        public override void Initialize(object[] parameters) { }

        public override bool AddChild(NPCNode node) {
            if (node != null) {
                Children = new List<NPCNode>();
                Children.Add(node);
                return true;
            }
            return false;
        }

        public NPCDecoratorLoop(NPCNode child = null) : base() {
            AddChild(child);
        }

        public NPCDecoratorLoop(NPCNode child, NPCAssert assertion) : this(child) {
            g_Assertion = assertion;
        }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            g_Status = BEHAVIOR_STATUS.RUNNING;
            // Only a single node will be considered
            g_Child = Children[0];
            g_Child.Start();
            while (!Finished) {
                if (g_Assertion != null) {
                    // Only considers the assertion's outcome
                    g_Assertion.Start();
                    g_Assertion.UpdateNode();
                    if (g_Assertion.Status == BEHAVIOR_STATUS.FAILURE)
                        g_Status = g_Assertion.Status;
                    else {
                        if (g_Child.Finished)
                            g_Child.Start();
                        g_Child.UpdateNode();
                        yield return g_Status;
                    }
                } else {
                    // Keep executing and restaring until failure
                    g_Child.UpdateNode();
                    if (g_Child.Finished) {
                        if (g_Child.Status == BEHAVIOR_STATUS.FAILURE) {
                            g_Status = g_Child.Status;
                            break;
                        } else {
                            g_Child.Start();
                        }
                    }
                    yield return g_Status;
                }
            }
            yield return g_Status;
        }
    }

}