using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    static private LevelController instance;

    #region Location Collections

    private List<Transform> hidingSpots;
    private List<Transform> wanderingSpots;
    public LayerMask zoneLayer;
    private List<Transform> zoneMarkersTransforms;
    private List<ZoneInfo> zoneInfoList;

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
        return this.zoneMarkersTransforms;
    }

    public List<int> GetShortestPath(int zone1,int zone2)
    {
        return null;
    }

    /// <summary>
    /// Initializes the LevelController and is called by the GameController
    /// </summary>
    public void InitiLevelCtl () {
        if (instance == null)
        {
            LevelController.instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        hidingSpots = new List<Transform>();
        wanderingSpots = new List<Transform>();


        GatherZoneMarkers();
        GatherWanderingSpots();

    }
   
    #endregion

    #region Aggregate Methods

    private void GatherZoneMarkers()
    {

        zoneMarkersTransforms = new List<Transform>();
        zoneInfoList = new List<ZoneInfo>();
        GameObject[] zoneObjects = GameObject.FindGameObjectsWithTag("ZoneMarker");

        for (int i = 0; i < zoneObjects.Length; i++)
        {
            GameObject markerObj = zoneObjects[i];
            ZoneInfo info = markerObj.GetComponent<ZoneInfo>();
            if (info == null)
            {
                Debug.LogError(string.Format("ERROR:gameobject({0}) at {1} is missing a ZoneInfo component"
                                        , markerObj.name, markerObj.transform.position.ToString()));
                continue;
            }
            info.zoneNum = zoneMarkersTransforms.Count;
            zoneMarkersTransforms.Add(markerObj.transform);
            zoneInfoList.Add(info);
        }
    }

    private int getZoneFromObj(GameObject obj)
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(obj.transform.position, 1.0f, zoneLayer);
        if (targetsInRadius.Length > 0)
        {
            int zoneNum = targetsInRadius[0].GetComponent<ZoneInfo>().zoneNum;
            return zoneNum;

        }
        return -1;
    }
    private void GatherWanderingSpots()
    {
        GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("Hiding Spot");
        spotObjects = GameObject.FindGameObjectsWithTag("Wandering Spot");
        for (int i = 0; i < spotObjects.Length; i++)
        {
            wanderingSpots.Add(spotObjects[i].transform);
        }
    }
    #endregion
}
