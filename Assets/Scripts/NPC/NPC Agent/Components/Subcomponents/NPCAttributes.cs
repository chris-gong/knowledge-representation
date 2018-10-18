using System;
using UnityEngine;
using System.Reflection;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

/// <summary>
/// An  attribute could be anything which might describe certain NPCEntity's state.
/// Location, Trait, State (opne, locked ... for specific System.Objects)
/// Because Attributes can be represented as Properties or Methods, we
/// use NPCAttribute to abstract their name, types and getters.
/// 
/// NPCAI will implement all the available attributes for each entity
/// 
/// </summary>

namespace NPC {

    [System.Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class NPCAttribute : System.Attribute, IComparable {
    
        public Type Type { get; set; }

        public string Name { get; set; }

        public NPCAttribute(string pName, Type pType) {
            this.Name = pName;
            this.Type = pType;
        }

        /// <summary>
        /// Implement IComparable interface
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int CompareTo(object o) {
            return 0;
        }
    }

}
