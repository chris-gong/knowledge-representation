using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Candidate
{
    public int agentID;
    public int trust;
    public int suspicion;
    public List<LocationClue> locationClues;


    public Candidate(int ID)
    {
        agentID = ID;
        trust = 100;
        suspicion = 0;
        locationClues = new List<LocationClue>();
    }

    public float CalculateLikelihood(){
        return 0f;
    }

    public string CluesToString(){
        string str = "";
        foreach(LocationClue clue in locationClues)
        {
            str += locationClues+"\n";
        }
        return str;
    }
}