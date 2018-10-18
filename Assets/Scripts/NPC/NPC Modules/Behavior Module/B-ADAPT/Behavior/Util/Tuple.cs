using System.Collections.Generic;
using System;

/// <summary>
/// 2-element tuple, with the first element being of type S, the second of type T.
/// </summary>
public class Tuple<S, T> : IEquatable<Tuple<S,T>>
{
	private S element1;
	private T element2;

	public Tuple(S element1, T element2)
	{
		this.element1 = element1;
		this.element2 = element2;
	}

	public override int GetHashCode()
	{
		return ((element1.GetHashCode() << 5)+ element1.GetHashCode()) ^ element2.GetHashCode();
		// ((h1 << 5) + h1) ^ h2)
	}
	
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return Equals((Tuple<S,T>)obj);
	}

	public bool Equals(Tuple<S,T> other)
	{
		return other.Item1.Equals(element1) && other.Item2.Equals(element2);
	}

	public override string ToString ()
	{
		return string.Format ("[Tuple: Element1={0}, Element2={1}]", Item1, Item2);
	}
	public Tuple(KeyValuePair<S, T> pair) : this(pair.Key, pair.Value) { }

	public S Item1 
	{
		get { return element1; }
		set { element1 = value; }
	}

	public T Item2 
	{
		get { return element2; }
		set { element2 = value; }
	}
}
