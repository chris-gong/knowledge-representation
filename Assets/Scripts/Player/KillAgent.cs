using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KillAgent : MonoBehaviour
{
    public GameObject obs;
    public LayerMask targetLayer;
    public LayerMask weaponLayer;
    public float radius;
    private bool equipped = false;
    
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
        //StartCoroutine("FindKillableAgents", .2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !equipped)
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, weaponLayer);

            if(targetsInRadius.Length > 0)
            {
                Destroy(targetsInRadius[0].transform.parent.gameObject);
                equipped = true;
            }
        }

        if (equipped && Input.GetKeyDown(KeyCode.Space))
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

            if (targetsInRadius.Length > 0)
            {
                Destroy(targetsInRadius[0].gameObject);
            }
        }
    }
    IEnumerator FindKillableAgents(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

            for (int i = 0; i < targetsInRadius.Length; i++)
            {
                Agent info = (targetsInRadius[i].gameObject).GetComponent<Agent>();
                GameObject eventObs = Instantiate(obs, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                Observable observableFacts = eventObs.GetComponent<Observable>();
                Debug.Log(observableFacts);
                string label = "killed";
                string[] values = new string[] {gameObject.name, info.agentName};

                observableFacts.AddObservableFact(label,values);


                Destroy(eventObs.gameObject, 2);

                GameObject deadObs = Instantiate(obs, transform.position, Quaternion.identity);
                observableFacts = deadObs.GetComponent<Observable>();
                label = "dead";
                values = new string[]{info.agentName, "level 1", "day"};
                observableFacts.AddObservableFact(label, values);

                Destroy(targetsInRadius[i].gameObject);
            }
        }
    }
}
