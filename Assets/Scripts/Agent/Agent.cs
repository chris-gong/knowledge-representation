using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public int agentId;
    public string agentName = null;
    private GameObject currentObs = null;
    public GameObject blankObs;
    public Solver solver;

    public void InitAgentInfo(int newID,int agentCount){
        blankObs = Resources.Load<GameObject>("Observable");
        if (agentName == null)
        {
            agentName = string.Format("Agent#{0}", newID);
        }
        agentId = newID;
        GameController.GetInstanceTimeController().onTimeTick.AddListener(UpdateAgentObs);
    }

    public void UpdateAgentObs(){
        if(currentObs != null){
            Destroy(currentObs);
        }
        GameObject newobs = Instantiate(blankObs,gameObject.transform);
        Observable obsInfo= newobs.GetComponent<Observable>();

        int time = GameController.GetInstanceTimeController().GetTime();
        int zoneID = GameController.GetInstanceLevelController().GetZoneFromObj(gameObject);

        obsInfo.AddLocationClue(new LocationClue(agentId, 0, time));
        currentObs = newobs;

        // TODO Add up-to-date information on the agent's current location
    }
}
