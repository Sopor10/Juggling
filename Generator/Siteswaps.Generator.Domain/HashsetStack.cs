﻿using System.Diagnostics.CodeAnalysis;

namespace Siteswaps.Generator.Domain;

public class HashsetStack<T>
{
    private HashSet<T> HashSet { get; } = new ();
    private Stack<T> Stack { get; } = new();

    public void Push(T item)
    {
        if (HashSet.Contains(item))
        {
            return;
        }
            
        Stack.Push(item);
        HashSet.Add(item);
    }

    public bool TryPop([MaybeNullWhen(false)] out T o)
    {
        return Stack.TryPop(out o);
    }

    public void Reset()
    {
        Stack.Clear();
        HashSet.Clear();
    }
}