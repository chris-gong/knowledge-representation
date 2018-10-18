using System.Diagnostics;

///
/// Created by Fernando Geraci on 2018
/// Copyright (c) 2018. All rights reserved.
/// 

namespace NPC {
    /// <summary>
    /// Decided to implement the timer on the same thread to sync up with
    /// actual animations and IK frames. Otherwise it will not truly represent
    /// the delta time between frames.
    /// </summary>
    public class NPCTimer {

        // default one second
        public float Duration;

        private Stopwatch g_Stopwatch;
	
        public NPCTimer(bool launch) {
            Finished = true;
            g_Stopwatch = new Stopwatch();
            g_Stopwatch.Reset();
            if (launch) g_Stopwatch.Start();
        }

        public NPCTimer() : this(true) { }

        public bool Finished;

        public void UpdateTimer() {
            if (!Finished) {
                if (g_Stopwatch.ElapsedMilliseconds >= Duration) {
                    Finished = true;
                    StopTimer();
                }
            }
        }

        public void StopTimer() {
            Finished = true;
            g_Stopwatch.Reset();
            g_Stopwatch.Stop();
        }

        // Default to one second if no parameter specified.
        // Calling start while running restarts the timer
        public void StartTimer(float dur = 1000) {
            Duration = dur;
            Finished = false;
            g_Stopwatch.Reset();
            g_Stopwatch.Start();
        }
    }
}