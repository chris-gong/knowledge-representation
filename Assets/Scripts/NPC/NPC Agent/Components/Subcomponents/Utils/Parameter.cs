using System;
using UnityEngine;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    [Serializable]
    public class Parameter {

        /// <summary>
        /// Use when referencing a parameter in a Blackboard
        /// </summary>
        [SerializeField]
        public string ParameterName;

        [SerializeField]
        public PARAM_TYPE AssignedType = PARAM_TYPE.UNASSIGNED;

        [SerializeField, HideInInspector]
        private string ParameterType;

        [SerializeField]
        private bool BoolValue;

        [SerializeField]
        private string StringValue;

        [SerializeField]
        private float NumericValue;

        [SerializeField]
        private int EnumValue;

        [SerializeField]
        private string ObjectUniqueName;

        public Parameter(object val = null) {
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
				} else if (value is GameObject) {
                    ObjectUniqueName = ((GameObject) value).name;
                } else if (value.GetType().IsEnum) {
                    EnumValue = (int)value;
                }
            }
        }

        public object GetValue() {
            object val = null;
            if (!string.IsNullOrEmpty(ParameterType)) {
                Type declaredType = Type.GetType(ParameterType);
                if (ParameterType == typeof(string).FullName) {
                    val = StringValue;
                } else if (ParameterType == typeof(bool).FullName || ParameterType == typeof(Boolean).FullName) {
                    val = BoolValue;
                } else if (IsNumeric(ParameterType)) {
                    val = NumericValue;
                } else if (ParameterType == typeof(UnityEngine.Object).FullName || ParameterType == typeof(UnityEngine.GameObject).FullName) {
                    val = GameObject.Find(ObjectUniqueName);
                } else if (ParameterType == typeof(Transform).FullName) {
                    val = GameObject.Find(ObjectUniqueName).transform;
                } else if (declaredType != null && declaredType.IsEnum) {
                    foreach (var v in Enum.GetValues(declaredType)) {
                        if ((int)v == EnumValue)
                            val = (Enum)v;
                    }
                }
            }
            return val;
        }

        private bool IsNumeric(string ParameterType) {
            return 
                ParameterType == typeof(long).FullName || 
                ParameterType == typeof(int).FullName || 
                ParameterType == typeof(Single).FullName;
        }

        public enum PARAM_TYPE {
            UNASSIGNED,
            BOOL,
            NUMERICAL,
            TRANSFORM,
            GAMEOBJECT,
            AGENT,
            STRING
        }

    }

}