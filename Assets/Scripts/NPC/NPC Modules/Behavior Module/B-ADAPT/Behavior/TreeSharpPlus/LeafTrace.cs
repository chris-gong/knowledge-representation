using System.Collections.Generic;
using UnityEngine;
using System;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Waits for a given period of time, set by the wait parameter
    /// </summary>
    public class LeafTrace : Node
    {
        protected string text;

        /// <summary>
        ///    Initializes with the wait period
        /// </summary>
        /// <param name="waitMax">The time (in seconds) for which to wait</param>
        public LeafTrace(string text)
        {
            this.text = text;
        }

        public override sealed IEnumerable<RunStatus> Execute()
        {
            Debug.Log(this.text);
            yield return RunStatus.Success;
            yield break;
        }
    }
}