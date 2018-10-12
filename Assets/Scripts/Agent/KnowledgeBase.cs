using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YieldProlog;

public class KnowledgeBase : MonoBehaviour {

    public List<string> facts;
    private FieldOfView fow;
    public int myId;

	// Use this for initialization
	void Start () {
        fow = gameObject.GetComponent<FieldOfView>();
        StartCoroutine("RetrieveFactsWithDelay", .2f);
        myId = gameObject.GetComponent<AgentInfo>().agentId;
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
            if(obj == null)
            {
                continue;
            }
            Observable obs = obj.GetComponent<Observable>();
            List< ObservableFact> factList = obs.GetFacts();
            foreach(ObservableFact fact in factList)
            {
                YP.assertFact(myId, fact.getLabel(), fact.getValues());
            }
        }

        fow.observables.Clear();
    }
}
