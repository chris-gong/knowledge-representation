using NPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAffordances : MonoBehaviour, INPCModule {

    public bool Enabled = true;

    private NPCController g_NPCController;

    #region Unity

    void Reset() {
        g_NPCController = GetComponent<NPCController>();
    }

    #endregion

    #region Behaviors


    
    #endregion

    #region NPCModule

    public void CleanupModule() { }

    public void InitializeModule() { }

    public bool IsEnabled() {
        return Enabled;
    }

    public bool IsUpdateable() {
        return false;
    }

    public string NPCModuleName() {
        return "NPC Behaviors";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.AI;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.BEHAVIOR;
    }

    public void RemoveNPCModule() { }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    public void TickModule() {
        throw new System.NotImplementedException();
    }

    #endregion
}
