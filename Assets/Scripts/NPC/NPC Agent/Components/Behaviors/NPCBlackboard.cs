
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [Serializable]
    public class NPCBlackboard : ScriptableObject {

        public NPCNode Root;
        public List<Parameter> Parameters;
        
        public void Init(List<Parameter> Parameters) {
            this.Parameters = new List<Parameter>();
            foreach(Parameter p in Parameters) {
                this.Parameters.Add(p);
            }
        }

        public bool AddValue(string Name, object Value) {
            Parameter p = Parameters.SingleOrDefault(curr => curr.ParameterName == Name);
            if(p != null) {
                if (p.AssignedType != Parameter.PARAM_TYPE.UNASSIGNED) {
                    try {
                        switch (p.AssignedType) {
                            case Parameter.PARAM_TYPE.TRANSFORM:
                                Value = ((GameObject)Value).transform;
                                break;
                            case Parameter.PARAM_TYPE.GAMEOBJECT:
                                Value = (GameObject)Value;
                                break;
                            case Parameter.PARAM_TYPE.AGENT:
                                Value = ((GameObject)Value).GetComponent<NPCController>();
                                break;
                            case Parameter.PARAM_TYPE.BOOL:
                                Value = Convert.ToBoolean(Value);
                                break;
                            case Parameter.PARAM_TYPE.NUMERICAL:
                                Value = Convert.ToSingle(Value);
                                break;
                            case Parameter.PARAM_TYPE.STRING:
                                Value = Value.ToString();
                                break;
                        }
                    } catch(Exception e) {
                        Debug.LogError("Invalid Blackboard parameter, declared type is: " + p.AssignedType + " but received: " + Value);
                    }
                }
                p.SetValue(Value);
                return true;
            }
            return false;
        }

        public object GetValue(string Name) {
            Parameter p = Parameters.SingleOrDefault(curr => curr.ParameterName == Name);
            object val = null;
            if (p != null)
                val = p.GetValue();
            return val;
        }

        public bool HasParameter(string Name) {
            Parameter p = Parameters.SingleOrDefault(curr => curr.ParameterName == Name);
            return p != null;
        }
    }
}
