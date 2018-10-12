using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView:MonoBehaviour {
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;//Our observables
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();
    public List<GameObject> observables = new List<GameObject>();
    private List<GameObject> seenObservables = new List<GameObject>();
    //have a closed set for observables


    void Start()
    {
        seenObservables.Add(gameObject);
        //Debug.Log("STARTING");
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    

    public void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for(int i = 0; i<targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            GameObject obj = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward,dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    //make sure to check that your observable is not yourself

                    visibleTargets.Add(target);
                    if (!seenObservables.Contains(obj))
                    {
                        //Debug.Log("Found gameobject:"+obj.name);
                        observables.Add(obj);
                        seenObservables.Add(obj);
                    }
                }
            }
        }
    }


    public Vector3 DifFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
