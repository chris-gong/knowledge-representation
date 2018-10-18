using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public static class ReflectionUtil 
{
    /// <summary>
    /// Gets all the types which are subclasses of the given base type.
    /// The base type can both be a class or an interface.
    /// </summary>
    public static List<Type> GetSubclassesOf(Type baseType)
    {
        if (!baseType.IsInterface)
        {
            return GetClassesForPredicate(Assembly.GetAssembly(baseType), (Type t) => t.IsSubclassOf(baseType));
        }
        else
        {
            return GetClassesForPredicate(Assembly.GetAssembly(baseType), (Type t) => t.GetInterface(baseType.Name) != null);
        }
    }

    /// <summary>
    /// Gets all the types with the given attribute. With inherit, define whether
    /// attributes should be looked for in the inheritance chain as well.
    /// </summary>
    public static List<Type> GetClassesWithAttribute(Type attributeType, bool inherit)
    {
        return GetClassesForPredicate(Assembly.GetExecutingAssembly(), (Type t) => t.GetCustomAttributes(attributeType, inherit).Length > 0);
    }

    /// <summary>
    /// Returns all types in the given assembly satisfying the given predicate,
    /// </summary>
    private static List<Type> GetClassesForPredicate(Assembly assembly, Predicate<Type> predicate)
    {
        List<Type> result = new List<Type>();
        foreach (Type t in assembly.GetTypes())
        {
            if (predicate.Invoke(t))
            {
                result.Add(t);
            }
        }
        return result;
    }


}
