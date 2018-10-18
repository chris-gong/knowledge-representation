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
    /// it is just skipped. The complete execution succeeds if at
    /// least one child does, otherwise it fails
    /// </summary>
    [Serializable]
    public class NPCSelectorParallel : NPCNode {

        public NPCSelectorParallel(NPCNode[] children) : base(children) { }

        public override void Initialize(object[] parameters) { }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            g_Status = BEHAVIOR_STATUS.RUNNING;
            foreach (NPCNode currentNode in Children) {
                if (currentNode.Status == BEHAVIOR_STATUS.PENDING) {
                    currentNode.Start();
                }
            }
            while (!Finished) {
                bool finished = true,
                    succeeded = false;
                foreach (NPCNode currentNode in Children) {
                    if (!currentNode.Finished) {
                        currentNode.UpdateNode();
                    }
                    if (currentNode.Status == BEHAVIOR_STATUS.SUCCESS) {
                        succeeded = finished = true;
                        break;
                    }
                    finished = finished && currentNode.Finished;
                }
                if (finished) {
                    g_Status = succeeded ? BEHAVIOR_STATUS.SUCCESS : BEHAVIOR_STATUS.FAILURE;
                } else {
                    yield return g_Status;
                }
            }
            yield return g_Status;
        }

    }

}