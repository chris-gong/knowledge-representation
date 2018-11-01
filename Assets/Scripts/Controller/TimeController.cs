using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeController : MonoBehaviour {
    #region Properties
    public int CurrentTime = 0;
    public string[] IntervalNames = { "Morning", "Day", "Evening", "Night" };
    public int[] IntervalLengths = { 45, 30, 30, 0 };
    public string TimeIntervalName;
    private int TimeIntervalIndex = 0;
    private int TimeLeft;
    public float TimeDelay = 1f;
    #endregion

    #region Property Functions
    public int GetTime()
    {
        UpdateInterval(0);
        return CurrentTime;
    }
    public string GetIntervalName()
    {
        return TimeIntervalName;
    }
    public string GetIntervalName(int n)
    {
        return IntervalNames[n];
    }
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
            }
        }
    }
    private void UpdateInterval(int n)
    {
        TimeIntervalName = IntervalNames[n];
        //Debug.Log("INTERVAL: " + IntervalNames[n]);
        TimeLeft = IntervalLengths[n];
    }

    #region Unity Methods

    // Use this for initialization
    void Start()
    {
        UpdateInterval(0);
        IEnumerator clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }
    #endregion
}
