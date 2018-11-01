using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTextLevel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float x = gameObject.transform.position.x;
        float z = gameObject.transform.position.z;

        TextMesh mesh = GetComponent<TextMesh>();
        mesh.text = string.Format("({0}, {1})", x, z);
	}
	
}
