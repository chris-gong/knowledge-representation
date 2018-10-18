using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC  {
    
    [System.Serializable]
    public class NPCGoal {

        private NPCGOAL_TYPE gType;
        private NPCGOAL_STATUS gStatus;
        private Dictionary<string, NPCAttribute> gAttributes;

        #region Properties
        public NPCGOAL_STATUS Status {
            get {
                return this.gStatus;
            }
        }

        public NPCGOAL_TYPE Type {
            get {
                return this.gType;
            }
        }



        #endregion
    }

}