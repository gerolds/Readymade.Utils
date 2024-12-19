using System;
using System.Collections.Generic;

public class DisposableCollectionItem<T> : IDisposable
{
    private readonly T _item;
    private readonly ICollection<T> _collection;

    public DisposableCollectionItem(T item, ICollection<T> collection)
    {
        _collection = collection;
        _item = item;
    }

    public void Dispose()
    {
        _collection.Remove(_item);
    }
}