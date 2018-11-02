using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentInfo : MonoBehaviour {

    public int agentId;
    public string agentName = null;
    private GameObject currentObs = null;
    public GameObject blankObs;

    public void InitAgentInfo(int newID, string name){
        blankObs = Resources.Load<GameObject>("Assets/Prefabs/Observable.prefab");
        if (agentName == null)
        {
            agentName = string.Format("Agent#{0}", newID);
        }
        agentId = newID;
        GameController.GetInstanceTimeController().onTimeTick.AddListener(UpdateAgentObs);
    }

    public void UpdateAgentObs(){
        if(!currentObs){
            Destroy(currentObs);
        }
        GameObject newobs = Instantiate(blankObs,gameObject.transform);
        Observable obsInfo= newobs.GetComponent<Observable>();
        // TODO Add up-to-date information on the agent's current location
    }
}
