using System;
using System.Collections.Generic;

namespace TreeSharpPlus
{
    public static class TreeUtils
    {
        /// <summary>
        /// Given a collection of objects, will keep calling func on them
        /// until the result is either success or failure for all items.
        /// Returns failure if any of them failed after every call completed.
        /// 
        /// TODO: Maybe make this a yield-based function that keeps a list 
        /// for efficiency - AS
        /// </summary>
        public static RunStatus DoUntilComplete<T>(
            Func<T, RunStatus> func,
            IEnumerable<T> items)
        {
            RunStatus final = RunStatus.Success;
            foreach (T item in items)
            {
                RunStatus rs = func.Invoke(item);
                if (rs == RunStatus.Running)
                    final = RunStatus.Running;
                else if (final != RunStatus.Running && rs == RunStatus.Failure)
                    final = RunStatus.Failure;
            }
            return final;
        }
    }
}
