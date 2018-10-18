using System;
using System.Collections.Generic;

namespace TreeSharpPlus
{
    /// <summary>
    /// When terminated, a Catch decorator will terminate its child as normal,
    /// but can then run an additional given function to clean up after the
    /// child node terminates.
    /// </summary>
    public class DecoratorCatch : Decorator
    {
        private readonly Action term_noReturn = null;
        private readonly Func<RunStatus> term_return = null;

        private DecoratorCatch(Node child)
            : base(child)
        {
            this.term_noReturn = null;
            this.term_return = null;
        }

        public DecoratorCatch(
            Func<RunStatus> function,
            Node child)
            : this(child)
        {
            this.term_return = function;
        }

        public DecoratorCatch(
            Action function,
            Node child)
            : this(child)
        {
            this.term_noReturn = function;
        }

        private RunStatus InvokeFunction()
        {
            if (this.term_return != null)
                return this.term_return.Invoke();
            else if (this.term_noReturn != null)
                this.term_noReturn.Invoke();
            return RunStatus.Success;
        }

        public override RunStatus Terminate()
        {
            // See if we've already finished terminating completely
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;

            // See if the child is still terminating
            RunStatus childTerm = this.DecoratedChild.Terminate();
            if (childTerm == RunStatus.Running)
                return this.ReturnTermination(childTerm);

            // Otherwise, use our given function
            return this.ReturnTermination(this.InvokeFunction());
        }
    }
}