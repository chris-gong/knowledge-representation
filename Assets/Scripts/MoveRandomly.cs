using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveRandomly : MonoBehaviour {

    public float timer;

    public int timeAvailable;

    public float speed; 

    public NavMeshAgent agent;
	// Use this for initialization
	void Start () {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.speed = speed;
        setNewTarget();

    }
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if(timer > timeAvailable)
        {
            setNewTarget();
            timer = 0;
        }
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
