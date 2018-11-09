using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public int agentId;
    public string agentName = null;
    public GameObject currentObs = null;
    public GameObject blankObs;
    public Solver solver;
    private bool isPlayer = false;
    private KnowledgeBase kb;
    public bool isAlive = true;

    public void InitAgentInfo(int newID){
        Debug.Log("Initiating agent:" + agentName + "/"+agentId);
        blankObs = Resources.Load<GameObject>("Observable");
        if (agentName == null)
        {
            agentName = string.Format("Agent#{0}", newID);
        }
        agentId = newID;
        UpdateAgentObs();
        solver = new Solver(this);
        GameController.GetInstanceTimeController().onTimeTick.AddListener(UpdateAgentObs);
    }

    public void InitPlayerAgentInfo(int newID){
        blankObs = Resources.Load<GameObject>("Observable");
        agentName = "Player";
        agentId = newID;
        UpdateAgentObs();
        GameController.GetInstanceTimeController().onTimeTick.AddListener(UpdateAgentObs);
    }

    public void UpdateAgentObs(){
        if(currentObs != null){
            Destroy(currentObs);
        }
        GameObject newobs = Instantiate(blankObs,gameObject.transform);
        Observable obsInfo= newobs.GetComponent<Observable>();

        int time = GameController.GetTime();
        int zoneID = GameController.GetInstanceLevelController().GetZoneFromObj(gameObject);

        obsInfo.AddLocationClue(new LocationClue(agentId, zoneID, time));
        currentObs = newobs;

    }
    public void Update(){
        if (Input.GetKeyDown("space") && solver != null)
        {
            solver.PrintAllCandidates();
        }
    }
}
