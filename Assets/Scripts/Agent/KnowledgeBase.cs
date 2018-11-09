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
        Transform head = transform.Find("Head");
        Color original = head.GetComponent<Renderer>().material.GetColor("_Color"); //for debugging/showcase purposes
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
                head.GetComponent<Renderer>().material.SetColor("_Color", Color.green); //for debugging/showcase purposes
                agent.solver.AddLocationClue(clue);
            }
        }
        StartCoroutine(stopProcessingFacts(head, original, 0.5f)); //for debugging/showcase purposes
        fow.observables.Clear();
    }

    //for debugging/showcase purposes
    IEnumerator stopProcessingFacts(Transform head, Color original, float delay)
    {
        yield return new WaitForSeconds(delay);
        head.GetComponent<Renderer>().material.SetColor("_Color", original);
        yield break;
    }

}

