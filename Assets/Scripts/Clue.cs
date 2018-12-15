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
        // Case 1: Empty clue list
        if(clues.Count < 1)
        {
            return null;
        }
        // Case 2: No clues before the murder
        if(clues[i].timeInt > murderTime)
        {
            return null;
        }
        // Case 3: Return the closest clue before the murder
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
        int i = clues.Count-1;
        LocationClue clue = null;
        // Case 1: Empty clue list
        if (clues.Count < 1)
        {
            return null;
        }
        // Case 2: No clues after the murder
        if(clues[i].timeInt < murderTime) {
            return null;
        }
        // Case 3: Return the closest clue after the murder time
        while (i >= 0  && clues[i].timeInt > murderTime)
        {
            i--;
        }
        clue = i == 0? clues[i]:clues[i + 1];
        //Debug.Log("Destination clue chosen: "+clue.ToString());
        return clue;
    }
}

public class MurderClue {
    public int agentID;
    public int zoneID;
    public float timeInt;

    public MurderClue(int agent, int zone, float time)
    {
        this.agentID = agent;
        this.zoneID = zone;
        this.timeInt = time;
    }
}
public class MurderItemClue
{
    private int agentID;
    private string itemName;
    private int timeInt;
}

