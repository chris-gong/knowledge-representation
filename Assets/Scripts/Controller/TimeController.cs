﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TimeController : MonoBehaviour {

    public static TimeController instance = null;

    #region Fields
    public int currentTime = 0;
    public string[] intervalNames = { "Morning", "Day", "Evening", "Night" };
    public int[] intervalLengths = { 45, 30, 30, 0 };
    public string currentInterval;
    public int timeSpeed = 1;

    private readonly float timeDelay = 1f;
    private int timeIntervalIndex = 0;
    private int remainingIntervalTime;

    public UnityEvent onDayEnd;
    public UnityEvent onTimeTick;

    #endregion


    IEnumerator TimeClock()
    {
        while (true) { 
            yield return new WaitForSeconds(this.timeDelay);
            currentTime+= timeSpeed;
            remainingIntervalTime-= timeSpeed;
            if(remainingIntervalTime <= 0) {
                timeIntervalIndex++;
                UpdateInterval(timeIntervalIndex);
                Debug.Log("Updating Time Interval to: {" + currentInterval + "}");
                if(timeIntervalIndex > intervalNames.Length) {
                    EndOfDay();
                }
            }
            onTimeTick.Invoke();
        }
    }

    private void UpdateInterval(int n)
    {
        currentInterval = intervalNames[n];
        //Debug.Log("INTERVAL: " + IntervalNames[n]);
        remainingIntervalTime = intervalLengths[n];
    }

    private void EndOfDay()
    {
        onDayEnd.Invoke();
        return;
    }

    #region Public Methods

    public int GetTime()
    {
        UpdateInterval(0);
        return currentTime;
    }

    public string GetIntervalName()
    {
        return currentInterval;
    }

    public string GetIntervalName(int n)
    {
        return intervalNames[n];
    }

    // Use this for initialization
    public void InitTimeCtl()
    {
        if (TimeController.instance == null) {
            TimeController.instance = this;
        }
        else {
            Object.Destroy(gameObject);
            return;
        }

        Debug.Assert(intervalLengths.Length == intervalNames.Length,
                    string.Format("ERROR: Count of Names/Lengths array does not match: {0}/{1}",
                                   intervalLengths.Length, intervalNames.Length));
        UpdateInterval(0);
        IEnumerator clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }

    #endregion

    #region Unity Methods

    #endregion
}
