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
    /// Executes all children simultaneously. If a child fails,
    /// the node fails, otherwise, it will continue executing all
    /// children until they all result in successful execution. 
    /// </summary>
    [Serializable]
    public class NPCSequenceParallel : NPCNode {

        public NPCSequenceParallel(NPCNode[] children) : base(children) { }

        public override void Initialize(object[] parameters) { }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            g_Status = BEHAVIOR_STATUS.RUNNING;
            foreach (NPCNode currentNode in Children) {
                if (currentNode.Status == BEHAVIOR_STATUS.PENDING) {
                    currentNode.Start();
                }
            }
            while (!Finished) {
                bool finished = true, failed = false;
                foreach (NPCNode currentNode in Children) {
                    if (!currentNode.Finished) {
                        currentNode.UpdateNode();
                    }
                    if (currentNode.Status == BEHAVIOR_STATUS.FAILURE) {
                        failed = finished = true;
                        break;
                    }
                    finished = finished && (currentNode.Status == BEHAVIOR_STATUS.SUCCESS);
                }
                if (finished)
                    g_Status = failed ? BEHAVIOR_STATUS.FAILURE : BEHAVIOR_STATUS.SUCCESS;
                else
                    yield return g_Status;
            }
            yield return g_Status;
        }
    }

}