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
    /// Waits for a repdetermined number of Milliseconds.
    /// </summary>
    [Serializable]
    public class NPCWait : NPCNode {

        [SerializeField]
        private long g_WaitTime;

        public override void Initialize(object[] parameters) {
            g_WaitTime = Convert.ToInt64(parameters[0]);
        }

        public NPCWait(long Milliseconds) : base() {
            g_WaitTime = Milliseconds;
        }

        protected override IEnumerable<BEHAVIOR_STATUS> Execute() {
            g_Status = BEHAVIOR_STATUS.RUNNING;
            long stop = NPCUtils.TimeMillis() + g_WaitTime;
            while (NPCUtils.TimeMillis() < stop) {
                yield return g_Status;
            }
            g_Status = BEHAVIOR_STATUS.SUCCESS;
            yield return g_Status;
        }
    }

}