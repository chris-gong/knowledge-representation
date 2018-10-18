using UnityEngine;
using System.Collections;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public interface INPCPerceivable {
        PERCEIVEABLE_TYPE   GetNPCEntityType();
        PERCEIVE_WEIGHT     GetPerceptionWeightType();
        float               GetPerceptionWeight();
        Transform           GetTransform();
        Vector3             GetCurrentVelocity();
        float               GetCurrentSpeed();
        Vector3             GetPosition();
        Vector3             GetForwardDirection();
        float               GetAgentRadius();
        Transform           GetMainLookAtPoint();
        string              GetCurrentContext();
        void                SetCurrentContext(string c);
        bool                HasTag(string s);
        GameObject          GetGameObject();
    }

}
