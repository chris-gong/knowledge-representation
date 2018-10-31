using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    static private LevelController instance;
    private List<Transform> hidingSpots;
    private List<Transform> wanderingSpots;
    private List<Transform> zoneMarkers;
    public int timeUntilWander;

	// Use this for initialization
	void Start () {
		if(instance == null)
        {
            timeUntilWander = 0;
            LevelController.instance = this;
            hidingSpots = new List<Transform>();
            wanderingSpots = new List<Transform>();
            zoneMarkers = new List<Transform>();
            GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("Hiding Spot");
            GameObject[] zoneObjects = GameObject.FindGameObjectsWithTag("ZoneMarker");

            for (int i = 0; i < spotObjects.Length; i++)
            {
                hidingSpots.Add(spotObjects[i].transform);
            }

            spotObjects = GameObject.FindGameObjectsWithTag("Wandering Spot");
            //Debug.Log("Number of wandering spots: " + spotObjects.Length);
            for (int i = 0; i < spotObjects.Length; i++)
            {
                wanderingSpots.Add(spotObjects[i].transform);
            }
            for( int i = 0; i < zoneObjects.Length; i++)
            {
                GameObject zoneMarker = zoneObjects[i];
                ZoneInfo info = zoneMarker.GetComponent<ZoneInfo>();
                if(info != null)
                {
                    info.zoneNum = i;
                }
                else
                {
                    Debug.Log(zoneMarker.name);
                }
                zoneMarkers.Add(zoneMarker.transform);
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

    public List<Transform> getZoneMarkers()
    {
        return this.zoneMarkers;
    }
}
