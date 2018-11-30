using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayer : MonoBehaviour {
    public float moveSpeed;
    public Camera camera;
    private NavMeshAgent agent;
    private LevelController levelController;

	// Use this for initialization
	void Start () {
        levelController = GameObject.Find("GameController").GetComponent<LevelController>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.updateRotation = false;
	}
	
	// Update is called once per frame
	void Update () {
        /*int zoneNum = levelController.getZoneFromObj(gameObject);
        Debug.Log("In zone " + zoneNum);*/
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButton(0)){
            //Debug.Log("Mouse clicked ");

            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Clicked on location " + hit.point);
                agent.SetDestination(hit.point);
            }

        }
	}

    void LateUpdate()
    {
        if(agent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
        }
    }
}
