using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Candidate
{
    public int agentID;
    public int trust;
    public int suspicion;


    public Candidate(int ID)
    {
        agentID = ID;
        trust = 100;
        suspicion = 0;
    }
}