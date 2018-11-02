using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance;

    #region Fields

    private TimeController timeCtl;
    private GameObject playerInstance;
    private List<GameObject> agentList;
    private LevelController levelCtl;

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

        GatherAgents();
        GatherPlayer();
    }

    #endregion

    #region Aggregate Methods

    /// <summary>
    /// Gathers the agents and assigns their agentinfo
    /// </summary>
    private void GatherAgents(){
        agentList = new List<GameObject>();
        // TODO assert that agent is configured correctly

        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        for(int i = 0; i < agents.Length; i++)
        {
            GameObject agentObj = agents[i];
            AgentInfo info = agentObj.GetComponent<AgentInfo>();
            Debug.Assert(info != null,"ERROR: No agent info component on gameobject tagged as agent NAME: "+ agentObj.name);
            if(info.agentName == null){
                info.agentName = string.Format("Agent{0}", i);
            }
            agentList.Add(agentObj);
        }       
    }

    private void GatherPlayer(){
        // Player gathering
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            playerInstance = players[0];
        }
    }

    #endregion

    #region Public Methods
    public int GetTime()
    {
        return timeCtl.GetTime();
    }

    #endregion
}
