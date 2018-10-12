using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YieldProlog;

public class AgentBehavior : MonoBehaviour {

    public float retargetDelay;
    public float behaviorDelay;
    public Transform runTarget;
    KnowledgeBase knowledgeBase;
    private string currentBehavior;
    private int myId;

    public float speed;

    private NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        knowledgeBase = gameObject.GetComponent<KnowledgeBase>();
        agent.speed = speed;
        myId = gameObject.GetComponent<AgentInfo>().agentId;
        currentBehavior = "RandomWalkBehavior";
        StartCoroutine("RandomwWalkBehavior");
        StartCoroutine("ChooseBehavior");

    }

    void StartNewBehavior(string newBehavior)
    {
        StopCoroutine(currentBehavior);
        StartCoroutine(newBehavior);
    }

    IEnumerator ChooseBehavior()
    {
        while (true)
        {
            yield return new WaitForSeconds(behaviorDelay);
            Variable deadAgent = new Variable();
            Variable whenVar = new Variable();
            Variable whereVar = new Variable();
            try {
                foreach (bool l1 in YP.matchDynamic(myId, Atom.a("dead"),
                    new object[] { deadAgent, whenVar, whereVar }))
                {
                    Debug.Log(string.Format("{0} HAS DIED IN {1} at {2}",
                        deadAgent.getValue(),
                        whereVar.getValue(),
                        whenVar.getValue()));
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
            }

            Variable killedAgent = new Variable();
            Variable deadAgent = new Variable();
            try
            {
                foreach (bool l1 in YP.matchDynamic(myId, Atom.a("dead"),
                    new object[] { deadAgent, whenVar, whereVar }))
                {
                    Debug.Log(string.Format("{0} HAS DIED IN {1} at {2}",
                        deadAgent.getValue(),
                        whereVar.getValue(),
                        whenVar.getValue()));
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

        }
    }

    IEnumerator RandomwWalkBehavior()
    {
        SetNewTarget();
        while (true)
        {
            yield return new WaitForSeconds(retargetDelay);
            SetNewTarget();
        }
    }

    IEnumerator RunAwayBehavior()
    {
        yield return false;
    }


    void SetNewTarget()
    {
        float x = gameObject.transform.position.x;
        float z = gameObject.transform.position.z;

        float newX = x + Random.Range(-9 - x, 9 - x);
        float newZ = z + Random.Range(-9 - z, 9 - z);

        Vector3 targetLoc = new Vector3(newX, gameObject.transform.position.y, newZ);

        agent.SetDestination(targetLoc);
    }
}
