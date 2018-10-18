using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance;
    GameObject player;

	// Use this for initialization
	void Start () {
        if (GameController.instance == null) {
            GameController.instance = this;
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length > 0)
        {
            player = players[0];
        }
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < agents.Length; i++)
        {
            AgentInfo info = agents[i].GetComponent<AgentInfo>();
            info.agentId = i;
            info.agentName = string.Format("Agent{0}", i);
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
