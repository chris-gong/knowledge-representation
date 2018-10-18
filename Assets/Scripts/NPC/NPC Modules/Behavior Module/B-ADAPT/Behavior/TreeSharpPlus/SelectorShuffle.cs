using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSharpPlus
{
    public class SelectorShuffle : NodeGroupWeighted
    {
        public SelectorShuffle(params NodeWeight[] weightedchildren)
            : base(weightedchildren)
        {
        }

        public SelectorShuffle(params Node[] children)
            : base(children)
        {
        }

        public override void Start()
        {
            base.Start();
            this.Shuffle();
        }

        public override IEnumerable<RunStatus> Execute()
        {
            // Proceed as we do with the original selector
            foreach (Node node in this.Children)
            {
                // Move to the next node
                this.Selection = node;
                node.Start();

                // If the current node is still running, report that. Don't 'break' the enumerator
                RunStatus result;
                while ((result = this.TickNode(node)) == RunStatus.Running)
                    yield return RunStatus.Running;

                // Call Stop to allow the node to clean anything up.
                node.Stop();

                // Clear the selection
                this.Selection.ClearLastStatus();
                this.Selection = null;

                // If it succeeded, we return success without trying any subsequent nodes
                if (result == RunStatus.Success)
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // Otherwise, we're still running
                yield return RunStatus.Running;
            }
            // We ran out of children, and none succeeded. Return failed.
            yield return RunStatus.Failure;
            // Make sure we tell our parent composite, that we're finished.
            yield break;
        }
    }
}
