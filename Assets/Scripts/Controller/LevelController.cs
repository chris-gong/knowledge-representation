using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    static private LevelController instance;
    private List<Transform> hidingSpots;
    private List<Transform> wanderingSpots;

	// Use this for initialization
	void Start () {
		if(instance == null)
        {
            LevelController.instance = this;
            hidingSpots = new List<Transform>();
            wanderingSpots = new List<Transform>();
            GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("Hiding Spot");
            for (int i = 0; i < spotObjects.Length; i++)
            {
                hidingSpots.Add(spotObjects[i].transform);
            }

            spotObjects = GameObject.FindGameObjectsWithTag("Wandering Spot");
            for (int i = 0; i < spotObjects.Length; i++)
            {
                wanderingSpots.Add(spotObjects[i].transform);
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

    public List<Transform> getHidingSpots()
    {
        return this.hidingSpots;
    }

    public List<Transform> getWanderingSpots()
    {
        return this.wanderingSpots;
    }
}
