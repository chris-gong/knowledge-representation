using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YieldProlog;
using NPC;

public class AgentBehavior : MonoBehaviour {

    public float retargetDelay;
    public float behaviorDelay;
    public Transform runTarget;
    public LayerMask zoneLayer;
    KnowledgeBase knowledgeBase;
    private AgentInfo info;
    private GameObject levelController;
    private Coroutine currentBehavior;
    private NPCController behaviorController;
    private int lastZone;
    public float speed;

    private NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
        lastZone = 0;
        agent = gameObject.GetComponent<NavMeshAgent>();
        knowledgeBase = gameObject.GetComponent<KnowledgeBase>();
        agent.speed = speed;
        info = gameObject.GetComponent<AgentInfo>();
        behaviorController = gameObject.GetComponent<NPCController>();
        levelController = GameObject.Find("LevelController");
        //currentBehavior = StartCoroutine("RandomwWalkBehavior");
        //thinkingBehavior = StartCoroutine("ChooseBehavior");

        NPCNode behaviorTree = new NPCDecoratorLoop(new NPCSequence(
            new NPCNode[] {
                new NPCAction(() => WanderAround())
            })
        );
        behaviorController.AI.AddBehavior(behaviorTree);
        behaviorController.AI.StartBehavior();
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

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "prop_powerCube")
        {
            Destroy(col.gameObject);
        }
    }

    [NPCAffordance("Wander_Behavior")]
    public BEHAVIOR_STATUS WanderAround()
    {
        //Debug.Log("Affordance activated");
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, 1.0f, zoneLayer);
        if(targetsInRadius.Length > 0)
        {
            int zoneNum = targetsInRadius[0].GetComponent<ZoneInfo>().zoneNum;
            Debug.Log(string.Format("Zone number {0}", zoneNum));
            if(lastZone != zoneNum)
            {
                Debug.Log(string.Format("Changed from zone {0} to zone {1}", lastZone, zoneNum));
                lastZone = zoneNum;
            }
            
        }
        if (agent.destination != null && Vector3.Distance(agent.destination, transform.position) < 1)
        {
            
            /*List<Transform> wanderingSpots = levelController.GetComponent<LevelController>().getWanderingSpots();
            if (wanderingSpots.Count == 0)
            {
                return BEHAVIOR_STATUS.SUCCESS;
            }*/
            //System.Random rand = new System.Random();
            //NOTE: only use unity random, NOT system random
            //int index = Random.Range(0, wanderingSpots.Count);
            //Debug.Log("Index:" + index);
            //Debug.Log("Going to " + wanderingSpots[index].position);
            //Vector3 newPosition = wanderingSpots[index].position;
            //float offsetXRange = Random.Range(-1, 1);
            //float offsetZRange = Random.Range(-1, 1);
            NavMeshHit hit;
            Vector3 randomLocation = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), Random.Range(-50, 50));
            NavMesh.SamplePosition(randomLocation, out hit, 20.0f, NavMesh.AllAreas);
            Debug.Log("Going to position " + hit.position);
            agent.SetDestination(hit.position);
            return BEHAVIOR_STATUS.SUCCESS;
        }
        return BEHAVIOR_STATUS.SUCCESS;
    }
}
