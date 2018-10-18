using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using TreeSharpPlus;

/// <summary>
/// Helper class takes a function returning IEnumerable of RunStatus
/// results and provides a single-invocation runner function for use in trees
/// </summary>
public class IteratorRunner
{
    private Func<IEnumerable<RunStatus>> func = null;
    private IEnumerator<RunStatus> enumerator = null;

    public RunStatus Run()
    {
        if (this.enumerator == null)
            this.enumerator = this.func().GetEnumerator();

        if (this.enumerator.MoveNext() == false)
        {
            this.enumerator = null;
            return RunStatus.Success;
        }

        RunStatus result = this.enumerator.Current;
        if (result != RunStatus.Running)
            this.enumerator = null;
        return result;
    }

    public IteratorRunner(Func<IEnumerable<RunStatus>> func)
    {
        this.func = func;
    }
}