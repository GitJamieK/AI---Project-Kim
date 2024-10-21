using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    // Set a value on the blackboard
    public void SetValue(string key, object value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }
    }

    // Get a value from the blackboard
    public T GetValue<T>(string key)
    {
        if (data.ContainsKey(key) && data[key] is T)
        {
            return (T)data[key];
        }

        return default(T);
    }

    // Check if the blackboard contains a key
    public bool ContainsKey(string key)
    {
        return data.ContainsKey(key);
    }
}
