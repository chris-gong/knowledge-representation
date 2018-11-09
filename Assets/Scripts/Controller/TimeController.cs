using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TimeController : MonoBehaviour {

    public static TimeController instance = null;

    #region Fields

    private GameController gameCtl;

    public int currentTime = 0;
    public string[] intervalNames = { "Morning", "Day", "Evening", "Night" };
    public int[] intervalLengths = { 15, 15, 15, 0 };
    public string currentInterval = "Morning";
    public int timeSpeed = 6;
    public int murderTime = -1; //-1 if the murder has not happened yet

    private readonly float timeDelay = 1f;
    private int timeIntervalIndex = 0;
    private int remainingIntervalTime;

    public UnityEvent onDayEnd;
    public UnityEvent onTimeTick;

    #endregion

    #region Clock Coroutines Methods and Events

    IEnumerator TimeClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(this.timeDelay);
            currentTime += timeSpeed;
            remainingIntervalTime -= timeSpeed;
            if (remainingIntervalTime <= 0)
            {
                if (timeIntervalIndex >= intervalNames.Length - 1)
                {
                    EndOfDay();
                    yield break;
                }
                timeIntervalIndex++;
                UpdateInterval(timeIntervalIndex);
                Debug.Log("Updating Time Interval to: {" + currentInterval + "}");
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

    /// <summary>
    /// Ends the day and invokes all end of day events
    /// </summary>
    private void EndOfDay()
    {
        timeIntervalIndex = 0;
        onDayEnd.Invoke();
        return;
    }

    #endregion

    #region Public Methods

    public int GetTime()
    {
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

    /// <summary>
    /// Intializes the TimeController and is called by the GameController
    /// </summary>
    public void InitTimeCtl()
    {
        if (TimeController.instance == null) {
            TimeController.instance = this;
        }
        else {
            Object.Destroy(gameObject);
            return;
        }
        gameCtl = GameController.GetInstance();
        Debug.Assert(intervalLengths.Length == intervalNames.Length,
                    string.Format("ERROR: Count of Names/Lengths array does not match: {0}/{1}",
                                   intervalLengths.Length, intervalNames.Length));
        UpdateInterval(0);
        IEnumerator clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }

    public void SetMurderTime(int time)
    {
        murderTime = time;
    }

    public int GetMurderTime()
    {
        return murderTime;
    }
    #endregion

    #region Unity Methods

    #endregion
}
