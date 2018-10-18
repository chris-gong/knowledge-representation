using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [Serializable]
    public class NodeParam {
        
        public string Name;

        [SerializeField, HideInInspector]
        private Parameter.PARAM_TYPE g_AssignedType;

        [SerializeField]
        private NPCAffordance Affordance;

        [SerializeField]
        private NPCAssertion Assertion;

        [SerializeField]
        private string ParameterType;

        [SerializeField]
        private bool BoolValue;

        [SerializeField]
        private string StringValue;

        [SerializeField]
        private float NumericValue;

        [SerializeField]
        private string ObjectUniqueName;

        public NodeParam(object val = null) {
            if (val != null)
                SetValue(val);
        }

        public void SetValue(object value, Type type = null) {
            if (value != null) {
                ParameterType = type == null ? value.GetType().FullName : type.FullName;
                if (ParameterType == typeof(string).FullName) {
                    StringValue = value.ToString();
                } else if (value is bool || value is Boolean) {
                    BoolValue = Convert.ToBoolean(value);
                } else if (IsNumeric(ParameterType)) {
                    NumericValue = Convert.ToSingle(value);
                } else if (value is UnityEngine.Object) {
                    ObjectUniqueName = ((UnityEngine.Object)value).name;
                } else if (value is NPCAffordance) {
                    Affordance = (NPCAffordance)value;
                } else if (value is NPCAssertion) {
                    Assertion = (NPCAssertion)value;
                }

            }
        }
        
        public object GetValue() {
            object val = null;
            if (ParameterType == typeof(string).FullName) {
                val = StringValue;
            } else if (ParameterType == typeof(bool).FullName || ParameterType == typeof(Boolean).FullName) {
                val = BoolValue;
            } else if (IsNumeric(ParameterType)) {
                val = NumericValue;
            } else if (ParameterType == typeof(UnityEngine.Object).FullName) {
                val = GameObject.Find(ObjectUniqueName);
            } else if (ParameterType == typeof(Transform).FullName) {
                val = GameObject.Find(ObjectUniqueName).transform;
            } else if (ParameterType == typeof(NPCAffordance).FullName) {
                val = Affordance;
            } else if (ParameterType == typeof(NPCAssertion).FullName) {
                val = Assertion;
            }
            return val;
        }
        
        #region Properties

        public Parameter.PARAM_TYPE AssignedType {
            get {
                return g_AssignedType;
            }
            set {
                g_AssignedType = value;
            }
        }

        #endregion
        
        private bool IsNumeric(string ParameterType) {
            return
                ParameterType != null && 
                (ParameterType == typeof(long).FullName ||
                ParameterType == typeof(int).FullName ||
                ParameterType == typeof(float).FullName);
        }

    }

}
