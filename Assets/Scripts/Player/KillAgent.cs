using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KillAgent : MonoBehaviour
{
    public GameObject obs;
    public LayerMask targetLayer;
    public float radius = 1.5f;
    
    // Use this for initialization
    /*
    void OnCollisionEnter(Collision collision)
    {
        return;
        if (collision.gameObject.tag == "Agent")
        {
            GameObject eventObs = Instantiate(obs, transform.position+new Vector3(0,1,0), Quaternion.identity);
            Observable facts = eventObs.GetComponent<Observable>();
            facts.addFact(string.Format("killed({0},{1})",
                          gameObject.name,
                          collision.gameObject.name));
            Destroy(eventObs.gameObject, 2);

            GameObject deadObs = Instantiate(obs, transform.position, Quaternion.identity);
            facts = eventObs.GetComponent<Observable>();
            facts.addFact(string.Format("dead({0},{1},{2})",
                          collision.gameObject.name,
                          "level 1",
                          "day"));

            Destroy(collision.gameObject);
            //Instantiate(deadObs, transform.position, Quaternion.identity);
        }
    }
    */
    private void Start()
    {
        StartCoroutine("FindKillableAgents", .2f);
    }

    IEnumerator FindKillableAgents(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

            for (int i = 0; i < targetsInRadius.Length; i++)
            {
                AgentInfo info = (targetsInRadius[i].gameObject).GetComponent<AgentInfo>();
                GameObject eventObs = Instantiate(obs, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                Observable observableFacts = eventObs.GetComponent<Observable>();
                string label = "killed";
                string[] values = new string[] {gameObject.name, info.agentName};

                observableFacts.AddFact(label,values);


                Destroy(eventObs.gameObject, 2);

                GameObject deadObs = Instantiate(obs, transform.position, Quaternion.identity);
                observableFacts = deadObs.GetComponent<Observable>();
                label = "dead";
                values = new string[]{info.agentName, "level 1", "day"};
                observableFacts.AddFact(label, values);

                Destroy(targetsInRadius[i].gameObject);
            }
        }
    }
}
