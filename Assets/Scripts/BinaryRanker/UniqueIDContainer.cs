using System.Collections.Generic;

public class UniqueIDContainer<T> where T : UniqueIDProperty<T>
{
    Dictionary<int, T> _container = new();

    public UniqueIDContainer() { }

    public UniqueIDContainer(Dictionary<int, T> container) => _container = container;

    public Dictionary<int, T> All => new(_container);

    public bool TryGetValue(int id, out T val)
    {
        return _container.TryGetValue(id, out val);
    }

    public int GetID(T val, bool addIfNotExist = true)
    {
        if(!val.IsValid())
            return -1;

        foreach(var kvp in _container)
        {
            if(kvp.Value.IsEqual(val))
                return kvp.Key;
        }

        if(!addIfNotExist)
            return -1;

        var id = GetNextAvailableID();
        _container.Add(id, val);
        val.SetId(id);
        return id;
    }

    int GetNextAvailableID()
    {
        for (int i = 0; i < int.MaxValue; i++)
        {
            if (_container.ContainsKey(i))
                continue;

            return i;
        }

        return -1;
    }
}
