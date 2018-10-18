using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;

public enum BehaviorStatus
{
    Idle,         //< Object is doing nothing
    Running,      //< Object is running its own tree
    Terminating,  //< Object is terminating its own tree
    InEvent,      //< Object is suspended and locked by an event
    Restarting,   //< Object is terminating, but will immediately restart
}

public class BehaviorObject
{
    public delegate void StatusChangedEventHandler(
        BehaviorObject sender, 
        BehaviorStatus newStatus);

    public event StatusChangedEventHandler StatusChanged;

    private BehaviorStatus status;
    /// <summary>
    /// The status of the agent. Use the OnStatusChanged event to be notified
    /// of changes
    /// </summary>
    public BehaviorStatus Status 
    { 
        get
        {
            return this.status;
        }
        
        protected set
        {
            this.status = value;
            if (this.StatusChanged != null)
                this.StatusChanged.Invoke(this, value);
        }
    }

    /// <summary>
    /// The priority that an event must exceed in order to be run on this
    /// agent. Reports zero if the agent isn't running any events.
    /// </summary>
    public float CurrentPriority
    {
        get
        {
            if (this.PendingEvent != null)
                return this.PendingEvent.Priority;
            if (this.CurrentEvent != null)
                return this.CurrentEvent.Priority;
            return 0.0f;
        }
    }

    /// <summary>
    /// User-defined token to attach to this object if necessary
    /// </summary>
    public object Token { get; set; }

    /// <summary>
    /// The event the agent is currently involved in, if any
    /// </summary>
    public BehaviorEvent CurrentEvent { get; private set; }

    private BehaviorEvent pendingEvent = null;
    /// <summary>
    /// Keep track of which event, if any, is interested in us
    /// </summary>
    public BehaviorEvent PendingEvent 
    {
        get
        {
            return this.pendingEvent;
        }

        set
        {
            if (this.pendingEvent != null
                && value != null
                && BehaviorEvent.ComparePriority(value, this.pendingEvent) <= 0)
                Debug.LogWarning("Replacing current evt with bad priority");
            this.pendingEvent = value;
        }
    }

    /// <summary>
    /// We don't register to receive updates from the BehaviorManager
    /// </summary>
    protected virtual void Register()
    {
    }
    
    /// <summary>
    /// Constructs a new BehaviorObject capable of participating in events
    /// </summary>
    /// <param name="root">The root node of the tree</param>
    /// <param name="statusChanged">An OnStatusChanged delegate for receiving 
    /// status change events</param>
    public BehaviorObject()
    {
        this.CurrentEvent = null;
        this.PendingEvent = null;
        this.Status = BehaviorStatus.Idle;
        this.Register();
    }

    /// <summary>
    /// Returns true if and only if the candidate event is higher
    /// priority than both the running and next pending event, if any
    /// </summary>
    internal RunStatus IsElegible(BehaviorEvent candidate)
    {
        if (this.CurrentEvent != null && pendingEvent != candidate
            && BehaviorEvent.ComparePriority(candidate, this.CurrentEvent) <= 0)
            return RunStatus.Failure;

        if (this.pendingEvent != null && pendingEvent != candidate
            && BehaviorEvent.ComparePriority(candidate, this.pendingEvent) <= 0)
            return RunStatus.Failure;

        return RunStatus.Success;
    }

    /// <summary>
    /// Notification that our pending event has become our current event
    /// </summary>
    internal void LaunchEvent()
    {
        if (this.CurrentEvent != null)
            throw new ApplicationException(
                this + ".EventStarted(): Clearing active event");
        else if (this.Status != BehaviorStatus.Idle)
            throw new ApplicationException(
                this + ".EventStarted(): Starting evt on busy agent");

        this.CurrentEvent = this.pendingEvent;
        this.pendingEvent = null;
        this.Status = BehaviorStatus.InEvent;
    }

    /// <summary>
    /// Clears out the current event
    /// </summary>
    internal void ClearEvent()
    {
        if (this.CurrentEvent != null)
            if (this.CurrentEvent.Status != EventStatus.Finished)
                throw new ApplicationException("Clearing active event!");
        this.CurrentEvent = null;
    }

    /// <summary>
    /// Notification that the event has finished.
    /// </summary>
    internal void FinishEvent()
    {
        if (this.CurrentEvent == null)
            throw new ApplicationException(
                this + ".EventFinished(): No active event");
        this.CurrentEvent = null;
        this.Status = BehaviorStatus.Idle;
    }

    /// <summary>
    /// External command for resuming autonomy, will always succeed
    /// unless an error is thrown (i.e. resuming while in an event
    /// or terminating)
    /// </summary>
    internal virtual void StartBehavior()
    {
    }

    /// <summary>
    /// Stops whatever the agent is doing autonomously, if anything
    /// </summary>
    /// <returns>true if the agent is idle, false otherwise</returns>
    internal virtual RunStatus StopBehavior()
    {
        return RunStatus.Success;
    }
}
