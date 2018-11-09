using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

    private int score;
    private List<int> zonesInOrder;

    public Path()
    {
        score = 0;
        zonesInOrder = new List<int>();
    }

    public void SetScore(int score)
    {
        this.score = score;
    }

    public int GetScore()
    {
        return score;
    }

    public void incrementScore()
    {
        score++;
    }

    public static Path CreatePath(int[] prev, int dest, int origin)
    {
        //create a path by backtracking a dictionary starting from
        //the destination until the origin, assuming the dictionary
        //key is the zoneid and the value is the zoneid it came from to get there
        Path p = new Path();
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
            p.incrementScore();
            p.PrependToPath(nextZone);
            nextZone = prev[nextZone];
        }

        p.incrementScore();
        p.PrependToPath(origin);
        return p;
    }

    public static Path CombinePaths(Path p1, Path p2)
    {
        Path combinedPath = new Path();
        combinedPath.SetScore(p1.score + p2.score);

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
        pathStr += string.Format("with a score of {0}", score);
        return pathStr;
    }
   

}
