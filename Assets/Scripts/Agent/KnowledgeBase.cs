using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YieldProlog;

public class KnowledgeBase : MonoBehaviour {

    private FieldOfView fow;
    private Agent agent;
    public Agent info;
    public GameObject agentToTalkTo;
    public GameObject lastAgentSpokenTo;

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
            //retrieve agents to talk to only if there was a murder
            if(GameController.GetInstanceTimeController().GetMurderTime() > -1)
            {
                RetrieveAgents();
            }
        }
    }

    void RetrieveFacts()
    {
        //Debug.Log("# of observables: " + fow.observables.Count);
        /*Transform head = transform.Find("Head");
        Color original = head.GetComponent<Renderer>().material.GetColor("_Color"); //for debugging/showcase purposes*/
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
                //head.GetComponent<Renderer>().material.SetColor("_Color", Color.green); //for debugging/showcase purposes
                agent.solver.AddLocationClue(clue);
            }
            foreach (MurderClue clue in obs.murderClues)
            {
                transform.Find("Head").GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                agent.solver.AddMurderClue(clue);
            }
        }
        //StartCoroutine(stopProcessingFacts(head, original, 0.5f)); //for debugging/showcase purposes
        fow.observables.Clear();
    }

    //method for picking one of the agents recently seen for possibly exchanging facts
    void RetrieveAgents()
    {
        //remove the agent with the same id as this one or if the agent is dead
        fow.observedAgents.RemoveAll(x => x.GetComponent<Agent>().agentId == info.agentId || !x.GetComponent<Agent>().isAlive);
        //pick a random agent to talk to
        if (fow.observedAgents.Count > 0)
        {
            agentToTalkTo = fow.observedAgents[Random.Range(0, fow.observedAgents.Count - 1)];
        }
        fow.observedAgents.Clear();
    }

    public void ResetAgentFollowing()
    {
        fow.ClearSeenAgents();
        Agent otherAgent = agentToTalkTo.GetComponent<Agent>();
        Destroy(agent.talkingState);
        Destroy(otherAgent.talkingState);
    }
    //for debugging/showcase purposes
    IEnumerator StopProcessingFacts(Transform head, Color original, float delay)
    {
        yield return new WaitForSeconds(delay);
        head.GetComponent<Renderer>().material.SetColor("_Color", original);
        yield break;
    }

    public void ExchangeClues()
    {
        Agent otherAgent = agentToTalkTo.GetComponent<Agent>();
        Candidate c1 = agent.solver.GetLeastKnownCandidate();
        Candidate c2 = otherAgent.solver.GetLeastKnownCandidate();
        //destroy old hovering texts to prevent duplicate hovertext gameobjects from spawning
        Destroy(agent.talkingState);
        Destroy(otherAgent.talkingState);
        //add a hovertext to denote exchange of information
        agent.talkingState = Instantiate(agent.blankHoverText, gameObject.transform);
        agent.talkingState.GetComponent<TextMesh>().text = string.Format("{0}", c1.agentID);
        otherAgent.talkingState = Instantiate(otherAgent.blankHoverText, otherAgent.gameObject.transform);
        otherAgent.talkingState.GetComponent<TextMesh>().text = string.Format("{0}", c2.agentID);
        //store old knowledge base somewhere for visual purposes
        Solver thisAgentSolver = agent.solver;
        Solver otherAgentSolver = otherAgent.solver;
        Candidate oldC1 = new Candidate(c1.agentID);
        oldC1.locationClues = oldC1.IncorporateOtherClues();
        thisAgentSolver.oldCandidateInfo[(c1.agentID)].Add(oldC1);
        Candidate oldC2 = new Candidate(c2.agentID);
        oldC2.locationClues = oldC2.IncorporateOtherClues();
        otherAgentSolver.oldCandidateInfo[(c2.agentID)].Add(oldC2);
        //add new clues
        c1.AddOtherClues(otherAgent.solver.candidates[c1.agentID].locationClues, otherAgent.agentId);
        c2.AddOtherClues(agent.solver.candidates[c2.agentID].locationClues, agent.agentId);
        Debug.Log("Facts Exchanged");

        agentToTalkTo.GetComponent<KnowledgeBase>().lastAgentSpokenTo = gameObject;
        lastAgentSpokenTo = agentToTalkTo;
    }
}

