using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance;

    #region Fields

    private TimeController timeCtl;
    private LevelController levelCtl;
    private GameObject playerInstance;
    private List<GameObject> agentList;

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

        InitializeAgents();
        GatherPlayer();
    }

    #endregion

    #region Collection Methods

    /// <summary>
    /// Gathers the agents and assigns their agentinfo
    /// </summary>
    private void InitializeAgents(){
        agentList = new List<GameObject>();
        // TODO assert that agent is configured correctly

        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        for (int i = 0; i < agents.Length; i++)
        {
            GameObject agentObj = agents[i];
            AgentInfo info = agentObj.GetComponent<AgentInfo>();
            if(info == null)
            {
                Debug.LogError("ERROR: No agent info component on gameobject tagged as agent NAME: " + agentObj.name);
                continue;
            }
            info.InitAgentInfo(agentList.Count);
            agentList.Add(agentObj);
        }
        for (int i = 0; i < agentList.Count; i++)
        {
            GameObject obj = agentList[i];
            Solver solver = obj.GetComponent<Solver>();
            if (solver == null)
            {
                Debug.LogError("ERROR: No solver component on gameobject tagged as agent NAME: " + obj.name);
                continue;
            }else
            {
                solver.InitSolver(i);
            }
        }
    }

    private void GatherPlayer(){
        // Player gathering
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            playerInstance = players[0];
        }else{
            Debug.LogError("No player");
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
