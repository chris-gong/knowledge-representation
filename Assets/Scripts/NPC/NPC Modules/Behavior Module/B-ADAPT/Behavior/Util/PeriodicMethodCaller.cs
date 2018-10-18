using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class to periodically call methods.
/// </summary>
public class PeriodicMethodCaller : MonoBehaviour 
{

    private static PeriodicMethodCaller instance;

    /// <summary>
    /// Self-generating singleton.
    /// </summary>
    /// <returns></returns>
    public static PeriodicMethodCaller GetInstance()
    {
        if (instance == null)
        {
            instance = new GameObject("MethodCaller").AddComponent<PeriodicMethodCaller>();
            instance.Initialize();
        }
        return instance;
    }

    private class PeriodicMethodClass
    {
        public PeriodicMethod method;

        public float timeBetweenCalls;

        public float timeSinceLastCall;

        public PeriodicMethodClass(PeriodicMethod method, float timeBetweenCalls)
        {
            this.method = method;
            this.timeBetweenCalls = timeBetweenCalls;
            this.timeSinceLastCall = timeBetweenCalls;
        }
    }

    public delegate void PeriodicMethod();

    private List<PeriodicMethodClass> methodsToCall;

    private bool initialized;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (!initialized)
        {
            Initialize();
        }
    }

    void Initialize()
    {
        methodsToCall = new List<PeriodicMethodClass>();
        initialized = true;
    }

    //Called on each new frame
	void Update () 
    {
        foreach (PeriodicMethodClass method in methodsToCall)
        {
            method.timeSinceLastCall += Time.deltaTime;
            if (method.timeSinceLastCall >= method.timeBetweenCalls)
            {
                method.method.Invoke();
                method.timeSinceLastCall = method.timeSinceLastCall - method.timeBetweenCalls;
            }
        }
	}

    /// <summary>
    /// Add a method to be called periodically every secondsBetweenCalls seconds.
    /// </summary>
    public void StartCallPeriodically(PeriodicMethod method, float secondsBetweenCalls)
    {
        methodsToCall.Add(new PeriodicMethodClass(method, secondsBetweenCalls));
    }

    /// <summary>
    /// StopUpdating calling the given method periodically. Note that the method must be equal to a previously added method
    /// in the sense of pointer equality for it to be removed successfully.
    /// </summary>
    public void StopCallPeriodically(PeriodicMethod method)
    {
        PeriodicMethodClass toRemove = null;
        foreach (PeriodicMethodClass methodClass in methodsToCall)
        {
            if (methodClass.method == method)
            {
                toRemove = methodClass;
            }
        }
        if (toRemove != null)
        {
            methodsToCall.Remove(toRemove);
        }
    }
}
