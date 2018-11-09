using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

    private int score;
    private List<int> zonesInOrder;
    private int beginningTime;
    private int endingTime;

    public Path()
    {
        score = 0;
        zonesInOrder = new List<int>();
        beginningTime = 0;
        endingTime = 0;
    }

    public void SetScore(int score)
    {
        this.score = score;
    }

    public int GetScore()
    {
        return score;
    }

    public void SetBeginningTime(int time)
    {
        beginningTime = time;
    }

    public int GetBeginningTime()
    {
        return beginningTime;
    }

    public void SetEndingTime(int time)
    {
        endingTime = time;
    }

    public int GetEndingTime()
    {
        return endingTime;
    }

    public void IncrementScore()
    {
        score++;
    }

    public static Path CreatePath(int[] prev, int dest, int origin, int destTime, int originTime)
    {
        //create a path by backtracking a dictionary starting from
        //the destination until the origin, assuming the dictionary
        //key is the zoneid and the value is the zoneid it came from to get there
        Path p = new Path();
        p.SetBeginningTime(originTime);
        p.SetEndingTime(destTime);
        p.PrependToPath(dest);
        if(prev[dest] == -1 || dest == origin)
        {
            //means either the destination is the same as the origin 
            //or there is no way to get to the destination from the origin
            return p;
        }
        int nextZone = prev[dest];
        //loop below assumes rest of prev is perfect (graph is connected)
        while (nextZone != origin)
        {
            p.IncrementScore();
            p.PrependToPath(nextZone);
            nextZone = prev[nextZone];
        }

        p.IncrementScore();
        p.PrependToPath(origin);
        return p;
    }

    public static Path CombinePaths(Path p1, Path p2)
    {
        Path combinedPath = new Path();
        combinedPath.SetScore(p1.score + p2.score);
        combinedPath.SetBeginningTime(p1.beginningTime);
        combinedPath.SetEndingTime(p2.endingTime);

        combinedPath.zonesInOrder.AddRange(p1.zonesInOrder);
        combinedPath.zonesInOrder.RemoveAt(combinedPath.zonesInOrder.Count - 1); //to prevent duplication of murderzone
        combinedPath.zonesInOrder.AddRange(p2.zonesInOrder);

        return combinedPath;
    }

    public void PrependToPath(int zoneNum)
    {
        zonesInOrder.Insert(0, zoneNum);

    }
    
    public override string ToString()
    {
        string pathStr = "";
       
        for(int i = 0; i < zonesInOrder.Count; i++)
        {
            pathStr += string.Format("{0}->", zonesInOrder[i]);
        }
        pathStr = pathStr.Substring(0, pathStr.Length - 2);
        pathStr += string.Format(" from time {0} to time {1} with a score of {2}", beginningTime, endingTime, score);
        return pathStr;
    }
   

}
