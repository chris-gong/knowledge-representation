using UnityEngine;
using System;
using System.Collections;


public class BehaviorUpdater : MonoBehaviour
{
    public float updateTime = 0.05f;
    protected float nextUpdate = 0.0f;

    private static BehaviorUpdater instance = null;

    void OnEnable()
    {
        if (instance != null)
            throw new ApplicationException("Multiple BehaviorUpdaters found");
        instance = this;
    }

    void Start()
    {
        this.nextUpdate = Time.time + this.updateTime;
    }

    void FixedUpdate()
    {
        if (Time.time > this.nextUpdate)
        {
            BehaviorManager.Instance.Update(this.updateTime);
            this.nextUpdate += this.updateTime;
        }
    }
}