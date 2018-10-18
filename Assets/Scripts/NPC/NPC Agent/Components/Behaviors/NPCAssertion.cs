using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    /// <summary>
    /// Represents an expression which can be evaluated.
    /// This evaluation, returns true or false. If true, the
    /// resulting value, if required, can be obtained from the assertion instance.
    /// </summary>
    [Serializable]
    public class NPCAssertion {

        private PropertyInfo g_PropertyInfo;

        [SerializeField, HideInInspector]
        private string ReflectedType;

        /// <summary>
        /// Serializable name of the property to be evaluated
        /// </summary>
        public string PropertyName;

        /// <summary>
        /// Negates the result of the evaluation
        /// </summary>
        public bool Negate;

        /// <summary>
        /// This target could be Self, a Perceived Agent, an Object or both perceived agents and objects (entities)
        /// </summary>
        public TARGET Target;
        /// <summary>
        /// What to assert on; a Property, a Tag or a condition on the transform
        /// </summary>
        public ASSERT Assert;
        /// <summary>
        /// Assertion criteria for a property of numerical or boolean value
        /// </summary>
        public OPERATION EqualityOperation;
        /// <summary>
        /// Assertion criteria for a transform
        /// </summary>
        public TRANSFORM_ASSERT TransformAssert;
        /// <summary>
        /// The Agent on which the assertion will be performed
        /// </summary>
        public NPCController Agent;
        /// <summary>
        /// Information of the optional property to be evaluated
        /// </summary>
        public PropertyInfo Property {
            get {
                return g_PropertyInfo;
            }
            set {
                if (value != null) {
                    ReflectedType = value.ReflectedType.Name;
                    PropertyName = value.Name;
                    g_PropertyInfo = value;
                }
            }
        }
        /// <summary>
        /// Evaluation criteria
        /// </summary>
        [SerializeField]
        public Parameter TargetValue;

        /// <summary>
        /// When matched, the Result will be populated with the Agent or Entity which this assertion found
        /// </summary>
		public GameObject Result;

        // Is the result to be set on blackboard
        public bool SetResultInBlackboard;
        // Blackboard name
        public string BlackboardValue;

        private NPCAssertion(NPCController Agent, TARGET Target, ASSERT Assert) : this() {
            this.Agent = Agent;
            this.Target = Target;
            this.Assert = Assert;
        }

        /// <summary>
        /// Creates an empty assertion
        /// </summary>
        public NPCAssertion() {
            TargetValue = new Parameter();
        }

        /// <summary>
        /// Creates an empty assertion instance for a specific agent
        /// </summary>
        /// <param name="Agent"></param>
        public NPCAssertion(NPCController Agent) : this() {
            this.Agent = Agent;
        }

        /// <summary>
        /// Assert for a specific Property on either self or perceived agents.
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="Assert"></param>
        /// <param name="Property"></param>
        /// <param name="Equality"></param>
        /// <param name="TargetValue"></param>
        public NPCAssertion(NPCController Agent, TARGET Target, ASSERT Assert, PropertyInfo Property, OPERATION Equality, Parameter TargetValue) : this(Agent, Target, Assert) {
            EqualityOperation = Equality;
            this.Property = Property;
            this.TargetValue = TargetValue;
        }

        /// <summary>
        /// Assert for a specific tag on either self or perceived agents.
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="Assert"></param>
        /// <param name="Tag"></param>
        public NPCAssertion(NPCController Agent, TARGET Target, ASSERT Assert, string Tag) : this(Agent, Target, Assert) {
            TargetValue.SetValue(Tag);
        }

        /// <summary>
        /// Assert for a particular state on the transform of either self of perceived agents.
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="Assert"></param>
        /// <param name="Transform"></param>
        /// <param name="TargetValue"></param>
        public NPCAssertion(NPCController Agent, TARGET Target, ASSERT Assert, TRANSFORM_ASSERT Transform, object TargetValue) : this(Agent, Target, Assert) {
            TransformAssert = Transform;
            this.TargetValue.SetValue(TargetValue);
        }

        /// <summary>
        /// Evaluates the instance assertion.
        /// </summary>
        /// <returns>True if succeeded, False otherwise</returns>
        public bool Evaluate() {
            bool result = false;
            // Populate evaluation targets
            List<object> targets = new List<object>();
            switch (Target) {
                case TARGET.SELF:
                    targets.Add(Agent);
                    break;
                case TARGET.AGENT:
                    foreach (NPCController agent in Agent.PerceivedAgents) {
                        targets.Add(agent);
                    }
                    break;
                case TARGET.OBJECT:
                    foreach (INPCPerceivable obj in Agent.PerceivedEntities) {
                        if (obj is NPCObject)
                            targets.Add(obj);
                    }
                    break;
                case TARGET.ENTITY:
                    foreach (INPCPerceivable obj in Agent.PerceivedEntities) {
                        targets.Add(obj);
                    }
                    break;
            }
            // Perform Evaluation
            foreach (object o in targets) {
                switch (Assert) {
                    case ASSERT.TAG:
                        try {
                            if (o.GetType().GetMethod("HasTag") != null) {
                                INPCPerceivable per = ((INPCPerceivable)o);
                                bool res = per.HasTag(TargetValue.GetValue().ToString());
                                if (res) {
                                    Result = per.GetGameObject();
                                    result = true;
                                }
                            }
                        } catch (Exception e) { Debug.LogError(this + " - Error evaluating Tag: " + e.Message); }
                        break;
                    case ASSERT.PROPERTY:
                        try {
                            if (o.GetType().GetProperty(Property.Name) != null) {
                                if (Property.PropertyType == typeof(int) || Property.PropertyType == typeof(float) || Property.PropertyType == typeof(long)) {
                                    bool matched = false;
                                    var val = Convert.ToSingle(Property.GetValue(o, null));
                                    var val2 = Convert.ToSingle(TargetValue.GetValue());
                                    switch (EqualityOperation) {
                                        case OPERATION.EQUALS:
                                            matched = val == val2;
                                            break;
                                        case OPERATION.GREATER:
                                            matched = val > val2;
                                            break;
                                        case OPERATION.LESS:
                                            matched = val < val2;
                                            break;
                                    }
                                    if (matched) {
                                        Result = ((INPCPerceivable) o).GetGameObject();
                                        result = true;
                                    }
                                } else if (Property.PropertyType == typeof(int)) {
                                    var val = Convert.ToBoolean(Property.GetValue(o, null));
                                    var val2 = Convert.ToBoolean(TargetValue.GetValue());
                                    if (val.Equals(val2)) {
                                        Result = ((INPCPerceivable)o).GetGameObject();
                                        result = true;
                                    }
                                }
                            }
                        } catch (Exception e) { Debug.LogError(this + " - Error evaluating Tag: " + e.Message); }
                        break;
                    case ASSERT.TRANSFORM:
                        INPCPerceivable p = o as INPCPerceivable;
                        if (p != null) {
                            Transform t = p.GetTransform();
                            float val = 0;
                            float targetVal = Convert.ToSingle(TargetValue.GetValue());
                            bool matched = false;
                            switch (TransformAssert) {
                                case TRANSFORM_ASSERT.DISTANCE:
                                    val = Vector3.Distance(Agent.transform.position, p.GetTransform().position);
                                    break;
                                case TRANSFORM_ASSERT.ORIENTATION:
                                    val = Vector3.Angle(Agent.transform.forward, p.GetTransform().forward);
                                    break;
                            }
                            switch (EqualityOperation) {
                                case OPERATION.EQUALS:
                                    matched = val == targetVal;
                                    break;
                                case OPERATION.GREATER:
                                    matched = val > targetVal;
                                    break;
                                case OPERATION.LESS:
                                    matched = val < targetVal;
                                    break;
                            }
                            if (matched) {
                                Result = ((INPCPerceivable)o).GetGameObject();
                                result = true;
                            }
                        }
                        break;
                }
            }
            return Negate ? !result : result;
        }

        #region Enums
        /// <summary>
        /// Target agent, self, object or entity (both object or agents)
        /// </summary>
        [Serializable]
        public enum TARGET {
            SELF,
            AGENT,
            OBJECT,
            ENTITY
        }

        /// <summary>
        /// What to assert on
        /// </summary>
        [Serializable]
        public enum ASSERT {
            PROPERTY,
            TAG,
            TRANSFORM
        }

        /// <summary>
        /// Transform's property to evaluate
        /// </summary>
        [Serializable]
        public enum TRANSFORM_ASSERT {
            DISTANCE,
            ORIENTATION
        }

        /// <summary>
        /// Operation to perform
        /// </summary>
        [Serializable]
        public enum OPERATION {
            EQUALS,
            GREATER,
            LESS
        }

        #endregion

    }
}