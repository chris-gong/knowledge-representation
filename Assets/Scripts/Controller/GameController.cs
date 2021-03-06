﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance;

    #region Fields

    private TimeController timeCtl;
    private LevelController levelCtl;
    private GameObject playerInstance;
    private List<Agent> agents;
    private InventoryController invCtl;

    #endregion

    #region Controller Instance Methods

    public static GameController GetInstance()
    {
        return GameController.instance;
    }

    public TimeController GetTimeController()
    {
        return timeCtl;
    }

    public static TimeController GetInstanceTimeController()
    {
        return instance.timeCtl;
    }

    public LevelController GetLevelController()
    {
        return levelCtl;
    }

    public static LevelController GetInstanceLevelController()
    {
        return instance.levelCtl;
    }

    public InventoryController GetInventoryController()
    {
        return invCtl;
    }

    public static InventoryController GetInstanceInventoryController()
    {
        return instance.invCtl;
    }

    #endregion

    #region Unity Methods

    void Start()
    {
        if (GameController.instance == null)
        {
            GameController.instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initiate Time Controller
        timeCtl = GetComponent<TimeController>();
        timeCtl.InitTimeCtl();
        Debug.Assert(timeCtl != null, "ERROR: Gamecontroller gameobject is missing TimeController Component");
        // Initiate Level Controller
        levelCtl = GetComponent<LevelController>();
        levelCtl.InitiLevelCtl();
        Debug.Assert(levelCtl != null, "ERROR: Gamecontroller gameobject is missing LevelController Component");
        // Initiate Level Controller
        invCtl = GetComponent<InventoryController>();
        invCtl.InitInvCtl();
        Debug.Assert(levelCtl != null, "ERROR: Gamecontroller gameobject is missing LevelController Component");

        agents = new List<Agent>();
        //order matters here
        InitializePlayer();
        InitializeAgents();

    }

    #endregion

    #region Collection Methods

    /// <summary>
    /// Gathers the agents and assigns their agentinfo
    /// </summary>
    private void InitializeAgents(){
        GameObject[] agentObjs = GameObject.FindGameObjectsWithTag("Agent");

        for (int i = 0; i < agentObjs.Length; i++)
        {
            Debug.Log("found agent");
            GameObject agentObj = agentObjs[i];
            Agent agent = agentObj.GetComponent<Agent>();
            KnowledgeBase kb = agentObj.GetComponent<KnowledgeBase>();
            kb.InitKnowledgeBase();
            if (agent == null)
            {
                Debug.LogError("ERROR: No agent info component on gameobject tagged as agent NAME: " + agentObj.name);
                continue;
            }
            Debug.Log("valid agent");
            agents.Add(agent);
        }
        for (int i = 1; i < agents.Count; i++) //i starting at one since the player should be the first agent added
        {
            Agent info = agents[i];
            info.InitAgentInfo(i);
        }

    }

    private void InitializePlayer(){
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            playerInstance = players[0];
            Agent agent = playerInstance.GetComponent<Agent>();
            agent.InitPlayerAgentInfo(0);
            agents.Add(agent);
        }else{
            Debug.LogError("No player");
            return;
        }
    }

    #endregion

    #region Public Methods
    public static float GetTime()
    {
        return instance.timeCtl.GetTime();
    }

    public int GetAgentCount(){
        return agents.Count;
    }

    public List<Agent> GetAgents()
    {
        return agents;
    }
    public GameObject GetPlayer(){
        return this.playerInstance;
    }
 
    public InventoryController GetInvCtl()
    {
        return invCtl;
    }

    public int GetAliveAgentCount()
    {
        int count = 0;
        for(int i = 0; i < agents.Count; i++)
        {
            if (agents[i].isAlive)
            {
                count++;
            }
        }
        return count;
    }

    public void ResetTalkingCooldowns()
    {
        foreach(Agent agent in agents)
        {
            //set the talking cooldown back to zero, this should be called after a round just ended
            if (agent.agentId != 0 && agent.isAlive)
            {
                agent.gameObject.GetComponent<AgentBehavior>().ResetCooldown();
            }
        }
    }

    public void ResetLightFlicker()
    {
        LightController lightCtl = GameObject.Find("Directional Light").GetComponent<LightController>();
        lightCtl.ResetFlickered();
    }

    public void ResetMostLikelyCandidates()
    {
        foreach (Agent agent in agents)
        {
            //set the most likely candidate back to -1 after the round ended
            if (agent.agentId != 0 && agent.isAlive)
            {
                agent.solver.mostLikelyCand = -1; //TODO: does this really need to be done? may not be needed, added just in case
                agent.solver.mostLikelyPath = null;
            }
        }
    }
    #endregion
}
