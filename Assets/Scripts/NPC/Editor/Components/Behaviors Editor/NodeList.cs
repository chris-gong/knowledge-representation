using System;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    namespace Behavior {

        [Serializable]
        public class NodeList : ScriptableObject {
            public string TreeName;
            public List<Node> Nodes;
            public void Init() {
                Nodes = new List<Node>();
            }
            public void OnEnable() {
                hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
        }
        
    }
}