using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSharpPlus.ExtensionMethods;

namespace TreeSharpPlus
{
    public abstract class NodeGroupWeighted : NodeGroup
    {
        public List<float> Weights { get; set; }

        /// <summary>
        /// Shuffles the children using the given weights
        /// </summary>
        protected void Shuffle()
        {
            this.Children.Shuffle(this.Weights);
        }

        /// <summary>
        /// Initializes, fully normalized with no given weights
        /// </summary>
        public NodeGroupWeighted(params Node[] children)
            : base(children)
        {
            this.Weights = new List<float>();
            for (int i = 0; i < this.Children.Count; i++)
                this.Weights.Add(1.0f);
        }

        public NodeGroupWeighted(params NodeWeight[] weightedchildren)
        {
            // Initialize the base Children list and our new Weights list
            this.Children = new List<Node>();
            this.Weights = new List<float>();

            // Unpack the pairs and store their individual values
            foreach (NodeWeight weightedchild in weightedchildren)
            {
                this.Children.Add(weightedchild.Composite);
                this.Weights.Add(weightedchild.Weight);
            }
        }
    }

    /// <summary>
    /// A simple pair class for composites and weights, used for stochastic control nodes
    /// </summary>
    public class NodeWeight
    {
        public Node Composite { get; set; }
        public float Weight { get; set; }

        public NodeWeight(float weight, Node composite)
        {
            this.Composite = composite;
            this.Weight = weight;
        }
    }
}
