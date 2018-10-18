using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Can use this helper class to spawn prefabbed objects in edit mode, without having
/// to drag them all into the scene one by one.
/// </summary>
[ExecuteInEditMode]
public class ObjectGenerator : MonoBehaviour 
{
    /// <summary>
    /// Check box in inspector to generate objects.
    /// </summary>
    public bool Generate;

    /// <summary>
    /// Assign nr of objects to generate in inspector.
    /// </summary>
    public int NrOfObjects;

    /// <summary>
    /// Reset the state in inspector.
    /// </summary>
    public bool Reset;

    /// <summary>
    /// The pool of objects to choose from.
    /// </summary>
    public GameObject[] ObjectPool;

    /// <summary>
    /// Can assign a prefix for the name in the inspector.
    /// </summary>
    public string NamePrefix;

    /// <summary>
    /// All generated objects will be a child of this parent.
    /// </summary>
    public GameObject parent;

    private int generatedObjects = 0;

    void OnRenderObject()
    {
        if (Reset)
        {
            Generate = false;
            Reset = false;
            generatedObjects = 0;
        }
        if (Generate && generatedObjects < NrOfObjects)
        {
            if (ObjectPool == null || ObjectPool.Length == 0)
            {
                Debug.LogError("Tried generating objects without selecting any prefabs");
                Generate = false;
                return;
            }
            GameObject newGO = (GameObject)
                Object.Instantiate(ObjectPool[Random.Range(0, ObjectPool.Length)]);
            newGO.name = NamePrefix + generatedObjects.ToString();
            if (parent != null)
            {
                newGO.transform.parent = parent.transform;
            }
            generatedObjects++;
            Generate = generatedObjects < NrOfObjects;
        }
        else
        {
            Generate = false;
        }
    }
}
