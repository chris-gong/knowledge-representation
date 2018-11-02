using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver{
    public List<Candidate> candidates;
    public Agent agent;
    
    public Solver(Agent myAgent)
    {
        Debug.Log("Initiating solver:" + myAgent.agentId);
        candidates = new List<Candidate>();
        agent = myAgent;
        int agentCount = GameController.GetInstance().GetAgentCount();
        for (int i = 0; i < agentCount; i++)
        {
            candidates.Add(new Candidate(i));
        }
    }

    public void AddLocationClue(LocationClue clue){
        int candidateID = clue.agentID;
        Candidate candidate = candidates[candidateID];
        candidate.locationClues.Add(clue);
    }


    public void PrintAllCandidates(){
        string str = "Agent(x):";
        foreach(Candidate candidate in candidates){
            if (candidate.agentID == agent.agentId)
            {
                continue;
            }
            str += candidate.CluesToString();
        }
        Debug.Log(str);
    }




}
