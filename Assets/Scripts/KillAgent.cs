using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAgent : MonoBehaviour
{

    // Use this for initialization
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Agent")
        {
            Destroy(collision.gameObject);
        }
    }
}
