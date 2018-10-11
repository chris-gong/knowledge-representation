using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour {
    public float moveSpeed;
    public Rigidbody rb;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 translation = new Vector3(moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime, 0, moveSpeed* Input.GetAxis("Vertical") * Time.deltaTime);
        rb.MovePosition(transform.position + translation);
  
	}
}
