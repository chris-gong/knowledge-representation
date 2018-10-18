using System;
using System.Collections.Generic;

namespace TreeSharpPlus.ExtensionMethods
{
    public static class Extensions
    {
        public static Random rng = null;

        /// <summary>
        /// Shuffles a list
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (rng == null)
                rng = new System.Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Returns a shuffle of the child nodes similar to that of Fisher-Yates, but
        /// incorporating the weights to increase the probability of a node being
        /// placed first
        /// 
        /// TODO: This is terribly inefficient and could be done better
        /// </summary>
        public static void Shuffle<T>(this IList<T> list, IList<float> weights)
        {
            if (rng == null)
                rng = new System.Random();

            // Iterate through the list and build a range list (0..n-1) and count
            // the weight total
            double total = 0.0;
            List<int> unused = new List<int>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                total += weights[i];
                unused.Add(i);
            }

            // Now, perform the shuffle
            List<T> order = new List<T>(list.Count);
            while (unused.Count > 0)
            {
                double subtotal = 0.0;
                double next = rng.NextDouble() * total;

                // The node we selected for the next child
                int selected = -1;

                // Look through all of the unused children remaining
                foreach (int unusedchild in unused)
                {
                    // If we can overtake the random value with the weight mass
                    // of this particular child, select it
                    double weight = weights[unusedchild];
                    if ((subtotal + weight) >= next)
                    {
                        selected = unusedchild;
                        break;
                    }

                    // Otherwise, add to the subtotal and keep going
                    subtotal += weight;
                }

                // Add the child we selected
                order.Add(list[selected]);

                // Remove the weight for de-facto renormalization
                total -= weights[selected];

                // Remove the child from consideration
                unused.Remove(selected);
            }

            list.Clear();
            foreach (T val in order)
                list.Add(val);
        }
    }
}
