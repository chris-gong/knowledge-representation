using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KillAgent : MonoBehaviour
{
    public Transform killedObs;
    public Transform deadObs;
    // Use this for initialization
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Agent")
        {
            Destroy(collision.gameObject);
            Transform killedFact = Instantiate(killedObs, transform.position, Quaternion.identity);
            Instantiate(deadObs, transform.position, Quaternion.identity);
            Destroy(killedFact.gameObject, 2);
        }
    }
}
