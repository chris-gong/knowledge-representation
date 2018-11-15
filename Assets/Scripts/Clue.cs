using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationClue {

    public int agentID;
    public int zoneID;
    public float timeInt;
    
    public LocationClue(int agent, int zone, float time)
    {
        this.agentID = agent;
        this.zoneID = zone;
        this.timeInt = time;
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})",agentID,zoneID,timeInt);
    }

    public static float CmpTime(LocationClue obj, LocationClue other)
    {
        return obj.timeInt - other.timeInt;
    }
    
    //assumes that list of clues is sorted in ascending order by time
    public static LocationClue GetOriginClue(List<LocationClue> clues, float murderTime)
    {
        int i = 0;
        LocationClue clue = null;
        if(clues.Count < 1)
        {
            return clue;
        }
        while (i < clues.Count && clues[i].timeInt < murderTime)
        {
            i++;
        }
        if (i > 0)
        {
            clue = clues[i - 1];
        }
        else
        {
            clue = clues[i];
        }
        return clue;
    }
    //assumes that list of clues is sorted in ascending order by time
    public static LocationClue GetDestinationClue(List<LocationClue> clues, float murderTime)
    {
        int i = 0;
        LocationClue clue = null;
        if (clues.Count < 1)
        {
            return clue;
        }
        while (i < clues.Count && clues[i].timeInt <= murderTime)
        {
            i++;
        }
        if (i < clues.Count)
        {
            clue = clues[i];
        }
        else
        {
            clue = clues[i - 1];
        }
        return clue;
    }
}

public class MurderItemClue
{
    private int agentID;
    private string itemName;
    private int timeInt;
}

