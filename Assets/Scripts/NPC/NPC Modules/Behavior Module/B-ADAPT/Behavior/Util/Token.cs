using UnityEngine;
using System.Collections;

/// <summary>
/// A typesafe wrapper for attaching generic object tokens to internals
/// </summary>
public class Token 
{
    public static object Get(Token token)
    {
        if (token == null)
            return null;
        return token.Contents;
    }

    public object Contents { get; set; }
    public bool HasValue { get { return this.Contents != null; } }

    public Token() { this.Contents = null; }
    public Token(object contents) { this.Contents = contents; }

    public T Get<T>() { return (T)this.Contents; }
}
