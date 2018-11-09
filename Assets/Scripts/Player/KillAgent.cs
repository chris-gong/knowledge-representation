using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KillAgent : MonoBehaviour
{
    public GameObject obs;
    public LayerMask targetLayer;
    public LayerMask weaponLayer;
    public GameController gameController;
    public float radius;
    private bool equipped = false;
    private Item equippedItem;
    
    // Use this for initialization

    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !equipped)
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, weaponLayer);

            if(targetsInRadius.Length > 0)
            {
                Destroy(targetsInRadius[0].transform.parent.gameObject);
                GameController.GetInstanceInventoryController().AddItem(new MurderWeaponItem("Knife"));
                GameController.GetInstanceLevelController().setEventText("Knife Added to Inventory (press i to see)", 4);
            }
        }

        if (equipped && Input.GetKeyDown(KeyCode.Space))
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetLayer);

            if (targetsInRadius.Length > 0)
            {
                targetsInRadius[0].gameObject.SetActive(false);
                Agent agent = targetsInRadius[0].gameObject.GetComponent<Agent>();
                agent.isAlive = false;
                unEquipPlayer(); //one kill per weapon only
                int time = gameController.GetTimeController().GetTime();
                gameController.GetTimeController().SetMurderTime(time);
                gameController.GetLevelController().SetMurderZone(gameController.GetLevelController().GetZoneFromObj(gameObject));
                GameController.GetInstanceLevelController().setEventText(string.Format("Agent {0} was killed", agent.agentId), 3);
            }
        }
    }

    public void equipPlayer(Item weapon)
    {
        equipped = true;
        equippedItem = weapon;
    }

    public void unEquipPlayer()
    {
        equipped = false; //one kill per weapon only
        equippedItem = null;
    }
    #region deprecated
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
    #endregion
    
}
