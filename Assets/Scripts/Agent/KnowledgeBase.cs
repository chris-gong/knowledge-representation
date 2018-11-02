using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YieldProlog;

public class KnowledgeBase : MonoBehaviour {

    private FieldOfView fow;
    private Agent agent;
    public Agent info;

	// Use this for initialization
	public void InitKnowledgeBase() {

        fow = gameObject.GetComponent<FieldOfView>();
        agent = gameObject.GetComponent<Agent>();
        StartCoroutine("RetrieveFactsWithDelay", .2f);
        info = gameObject.GetComponent<Agent>();
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
        //Debug.Log("# of observables: " + fow.observables.Count);
        foreach (GameObject obj in fow.observables)
        {
            if(obj == null)
            {
                continue;
            }
            Observable obs = obj.GetComponent<Observable>();
            //Debug.Log("Observable " + obs);
            foreach(ObservableFact fact in obs.observableFacts)
            {
                YP.assertFact(info.agentId, fact.getLabel(), fact.getValues());
            }
            foreach (LocationClue clue in obs.locationClues)
            {
                Debug.Log("KB adding clue " + clue);
                agent.solver.AddLocationClue(clue);
            }
        }
        fow.observables.Clear();
    }



}

