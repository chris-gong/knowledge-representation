using UnityEngine;
using System.Collections;

/// <summary>
/// Highlights an object using a Halo.
/// </summary>
public class HaloHighlight : MonoBehaviour 
{   

    private Behaviour halo;

    /// <summary>
    /// The target's transform. Used preferrably over TargetPosition.
    /// </summary>
    public Transform TargetTransform;

    /// <summary>
    /// The target's position.
    /// </summary>
    public Vector3 TargetPosition;

    /// <summary>
    /// Is our halo enabled?
    /// </summary>
    public bool haloEnabled;

	// Use this for initialization
	void Start () 
    {
        halo = (Behaviour) GetComponent("Halo");
	}
	
	// Update is called once per frame
	void Update () 
    {
        halo.enabled = this.haloEnabled;
        if (TargetTransform != null)
        {
            MoveTo(TargetTransform.position);
        }
        else
        {
            MoveTo(TargetPosition);
        }
	}

    private void MoveTo(Vector3 position)
    {
        this.transform.position = position;
    }
}
