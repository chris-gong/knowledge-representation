using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Candidate
{
    public int agentID;
    public int trust;
    public int suspicion;
    public List<LocationClue> locationClues;
    public List<List<LocationClue>> otherLocationClues; //clues from other agents


    public Candidate(int ID)
    {
        agentID = ID;
        trust = 100;
        suspicion = 0;
        locationClues = new List<LocationClue>();
        otherLocationClues = new List<List<LocationClue>>();
        for(int i = 0; i < GameController.GetInstance().GetAgentCount(); i++)
        {
            otherLocationClues.Add(new List<LocationClue>());
        }
    }

    public float CalculateLikelihood(){
        return 0f;
    }

    public string CluesToString(){
        string str = "";
        foreach(LocationClue clue in locationClues)
        {
            str += clue+"\n";
        }
        return str;
    }

    public void AddOtherClues(List<LocationClue> clues, int agentId)
    {
        otherLocationClues[agentId] = clues;
    }

    public List<LocationClue> IncorporateOtherClues()
    {
        List<LocationClue> allClues = locationClues;
        foreach(List<LocationClue> clues in otherLocationClues) {
            allClues.AddRange(clues);
        }

        //sort the clues by time
        allClues.Sort((x, y) => x.timeInt.CompareTo(y.timeInt));
        return allClues;
    }
}