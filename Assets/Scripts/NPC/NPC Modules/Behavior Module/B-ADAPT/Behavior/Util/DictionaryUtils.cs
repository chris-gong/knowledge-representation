// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;
using System.Collections.Generic;

public static class DictionaryUtils
{
    /// <summary>
    /// Extension method for comparing two dictionaries
    /// </summary>
    public static bool DictionaryEqual<TKey, TValue>(
        this IDictionary<TKey, TValue> first, 
        IDictionary<TKey, TValue> second)
    {
        if (first == second) return true;
        if ((first == null) || (second == null)) return false;
        if (first.Count != second.Count) return false;

        var comparer = EqualityComparer<TValue>.Default;

        foreach (KeyValuePair<TKey, TValue> kvp in first)
        {
            TValue secondValue;
            if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
            if (!comparer.Equals(kvp.Value, secondValue)) return false;
        }
        return true;
    }
}
