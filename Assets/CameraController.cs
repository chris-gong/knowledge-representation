using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Vector3 offset;
    public Camera orthoCamera;
    public int cameraMovementSpeed;
    private GameObject player;
    private bool mounted;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        orthoCamera = GetComponent<Camera>();
        mounted = true;
        transform.position = player.transform.position + offset;
    }
	
	// Update is called once per frame
	void Update () {
        if (mounted)
        {
            transform.position = player.transform.position + offset;
        }

        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0)
        {
            transform.position += new Vector3(horizontal * cameraMovementSpeed, 0, 0) * Time.deltaTime;
            mounted = false;
        }

        float vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            transform.position += new Vector3(0, 0, vertical * cameraMovementSpeed) * Time.deltaTime;
            mounted = false;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && orthoCamera.orthographicSize > 1)
        {
            orthoCamera.orthographicSize--;
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0 && orthoCamera.orthographicSize < 100)
        {
            orthoCamera.orthographicSize++;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            mounted = true;
            transform.position = player.transform.position + offset;
        }
	}
}
