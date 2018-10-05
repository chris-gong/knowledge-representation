using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnowledgeBase : MonoBehaviour {

    public List<string> facts;
    private FieldOfView fow;

	// Use this for initialization
	void Start () {
        fow = gameObject.GetComponent<FieldOfView>();
        StartCoroutine("RetrieveFactsWithDelay", 1f);
    }
	
    IEnumerator RetrieveFactsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            RetrieveFacts();
        }
    }

    void RetrieveFacts()
    {
        foreach (GameObject obj in fow.observables)
        {
            Observable obs = obj.GetComponent<Observable>();
            Debug.Log("NAME: "+obj.name);
            facts.AddRange(obs.GetFacts());
        }

        fow.observables.Clear();
    }
}
