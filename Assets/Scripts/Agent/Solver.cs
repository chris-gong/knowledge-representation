using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver{
    public List<Candidate> candidates;
    
    public void InitSolver(int agentCount)
    {
        for (int i = 0; i < agentCount; i++)
        {
            candidates.Add(new Candidate(i));
        }
    }
}
