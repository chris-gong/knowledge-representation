using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneInfo : MonoBehaviour {

    public int zoneNum;
    public ZoneInfo[] neighbors;
    private Vector3 coords;

	void Start () {
        coords = transform.position;
	}
	
}
