using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour {
    public List<Candidate> candidates;

    public void InitSolver(int agentCount)
    {
        for (int i = 0; i < agentCount; i++)
        {
            candidates.Add(new Candidate(i));
        }
    }


}

public class Candidate{

    public int agentID;
    public int trust;
    public int suspicion;


    public Candidate(int ID){
        agentID = ID;
        trust = 100;
        suspicion = 0;
    }
}