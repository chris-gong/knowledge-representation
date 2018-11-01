using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour {

    public static TimeController instance = null;

    #region Members
    public int CurrentTime = 0;
    public string[] IntervalNames = { "Morning", "Day", "Evening", "Night" };
    public int[] IntervalLengths = { 45, 30, 30, 0 };
    public string CurrentInterval;
    private int TimeIntervalIndex = 0;
    private int TimeLeft;
    public float TimeDelay = 1f;
    #endregion

    
    IEnumerator TimeClock()
    {
        while (true) { 
            yield return new WaitForSeconds(this.TimeDelay);
            CurrentTime++;
            TimeLeft--;
            if(TimeLeft == 0) {
                TimeIntervalIndex++;
                UpdateInterval(TimeIntervalIndex);
                Debug.Log("Updating Time Interval to: {" + CurrentInterval + "}");
                if(TimeIntervalIndex > IntervalNames.Length) {
                    EndOfDay();
                }
            }
        }
    }

    private void UpdateInterval(int n)
    {
        CurrentInterval = IntervalNames[n];
        //Debug.Log("INTERVAL: " + IntervalNames[n]);
        TimeLeft = IntervalLengths[n];
    }

    private void EndOfDay()
    {
        return;
    }

    #region Public Methods

    public int GetTime()
    {
        UpdateInterval(0);
        return CurrentTime;
    }

    public string GetIntervalName()
    {
        return CurrentInterval;
    }

    public string GetIntervalName(int n)
    {
        return IntervalNames[n];
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

        Debug.Assert(IntervalLengths.Length == IntervalNames.Length,
                    string.Format("ERROR: Count of Names/Lengths array does not match: {0}/{1}",
                                   IntervalLengths.Length, IntervalNames.Length));
        UpdateInterval(0);
        IEnumerator clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }

    #endregion

    #region Unity Methods

    #endregion
}
