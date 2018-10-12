using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    static private LevelController instance;
    private List<Transform> hidingSpots;

	// Use this for initialization
	void Start () {
		if(instance == null)
        {
            LevelController.instance = this;
            GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("Hiding Spot");
            for (int i = 0; i < spotObjects.Length; i++)
            {
                hidingSpots.Add(spotObjects[i].transform);
            }
        }
        else
        {
            Destroy(gameObject);
        }


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
