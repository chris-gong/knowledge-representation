using UnityEngine;
using System.Collections;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    
    // Anything which implements this interface might be a module of the NPC
    public interface INPCModule {

        void InitializeModule();

        bool IsUpdateable();

        void TickModule();

        bool IsEnabled();

        void SetEnable(bool e);

        void RemoveNPCModule();

        NPC_MODULE_TYPE NPCModuleType();

        NPC_MODULE_TARGET NPCModuleTarget();

        string NPCModuleName();

        void CleanupModule();

    }
}
