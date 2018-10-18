using UnityEngine;
using System;
using System.Collections.Generic;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {

    public static class NPCUtils {

        public static Vector3 CalculateObjectNormal(INPCPerceivable object1, INPCPerceivable object2) {
            Vector3 basicNormal = object2.GetPosition() - object1.GetPosition();
            if(Mathf.Abs(basicNormal.x) > Mathf.Abs(basicNormal.z)){
                // west or east
                return object2.GetPosition().x - object1.GetPosition().x <= 0 ?
                    new Vector3(1f, 0f, 0f) : new Vector3(-1f, 0f, 0f);
            } else {
                // north or south
                return object2.GetPosition().z - object1.GetPosition().z <= 0 ?
                    new Vector3(1f, 0f, -1f) : new Vector3(0f, 0f, 1f);
            }
        }

        /// <summary>
        /// Comparer for comparing two keys, handling equality as beeing greater
        /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class DuplicateKeyComparer<TKey>
                    :
                 IComparer<TKey> where TKey : IComparable {

            #region IComparer<TKey> Members
            public int Compare(TKey x, TKey y) {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }
            #endregion
        }

        /// <summary>
        /// Calculates whether an object is at the right or left of the transform
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="transform"></param>
        /// <returns>-1 if left, 1 if right, 0 otherwise.</returns>
        public static float Direction(Vector3 direction, Transform transform) {
            Vector3 perp = Vector3.Cross(transform.forward, direction);
            float dir = Vector3.Dot(perp, transform.up);
            return dir > 0f ? 1.0f : (dir < 0 ? -1.0f : 0f);
        }

        public static long TimeMillis() {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

    } 

}