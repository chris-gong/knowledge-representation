using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver{
    public List<Candidate> candidates;
    public Agent agent;
    public List<int[]> paths;
    public List<List<int>> actualPaths;
    public int agentId;
    public Solver(Agent myAgent)
    {
        candidates = new List<Candidate>();
        agent = myAgent;
        int agentCount = GameController.GetInstance().GetAgentCount();
        for (int i = 0; i < agentCount; i++)
        {
            candidates.Add(new Candidate(i));
        }
        GameController.GetInstanceTimeController().onDayEnd.AddListener(TryAndSolve); //should also make all the npc's stop moving
    }

    public void AddLocationClue(LocationClue clue){
        int candidateID = clue.agentID;
        Candidate candidate = candidates[candidateID];
        candidate.locationClues.Add(clue);
    }


    public void PrintAllCandidates(){
        string str = "Agent("+agent.agentId+"):";
        foreach(Candidate candidate in candidates){
            if (candidate.agentID == agent.agentId)
            {
                continue;
            }
            str += candidate.CluesToString();
        }
        Debug.Log(str);
    }

    public void TryAndSolve(){
        //Debug.Log(agentId + " " + agent.isAlive);
        if (!agent.isAlive)
        {
            return;
        }
        
        paths = new List<int[]>();
        actualPaths = new List<List<int>>();
        int numCandidates = candidates.Count;
        List<int> scores = new List<int>();
        int greatestScore = 0;
        int[] bestPath;
        List<int> bestActualPath = new List<int>();
        int leastLikely = 0;
        /*for (int i = 0; i < numCandidates; i++){
            scores.Add(0);
        }*/
        int index = 0;
        for (int i = 0; i < numCandidates; i++){
            int score = CalculateScore(candidates[i]);
            if (score < 0)
            {
                continue;
            }
            scores.Add(score);
            
            if(score > greatestScore)
            {
                greatestScore = score;
                bestPath = paths[index];
                bestActualPath = actualPaths[index];
                leastLikely = candidates[i].agentID;
                index++;
            }
        }
        string pathWrittenOut = "";
        for(int i = 0; i < bestActualPath.Count; i++)
        {
            pathWrittenOut += bestActualPath[i] + " ";
        }
        Debug.Log(string.Format("Agent {0} believes Agent {1} went through path {2}", agentId, leastLikely, pathWrittenOut));
    }

    private int CalculateScore(Candidate candidate){
        if (candidate.locationClues.Count < 2)
        {
            return -1;
        }
        int score = 0;
        int i = 0;
        int murderTime = GameController.GetInstanceTimeController().getMurderTime();
        LocationClue origin;
        LocationClue destination;
        while (i < candidate.locationClues.Count && candidate.locationClues[i].timeInt < murderTime - 1)
        {
            Debug.Log("Loop 1");
            i++;
        }
        if (i > 0)
        {
            origin = candidate.locationClues[i - 1];
        }
        else
        {
            origin = candidate.locationClues[i];
        }
        while (i < candidate.locationClues.Count && candidate.locationClues[i].timeInt >= murderTime && candidate.locationClues[i].timeInt < murderTime + 2)
        {
            Debug.Log("Loop 2");
            i++;
        }
        if(i < candidate.locationClues.Count)
        {
            destination = candidate.locationClues[i];
        }
        else
        {
            destination = candidate.locationClues[i - 1];
        }

        //BFS
        int[] weights = new int[10];
        for(i = 0; i < 10; i++)
        {
            weights[i] = -1;
        }
        List<ZoneInfo> zones = GameController.GetInstanceLevelController().GetZoneInfos();
        ZoneInfo start = zones[origin.zoneID];
        weights[origin.zoneID] = 0;
        Queue<ZoneInfo> fringe = new Queue<ZoneInfo>();
        fringe.Enqueue(start);
        int[] path = new int[10];
        List<int> actualPath = new List<int>();
        while(fringe.Count > 0)
        {
            ZoneInfo zone = fringe.Dequeue();
            int parentZoneNum = zone.zoneNum;
            int cost = weights[zone.zoneNum];
            for(i = 0; i < zone.neighbors.Length; i++)
            {
                if (weights[zone.neighbors[i].zoneNum] == -1 || cost + 1 < weights[zone.neighbors[i].zoneNum])
                {
                    weights[zone.neighbors[i].zoneNum] = cost + 1;
                    path[zone.neighbors[i].zoneNum] = parentZoneNum;
                }
                if (weights[zone.neighbors[i].zoneNum] == -1)
                {
                    fringe.Enqueue(zone.neighbors[i]);
                }
            }
        }

        int distance = 0;
        i = destination.zoneID;
        actualPath.Insert(0, i);
        Debug.Log(destination.zoneID + " " + origin.zoneID);
        while (i != origin.zoneID)
        {
            Debug.Log(i);
            i = path[i];
            actualPath.Insert(0, i);
            distance++;
        }
        paths.Add(path);
        actualPaths.Add(actualPath);
        score = 100 - Mathf.Abs(distance * (destination.timeInt - origin.timeInt));
        return score;
    }


}
