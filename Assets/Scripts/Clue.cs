using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationClue {

    private int agentID;
    private int zoneID;
    private int timeInt;
    
    public LocationClue(int agent, int zone, int time)
    {
        this.agentID = agent;
        this.zoneID = zone;
        this.timeInt = time;
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

