using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver{
    public List<Candidate> candidates;
    public Agent agent;

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
    #region Path finding algorithm
    public void TryAndSolve(){
        //if this agent is not alive, then it should not try to find the murderer
        if (!agent.isAlive)
        {
            return;
        }
        
        int numCandidates = candidates.Count;
        int greatestScore = 0;
        int mostLikelyCand = -1;
        Path bestPath = new Path();

        for (int i = 0; i < numCandidates; i++){
            Debug.Log("Score being calculated");
            
            Path p = CalculateScore(candidates[i]);
            if (p == null)
            {
                continue;
            }
            
            if(p.GetScore() > greatestScore)
            {
                greatestScore = p.GetScore();
                mostLikelyCand = candidates[i].agentID;
                bestPath = p;
            }
        }

        Debug.Log(string.Format("Agent {0} believes Agent {1} went through path {2}", agent.agentId, mostLikelyCand, bestPath.ToString()));
    }

    private Path CalculateScore(Candidate candidate){
        if (candidate.locationClues.Count < 2)
        {
            return null; //if we do not have more than two location clues about this candidate
        }
        int score = 0;
        int murderTime = GameController.GetInstanceTimeController().GetMurderTime();
        int murderZone = GameController.GetInstanceLevelController().GetMurderZone();
        LocationClue origin = LocationClue.GetOriginClue(candidate.locationClues, murderTime);
        LocationClue destination = LocationClue.GetDestinationClue(candidate.locationClues, murderTime); ;

        List<ZoneInfo> zones = GameController.GetInstanceLevelController().GetZoneInfos();

        ZoneInfo start = zones[origin.zoneID]; //set the starting zone to origin
        int[] prev = findPath(start, zones.Count);
        Path originToMurderZone = Path.CreatePath(prev, murderZone, origin.zoneID);

        start = zones[murderZone]; //set the starting zone to murder zone
        prev = findPath(start, zones.Count); 
        Path murderZoneToDest = Path.CreatePath(prev, destination.zoneID, murderZone);

        Path combinedPath = Path.CombinePaths(originToMurderZone, murderZoneToDest);
        //calculating actual time elapsed from origin to destination
        int timeElapsed = destination.timeInt - origin.timeInt;
        //how close is the score of the path compared to the actual time elapsed
        //will show us how likely the candidate took a path going through the murder zone
        score = 100 - (Mathf.Abs(combinedPath.GetScore() - timeElapsed));
        combinedPath.SetScore(score);
        return combinedPath;
    }

    private int[] findPath(ZoneInfo start, int numberOfZones)
    {
        //BFS
        int[] weights = new int[numberOfZones]; //cost of shortest path to the zone 
        for (int i = 0; i < numberOfZones; i++)
        {
            weights[i] = -1;
        }

        weights[start.zoneNum] = 0; //set the starting zone's cost
        Queue<ZoneInfo> fringe = new Queue<ZoneInfo>(); //queue for bfs
        fringe.Enqueue(start); //adding the starting zone to the fringe
        int[] prev = new int[numberOfZones]; //each node's previous/parent node by zone number
        for (int i = 0; i < numberOfZones; i++)
        {
            prev[i] = -1;
        }
        //run bfs from origin to murderzone
        while (fringe.Count > 0)
        {
            ZoneInfo zone = fringe.Dequeue();
            int parentZoneNum = zone.zoneNum;
            int cost = weights[zone.zoneNum];
            for (int i = 0; i < zone.neighbors.Length; i++)
            {
                ZoneInfo zi = zone.neighbors[i].GetComponent<ZoneInfo>();
                if (weights[zi.zoneNum] == -1)
                {
                    fringe.Enqueue(zi);
                }
                if (weights[zi.zoneNum] == -1 || cost + 1 < weights[zi.zoneNum])
                {
                    weights[zi.zoneNum] = cost + 1;
                    prev[zi.zoneNum] = parentZoneNum;
                }

            }
        }

        return prev;
    }
    #endregion

}
