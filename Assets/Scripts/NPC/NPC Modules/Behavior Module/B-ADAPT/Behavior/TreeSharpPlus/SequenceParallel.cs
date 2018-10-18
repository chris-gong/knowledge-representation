#region License

// A simplistic Behavior Tree implementation in C#
// Copyright (C) 2010-2011 ApocDev apocdev@gmail.com
// 
// This file is part of TreeSharp
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

// TODO: THIS WAS A NEW FILE -- MODIFY THIS HEADER
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Parallel Sequence nodes execute all of their children in parallel. If any
    ///    sequence reports failure, we finish all of the other ticks, but then stop
    ///    all other children and report failure. We report success when all children
    ///    report success.
    /// </summary>
    public class SequenceParallel : Parallel
    {
        public SequenceParallel(params Node[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
				for (int i = 0; i < this.Children.Count; i++)
                {
                    if (this.childStatus[i] == RunStatus.Running)
                    {
						Node node = this.Children[i];
                        RunStatus tickResult = this.TickNode(node);

                        // Check to see if anything finished
                        if (tickResult != RunStatus.Running)
                        {
                            // Clean up the node
                            node.Stop();
                            this.childStatus[i] = tickResult;
                            this.runningChildren--;

                            // If the node failed
                            if (tickResult == RunStatus.Failure)
                            {
                                // We may be stopping nodes in progress, so it's best
                                // to do a clean terminate and give them time to end
                                while (this.TerminateChildren() == RunStatus.Running)
                                    yield return RunStatus.Running;
                                // TODO: Timeout? - AS
                                // TODO: What if Terminate() fails? - AS

                                // Report failure
                                this.runningChildren = 0;
                                yield return RunStatus.Failure;
                                yield break;
                            }
                        }
                    }
                }

                // If we're out of running nodes, we're done
                if (this.runningChildren == 0)
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // For forked ticking
                yield return RunStatus.Running;
            }
        }
    }
}