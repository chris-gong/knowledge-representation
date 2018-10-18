using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using UnityEngine;

namespace TreeSharpPlus
{
    /// <summary>
    /// Executes a lambda function in a node. If the given lambda returns a 
    /// RunStatus, will return the resulting RunStatus from execution of the
    /// lambda. Otherwise, will always return RunStatus.Success.
    /// 
    /// NOTE: Do NOT use this for comparisons or evaluation! If you want to check
    /// boolean function results, use LeafAssert.
    /// </summary>
    public class LeafInvoke : Node
    {
        // A clunky way of allowing the user to specify whether we want
        // to use a function that returns a RunStatus or not. This is
        // ugly in code, but more efficient than, say, nesting lambdas
        protected Action func_noReturn = null;
        protected Func<RunStatus> func_return = null;

        protected Action term_noReturn = null;
        protected Func<RunStatus> term_return = null;

        protected LeafInvoke()
        {
            this.func_noReturn = null;
            this.func_return = null;

            this.term_noReturn = null;
            this.term_return = null;
        }

        public LeafInvoke(
            Func<RunStatus> function)
            : this()
        {
            this.func_return = function;
            this.term_return = null;
        }

        public LeafInvoke(
            Action function)
            : this()
        {
            this.func_noReturn = function;
            this.term_return = null;
        }

        public LeafInvoke(
            Func<RunStatus> function,
            Action terminate)
            : this()
        {
            this.func_return = function;
            this.term_noReturn = terminate;
        }

        public LeafInvoke(
            Action function,
            Action terminate)
            : this()
        {
            this.func_noReturn = function;
            this.term_noReturn = terminate;
        }

        public LeafInvoke(
            Func<RunStatus> function,
            Func<RunStatus> terminate)
            : this()
        {
            this.func_return = function;
            this.term_return = terminate;
        }

        public LeafInvoke(
            Action function,
            Func<RunStatus> terminate)
            : this()
        {
            this.func_noReturn = function;
            this.term_return = terminate;
        }

        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;

            // Do we have a termination function that returns a RunStatus?
            if (this.term_return != null)
                return this.ReturnTermination(this.term_return.Invoke());
            // If not, do we have a termination function that doesn't?
            else if (this.term_noReturn != null)
                this.term_noReturn.Invoke();

            return this.ReturnTermination(RunStatus.Success);
        }

        public override IEnumerable<RunStatus> Execute()
        {
            if (this.func_return != null)
            {
				//Debug.Log ("YesInvoked!");
                RunStatus status = RunStatus.Running;
                while (status == RunStatus.Running)
                {
                    status = this.func_return.Invoke();
					//Debug.Log (status);
					if (status != RunStatus.Running)
                        break;
                    yield return status;
                }
                yield return status;
                yield break;
            }
            else if (this.func_noReturn != null)
            {
                this.func_noReturn.Invoke();
				//Debug.Log ("NOInvoked!");
                yield return RunStatus.Success;
                yield break;
            }
            else
            {
                throw new ApplicationException(this + ": No method given");
            }
        }
    }
}