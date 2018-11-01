using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance;

    // Sub Controllers
    public TimeController timeCtl;
    public LevelController levelCtl;

    GameObject player;

	// Use this for initialization
	void Start () {
        if (GameController.instance == null) {
            GameController.instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        // Initiate Time Controller
        timeCtl = GetComponent<TimeController>();
        Debug.Assert(timeCtl != null, "ERROR: Gamecontroller gameobject is missing TimeController Component");
        // Initiate Level Controller
        levelCtl = GetComponent<LevelController>();
        Debug.Assert(levelCtl != null, "ERROR: Gamecontroller gameobject is missing LevelController Component");
        
        // Player instancing
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length > 0)
        {
            player = players[0];
        }

        // Agent gathering
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        for(int i = 0; i < agents.Length; i++)
        {
            AgentInfo info = agents[i].GetComponent<AgentInfo>();
            info.agentId = i;
            info.agentName = string.Format("Agent{0}", i);
        }       
    }

    #region Public Methods
        public 
    #endregion
}
