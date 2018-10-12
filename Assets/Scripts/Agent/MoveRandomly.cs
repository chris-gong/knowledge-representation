using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveRandomly : MonoBehaviour {

    public float delay;
    public Transform runTarget;
    public int timeAvailable;
    KnowledgeBase knowledgeBase;

    public float speed; 

    private NavMeshAgent agent;
	// Use this for initialization
	void Start () {
        agent = gameObject.GetComponent<NavMeshAgent>();
        knowledgeBase = gameObject.GetComponent<KnowledgeBase>();
        agent.speed = speed;
        setNewTarget();
        StartCoroutine("WalkRoutine");

    }
	
    IEnumerator WalkRoutine()
    {
        while (knowledgeBase.facts.Find(x=> x.Contains("dead"))==null)
        {
            yield return new WaitForSeconds(delay);
            setNewTarget();
        }
        //agent.speed *= 2;
        //agent.SetDestination(runTarget.position);
    }

    void setNewTarget()
    {
        float x = gameObject.transform.position.x;
        float z = gameObject.transform.position.z;

        float newX = x + Random.Range(-9 - x, 9 - x);
        float newZ = z + Random.Range(-9 - z, 9 - z);

        Vector3 targetLoc = new Vector3(newX, gameObject.transform.position.y, newZ);

        agent.SetDestination(targetLoc);
    }
}
