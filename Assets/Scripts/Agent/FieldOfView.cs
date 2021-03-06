﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView:MonoBehaviour {
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;//Our observables
    public LayerMask obstacleMask;
    public LayerMask agentMask;
    private KnowledgeBase kb; 

    public List<Transform> visibleTargets = new List<Transform>();
    public List<Transform> visibleAgents = new List<Transform>();
    public List<GameObject> observables = new List<GameObject>();
    private List<GameObject> seenObservables = new List<GameObject>();
    public List<GameObject> observedAgents = new List<GameObject>();
    private List<GameObject> seenAgents = new List<GameObject>();
    //have a closed set for observables and clear it at the end of the day


    void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            FindVisibleAgents();
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

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask,QueryTriggerInteraction.Collide))
                {
                    //make sure to check that your observable is not yourself
                    //Debug.Log("target in field of view " + target);
                    visibleTargets.Add(target);
                    if (!seenObservables.Contains(obj))
                    {
                        observables.Add(obj);
                        seenObservables.Add(obj);
                    }
                }
            }
        }
    }

    public void FindVisibleAgents()
    {
        visibleAgents.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, agentMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            GameObject obj = targetsInViewRadius[i].gameObject;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask, QueryTriggerInteraction.Collide))
                {
                    visibleAgents.Add(target);
                    //need to include extra check to prevent it from seeing itself
                    if (!seenAgents.Contains(obj))
                    {
                        observedAgents.Add(obj);
                        seenAgents.Add(obj);
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

    public void ClearSeenAgents()
    {
        seenAgents.Clear();
    }
}
