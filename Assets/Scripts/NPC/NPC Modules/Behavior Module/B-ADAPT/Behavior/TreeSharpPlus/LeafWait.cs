using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Waits for a given period of time, set by the wait parameter
    /// </summary>
    public class LeafWait : Node
    {
        protected Stopwatch stopwatch;
        protected long waitMax;

        public long GetWait() {
            return waitMax;
        }

        /// <summary>
        ///    Initializes with the wait period
        /// </summary>
        /// <param name="waitMax">The time (in milliseconds) for which to 
        /// wait</param>
        public LeafWait(Val<long> waitMax)
        {
            this.waitMax = waitMax.Value;
            this.stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Dynamically reduces the wait time if needed
        /// </summary>
        /// <param name="trim"></param>
        public void TrimWait(long trim) {
            waitMax -= trim;
        }

        /// <summary>
        ///    Resets the wait timer
        /// </summary>
        /// <param name="context"></param>
        public override void Start()
        {
            base.Start();
            this.stopwatch.Reset();
            this.stopwatch.Start();
        }

        public override void Stop()
        {
            base.Stop();
            this.stopwatch.Stop();
        }

        public override sealed IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
                // Count down the wait timer
                // If we've waited long enough, succeed
                if (this.stopwatch.ElapsedMilliseconds >= this.waitMax)
                {
                    yield return RunStatus.Success;
                    yield break;
                }
                // Otherwise, we're still waiting
                yield return RunStatus.Running;
            }
        }
    }
}