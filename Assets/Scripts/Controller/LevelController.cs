using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    static private LevelController instance;

    #region Location Collections

    private List<Transform> hidingSpots;
    private List<Transform> wanderingSpots;
    private List<Transform> zoneMarkers;

    #endregion

    #region Public Methods

    public List<Transform> GetHidingSpots()
    {
        return this.hidingSpots;
    }

    public List<Transform> GetWanderingSpots()
    {
        return this.wanderingSpots;
    }

    public List<Transform> GetZoneMarkers()
    {
        return this.zoneMarkers;
    }

    public List<int> GetShortestPath()
    {
        return null;
    }

    /// <summary>
    /// Initializes the LevelController and is called by the GameController
    /// </summary>
    public void InitiLevelCtl () {
        if(instance == null)
        {
            LevelController.instance = this;
            hidingSpots = new List<Transform>();
            wanderingSpots = new List<Transform>();
            zoneMarkers = new List<Transform>();
            GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("Hiding Spot");
            GameObject[] zoneObjects = GameObject.FindGameObjectsWithTag("ZoneMarker");
            for(int i = 0; i < zoneObjects.Length; i++) {
                AllocateZoneId(zoneObjects[i]);
            }
            
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
   
    #endregion

    #region Aggregate Methods

    private void AllocateZoneId(GameObject zoneMarker)
    {
        zoneMarkers.Add(zoneMarker.transform);
        ZoneInfo info = zoneMarker.GetComponent<ZoneInfo>();
        if (info != null) {
            info.zoneNum = zoneMarkers.Count;
        }
        else {
            Debug.Log(string.Format("ERROR:gameobject({0}) at {1} is missing a ZoneInfo component"
                                    ,zoneMarker.name,zoneMarker.transform.position.ToString()));
        }
    }

    #endregion
}
