using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationClue {

    public int agentID;
    public int zoneID;
    public int timeInt;
    
    public LocationClue(int agent, int zone, int time)
    {
        this.agentID = agent;
        this.zoneID = zone;
        this.timeInt = time;
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})",agentID,zoneID,timeInt);
    }

    public static int CmpTime(LocationClue obj, LocationClue other)
    {
        return obj.timeInt - other.timeInt;
    }
        
}

public class MurderItemClue
{
    private int agentID;
    private string itemName;
    private int timeInt;
}

