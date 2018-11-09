using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    static private LevelController instance;

    #region Location Collections

    private List<Transform> hidingSpots;
    private List<Transform> wanderingSpots;
    public LayerMask zoneLayer;
    private List<Transform> zoneMarkersTransforms;
    private List<ZoneInfo> zoneInfoList;
    private int murderedZone;
    private GameObject gameOverBackground;
    private Text gameResults;
    private GameObject restartButton;
    private Text playerEvents;

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

    public List<ZoneInfo> GetZoneInfos()
    {
        return this.zoneInfoList;
    }

    public int GetMurderZone()
    {
        return murderedZone;
    }

    public void SetMurderZone(int zoneNum)
    {
        murderedZone = zoneNum;
    }

    public void AddResultText(string result)
    {
        gameResults.text += string.Format("{0}\n", result);
    }

    public void setEventText(string playerEvent, int duration){
        playerEvents.text = playerEvent;
        Invoke("clearEventText", duration);
    }

    public void clearEventText()
    {
        playerEvents.text = "";
    }
    public void enableBackground()
    {
        gameOverBackground.GetComponent<Image>().enabled = true;
    }

    public void disableBackground()
    {
        gameOverBackground.GetComponent<Image>().enabled = false;
    }

    public void enableRestartButton()
    {
        restartButton.GetComponent<Image>().enabled = true;
        restartButton.GetComponent<Button>().enabled = true;
        restartButton.transform.Find("Text").gameObject.SetActive(true);
    }

    public void disableRestartButton()
    {
        restartButton.GetComponent<Image>().enabled = false;
        restartButton.GetComponent<Button>().enabled = false;
        restartButton.transform.Find("Text").gameObject.SetActive(false);
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
        GatherUIElements();
        GatherZoneMarkers();
        GatherWanderingSpots();

    }
   
    #endregion

    #region Aggregate Methods

    private void GatherUIElements()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Transform[] children = canvas.GetComponentsInChildren<Transform>();
        foreach (Transform t in children)
        {
            GameObject obj = t.gameObject;
            if (obj.name == "GameResults")
            {
                gameResults = obj.GetComponent<Text>();
            }
            else if (obj.name == "Background")
            {
                gameOverBackground = obj;
            }
            else if (obj.name == "RestartButton")
            {
                restartButton = obj;
                restartButton.GetComponent<Button>().onClick.AddListener(RestartLevel);
            }
            else if (obj.name == "PlayerEvents")
            {
                playerEvents = obj.GetComponent<Text>();
            }
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
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
            TextMesh mesh = markerObj.transform.parent.Find("HoverText").GetComponent<TextMesh>();
            Vector3 objPos = markerObj.transform.position;
            mesh.text = string.Format("Zone {0}", info.zoneNum);
            zoneMarkersTransforms.Add(markerObj.transform);
            zoneInfoList.Add(info);
        }
    }

    public int GetZoneFromObj(GameObject obj)
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
