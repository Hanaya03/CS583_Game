using UnityEngine;
using System.Collections.Generic;

public class SushiBoard : MonoBehaviour
{
    [SerializeField] private GameObject[] _foodObj;
    private Dictionary<string, Item> _ObjCache = new Dictionary<string, Item>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(GameObject s in _foodObj)
        {

            _ObjCache.Add(s.name, s.GetComponent<Item>());
        }
    }

    public Item GetItem(string n)
    {
        return _ObjCache[n];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
