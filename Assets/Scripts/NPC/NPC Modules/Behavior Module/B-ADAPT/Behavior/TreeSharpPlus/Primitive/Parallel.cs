using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSharpPlus
{
    /// <summary>
    /// The base Parallel class. Parallel nodes execute all of their children simultaneously, with
    /// varying termination conditions.
    /// </summary>
    public abstract class Parallel : NodeGroup
    {
        public Parallel(params Node[] children)
            : base(children)
        {
        }

        public abstract override IEnumerable<RunStatus> Execute();
        protected List<RunStatus> childStatus = null;
        protected int runningChildren;

        public override void Start()
        {
            if (this.childStatus == null)
                this.childStatus = new List<RunStatus>(this.Children.Count);

            foreach (Node child in this.Children)
            {
                child.Start();
                this.childStatus.Add(RunStatus.Running);
            }

            base.Start();
            this.runningChildren = this.Children.Count;
        }

        public override void Stop()
        {
            base.Stop();
            this.runningChildren = 0;
            if (this.childStatus != null)
            {
                this.childStatus.Clear();
            }
        }

        protected RunStatus TerminateChildren()
        {
            return TreeUtils.DoUntilComplete<Node>(
                (Node n) => n.Terminate(),
                this.Children);
        }

        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;
            // Just terminate each child
            return this.ReturnTermination(this.TerminateChildren());
        }
    }
}