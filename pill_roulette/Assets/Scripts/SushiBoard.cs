using UnityEngine;
using System.Collections.Generic;


/*
* Mono behaviour class that caches the Item script of every sushi gameObject.
* Use the GetItem() function to get the Item script of the sushi gameObject you want.
*/
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

    /*
    * Input: Name of a sushi GameObject as a string
    *
    * Output: The Item script attached to that GameObject
    *
    * In order to avoid repeated calls of GetComponent(), this class has a dictionary with every
    * sushi GameObject's attached item script.
    */
    public Item GetItem(string n)
    {
        return _ObjCache[n];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
