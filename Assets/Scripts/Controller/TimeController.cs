using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeController : MonoBehaviour {

    public static TimeController instance = null;

    #region Fields

    private GameController gameCtl;

    public float currentTime = 0;
    public string[] intervalNames = { "Morning", "Day", "Evening", "Night" };
    public int[] intervalLengths = { 15, 15, 15, 0 };
    public string currentInterval = "Morning";
    public float timeSpeed;
    public float murderTime = -1; //-1 if the murder has not happened yet

    private readonly float timeDelay = 0.2f;
    private int timeIntervalIndex = 0;
    private float remainingIntervalTime;
    private Text timer;
    public UnityEvent onDayEnd;
    public UnityEvent onTimeTick;
    private IEnumerator clockCoroutine;

    #endregion

    #region Clock Coroutines Methods and Events

    IEnumerator TimeClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(this.timeDelay);
            currentTime += timeSpeed;
            timer.text = string.Format("{0}",Math.Round(currentTime, 1));
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
        Time.timeScale = 0;
        StopCoroutine(clockCoroutine);
        timeIntervalIndex = 0;
        LevelController lc = GameController.GetInstanceLevelController();
        lc.ClearEventText();
        lc.EnableBackground();
        lc.EnableRestartButton();
        lc.AddResultText("Round Over");
        
        //lc.AddResultText(string.Format("The murdered occurred in zone {0} at time {1}", lc.GetMurderZone(), murderTime));
        if(murderTime > -1)
        {
            lc.AddResultText(string.Format("The murdered occurred in zone {0} at time {1}", lc.GetMurderZone(), Math.Round(murderTime, 1)));
            onDayEnd.Invoke();
        }
        else
        {
            GameController.GetInstanceLevelController().gameOver = true;
            GameController.GetInstanceLevelController().gameWon = false;
            lc.AddResultText(string.Format("Game Over, failed to commit murder"));
        }

        if (!lc.gameOver && GameController.GetInstance().GetAliveAgentCount() == 2)
        {
            lc.AddResultText("Game Won! You made it to the final two agents");
            lc.gameOver = true;
            lc.gameWon = true;
        }
        return;
    }

    #endregion

    #region Public Methods

    public float GetTime()
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
            Destroy(gameObject);
            return;
        }
        gameCtl = GameController.GetInstance();
        timer = GameObject.Find("Timer").GetComponent<Text>();
        Debug.Assert(intervalLengths.Length == intervalNames.Length,
                    string.Format("ERROR: Count of Names/Lengths array does not match: {0}/{1}",
                                   intervalLengths.Length, intervalNames.Length));
        UpdateInterval(0);
        clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }

    public void SetMurderTime(float time)
    {
        murderTime = time;
    }

    public float GetMurderTime()
    {
        return murderTime;
    }

    public void ResetDay()
    {
        SetMurderTime(-1);
        GameController.GetInstance().ResetTalkingCooldowns();
        GameController.GetInstance().ResetLightFlicker();
        clockCoroutine = TimeClock();
        StartCoroutine(clockCoroutine);
    }
    #endregion

    #region Unity Methods

    #endregion
}
