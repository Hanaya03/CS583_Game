using UnityEngine;

public class Item : MonoBehaviour
{
    private bool _poisoned = false;
    public bool IsPoisoned => _poisoned;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PoisonItem()
    {
        _poisoned = true;
    }

    public void EatItem()
    {
        Object.Destroy(this.gameObject);
    }
}
