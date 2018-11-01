using UnityEngine;
using System.Collections;
using TreeSharpPlus;

using NPC;
using System;
using System.Collections.Generic;
using System.Linq;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

/// <summary>
/// We will use this module as the connector of NPC with
/// TreeSharpPlus. The main purpose of this module is to
/// be a shell for the already implemented affordances of
/// the agents. Nodes will tick here and the module on each
/// tick will return the current status of each node.
/// 
/// Note that for each affordance, we will need two functions:
///     1. Node return to create the actual Behavior Tree component
///     2. A wrapper controller on the affordance which will be ticked on
///        and need to return, on tick, the status of the node.
/// Although this will add an extra layer of complexity, I will concentrate
/// all the implementation on this module rather than mixing TreeSharpPlus
/// with the NPC affordances. That will allow, in the long run, for more
/// freedom, scalable, maitainable and clear code.
/// 
/// </summary>

[RequireComponent(typeof(NPCController))]
public class NPCBehavior : MonoBehaviour, INPCModule, IHasBehaviorObject {

    #region Members

    private NPCController g_NPCController;
    public bool Enabled = true;
    private bool g_GestureRunning = false;
    BehaviorObject g_BehaviorObject;
    private bool g_Initialized = false;
    private Transform[] g_RoamingPoints;

    public BehaviorObject Behavior {
        get {
            return g_BehaviorObject;
        }
    }

    #endregion

    #region Properties
    public bool InEvent {
        get {
            return g_BehaviorObject.Status != BehaviorStatus.Idle &&
                g_BehaviorObject.CurrentEvent != null;
        }
    }
    #endregion

    #region Unity_Methods

    public void Awake() { }

    #endregion

    #region Public_Functions

    public void StartEvent(Node behavior, bool interruptible = false, IHasBehaviorObject[] agents = null) {
        if (Enabled) {
            if (interruptible) {
                float priority = 1f;
                if (InEvent)
                    priority = g_BehaviorObject.CurrentEvent.Priority + 0.1f;
            }
            agents = agents == null ? new IHasBehaviorObject[] { this } : agents;
            BehaviorEvent ev = new BehaviorEvent(doEvent => behavior, agents);
            ev.StartEvent(InEvent ? g_BehaviorObject.CurrentPriority + 1f : 1f);
        }
    }

    public void StopBehavior() {
        if(InEvent)
            g_BehaviorObject.FinishEvent();
        g_BehaviorObject.StopBehavior();
        g_NPCController.Body.StopAllMotion();
    }

    public Node NPCBehavior_Grab(NPCObject obj) {
        return new Sequence(
                NPCBehavior_LookAt(obj.GetMainLookAtPoint(),true),
                NPCBehavior_GoToAtDistance(new Vector3(obj.GetPosition().x,this.transform.position.y, obj.GetPosition().z), 0.8f),
                new LeafInvoke(() => Behavior_GrabRightHand(obj)),
                new LeafWait(1000),
                NPCBehavior_LookAt(obj.GetMainLookAtPoint(), false)
            );
    }

    public Node NPCBehavior_CasualConversation(NPCBehavior agentA, NPCBehavior agentB) {
        return new Sequence(
                agentB.NPCBehavior_GoToAtDistance(agentA.transform, 1.5f , false),
                agentB.NPCBehavior_OrientTowards(agentA.transform),
                agentB.NPCBehavior_LookAt(agentA.transform,true),
                agentB.NPCBehavior_DoGesture(GESTURE_CODE.WAVE_HELLO),
                agentA.NPCBehavior_OrientTowards(agentB.transform),
                agentA.NPCBehavior_LookAt(agentB.transform, true),
                new DecoratorLoop(
                    new SelectorParallel(
                        new SequenceShuffle(
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.TALK_SHORT),
                            agentB.NPCBehavior_DoGesture(GESTURE_CODE.TALK_LONG),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.ACKNOWLEDGE),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.HURRAY),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.DRUNK)
                            ),
                        new SequenceShuffle(
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.PHONEME_A),
                            agentB.NPCBehavior_DoGesture(GESTURE_CODE.PHONEME_E),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.PHONEME_O),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.PHONEME_U),
                            agentA.NPCBehavior_DoGesture(GESTURE_CODE.PHONEME_P)
                            )
                        )

                ),
                agentB.NPCBehavior_LookAt(null, false),
                agentA.NPCBehavior_LookAt(null, false),
                agentB.NPCBehavior_DoGesture(GESTURE_CODE.WAVE_HELLO)
            );
    }

    public Node NPCBehavior_Dance_Energetic() {
        return new Sequence(
            new DecoratorLoop(
                new SequenceShuffle(
                    NPCBehavior_DoGesture(GESTURE_CODE.DANCE_HIPHOP),
                    NPCBehavior_DoGesture(GESTURE_CODE.DANCE_HOUSE),
                    NPCBehavior_DoGesture(GESTURE_CODE.DANCE_HIPHOP_2)
                )
            )
        );
    }

    public Node NPCBehavior_SimpleFollow(Transform leader, NPCBehavior member) {
        return new Sequence(
                NPCBehavior_Follow(leader, false)
            );
    }

    public Node NPCBehavior_BecomeLeaderAndWander(string tag, long speed, Transform[] points, Color c) {
        return new Sequence(
            new LeafInvoke(() => {
                g_NPCController.SetNPCTag(tag);
                return RunStatus.Success;
            }),
            NPCBehavior_ChangeColor(speed, c),
            NPCBehavior_GoToRandomPoint(points, false)
        );
    }

    public Node NPCBehavior_ApproachAndWait(Transform target, bool run) {
        return new Sequence(NPCBehavior_GoTo(target, run), new LeafWait(1000));
    }

    public Node NPCBehavior_TakeSit(NPCObject chair) {
        Transform t = chair.MainInteractionPoint;
        return NPCBehavior_TakeSit(t);
    }
   

    public Node NPCBehavior_TakeSit(NPCObject chair, Transform table)
    {
        Transform t = chair.MainInteractionPoint;
        return NPCBehavior_TakeSit(t, table);
    }

    public Node NPCBehavior_TakeSit(Transform t) {
        return new Sequence(
                NPCBehavior_GoTo(t, false),
                NPCBehavior_OrientTowards(t.position + t.forward),
                NPCBehavior_DoTimedGesture(GESTURE_CODE.SITTING,true)
            );
    }

    public Node NPCBehavior_TakeSit(Transform t, Transform p)
    {
        return new Sequence(
                NPCBehavior_GoTo(t, false),
                NPCBehavior_OrientTowards(p),
                NPCBehavior_DoGesture(GESTURE_CODE.SITTING, true)
            );
    }

    public Node NPCBehavior_PatrolRandomPoints(Transform[] points) {
        g_RoamingPoints = g_RoamingPoints == null ? points : g_RoamingPoints;
        return new Sequence(
            new SequenceShuffle(
                new NodeWeight(1.2f, NPCBehavior_DoTimedGesture(GESTURE_CODE.IDLE_SMALL_STEPS)),
                new NodeWeight(0.8f, NPCBehavior_DoTimedGesture(GESTURE_CODE.LOOK_AROUND)),
                new NodeWeight(0.85f, new LeafInvoke(() => Behavior_Wander(false))),
                new NodeWeight(0.75f, new LeafWait(2500)),
                new NodeWeight(0.8f, NPCBehavior_DoTimedGesture(GESTURE_CODE.YAWN))
            )
        );
    }

    public Node NPCBehavior_StopFollow() {
        return new Sequence(
            new LeafInvoke(() => {
                g_NPCController.Body.StopFollow();
                return RunStatus.Success; } 
            )
        );

    }

    public Node NPCBehavior_Follow(Transform target, bool run) {
        return new Sequence(
            new LeafInvoke(() => Behavior_Follow(target, false))
        );
        
    }

    public Node NPCBehavior_GoToRandomPoints(Transform[] points) {
        g_RoamingPoints = g_RoamingPoints == null ? points : g_RoamingPoints;
        return new DecoratorLoop(
                new Sequence(
                    new LeafInvoke(() => Behavior_Wander(false)),
                    new LeafWait(2000)
                )
        );
    }

    public Node NPCBehavior_GoToRandomPoint(Transform[] points, bool run) {
        g_RoamingPoints = points != null ? points : g_RoamingPoints;
        return new LeafInvoke(() => Behavior_Wander(run));
    }
        

    public Node NPCBehavior_ConglomerateAndGreetInGroup(NPCBehavior[] agents) {
        return new Sequence(
                new LeafInvoke(() => {
                    g_NPCController.Body.StopNavigation();
                    return RunStatus.Success;
                }),
                new LeafWait(1000),
                new LeafInvoke(() => {
                    g_NPCController.Body.StopAllMotion();
                    return RunStatus.Success;
                }),
                new LeafInvoke(() => {
                    foreach(NPCBehavior b in agents) {
                        if (b.GetComponent<NPCController>().Body.Navigating)
                            return RunStatus.Running;
                    }
                    return RunStatus.Success;
                }),
                NPCBehavior_OrientTowards(Camera.main.transform),
                new LeafWait(1500),
                NPCBehavior_LookAt(Camera.main.transform, true),
                new SequenceShuffle(
                    NPCBehavior_DoGesture(GESTURE_CODE.WAVE_HELLO),
                    NPCBehavior_DoGesture(GESTURE_CODE.GREET_AT_DISTANCE)
                )
            );
    }

    public Node NPCBehavior_WaitFadeInAndLive(long wait, float Speed, Transform[] points, NPCObject[] objects, Color c) {
        return new Sequence(
            new LeafWait(wait),
            NPCBehavior_FadeInAndLive(Speed, points, objects,c)
        );
    }

    public Node NPCBehavior_FadeInAndLive(float Speed,Transform[] points, NPCObject[] objects, Color c) {
        return new Sequence(
            new LeafInvoke(() => {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                return RunStatus.Success;
            }),
            NPCBehavior_PatrolRandomPoints(points),
            NPCBehavior_InPlaceBehaviors()
        );
    }

    public Node NPCBehavior_GoToClosestPoint(Transform[] points, bool run) {
        float f = Vector3.Distance(transform.position, points[0].position);
        Transform target = points[0];
        foreach (Transform p in points) {
            float d = Vector3.Distance(transform.position, p.position);
            if (d < f) {
                target = p;
                f = d;
            }
        }
        return NPCBehavior_GoTo(target, run);
    }

    public Node NPCBehavior_GoToAtDistance(Transform point, float distance, bool run = false) {
        return NPCBehavior_GoToAtDistance(point.position, distance, run);
    }

    public Node NPCBehavior_GoToAtDistance(Vector3 target, float distance, bool run = false) {
        return new LeafInvoke(() => Behavior_GoToDistance(target, distance, run));
    }

    public Node NPCBehavior_ApproachAndPonder(Transform r, float distance) {
        return new Sequence(
                NPCBehavior_GoToAtDistance(r,distance),
                NPCBehavior_OrientTowards(r),
                NPCBehavior_InPlaceBehaviors()
            );
    }

    public Node NPCBehavior_FadeIn(float Speed) {
        return new LeafInvoke(() => Behavior_FadeInOut(Speed, true));
    }

    public Node NPCBehavior_FadeOut(float Speed) {
        return new LeafInvoke(() => Behavior_FadeInOut(Speed, false));
    }

    public Node NPCBehavior_InPlaceBehaviors() {
        return new SequenceShuffle(
                NPCBehavior_DoTimedGesture(GESTURE_CODE.IDLE_SMALL_STEPS),
                NPCBehavior_DoTimedGesture(GESTURE_CODE.LOOK_AROUND),
                NPCBehavior_DoGesture(GESTURE_CODE.THINK),
                new Sequence(
                    NPCBehavior_DoGesture(GESTURE_CODE.TEXTING,true),
                    new LeafWait(3500),
                    NPCBehavior_DoGesture(GESTURE_CODE.TEXTING, false)
                    )
            );
    }

    public Node NPCBehavior_StandUpAndWander(Transform[] points, bool run) {
        return new DecoratorLoop (
            new Sequence(
                NPCBehavior_DoGesture(GESTURE_CODE.DESK_WRITING, false),
                NPCBehavior_DoTimedGesture(GESTURE_CODE.SITTING, false),
                NPCBehavior_Wander(points,run)
            )
        );
    }

    public Node NPCBehavior_Wander(Transform[] points, bool run) {
        return new DecoratorLoop(new LeafInvoke(() => Behavior_Wander(run, points)));
    }

    public Node NPCBehavior_EnvironmentBehaviors(Transform[] points, NPCObject[] objects) {
        return new SequenceShuffle(
                new NodeWeight(1f,
                    new LeafInvoke(() => Behavior_Wander(false))),
                new NodeWeight(1.5f, NPCBehavior_PatrolRandomPoints(points)),
                new NodeWeight(0.8f, new LeafWait(2000))
            );
    }

    public Node NPCBehavior_PatrolBetween(Transform a, Transform b)
    {
        return new DecoratorLoop(
                new Sequence(
                    NPCBehavior_GoTo(a, false),
                    NPCBehavior_GoTo(b, false)
                )
            );
    }

    public Node NPCBehavior_OrientTowards(Transform t) {
        return new LeafInvoke(() => Behavior_OrientTowards(t));
    }

    public Node NPCBehavior_OrientTowards(Vector3 t) {
        return new LeafInvoke(() => Behavior_OrientTowards(t));
    }

    public Node NPCBehavior_LookAt(Transform t, bool start) {
        if(start)
            return new LeafInvoke(() => Behavior_LookAt(t));
        else
            return new LeafInvoke(() => Behavior_StopLookAt());
    }

    public Node NPCBehavior_GoTo(Transform t, bool run = false) {
        return NPCBehavior_GoTo(t.position,run);
    }

    public Node NPCBehavior_GoTo(Vector3 t, bool run = false) {
        return new Sequence(
            NPCBehavior_DoGesture(GESTURE_CODE.SITTING, false),
            new LeafInvoke(() => Behavior_GoTo(t, run)
            )
        );
    }

    public Node NPCBehavior_DoTimedGesture(GESTURE_CODE gesture, System.Object o = null) {
        return NPCBehavior_DoGesture(gesture, o, true);
    }

    public Node NPCBehavior_DoGesture(GESTURE_CODE gesture, System.Object o = null, bool timed = false) {
        return new LeafInvoke(
            () => Behavior_DoGesture(gesture,o, timed)
        );
    }

    public Node NPCBehavior_ChangeColor(float speed, Color toColor) {
        return new LeafInvoke(() => Behavior_ChangeColor(speed, toColor));
    }

    public Node NPCBehavior_WaitInfectAndEscape(long wait, float speed, Color toColor, Transform[] evacPoints) {
        return new Sequence(
            NPCBehavior_InPlaceBehaviors(),
            new LeafWait(wait),
            new LeafInvoke(() => {
                g_NPCController.Body.StopNavigation();
                g_NPCController.SetNPCTag("infected");
                return RunStatus.Success;
            }),
            NPCBehavior_ChangeColor(speed,toColor),
            NPCBehavior_DoGesture(GESTURE_CODE.DESPERATION),
            new SequenceParallel(
                new SequenceParallel(
                    NPCBehavior_GoToClosestPoint(evacPoints, true),
                    new Sequence(
                        new LeafWait(3000),
                        NPCBehavior_ChangeColor(1.5f, Color.black)
                    )
                )
            ),
            new LeafInvoke(() => {
                g_NPCController.gameObject.SetActive(false);
                return RunStatus.Success;
            })
        );
    }

    public Node NPCBehavior_ApproximateAreas(Transform[] points, bool run = false) {

        Transform t =
                    points != null ? points[(int)(UnityEngine.Random.value * (points.Length - 1))]
                    : g_RoamingPoints[(int)(UnityEngine.Random.value * (g_RoamingPoints.Length - 1))];

        Vector3 targetPosition = t.position;
        float rand = UnityEngine.Random.value;

        if (rand < 0.33f) {
            targetPosition -= transform.right * g_NPCController.Body.NavDistanceThreshold;
        } else if (rand < 0.66f) {
            targetPosition -= transform.forward * g_NPCController.Body.NavDistanceThreshold;
        } else {
            targetPosition += transform.right * g_NPCController.Body.NavDistanceThreshold;
        }

        return NPCBehavior_GoTo(targetPosition, run);
    }

    public Node NPCBehavior_ApproximateArea(Transform target, bool run = false) {

        Vector3 targetPosition = target.position;
        float rand = UnityEngine.Random.value;

        if (rand < 0.33f) {
            targetPosition -= transform.right * g_NPCController.Body.NavDistanceThreshold;
        } else if (rand < 0.66f) {
            targetPosition -= transform.forward * g_NPCController.Body.NavDistanceThreshold;
        } else {
            targetPosition += transform.right * g_NPCController.Body.NavDistanceThreshold;
        }

        return NPCBehavior_GoTo(targetPosition, run);
    }

    public Node NPCBehavior_PerformForAudience(Transform stage, Transform audience) {
        return new Sequence(
            new SequenceParallel(
                NPCBehavior_ChangeColor(0.5f, new Color(1f, 0.41f, 0.75f, 1f)),
                NPCBehavior_DoTimedGesture(GESTURE_CODE.WAVE_HELLO),
                NPCBehavior_DoTimedGestureAtPoint(stage,
                        GESTURE_CODE.SING,
                        audience.position)
            ),
            new Sequence(
                NPCBehavior_DoGesture(GESTURE_CODE.SING, false),
                new LeafInvoke(() => {
                    g_NPCController.Body.Move(LOCO_STATE.FORWARD);
                    return RunStatus.Success;
                }),
                NPCBehavior_DoGesture(GESTURE_CODE.GREET_AT_DISTANCE)
            )
        );
    }

    public Node NPCBehavior_ApproachAndCheerMusic(Transform location, NPCBehavior performer) {
        return new Sequence(
            NPCBehavior_ApproximateArea(location),
            NPCBehavior_OrientTowards(performer.transform.position),
            NPCBehavior_LookAt(performer.transform, true),
            new DecoratorLoop(
                new Sequence(
                    new LeafAssert(() => {
                        return performer.InEvent;
                    }),
                    new SequenceShuffle(
                            new NodeWeight(0.4f, new Sequence(
                                    NPCBehavior_DoGesture(GESTURE_CODE.CLAP, true),
                                    new LeafWait(5000),
                                    NPCBehavior_DoGesture(GESTURE_CODE.CLAP, false)
                                )
                            ),
                            new NodeWeight(0.3f, new Sequence(
                                    NPCBehavior_DoGesture(GESTURE_CODE.MUSIC_HEADBANG, true),
                                    new LeafWait(5000),
                                    NPCBehavior_DoGesture(GESTURE_CODE.MUSIC_HEADBANG, false)
                                )
                            ),
                            new NodeWeight(0.8f, NPCBehavior_DoGesture(GESTURE_CODE.HURRAY)),
                            new NodeWeight(0.8f, NPCBehavior_DoGesture(GESTURE_CODE.GREET_AT_DISTANCE))
                        )
                    )
            ),
            NPCBehavior_LookAt(performer.transform, false)
        );
    }

    public Node NPCBehavior_DoTimedGestureAtPoint(Transform location, GESTURE_CODE gesture, Vector3 orientation) {
        return new Sequence(
                NPCBehavior_GoTo(location, false),
                NPCBehavior_OrientTowards(orientation),
                NPCBehavior_DoGesture(gesture)
            );
    }

    public override string ToString() {
        return g_NPCController.name;
    }

    #endregion

    #region Private_Functions

    private RunStatus Behavior_GrabRightHand(NPCObject t, bool grab = true) {
        g_NPCController.Body.GrabRightHand(t, grab);
        return RunStatus.Success;
    }

    private RunStatus Behavior_FadeInOut(float Speed, bool fadeIn) {
        float curVal = g_NPCController.MainMaterial.GetFloat("_Outline");
        bool trigger = fadeIn ? g_NPCController.MainMaterial.GetFloat("_Outline") > 0.09f :
            g_NPCController.MainMaterial.GetFloat("_Outline") < 0.005f;
        if (trigger) {
            foreach (Renderer r in g_NPCController.AllRenderers) {
                r.sharedMaterial.SetFloat("_Outline", fadeIn ? 0.1f : 0f);
            }
            return RunStatus.Success;
        }
        foreach(Renderer r in g_NPCController.AllRenderers) {
            r.sharedMaterial.SetFloat("_Outline", Mathf.Lerp(curVal, fadeIn ? 0.1f : 0, Time.deltaTime * Speed));
        }
        return RunStatus.Running;
    }

    private RunStatus Behavior_ChangeColor(float Speed, Color toColor) {
        Vector3 curColor = g_NPCController.MainMaterial.GetVector("_OutlineColor"),
            toColorV = new Vector3(toColor.r, toColor.g, toColor.b);
        if ((Vector3) g_NPCController.MainMaterial.GetVector("_OutlineColor") == toColorV) {
            foreach (Renderer r in g_NPCController.AllRenderers) {
                r.sharedMaterial.SetVector("_OutlineColor", toColor);
            }
            return RunStatus.Success;
        }
        foreach (Renderer r in g_NPCController.AllRenderers) {
            r.sharedMaterial.SetVector("_OutlineColor",
                Vector3.Lerp(curColor, toColorV, Time.deltaTime * Speed));
        }
        return RunStatus.Running;
    }

    private RunStatus Behavior_OrientTowards(Vector3 t) {
        Vector3 target = t - transform.position;
        if (Vector3.Distance(g_NPCController.Body.TargetOrientation, target) >= 0.01f) {
            g_NPCController.Body.OrientTowards(target);
            return RunStatus.Running;
        }
        return g_NPCController.Body.Oriented ? RunStatus.Success : RunStatus.Running;
    }

    private RunStatus Behavior_OrientTowards(Transform t) {
        return Behavior_OrientTowards(t.position);
    }

    private RunStatus Behavior_DoGesture(GESTURE_CODE gest, System.Object o = null, bool timed = false) {
        if (g_NPCController.Body.IsTimedGesturePlaying(gest)) {
            return RunStatus.Running;
        } else if (g_GestureRunning) {
            g_GestureRunning = false;
            return RunStatus.Success;
        }  else {
            try {
                g_NPCController.Body.DoGesture(gest, o, timed);
                g_GestureRunning = true;
                return RunStatus.Running;
            } catch (System.Exception e ) {
                g_NPCController.Debug("Could not initialize gesture with error: " + e.Message);
                return RunStatus.Failure;
            }
        }
        
    }

    private RunStatus Behavior_Wander(bool run) {
        return Behavior_Wander(run, null);
    }

    private RunStatus Behavior_Wander(bool run, Transform[] points) {
        if (!g_NPCController.Body.Navigating) {
            try {
                Transform t =
                    points != null ? points[(int)(UnityEngine.Random.value * (points.Length - 1))]
                    : g_RoamingPoints[(int)(UnityEngine.Random.value * (g_RoamingPoints.Length - 1))];

                Vector3 targetPosition = t.position;
                float rand = UnityEngine.Random.value;

                if (rand < 0.33f) {
                    targetPosition -= transform.right * g_NPCController.Body.NavDistanceThreshold;
                } else if (rand < 0.66f) {
                    targetPosition -= transform.forward * g_NPCController.Body.NavDistanceThreshold;
                } else {
                    targetPosition += transform.right * g_NPCController.Body.NavDistanceThreshold;
                }

                if (rand < .65f && Mathf.Abs(targetPosition.y - transform.position.y) > 1.5f)
                    targetPosition = transform.position;

                if (run)
                    g_NPCController.Body.RunTo(targetPosition);
                else g_NPCController.Body.GoTo(targetPosition);
                return RunStatus.Running;
            } catch (System.Exception e) {
                g_NPCController.Debug(e.Message);
                return RunStatus.Failure;
            }
        } else { return RunStatus.Success; }
    }

    private RunStatus Behavior_GoTo(Transform t, bool run) {
        return Behavior_GoTo(t.position, run);
    }

    private RunStatus Behavior_GoToDistance(Vector3 t, float distance, bool run = false) {
        float currentDistance = Vector3.Distance(transform.position, t);
        if (currentDistance <= distance || g_NPCController.Body.IsAtTargetLocation(t)) {
            g_NPCController.Body.StopNavigation();
            return RunStatus.Success;
        } else {
            try {
                if (run)
                    g_NPCController.Body.RunTo(t);
                else g_NPCController.Body.GoTo(t);
                return RunStatus.Running;
            } catch (System.Exception e) {
                // this will occur if the target is unreacheable
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_GoTo(Vector3 t, bool run) {
        if (g_NPCController.Body.IsAtTargetLocation(t)) {
            g_NPCController.Debug("Finished go to");
            return RunStatus.Success;
        }
        else {
            try {
                if (run)
                    g_NPCController.Body.RunTo(t);
                else g_NPCController.Body.GoTo(t);
                return RunStatus.Running;
            } catch(System.Exception e) {
                // this will occur if the target is unreacheable
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_Follow(Transform target, bool run) {
        if(g_NPCController.Following) {
            return RunStatus.Running;
        } else {
            try {
                g_NPCController.Body.Follow(target, run);
                return RunStatus.Running;
            } catch (System.Exception e) {
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_StopLookAt() {
        g_NPCController.Body.StopLookAt();
        return RunStatus.Success;
    }

    private RunStatus Behavior_LookAt(Transform t) {
        g_NPCController.Body.StartLookAt(t);
        return RunStatus.Success;
    }

    public Node NPCBehavior_Dummy()
    {
        return new LeafInvoke(() => { return Behavior_Dummy(); });
    }

    private RunStatus Behavior_Dummy()
    {
        return RunStatus.Success;
    }

    #endregion

    #region INPCModule

    public void InitializeModule() {
        g_NPCController = GetComponent<NPCController>();
        g_BehaviorObject = new BehaviorAgent(
                                new DecoratorLoop(
                                    new LeafAssert(() => true)));
        BehaviorManager.Instance.Register((IBehaviorUpdate) g_BehaviorObject);
        g_NPCController.Debug("NPCBehavior - Initialized: " + name);
        g_Initialized = true;
    }

    public bool IsEnabled() {
        return Enabled;
    }

    public string NPCModuleName() {
        return "NPC TreeSP/Connector";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.AI;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.BEHAVIOR;
    }

    public void RemoveNPCModule() {
        throw new NotImplementedException();
    }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    public bool IsUpdateable() {
        return false;
    }

    public void TickModule() {
        throw new NotImplementedException();
    }
    
    public void CleanupModule() {

    }

    #endregion
}
