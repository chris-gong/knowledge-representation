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
    private AgentInfo info;
    private GameObject levelController;
    private Coroutine currentBehavior;
    private Coroutine thinkingBehavior;
    public float speed;

    private NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        knowledgeBase = gameObject.GetComponent<KnowledgeBase>();
        agent.speed = speed;
        info = gameObject.GetComponent<AgentInfo>();
        levelController = GameObject.Find("LevelController");
        currentBehavior = StartCoroutine("RandomwWalkBehavior");
        thinkingBehavior = StartCoroutine("ChooseBehavior");

    }

    void StartNewBehavior(string newBehavior)
    {
        StopCoroutine(currentBehavior);
        currentBehavior = StartCoroutine(newBehavior);
    }

    IEnumerator ChooseBehavior()
    {
        while (true)
        {
            yield return new WaitForSeconds(behaviorDelay);
            //Debug.Log("Choosing behavior");
            Variable deadAgent = new Variable();
            Variable whenVar = new Variable();
            Variable whereVar = new Variable();
            //Debug.Log("AgentBehavior" + info.agentName);
            int deadCount = 0;
            bool killerFound = false;
            try {
                foreach (bool l1 in YP.matchDynamic(info.agentId, Atom.a("dead"),
                    new object[] { deadAgent, whenVar, whereVar }))
                {
                    Debug.Log(string.Format("{0} KNOWS {1} HAS DIED IN {2} at {3}",
                        info.agentName,
                        deadAgent.getValue(),
                        whereVar.getValue(),
                        whenVar.getValue()));
                    deadCount++;
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
            }

            Variable killerAgent = new Variable();
            try
            {
                foreach (bool l1 in YP.matchDynamic(info.agentId, Atom.a("killed"),
                    new object[] { killerAgent, deadAgent}))
                {
                    Debug.Log(string.Format("{0} KNOWS {1} HAS KILLED {2}",
                        info.agentName,
                        killerAgent.getValue(),
                        deadAgent.getValue()));
                    killerFound = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            if(deadCount == 3 || killerFound)
            {
                StartNewBehavior("RunAwayBehavior");
                StopCoroutine(thinkingBehavior);
            }
        }
    }

    IEnumerator RandomwWalkBehavior()
    {
        SetNewTarget();
        while (true)
        {
            yield return new WaitForSeconds(retargetDelay);
            //Debug.Log("setting new target");
            SetNewTarget();
            //StopCoroutine("RunAwayBehavior");
        }
    }

    IEnumerator RunAwayBehavior()
    {
        SetHidingSpot();
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
    void SetHidingSpot()
    {
        List<Transform> hidingSpots = levelController.GetComponent<LevelController>().getHidingSpots();
        System.Random rand = new System.Random();
        int index = rand.Next(hidingSpots.Count);
        //Debug.Log(hidingSpots[index].position);
        agent.SetDestination(hidingSpots[index].position);
        agent.speed = agent.speed * 3;

    }
}
