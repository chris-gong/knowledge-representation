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

namespace TreeSharpPlus
{
    /// <summary>
    /// "Invert" Decorators execute their child as normal, but flip the 
    /// termination status when that child finishes. "Success" becomes 
    /// "Failure", and vice versa. Useful for assertion actions.
    /// </summary>
    public class DecoratorInvert : Decorator
    {
        public DecoratorInvert(Node child)
            : base(child)
        {
        }

        public override IEnumerable<RunStatus> Execute()
        {
            DecoratedChild.Start();

            // While the child subtree is running, report that as our status
            RunStatus result;
            while ((result = this.TickNode(this.DecoratedChild)) == RunStatus.Running)
                yield return RunStatus.Running;

            DecoratedChild.Stop();

            // Return the opposite result that we received
            if (result == RunStatus.Failure)
            {
                yield return RunStatus.Success;
                yield break;
            }

            yield return RunStatus.Failure;
            yield break;
        }
    }
}