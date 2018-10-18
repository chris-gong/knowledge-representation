using UnityEngine;
using System.Collections.Generic;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    
    public interface INPCPathfinder {

        bool IsCurrentStateGoal(System.Object[] states = null);

        string ObjectIdentifier();

        List<Vector3> FindPath(Vector3 from, Vector3 to);

        bool IsReachable(Vector3 from, Vector3 to);

        float ComputeNodeCost(NavNode from, NavNode to, GRID_DIRECTION dir);

        float ComputeNodeHeuristic(NavNode n);

        List<Vector3> ConstructPath(NavNode goal, Dictionary<NavNode, NavNode> parents);

        void ClearPath();

        void DryRunAlgorithm();
	
    }
}
