using System;
using System.Collections.Generic;
using UnityEngine;

public class References : MonoBehaviour
{
    [Serializable]
    public class RefItem
    {
        public string Name;
        public Transform Item;
    }

    public RefItem[] Items;
    private Dictionary<string, Transform> _fastAccessMap = new();

    void Awake()
    {
        foreach (var refItem in Items)
            _fastAccessMap.Add(refItem.Name, refItem.Item);
    }

    public Transform GetItem(string itemName) => _fastAccessMap[itemName];
}
