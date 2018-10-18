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
    /// Executes all its children in sequential order, as provided in the array
    /// and returns success once all nodes have been successfully executed, failure
    /// if at least one fails.
    /// </summary>
    [Serializable]
    public class NPCSequence : NPCNode {

        public NPCSequence(NPCNode[] children) : base(children) { }

        public override void Initialize(object[] parameters) { }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            g_Status = BEHAVIOR_STATUS.RUNNING;
            bool succeeded = true;
            foreach (NPCNode currentNode in Children) {
                currentNode.Start();
                do {
                    currentNode.UpdateNode();
                    yield return g_Status;
                } while (!currentNode.Finished);
                succeeded = currentNode.Status == BEHAVIOR_STATUS.SUCCESS;
                if (!succeeded) {
                    g_Status = currentNode.Status;
                    break;
                }
            }
            g_Status = succeeded ? BEHAVIOR_STATUS.SUCCESS : BEHAVIOR_STATUS.FAILURE;
            yield return g_Status;
        }
    }

}
