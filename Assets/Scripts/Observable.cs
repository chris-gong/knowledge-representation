using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable : MonoBehaviour {

    public List<string> facts;

	public List<string> GetFacts()
    {
        Debug.Log("Getting Facts");
        return facts;
    }
    public void addFact(string fact)
    {
        facts.Add(fact);
    }
}
