﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public int agentId;
    public string agentName = null;
    public GameObject currentObs = null;
    public GameObject blankObs;
    public GameObject blankHoverText;
    public GameObject talkingState;
    public Solver solver;
    private bool isPlayer = false;
    private KnowledgeBase kb;
    public bool isAlive = true;

    public void InitAgentInfo(int newID){
        Debug.Log("Initiating agent:" + agentName + "/"+agentId);
        blankObs = Resources.Load<GameObject>("Observable");
        blankHoverText = Resources.Load<GameObject>("Text");
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
        blankHoverText = Resources.Load<GameObject>("Text");
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

        float time = GameController.GetTime();
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

    public void OnMouseOver()
    {
        //GameController.GetInstanceLevelController().setEventText(string.Format("Mouse hovering over agent {0}", agentId), 2);
    }

    public void OnMouseExit()
    {
        //GameController.GetInstanceLevelController().setEventText("", 0);
    }
}
