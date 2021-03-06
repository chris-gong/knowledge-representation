﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver{
    public List<Candidate> candidates;
    public List<List<Candidate>> oldCandidateInfo; //solely used for drawing graphs
    public Agent agent;
    public int mostLikelyCand;
    public Path mostLikelyPath;

    public Solver(Agent myAgent)
    {
        candidates = new List<Candidate>();
        oldCandidateInfo = new List<List<Candidate>>();
        agent = myAgent;
        mostLikelyCand = -1;
        int agentCount = GameController.GetInstance().GetAgentCount();
        for (int i = 0; i < agentCount; i++)
        {
            candidates.Add(new Candidate(i));
            oldCandidateInfo.Add(new List<Candidate>());
        }
        GameController.GetInstanceTimeController().onDayEnd.AddListener(TryAndSolve); //should also make all the npc's stop moving
    }

    public void AddLocationClue(LocationClue clue){
        int candidateID = clue.agentID;
        Candidate candidate = candidates[candidateID];
        candidate.locationClues.Add(clue);
    }
    public void AddMurderClue(MurderClue clue)
    {
        int candidateID = clue.agentID;
        Candidate candidate = candidates[candidateID];
        candidate.murderClues.Add(clue);
    }
    public Candidate GetLeastKnownCandidate()
    {
        Candidate leastKnown = candidates[0];
        int clueCount = leastKnown.locationClues.Count;
        foreach(List<LocationClue> clues in leastKnown.otherLocationClues)
        {
            clueCount += clues.Count;
        }
        List<Agent> agents = GameController.GetInstance().GetAgents();
        for (int i = 1; i < candidates.Count; i++){
            if(i == agent.agentId || !agents[i].isAlive)
            {
                continue;
            }
            int otherClueCount = candidates[i].locationClues.Count;
            foreach (List<LocationClue> clues in candidates[i].otherLocationClues)
            {
                otherClueCount += clues.Count;
            }
            if (otherClueCount < clueCount)
            {
                leastKnown = candidates[i];
                clueCount = otherClueCount;
            }
        }
        return leastKnown;
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
    //solve for all the agents at the end of the game
    public void TryAndSolve(){
        //if this agent is not alive, then it should not try to find the murderer
        if (!agent.isAlive)
        {
            return;
        }
        List<Agent> agents = GameController.GetInstance().GetAgents();

        int numCandidates = candidates.Count;
        float greatestScore = 0;
        Path bestPath = new Path();

        for (int i = 0; i < numCandidates; i++){
            Debug.Log("Score being calculated");
            int candidateId = candidates[i].agentID;
            if (!agents[candidateId].isAlive)
            {
                continue;
            }
            if(candidates[i].murderClues.Count > 0)
            {
                greatestScore = 100;
                mostLikelyCand = candidates[i].agentID;
                bestPath = new Path();
                bestPath.PrependToPath(GameController.GetInstanceLevelController().GetMurderZone());
                bestPath.SetScore(100);
                bestPath.SetBeginningTime(GameController.GetInstanceTimeController().GetMurderTime());
                bestPath.SetEndingTime(GameController.GetInstanceTimeController().GetMurderTime());
                break;
            }
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
        string result = string.Format("Agent {0} believes Agent {1} went through path {2}", agent.agentId, mostLikelyCand, bestPath.ToString());
        GameController.GetInstanceLevelController().AddResultText(result);
        if (mostLikelyCand == 0)
        {
            GameController.GetInstanceLevelController().AddResultText("Game Over, you were caught committing murder");
            GameController.GetInstanceLevelController().gameOver = true;
            GameController.GetInstanceLevelController().gameWon = false;
        }
        mostLikelyPath = bestPath;
    }

    private Path CalculateScore(Candidate candidate){
        List<LocationClue> clues = candidate.IncorporateOtherClues();
        if (clues.Count < 2)
        {
            return null; //if we do not have more than two location clues about this candidate
        }
        float score = 0;
        float murderTime = GameController.GetInstanceTimeController().GetMurderTime();
        int murderZone = GameController.GetInstanceLevelController().GetMurderZone();
        LocationClue origin = LocationClue.GetOriginClue(clues, murderTime);
        LocationClue destination = LocationClue.GetDestinationClue(clues, murderTime);
        if(origin == null || destination == null)
        {
            return null; //not enough information about the agent either before or after the murder
        }

        List<ZoneInfo> zones = GameController.GetInstanceLevelController().GetZoneInfos();

        ZoneInfo start = zones[origin.zoneID]; //set the starting zone to origin
        int[] prev = FindPath(start, zones.Count);
        Path originToMurderZone = Path.CreatePath(prev, murderZone, origin.zoneID, murderTime, origin.timeInt);

        start = zones[murderZone]; //set the starting zone to murder zone
        prev = FindPath(start, zones.Count);
        Path murderZoneToDest = Path.CreatePath(prev, destination.zoneID, murderZone, destination.timeInt, murderTime);

        Path combinedPath = Path.CombinePaths(originToMurderZone, murderZoneToDest);
        //calculating actual time elapsed from origin to destination
        float timeElapsed = destination.timeInt - origin.timeInt;
        //how close is the score of the path compared to the actual time elapsed
        //will show us how likely the candidate took a path going through the murder zone
        score = 100 - (Mathf.Abs(combinedPath.GetScore() - timeElapsed));
        combinedPath.SetScore(score);
        return combinedPath;
    }

    private int[] FindPath(ZoneInfo start, int numberOfZones)
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
