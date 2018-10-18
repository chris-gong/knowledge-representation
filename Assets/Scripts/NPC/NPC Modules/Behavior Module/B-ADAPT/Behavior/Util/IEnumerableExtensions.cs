// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections.Generic;
using System;

public static class IEnumerableExtensions
{
    /// <summary>
    /// Execute the given action on each item in the given collection of items.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <param name="list">The collection of items.</param>
    /// <param name="action">The action to execute on each item.</param>
    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (T elem in list)
        {
            action.Invoke(elem);
        }
    }

    /// <summary>
    /// Creates a new list of items, where the items are created by executing the
    /// given function on each item of the given collection.
    /// </summary>
    /// <typeparam name="T">The type of the input collection's items.</typeparam>
    /// <typeparam name="S">The type of the new list's items.</typeparam>
    /// <param name="list">The input collection.</param>
    /// <param name="func">Function converting items from the first collection
    /// to the result collection.</param>
    /// <returns></returns>
    public static List<S> Convert<T, S>(this IEnumerable<T> list, Func<T, S> func)
    {
        List<S> result = new List<S>();
        list.ForEach((T obj) => result.Add(func.Invoke(obj)));
        return result;
    }
}

