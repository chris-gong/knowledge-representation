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
    private Agent info;
    private Coroutine currentBehavior;
    private NPCController behaviorController;
    private int lastZone;
    public float speed;
    public GameObject targetMarker;
    public GameObject curMarker;

    private NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
        lastZone = 0;
        agent = gameObject.GetComponent<NavMeshAgent>();
        knowledgeBase = gameObject.GetComponent<KnowledgeBase>();
        agent.speed = speed;
        info = gameObject.GetComponent<Agent>();
        behaviorController = gameObject.GetComponent<NPCController>();

        NPCNode behaviorTree = new NPCDecoratorLoop(new NPCSequence(
            new NPCNode[] {
                new NPCAction(() => WanderAround())
            })
        );
        behaviorController.AI.AddBehavior(behaviorTree);
        behaviorController.AI.StartBehavior();
    }

    #region deprecated
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
                //StopCoroutine(thinkingBehavior);
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
        List<Transform> hidingSpots = GameController.GetInstanceLevelController().GetHidingSpots();
        System.Random rand = new System.Random();
        int index = rand.Next(hidingSpots.Count);
        //Debug.Log(hidingSpots[index].position);
        agent.SetDestination(hidingSpots[index].position);
        agent.speed = agent.speed * 3;

    }

    void OnCollisionEnter(Collision col)
    {
        /*if (col.gameObject.name == "prop_powerCube")
        {
            Destroy(col.gameObject);
        }*/
    }
    #endregion
    [NPCAffordance("Wander_Behavior")]
    public BEHAVIOR_STATUS WanderAround()
    {
        if (agent.destination != null && Vector3.Distance(agent.destination, transform.position) < 1)
        {
            //random number used to make the agent wait before finding a new spot
            float pickANewSpot = Random.Range(0f, 1f);
            if(pickANewSpot > 0.98)
            {
                NavMeshHit hit;
                List<Transform> zoneMarkers = GameController.GetInstanceLevelController().GetZoneMarkers();
                Vector3 randomLocation = zoneMarkers[Random.Range(0, zoneMarkers.Count)].position;
                Vector3 offset = new Vector3(Random.Range(-2, 2), 0, Random.Range(2, 2));
                randomLocation += offset;
                /*if (curMarker == null)
                {
                    curMarker = Instantiate(targetMarker, randomLocation, Quaternion.identity);
                }
                curMarker.transform.position = randomLocation; */
                NavMesh.SamplePosition(randomLocation, out hit, 1.0f, NavMesh.AllAreas);
                agent.SetDestination(hit.position);
                return BEHAVIOR_STATUS.SUCCESS;
            }
        }
        return BEHAVIOR_STATUS.SUCCESS;
    }
}
