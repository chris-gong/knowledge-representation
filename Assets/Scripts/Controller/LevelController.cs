using System;
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
    private GameObject pathButton;
    private GameObject agentIdInput;
    private Text playerEvents;
    public bool gameOver;
    public bool gameWon;
    private IEnumerator pathCoroutine;
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

    public void SetEventText(string playerEvent, int duration){
        playerEvents.text = playerEvent;
        if(duration != 0)
        {
            Invoke("ClearEventText", duration);
        }
    }

    public void ClearEventText()
    {
        playerEvents.text = "";
    }

    public void ClearResultsText()
    {
        gameResults.text = "";
    }

    public void EnableBackground()
    {
        gameOverBackground.GetComponent<Image>().enabled = true;
    }

    public void DisableBackground()
    {
        gameOverBackground.GetComponent<Image>().enabled = false;
    }

    public void EnableRestartButton()
    {
        restartButton.GetComponent<Image>().enabled = true;
        restartButton.GetComponent<Button>().enabled = true;
        restartButton.transform.Find("Text").gameObject.SetActive(true);
    }

    public void DisableRestartButton()
    {
        restartButton.GetComponent<Image>().enabled = false;
        restartButton.GetComponent<Button>().enabled = false;
        restartButton.transform.Find("Text").gameObject.SetActive(false);
    }

    public void EnablePathButton()
    {
        pathButton.GetComponent<Image>().enabled = true;
        pathButton.GetComponent<Button>().enabled = true;
        pathButton.transform.Find("Text").gameObject.SetActive(true);
    }

    public void DisablePathButton()
    {
        pathButton.GetComponent<Image>().enabled = false;
        pathButton.GetComponent<Button>().enabled = false;
        pathButton.transform.Find("Text").gameObject.SetActive(false);
    }

    public void EnableGameResults()
    {
        gameResults.enabled = true;
    }

    public void DisableGameResults()
    {
        gameResults.enabled = false;
    }

    public void EnableAgentIdInput()
    {
        agentIdInput.GetComponent<Image>().enabled = true;
        agentIdInput.GetComponent<InputField>().enabled = true;
        agentIdInput.transform.Find("Text").gameObject.SetActive(true);
        agentIdInput.transform.Find("Placeholder").gameObject.SetActive(true);
    }

    public void DisableAgentIdInput()
    {
        agentIdInput.GetComponent<Image>().enabled = false;
        agentIdInput.GetComponent<InputField>().enabled = false;
        agentIdInput.transform.Find("Text").gameObject.SetActive(false);
        agentIdInput.transform.Find("Placeholder").gameObject.SetActive(false);
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
        gameOver = false;
        gameWon = false;
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
                SetEventText("Press e to pick up items", 0);
            }
            else if (obj.name == "PathButton")
            {
                pathButton = obj;
                pathButton.GetComponent<Button>().onClick.AddListener(ShowAgentPath);
            }
            else if (obj.name == "AgentInputField")
            {
                agentIdInput = obj;
            }
        }
    }

    private void ShowAgentPath()
    {
        //TODO: Error checking on the text
        DisableBackground();
        DisableAgentIdInput();
        DisablePathButton();
        DisableRestartButton();
        DisableGameResults();
        List<Agent> agents = GameController.GetInstance().GetAgents();
        int requestedAgent = Int32.Parse(agentIdInput.transform.Find("Text").GetComponent<Text>().text);
        Debug.Log(string.Format("Requested Agent {0}",requestedAgent));
        if (requestedAgent > 0 && requestedAgent < agents.Count && agents[requestedAgent].isAlive)
        {
            if(pathCoroutine != null)
            {
                StopCoroutine(pathCoroutine);
            }
            pathCoroutine = PathScene(agents[requestedAgent].solver.mostLikelyPath);
            StartCoroutine(pathCoroutine);
        }
    }

    IEnumerator PathScene(Path p)
    {
        Time.timeScale = 1;
        Material pathMat = Resources.Load<Material>("PathMat");
        Material oldMat = Resources.Load<Material>("BloodMat");
        GameObject oldZone = null;
        List<int> zones = p.GetZonesInOrder();
        for (int i = 0; i < zones.Count; i++)
        {
            Debug.Log(zones[i]);
            GameObject zone;
            if (oldZone != null)
            {
                zone = oldZone;
                for(int j = 0; j < zone.transform.childCount; j++)
                {
                    if(zone.transform.GetChild(j).tag == "Floor")
                    {
                        zone.transform.GetChild(j).GetComponent<Renderer>().material = oldMat;
                    }
                }
            }
            
            zone = zoneInfoList[zones[i]].transform.parent.gameObject;
            for (int j = 0; j < zone.transform.childCount; j++)
            {
                if (zone.transform.GetChild(j).tag == "Floor")
                {
                    zone.transform.GetChild(j).GetComponent<Renderer>().material = pathMat;
                }
            }
            oldZone = zoneInfoList[zones[i]].transform.parent.gameObject;
            yield return new WaitForSeconds(0.5f);
        }
        if (oldZone != null)
        {
            GameObject zone = oldZone;
            for (int j = 0; j < zone.transform.childCount; j++)
            {
                if (zone.transform.GetChild(j).tag == "Floor")
                {
                    zone.transform.GetChild(j).GetComponent<Renderer>().material = oldMat;
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        Debug.Log("after coroutine");
        Time.timeScale = 0;
        EnableBackground();
        EnableAgentIdInput();
        EnablePathButton();
        EnableRestartButton();
        EnableGameResults();
        yield return null;
    }
    private void RestartLevel()
    {
        //should be implicit that if gameover flag was set then player did not win the game
        if (gameOver)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            //check if one of the agents
            GoToNextRound();
        }
    }

    private void GoToNextRound()
    {
        //set the timescale back and disable menu
        Time.timeScale = 1;
        DisableBackground();
        DisableRestartButton();
        DisableAgentIdInput();
        DisablePathButton();
        ClearEventText();
        ClearResultsText();
        SetMurderZone(-1);
        gameOver = false;
        gameWon = false;
        GameController.GetInstanceTimeController().ResetDay();
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
